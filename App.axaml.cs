using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace UnityPeek;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            if (desktop.MainWindow is MainWindow mainWindow)
            {
                //var reader = new Reader(mainWindow.OutputDisplay, mainWindow.MethodsDisplay);
                var reader = new UIManager(mainWindow);
                reader.Start();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}