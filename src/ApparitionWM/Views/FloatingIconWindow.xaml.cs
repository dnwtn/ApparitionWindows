using System.Windows;
using System.Windows.Media.Animation;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using MouseButton = System.Windows.Input.MouseButton;
using Point = System.Windows.Point;

namespace ApparitionWM.Views;

public partial class FloatingIconWindow : Window
{
    private System.Windows.Threading.DispatcherTimer? _hoverTimer;

    /// <summary>Raised when the user hovers long enough to show the control panel.</summary>
    public event Action<Point>? ShowPanelRequested;

    /// <summary>Raised when the user clicks "Settings" in the context menu.</summary>
    public event Action? SettingsRequested;

    /// <summary>Raised when the user clicks "Exit".</summary>
    public event Action? ExitRequested;

    public FloatingIconWindow()
    {
        InitializeComponent();
    }

    /// <summary>Sets the idle opacity of the icon (0.0–1.0).</summary>
    public void SetIdleOpacity(double opacity)
    {
        var sb = (Storyboard)FindResource("FadeOut");
        var anim = (DoubleAnimation)sb.Children[0];
        anim.To = Math.Clamp(opacity, 0.1, 1.0);
    }

    // ── Event Handlers ──────────────────────────────────────────────────

    private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            // Cancel hover timer if dragging
            _hoverTimer?.Stop();
            DragMove();
        }
    }

    private void Icon_MouseEnter(object sender, MouseEventArgs e)
    {
        // Fade in
        var fadeIn = (Storyboard)FindResource("FadeIn");
        fadeIn.Begin();

        // Pulse glow
        var pulse = (Storyboard)FindResource("PulseGlow");
        pulse.Begin();

        // Start hover timer for panel reveal
        _hoverTimer?.Stop();
        _hoverTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };
        _hoverTimer.Tick += (_, _) =>
        {
            _hoverTimer.Stop();
            var screenPos = PointToScreen(new Point(ActualWidth / 2, ActualHeight / 2));
            ShowPanelRequested?.Invoke(screenPos);
        };
        _hoverTimer.Start();
    }

    private void Icon_MouseLeave(object sender, MouseEventArgs e)
    {
        _hoverTimer?.Stop();

        var fadeOut = (Storyboard)FindResource("FadeOut");
        fadeOut.Begin();
    }

    private void ShowPanel_Click(object sender, RoutedEventArgs e)
    {
        var screenPos = PointToScreen(new Point(ActualWidth / 2, ActualHeight / 2));
        ShowPanelRequested?.Invoke(screenPos);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        ExitRequested?.Invoke();
    }
}
