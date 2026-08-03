using System;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using static System.StringComparison;
using static Windows.Foundation.AsyncStatus;

namespace Flarial.Launcher.Installer;

static class InstallerService
{
    static readonly PackageManager s_manager = new();

    static readonly AddPackageOptions s_options = new()
    {
        ForceAppShutdown = true,
        ForceUpdateFromAnyVersion = true
    };

    const string LauncherPackageUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.msix";
    const string LauncherCertificateUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.cer";
    const string LauncherCertificateThumbprint = "080862035B63C6B01A1F7F5E2A286939808F502ADCA100BDCB6F805FB0DD4171";

    internal static Task InstallCertificateAsync() => Task.Run(static async () =>
    {
        using X509Store certificateStore = new(StoreName.TrustedPeople, StoreLocation.LocalMachine);
        certificateStore.Open(OpenFlags.ReadWrite);

        foreach (var installedCertificate in certificateStore.Certificates)
        {
            var installedCertificateThumbprint = installedCertificate.GetCertHashString(HashAlgorithmName.SHA256);
            if (LauncherCertificateThumbprint.Equals(installedCertificateThumbprint, OrdinalIgnoreCase)) return;
        }

        using X509Certificate2 launcherCertificate = new(await HttpService.GetBytesAsync(LauncherCertificateUri));
        var launcherCertificateFingerprint = launcherCertificate.GetCertHashString(HashAlgorithmName.SHA256);

        if (!launcherCertificateFingerprint.Equals(LauncherCertificateThumbprint, OrdinalIgnoreCase))
            throw new("The launcher's remote thumbprint doesn't match the installer's local thumbprint.");

        certificateStore.Add(launcherCertificate);
    });

    internal static Task InstallPackageAsync(Action<int> callback) => Task.Run(() =>
    {
        var info = s_manager.AddPackageByUriAsync(new(LauncherPackageUri), s_options);
        try
        {
            using ManualResetEventSlim handle = new();

            info.Completed += (sender, args) => handle.Set();
            info.Progress += (sender, args) => callback((int)args.percentage);

            handle.Wait();
            if (info.Status is Error) throw info.ErrorCode;
        }
        finally
        {
            info.Close();
        }
    });
}