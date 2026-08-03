using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Flarial.Launcher.Installer;

sealed class MainWindow : Window
{
    readonly TextBlock _textBlock1 = new()
    {
        Text = $"Installing Flarial Launcher..."
    };

    readonly TextBlock _textBlock2 = new()
    {
        Text = $"Installing Certificate..."
    };

    readonly ProgressBar _progressBar = new()
    {
        Width = 359,
        Height = 23,
        IsIndeterminate = true
    };

    internal MainWindow()
    {
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico"))
            Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

        Title = "Flarial Launcher Installer";

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        SizeToContent = SizeToContent.WidthAndHeight;
        Content = new Canvas { Width = 381, Height = 115 };

        ((Canvas)Content).Children.Add(_textBlock1);
        ((Canvas)Content).Children.Add(_textBlock2);
        ((Canvas)Content).Children.Add(_progressBar);

        Canvas.SetLeft(_textBlock1, 11);
        Canvas.SetTop(_textBlock1, 15);
        Canvas.SetLeft(_textBlock2, 11);
        Canvas.SetTop(_textBlock2, 84);
        Canvas.SetLeft(_progressBar, 11);
        Canvas.SetTop(_progressBar, 46);
    }

    protected override void OnClosing(CancelEventArgs args)
    {
        args.Cancel = true;
    }

    protected override async void OnContentRendered(EventArgs args)
    {
        base.OnContentRendered(args);
        await InstallerService.InstallCertificateAsync();

        _textBlock2.Text = "Installing Package...";
        await InstallerService.InstallPackageAsync(value => Dispatcher.Invoke(() =>
        {
            if (_progressBar.Value != value)
            {
                _progressBar.Value = value;
                _progressBar.IsIndeterminate = false;
            }
        }));

        using (Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = @"shell:appsFolder\Flarial.Launcher_0jrgakbnj75vr!App"
        })) { }

        Environment.Exit(0);
    }
}