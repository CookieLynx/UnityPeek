namespace UnityPeek
{
	using Avalonia;
	using Avalonia.Controls.ApplicationLifetimes;
	using Avalonia.Markup.Xaml;
	using UnityPeek.UI;

    /// <summary>
    /// Represents the main application class for UnityPeek.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the framework initialization is completed.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();

                if (desktop.MainWindow is MainWindow mainWindow)
                {
                    // Create the UIManager instance and start it
                    var uiManager = new UIManager(mainWindow);
                    uiManager.Start();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}