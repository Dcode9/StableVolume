using Microsoft.UI.Xaml;

namespace StableVolume;

public partial class App : Application
{
    private MainWindow? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        bool minimized = Environment.GetCommandLineArgs().Any(a => a == "--minimized");
        _window = new MainWindow();
        _window.Activate();
        if (minimized)
        {
            _window.MinimizeNow();
        }
    }
}
