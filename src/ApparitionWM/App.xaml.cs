using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using ApparitionWM.Models;
using ApparitionWM.Services;
using ApparitionWM.ViewModels;
using ApparitionWM.Views;
using Application = System.Windows.Application;

namespace ApparitionWM;

public partial class App : Application
{
    // ── Services ────────────────────────────────────────────────────────
    private readonly WindowInteropService _windowService = new();
    private readonly SettingsService _settingsService = new();
    private readonly ProcessWatcherService _processWatcher = new();
    private readonly HotkeyService _hotkeyService = new();
    private readonly AutoStartService _autoStartService = new();

    // ── Windows ─────────────────────────────────────────────────────────
    private FloatingIconWindow? _floatingIcon;
    private ControlPanelWindow? _controlPanel;
    private NotifyIcon? _trayIcon;

    // ── ViewModels ──────────────────────────────────────────────────────
    private ControlPanelViewModel? _controlPanelVm;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ── Single instance check ───────────────────────────────────────
        if (IsDuplicateInstance())
        {
            System.Windows.MessageBox.Show(
                "Apparition Window Manager is already running.",
                "Apparition", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // ── Load settings ───────────────────────────────────────────────
        _settingsService.Load();
        var settings = _settingsService.Settings;

        // ── Create ViewModel ────────────────────────────────────────────
        _controlPanelVm = new ControlPanelViewModel(_windowService, _settingsService);

        // ── Create Control Panel Window ─────────────────────────────────
        _controlPanel = new ControlPanelWindow
        {
            DataContext = _controlPanelVm
        };

        // Re-activate the panel after BringToFront steals focus
        _controlPanelVm.RequestReactivate += () =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _controlPanel?.Activate();
            }, System.Windows.Threading.DispatcherPriority.Background);
        };

        // ── Create Floating Icon ────────────────────────────────────────
        _floatingIcon = new FloatingIconWindow();
        _floatingIcon.Left = settings.IconX;
        _floatingIcon.Top = settings.IconY;
        _floatingIcon.SetIdleOpacity(settings.IconTransparencyPercent / 100.0);

        _floatingIcon.ShowPanelRequested += ShowControlPanel;
        _floatingIcon.SettingsRequested += ShowSettings;
        _floatingIcon.ExitRequested += ExitApplication;
        _floatingIcon.LocationChanged += (_, _) =>
        {
            _settingsService.Update(s =>
            {
                s.IconX = _floatingIcon.Left;
                s.IconY = _floatingIcon.Top;
            });
        };

        if (settings.ShowFloatingIcon)
            _floatingIcon.Show();

        // ── Setup System Tray Icon ──────────────────────────────────────
        SetupTrayIcon();

        // ── Setup Process Watcher ───────────────────────────────────────
        _processWatcher.ProcessChanged += (_, _) =>
        {
            Dispatcher.BeginInvoke(() => _controlPanelVm?.Refresh());
        };
        _processWatcher.Start();

        // ── Register Global Hotkeys ─────────────────────────────────────
        // We need an HWND — use the floating icon's window handle
        _floatingIcon.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(_floatingIcon).Handle;
            _hotkeyService.Initialize(hwnd);
            _hotkeyService.RegisterAll(settings.Hotkeys);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Save icon position
        if (_floatingIcon is not null)
        {
            _settingsService.Update(s =>
            {
                s.IconX = _floatingIcon.Left;
                s.IconY = _floatingIcon.Top;
            });
        }

        // Restore windows if configured
        if (_settingsService.Settings.RestoreOnClose)
        {
            var windows = _windowService.GetOpenWindows();
            foreach (var w in windows)
            {
                _windowService.RestoreWindow(w.Handle);
            }
        }

        // Cleanup
        _hotkeyService.Dispose();
        _processWatcher.Dispose();
        _trayIcon?.Dispose();

        base.OnExit(e);
    }

    // ── Panel Display ───────────────────────────────────────────────────

    private void ShowControlPanel(System.Windows.Point _)
    {
        if (_controlPanel is null || _controlPanelVm is null) return;

        _controlPanelVm.Refresh();

        // Use floating icon's position as anchor (already in WPF DIPs)
        double anchorX = _floatingIcon?.Left ?? 0;
        double anchorY = _floatingIcon?.Top ?? 0;
        double iconW = _floatingIcon?.ActualWidth ?? 60;
        double iconH = _floatingIcon?.ActualHeight ?? 60;

        // Panel dimensions (use ActualWidth/Height if laid out, otherwise fallback)
        double panelW = _controlPanel.ActualWidth > 0 ? _controlPanel.ActualWidth : 340;
        double panelH = _controlPanel.ActualHeight > 0 ? _controlPanel.ActualHeight : 460;

        // Get work area in DIPs using DPI scaling
        var dpiScale = GetDpiScale();
        var screen = Screen.FromPoint(new System.Drawing.Point(
            (int)((anchorX + iconW / 2) * dpiScale),
            (int)((anchorY + iconH / 2) * dpiScale)));
        var wa = screen.WorkingArea;

        // Convert physical pixels → WPF DIPs
        double waLeft = wa.Left / dpiScale;
        double waTop = wa.Top / dpiScale;
        double waRight = wa.Right / dpiScale;
        double waBottom = wa.Bottom / dpiScale;

        // Try placing to the right of the icon
        double x = anchorX + iconW + 8;
        double y = anchorY + iconH / 2 - panelH / 2;

        // If it overflows right, place to the left
        if (x + panelW > waRight)
            x = anchorX - panelW - 8;

        // Clamp vertically
        if (y + panelH > waBottom)
            y = waBottom - panelH;
        if (y < waTop)
            y = waTop;

        // Final horizontal clamp
        if (x + panelW > waRight)
            x = waRight - panelW;
        if (x < waLeft)
            x = waLeft;

        _controlPanel.Left = x;
        _controlPanel.Top = y;
        _controlPanel.ShowAnimated();
    }

    private static double GetDpiScale()
    {
        var source = PresentationSource.FromVisual(System.Windows.Application.Current.MainWindow!);
        if (source?.CompositionTarget != null)
            return source.CompositionTarget.TransformToDevice.M11;

        // Fallback: query from the desktop
        using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
        return g.DpiX / 96.0;
    }

    private void ShowSettings()
    {
        var settingsVm = new SettingsViewModel(_settingsService, _autoStartService);
        var settingsWindow = new SettingsWindow
        {
            DataContext = settingsVm
        };

        settingsVm.SaveCompleted += () =>
        {
            settingsWindow.Close();
            ApplySettingsToUI();
        };
        settingsVm.CancelRequested += () => settingsWindow.Close();

        settingsWindow.ShowDialog();
    }

    private void ApplySettingsToUI()
    {
        var settings = _settingsService.Settings;

        // Update floating icon visibility & opacity
        if (_floatingIcon is not null)
        {
            _floatingIcon.SetIdleOpacity(settings.IconTransparencyPercent / 100.0);
            if (settings.ShowFloatingIcon && !_floatingIcon.IsVisible)
                _floatingIcon.Show();
            else if (!settings.ShowFloatingIcon && _floatingIcon.IsVisible)
                _floatingIcon.Hide();
        }

        // Re-register hotkeys
        _hotkeyService.RegisterAll(settings.Hotkeys);
    }

    // ── Hotkey Handling ─────────────────────────────────────────────────

    private void OnHotkeyPressed(HotkeyAction action)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (action == HotkeyAction.ShowPanel)
            {
                if (_controlPanel?.IsVisible == true)
                {
                    _controlPanel.HideAnimated();
                }
                else
                {
                    // Show at cursor position
                    var cursorPos = System.Windows.Forms.Cursor.Position;
                    ShowControlPanel(new System.Windows.Point(cursorPos.X, cursorPos.Y));
                }
            }
            else
            {
                _controlPanelVm?.HandleHotkey(action);
            }
        });
    }

    // ── System Tray ─────────────────────────────────────────────────────

    private void SetupTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Text = "Apparition Window Manager",
            Visible = true
        };

        // Try to load the icon from resources
        try
        {
            var iconUri = new Uri("pack://application:,,,/Resources/apparition-icon.ico");
            using var stream = Application.GetResourceStream(iconUri)?.Stream;
            if (stream is not null)
                _trayIcon.Icon = new Icon(stream);
        }
        catch
        {
            _trayIcon.Icon = SystemIcons.Application;
        }

        // Context menu
        var menu = new ContextMenuStrip();
        menu.BackColor = System.Drawing.Color.FromArgb(37, 37, 64);
        menu.ForeColor = System.Drawing.Color.White;

        var showItem = menu.Items.Add("Show Panel");
        showItem.Click += (_, _) =>
        {
            var pos = System.Windows.Forms.Cursor.Position;
            Dispatcher.BeginInvoke(() =>
                ShowControlPanel(new System.Windows.Point(pos.X, pos.Y)));
        };

        menu.Items.Add(new ToolStripSeparator());

        var settingsItem = menu.Items.Add("Settings");
        settingsItem.Click += (_, _) => Dispatcher.BeginInvoke(ShowSettings);

        var toggleIconItem = menu.Items.Add("Toggle Floating Icon");
        toggleIconItem.Click += (_, _) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (_floatingIcon?.IsVisible == true)
                    _floatingIcon.Hide();
                else
                    _floatingIcon?.Show();

                _settingsService.Update(s => s.ShowFloatingIcon = _floatingIcon?.IsVisible ?? true);
            });
        };

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = menu.Items.Add("Exit");
        exitItem.Click += (_, _) => Dispatcher.BeginInvoke(ExitApplication);

        _trayIcon.ContextMenuStrip = menu;

        // Left-click: toggle panel
        _trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                var pos = System.Windows.Forms.Cursor.Position;
                Dispatcher.BeginInvoke(() =>
                {
                    if (_controlPanel?.IsVisible == true)
                        _controlPanel.HideAnimated();
                    else
                        ShowControlPanel(new System.Windows.Point(pos.X, pos.Y));
                });
            }
        };
    }

    // ── Utility ─────────────────────────────────────────────────────────

    private void ExitApplication()
    {
        Shutdown();
    }

    private static bool IsDuplicateInstance()
    {
        var currentName = System.IO.Path.GetFileNameWithoutExtension(
            System.Reflection.Assembly.GetEntryAssembly()?.Location ?? "Apparition");
        return System.Diagnostics.Process.GetProcessesByName(currentName).Length > 1;
    }
}
