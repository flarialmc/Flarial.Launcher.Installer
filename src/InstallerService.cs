using System;
using System.IO;
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
    static readonly string s_path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    static readonly AddPackageOptions s_options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

    const string LauncherPackageUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.msix";
    const string LauncherCertificateUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.cer";
    const string LauncherCertificateThumbprint = "080862035B63C6B01A1F7F5E2A286939808F502ADCA100BDCB6F805FB0DD4171";

    static bool IsCertificateInstalled
    {
        get
        {
            using X509Store store = new(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            foreach (var certificate in store.Certificates)
            {
                var thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
                if (LauncherCertificateThumbprint.Equals(thumbprint, OrdinalIgnoreCase)) return true;
            }

            return false;
        }
    }

    internal static Task InstallCertificateAsync() => Task.Run(static async () =>
    {
        if (!IsCertificateInstalled)
        {
            using X509Certificate2 certificate = new(await HttpService.GetBytesAsync(LauncherCertificateUri));
            var thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);

            if (!thumbprint.Equals(LauncherCertificateThumbprint, OrdinalIgnoreCase))
                throw new SecurityException("The launcher's remote thumbprint doesn't match the installer's local thumbprint.");

            using X509Store store = new(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite); store.Add(certificate);
        }
    });


    internal static Task DownloadPackageAsync(Action<int> callback)
    {
        return HttpService.DownloadAsync(LauncherPackageUri, s_path, callback);
    }

    internal static Task InstallPackageAsync(Action<int> callback) => Task.Run(() =>
    {
        var info = s_manager.AddPackageByUriAsync(new(s_path), s_options);
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