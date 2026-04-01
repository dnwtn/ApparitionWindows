namespace ApparitionWM.Models;

/// <summary>
/// Application settings, serialized to JSON.
/// </summary>
public sealed class AppSettings
{
    public bool RestoreOnClose { get; set; }
    public int QuickTransparencyPercent { get; set; } = 50;
    public int IconTransparencyPercent { get; set; } = 50;
    public double IconX { get; set; } = 100;
    public double IconY { get; set; } = 100;
    public bool AutoStart { get; set; }
    public bool ShowFloatingIcon { get; set; } = true;
    public List<WindowPreset> Presets { get; set; } = [];
    public List<HotkeyBinding> Hotkeys { get; set; } = GetDefaultHotkeys();

    private static List<HotkeyBinding> GetDefaultHotkeys() =>
    [
        new()
        {
            Action = HotkeyAction.ToggleGhost,
            Modifiers = 0x0002 | 0x0004, // Ctrl+Shift
            VirtualKey = 0x47            // G
        },
        new()
        {
            Action = HotkeyAction.ToggleTopMost,
            Modifiers = 0x0002 | 0x0004, // Ctrl+Shift
            VirtualKey = 0x54            // T
        },
        new()
        {
            Action = HotkeyAction.QuickOpacityToggle,
            Modifiers = 0x0002 | 0x0004, // Ctrl+Shift
            VirtualKey = 0x4F            // O
        },
        new()
        {
            Action = HotkeyAction.ShowPanel,
            Modifiers = 0x0002 | 0x0004, // Ctrl+Shift
            VirtualKey = 0x41            // A
        }
    ];
}
