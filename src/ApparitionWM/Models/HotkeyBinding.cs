namespace ApparitionWM.Models;

/// <summary>
/// Actions that can be bound to global hotkeys.
/// </summary>
public enum HotkeyAction
{
    ToggleGhost,
    ToggleTopMost,
    QuickOpacityToggle,
    ShowPanel
}

/// <summary>
/// Represents a global hotkey binding: modifier keys + a virtual key code mapped to an action.
/// </summary>
public sealed class HotkeyBinding
{
    public HotkeyAction Action { get; set; }
    public uint Modifiers { get; set; }
    public uint VirtualKey { get; set; }

    /// <summary>Human-readable display string for the binding.</summary>
    public string DisplayString
    {
        get
        {
            var parts = new List<string>();
            if ((Modifiers & 0x0002) != 0) parts.Add("Ctrl");
            if ((Modifiers & 0x0004) != 0) parts.Add("Shift");
            if ((Modifiers & 0x0001) != 0) parts.Add("Alt");
            if ((Modifiers & 0x0008) != 0) parts.Add("Win");

            var key = System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)VirtualKey);
            parts.Add(key.ToString());

            return string.Join(" + ", parts);
        }
    }
}
