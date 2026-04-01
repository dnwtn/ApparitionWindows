using System.Collections.ObjectModel;
using System.Windows.Input;
using ApparitionWM.Helpers;
using ApparitionWM.Models;
using ApparitionWM.Services;

namespace ApparitionWM.ViewModels;

/// <summary>
/// ViewModel for the main control panel — drives the window list,
/// opacity slider, mode selector, and preset management.
/// </summary>
public sealed class ControlPanelViewModel : ObservableObject
{
    private readonly WindowInteropService _windowService;
    private readonly SettingsService _settingsService;

    private List<ManagedWindow> _allWindows = [];
    private ObservableCollection<ManagedWindow> _windows = [];
    private ManagedWindow? _selectedWindow;
    private int _opacity = 100;
    private WindowMode _selectedMode = WindowMode.Normal;
    private bool _isUpdatingFromSelection;
    private string _filterText = string.Empty;

    public ControlPanelViewModel(WindowInteropService windowService, SettingsService settingsService)
    {
        _windowService = windowService;
        _settingsService = settingsService;

        RefreshCommand = new RelayCommand(Refresh);
        QuickToggleCommand = new RelayCommand(QuickToggle, () => SelectedWindow is not null);
        SavePresetCommand = new RelayCommand(SavePreset, () => SelectedWindow is not null);
        ApplyPresetCommand = new RelayCommand(p => ApplyPreset(p as WindowPreset));
        DeletePresetCommand = new RelayCommand(p => DeletePreset(p as WindowPreset));
    }

    // ── Bindable Properties ─────────────────────────────────────────────

    public ObservableCollection<ManagedWindow> Windows
    {
        get => _windows;
        set => SetProperty(ref _windows, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
                ApplyFilter();
        }
    }

    /// <summary>Raised after BringToFront so the host can re-activate the panel.</summary>
    public event Action? RequestReactivate;

    public ManagedWindow? SelectedWindow
    {
        get => _selectedWindow;
        set
        {
            if (SetProperty(ref _selectedWindow, value) && value is not null)
            {
                _isUpdatingFromSelection = true;
                Opacity = ByteToPercent(value.Opacity);
                SelectedMode = value.Mode;
                _isUpdatingFromSelection = false;

                _windowService.BringToFront(value.Handle);

                // Ask the panel to re-activate after focus was stolen
                RequestReactivate?.Invoke();
            }
        }
    }

    public int Opacity
    {
        get => _opacity;
        set
        {
            if (SetProperty(ref _opacity, Math.Clamp(value, 5, 100)))
            {
                if (!_isUpdatingFromSelection && SelectedWindow is not null)
                {
                    var byteVal = PercentToByte(_opacity);
                    _windowService.SetWindowOpacity(SelectedWindow.Handle, byteVal);
                    SelectedWindow.Opacity = byteVal;
                }
            }
        }
    }

