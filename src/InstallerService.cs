using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.System;
using static Windows.Management.Deployment.DeploymentOptions;

namespace Flarial.Launcher.Installer;

static class InstallerService
{
    static readonly PackageManager s_manager = new();

    const string PackageUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.msix";
    const string CertificateUri = "https://cdn.flarial.xyz/launcher/Flarial.Launcher.cer";

    internal static Task InstallCertificateAsync() => Task.Run(static async () =>
    {
        using X509Store store = new(StoreName.TrustedPeople, StoreLocation.LocalMachine);
        using X509Certificate2 certificate = new(await HttpService.GetBytesAsync(CertificateUri));

        store.Open(OpenFlags.ReadWrite);

        if (store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, false).Count > 0)
            return;

        store.Add(certificate);
    });

    internal static Task InstallPackageAsync(Action<int> callback) => Task.Run(() =>
    {
        var info = s_manager.AddPackageAsync(new(PackageUri), null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);
        try
        {
            using ManualResetEventSlim handle = new();

            info.Completed += (sender, args) => handle.Set();
            info.Progress += (sender, args) => callback((int)args.percentage);
            handle.Wait();

            if (info.ErrorCode is { })
                throw info.ErrorCode;
        }
        finally
        {
            info.Close();
        }
    });
}