using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UnityPeek.UI;

namespace UnityPeek;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}
	UIManager reader;
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow();

			if (desktop.MainWindow is MainWindow mainWindow)
			{
				//Create the UIManager instance and start it
				var uiManager = new UIManager(mainWindow);
				uiManager.Start();
			}
		}

		base.OnFrameworkInitializationCompleted();
	}
}