    public WindowMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (SetProperty(ref _selectedMode, value))
            {
                if (!_isUpdatingFromSelection && SelectedWindow is not null)
                {
                    _windowService.SetWindowMode(SelectedWindow.Handle, value);
                    SelectedWindow.Mode = value;
                }
            }
        }
    }

    public ObservableCollection<WindowPreset> Presets =>
        new(_settingsService.Settings.Presets);

    // ── Commands ────────────────────────────────────────────────────────

    public ICommand RefreshCommand { get; }
    public ICommand QuickToggleCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand ApplyPresetCommand { get; }
    public ICommand DeletePresetCommand { get; }

    // ── Public Methods ──────────────────────────────────────────────────

    public void Refresh()
    {
        var windowList = _windowService.GetOpenWindows();

        // Auto-apply saved presets to matching windows
        AutoApplyPresets(windowList);

        _allWindows = windowList;
        ApplyFilter();
    }

    /// <summary>Adjusts opacity by a relative amount (for keyboard shortcuts).</summary>
    public void AdjustOpacity(int delta)
    {
        Opacity += delta;
    }

    /// <summary>Cycles the mode: Normal → TopMost → Ghost → Normal.</summary>
    public void CycleMode()
    {
        if (SelectedWindow is null) return;
        SelectedMode = SelectedMode switch
        {
            WindowMode.Normal => WindowMode.TopMost,
            WindowMode.TopMost => WindowMode.Ghost,
            WindowMode.Ghost => WindowMode.Normal,
            _ => WindowMode.Normal
        };
    }

    /// <summary>
    /// Handles a global hotkey action on the foreground window.
    /// </summary>
    public void HandleHotkey(HotkeyAction action)
    {
        var fgHandle = _windowService.GetForegroundWindow();
        if (fgHandle == IntPtr.Zero) return;

        switch (action)
        {
            case HotkeyAction.ToggleGhost:
                ToggleMode(fgHandle, WindowMode.Ghost);
                break;
            case HotkeyAction.ToggleTopMost:
                ToggleMode(fgHandle, WindowMode.TopMost);
                break;
            case HotkeyAction.QuickOpacityToggle:
                QuickToggleForeground(fgHandle);
                break;
            case HotkeyAction.ShowPanel:
                // Handled by App.xaml.cs directly
                break;
        }
    }

    // ── Private Methods ─────────────────────────────────────────────────

    private void QuickToggle()
    {
        if (SelectedWindow is null) return;

        if (SelectedWindow.Opacity < 255)
        {
            // Restore to full
            _windowService.SetWindowOpacity(SelectedWindow.Handle, 255);
            _windowService.SetWindowMode(SelectedWindow.Handle, WindowMode.Normal);
            SelectedWindow.Opacity = 255;
            SelectedWindow.Mode = WindowMode.Normal;
        }
        else
        {
            // Apply quick transparency
            var pct = _settingsService.Settings.QuickTransparencyPercent;
            var newOpacity = (byte)(pct / 100.0 * 255);
            _windowService.SetWindowOpacity(SelectedWindow.Handle, newOpacity);
            _windowService.SetWindowMode(SelectedWindow.Handle, WindowMode.TopMost);
            SelectedWindow.Opacity = newOpacity;
            SelectedWindow.Mode = WindowMode.TopMost;
        }

        _isUpdatingFromSelection = true;
        Opacity = ByteToPercent(SelectedWindow.Opacity);
        SelectedMode = SelectedWindow.Mode;
        _isUpdatingFromSelection = false;
    }

    private void QuickToggleForeground(IntPtr handle)
    {
        var currentOpacity = _windowService.GetWindowOpacity(handle);
        if (currentOpacity < 255)
        {
            _windowService.SetWindowOpacity(handle, 255);
            _windowService.SetWindowMode(handle, WindowMode.Normal);
        }
        else
        {
            var pct = _settingsService.Settings.QuickTransparencyPercent;
            var newOpacity = (byte)(pct / 100.0 * 255);
            _windowService.SetWindowOpacity(handle, newOpacity);
            _windowService.SetWindowMode(handle, WindowMode.TopMost);
        }
    }

    private void ToggleMode(IntPtr handle, WindowMode targetMode)
    {
        var currentMode = _windowService.GetWindowMode(handle);
        var newMode = currentMode == targetMode ? WindowMode.Normal : targetMode;
        _windowService.SetWindowMode(handle, newMode);

        if (newMode == WindowMode.Normal)
        {
            _windowService.SetWindowOpacity(handle, 255);
        }
    }

    private void SavePreset()
    {
        if (SelectedWindow is null) return;

        var preset = new WindowPreset
        {
            WindowTitlePattern = SelectedWindow.Title,
            DisplayName = SelectedWindow.Title,
            Opacity = SelectedWindow.Opacity,
            Mode = SelectedWindow.Mode
        };

        _settingsService.Update(s =>
        {
            s.Presets.RemoveAll(p => p.WindowTitlePattern == preset.WindowTitlePattern);
            s.Presets.Add(preset);
        });

        OnPropertyChanged(nameof(Presets));
    }

    private void ApplyPreset(WindowPreset? preset)
    {
        if (preset is null || SelectedWindow is null) return;

        _windowService.SetWindowOpacity(SelectedWindow.Handle, preset.Opacity);
        _windowService.SetWindowMode(SelectedWindow.Handle, preset.Mode);
        SelectedWindow.Opacity = preset.Opacity;
        SelectedWindow.Mode = preset.Mode;

        _isUpdatingFromSelection = true;
        Opacity = ByteToPercent(preset.Opacity);
        SelectedMode = preset.Mode;
        _isUpdatingFromSelection = false;
    }

    private void DeletePreset(WindowPreset? preset)
    {
        if (preset is null) return;

        _settingsService.Update(s =>
            s.Presets.RemoveAll(p => p.WindowTitlePattern == preset.WindowTitlePattern));

        OnPropertyChanged(nameof(Presets));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static int ByteToPercent(byte b) => (int)Math.Round(b / 255.0 * 100);
    private static byte PercentToByte(int pct) => (byte)Math.Round(pct / 100.0 * 255);

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(_filterText))
        {
            Windows = new ObservableCollection<ManagedWindow>(_allWindows);
        }
        else
        {
            var filtered = _allWindows.Where(w =>
                w.Title.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
                w.FullTitle.Contains(_filterText, StringComparison.OrdinalIgnoreCase));
            Windows = new ObservableCollection<ManagedWindow>(filtered);
        }
    }

    private void AutoApplyPresets(List<ManagedWindow> windows)
    {
        var presets = _settingsService.Settings.Presets;
        if (presets.Count == 0) return;

        foreach (var window in windows)
        {
            var match = presets.FirstOrDefault(p =>
                window.FullTitle.Contains(p.WindowTitlePattern, StringComparison.OrdinalIgnoreCase));

            if (match is not null && (window.Opacity == 255 && window.Mode == WindowMode.Normal))
            {
                // Only auto-apply to windows that haven't been modified yet
                _windowService.SetWindowOpacity(window.Handle, match.Opacity);
                _windowService.SetWindowMode(window.Handle, match.Mode);
                window.Opacity = match.Opacity;
                window.Mode = match.Mode;
            }
        }
    }
}
