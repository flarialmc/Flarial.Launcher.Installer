using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Flarial.Launcher.Installer;

static class Program
{
    static Program() => AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
    {
        var exception = (Exception)args.ExceptionObject;
        while (exception.InnerException is not null) exception = exception.InnerException;

        MessageBox.Show(exception.Message, "Flarial Launcher Installer: Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(1);
    };

    [STAThread]
    static void Main()
    {
        using Mutex mutex = new(false, "68AA5B94-789D-4278-AB90-DA91B9CCB418", out var value);
        if (value) new Application().Run(new MainWindow());
    }
}