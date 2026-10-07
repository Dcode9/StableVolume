using Microsoft.Win32;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace StableVolume;

public sealed partial class MainWindow : Window
{
    private readonly VolumeGuard _guard = new();
    private readonly AppSettings _settings = AppSettings.Load();
    private readonly DispatcherTimer _uiTimer = new();
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Stable Volume";
        try { SystemBackdrop = new MicaBackdrop(); } catch { }
        try { AppWindow.Resize(new Windows.Graphics.SizeInt32(600, 860)); } catch { }

        TargetSlider.Value = _settings.Target;
        ModeBox.SelectedIndex = _settings.CeilingOnly ? 1 : 0;
        UnmuteCheck.IsChecked = _settings.KeepUnmuted;
        StartupCheck.IsChecked = _settings.StartWithWindows;
        HoldSwitch.IsOn = _settings.Hold;
        _loading = false;

        ApplyToGuard();
        _guard.Start();

        _uiTimer.Interval = TimeSpan.FromMilliseconds(400);
        _uiTimer.Tick += (_, _) => RefreshUi();
        _uiTimer.Start();
        RefreshUi();

        Closed += (_, _) =>
        {
            _uiTimer.Stop();
            _guard.Dispose();
        };
    }

    public void MinimizeNow()
    {
        try
        {
            (AppWindow.Presenter as OverlappedPresenter)?.Minimize();
        }
        catch
        {
        }
    }

    private void ApplyToGuard()
    {
        _guard.TargetPercent = (int)TargetSlider.Value;
        _guard.CeilingOnly = ModeBox.SelectedIndex == 1;
        _guard.KeepUnmuted = UnmuteCheck.IsChecked == true;
        _guard.Enabled = HoldSwitch.IsOn;
    }

    private void SaveSettings()
    {
        _settings.Hold = HoldSwitch.IsOn;
        _settings.Target = (int)TargetSlider.Value;
        _settings.CeilingOnly = ModeBox.SelectedIndex == 1;
        _settings.KeepUnmuted = UnmuteCheck.IsChecked == true;
        _settings.StartWithWindows = StartupCheck.IsChecked == true;
        _settings.Save();
    }

    private void RefreshUi()
    {
        int cur = _guard.CurrentPercent;
        CurrentText.Text = cur < 0 ? "--%" : cur + "%";
        DeviceText.Text = _guard.DeviceName;

        int target = (int)TargetSlider.Value;
        bool ceiling = ModeBox.SelectedIndex == 1;
        TargetLabel.Text = (ceiling ? "Maximum volume: " : "Volume to hold: ") + target + "%";

        if (HoldSwitch.IsOn)
        {
            string what = ceiling ? "Capped at " : "Holding at ";
            StatusText.Text = what + target + "%. Fixed " + _guard.Corrections + " time" +
                              (_guard.Corrections == 1 ? "" : "s") + " so far.";
        }
        else
        {
            StatusText.Text = "Off. Windows is in control.";
        }
    }

    private void HoldSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        ApplyToGuard();
        SaveSettings();
        RefreshUi();
    }

    private void TargetSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_loading) return;
        ApplyToGuard();
        SaveSettings();
        RefreshUi();
    }

    private void UseCurrentButton_Click(object sender, RoutedEventArgs e)
    {
        int cur = _guard.CurrentPercent;
        if (cur >= 0)
        {
            TargetSlider.Value = cur;
        }
    }

    private void ModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        ApplyToGuard();
        SaveSettings();
        RefreshUi();
    }

    private void Option_Click(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        SetStartup(StartupCheck.IsChecked == true);
        ApplyToGuard();
        SaveSettings();
    }

    private static void SetStartup(bool on)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            if (on)
            {
                key.SetValue("StableVolume", "\"" + Environment.ProcessPath + "\" --minimized");
            }
            else
            {
                key.DeleteValue("StableVolume", false);
            }
        }
        catch
        {
        }
    }
}
