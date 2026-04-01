namespace ApparitionWM.Models;

/// <summary>
/// A saved transparency + mode combination for a window matched by title.
/// </summary>
public sealed class WindowPreset
{
    public string WindowTitlePattern { get; set; } = string.Empty;
    public byte Opacity { get; set; } = 255;
    public WindowMode Mode { get; set; } = WindowMode.Normal;
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString() =>
        string.IsNullOrEmpty(DisplayName) ? WindowTitlePattern : DisplayName;
}
