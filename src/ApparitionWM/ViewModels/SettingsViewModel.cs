using System.Collections.ObjectModel;
using System.Windows.Input;
using ApparitionWM.Helpers;
using ApparitionWM.Models;
using ApparitionWM.Services;

namespace ApparitionWM.ViewModels;

/// <summary>
/// ViewModel for the Settings window.
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly AutoStartService _autoStartService;

    private bool _restoreOnClose;
    private int _quickTransparency;
    private int _iconTransparency;
    private bool _autoStart;
    private bool _showFloatingIcon;

    public SettingsViewModel(SettingsService settingsService, AutoStartService autoStartService)
    {
        _settingsService = settingsService;
        _autoStartService = autoStartService;

        LoadFromSettings();

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());
    }

    // ── Events ──────────────────────────────────────────────────────────

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    // ── Bindable Properties ─────────────────────────────────────────────

    public bool RestoreOnClose
    {
        get => _restoreOnClose;
        set => SetProperty(ref _restoreOnClose, value);
    }

    public int QuickTransparency
    {
        get => _quickTransparency;
        set => SetProperty(ref _quickTransparency, value);
    }

    public int IconTransparency
    {
        get => _iconTransparency;
        set => SetProperty(ref _iconTransparency, value);
    }

    public bool AutoStart
    {
        get => _autoStart;
        set => SetProperty(ref _autoStart, value);
    }

    public bool ShowFloatingIcon
    {
        get => _showFloatingIcon;
        set => SetProperty(ref _showFloatingIcon, value);
    }

    public ObservableCollection<HotkeyBinding> Hotkeys { get; private set; } = [];

    public ObservableCollection<WindowPreset> Presets { get; private set; } = [];

    // ── Commands ────────────────────────────────────────────────────────

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    // ── Methods ─────────────────────────────────────────────────────────

    private void LoadFromSettings()
    {
        var s = _settingsService.Settings;
        RestoreOnClose = s.RestoreOnClose;
        QuickTransparency = s.QuickTransparencyPercent;
        IconTransparency = s.IconTransparencyPercent;
        AutoStart = _autoStartService.IsEnabled();
        ShowFloatingIcon = s.ShowFloatingIcon;
        Hotkeys = new ObservableCollection<HotkeyBinding>(s.Hotkeys);
        Presets = new ObservableCollection<WindowPreset>(s.Presets);
    }

    private void Save()
    {
        _settingsService.Update(s =>
        {
            s.RestoreOnClose = RestoreOnClose;
            s.QuickTransparencyPercent = QuickTransparency;
            s.IconTransparencyPercent = IconTransparency;
            s.ShowFloatingIcon = ShowFloatingIcon;
            s.Hotkeys = [.. Hotkeys];
            s.Presets = [.. Presets];
        });

        _autoStartService.SetEnabled(AutoStart);

        SaveCompleted?.Invoke();
    }
}
