using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ApparitionWM.Models;
using ApparitionWM.ViewModels;

namespace ApparitionWM.Views;

// Provides WindowMode enum values for XAML binding
public static class WindowModeValues
{
    public static WindowMode[] All { get; } = Enum.GetValues<WindowMode>();
}

// Converts byte opacity (0-255) to percentage string
public sealed class ByteToPercentConverter : IValueConverter
{
    public static readonly ByteToPercentConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte b)
            return $"{(int)(b / 255.0 * 100)}";
        return "100";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

// Converts empty/null string to Visible, non-empty to Collapsed (for placeholder text)
public sealed class EmptyToVisibleConverter : IValueConverter
{
    public static readonly EmptyToVisibleConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public partial class ControlPanelWindow : Window
{
    private bool _isClosingViaFade;
    private System.Windows.Threading.DispatcherTimer? _deactivateTimer;

    public ControlPanelWindow()
    {
        InitializeComponent();

        PreviewKeyDown += OnPanelKeyDown;
    }

    private void OnPanelKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Don't steal keys when typing in the search box
        if (e.OriginalSource is System.Windows.Controls.TextBox && e.Key != Key.Escape)
            return;

        var vm = DataContext as ControlPanelViewModel;
        if (vm is null) return;

        switch (e.Key)
        {
            case Key.Escape:
                HideAnimated();
                e.Handled = true;
                break;

            case Key.OemPlus or Key.Add:
                vm.AdjustOpacity(5);
                e.Handled = true;
                break;

            case Key.OemMinus or Key.Subtract:
                vm.AdjustOpacity(-5);
                e.Handled = true;
                break;

            case Key.G:
                if (vm.SelectedWindow is not null)
                {
                    vm.SelectedMode = vm.SelectedMode == WindowMode.Ghost
                        ? WindowMode.Normal : WindowMode.Ghost;
                }
                e.Handled = true;
                break;

            case Key.T:
                if (vm.SelectedWindow is not null)
                {
                    vm.SelectedMode = vm.SelectedMode == WindowMode.TopMost
                        ? WindowMode.Normal : WindowMode.TopMost;
                }
                e.Handled = true;
                break;

            case Key.M:
                vm.CycleMode();
                e.Handled = true;
                break;

            case Key.Enter:
                vm.QuickToggleCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }

    /// <summary>Shows the panel with a fade+slide animation.</summary>
    public void ShowAnimated()
    {
        _deactivateTimer?.Stop();
        Show();
        Activate();
        var sb = (Storyboard)FindResource("PanelFadeIn");
        sb.Begin();
    }

    /// <summary>Fades out and hides the panel.</summary>
    public void HideAnimated()
    {
        _deactivateTimer?.Stop();
        if (!IsVisible || _isClosingViaFade) return;
        _isClosingViaFade = true;

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (_, _) =>
        {
            Hide();
            _isClosingViaFade = false;
        };
        RootBorder.BeginAnimation(OpacityProperty, fadeOut);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        // Use a short delay before hiding — allows re-activation when
        // BringToFront momentarily steals focus from this panel.
        _deactivateTimer?.Stop();
        _deactivateTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(600)
        };
        _deactivateTimer.Tick += (_, _) =>
        {
            _deactivateTimer.Stop();
            // Only hide if we're still not the active window
            if (!IsActive)
                HideAnimated();
        };
        _deactivateTimer.Start();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        // Cancel any pending deactivation hide
        _deactivateTimer?.Stop();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
}
