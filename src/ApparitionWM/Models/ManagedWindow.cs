using System.Windows.Media;
using ApparitionWM.Helpers;

namespace ApparitionWM.Models;

/// <summary>
/// Represents a managed open window with its current state.
/// </summary>
public sealed class ManagedWindow : ObservableObject
{
    private string _title = string.Empty;
    private IntPtr _handle;
    private byte _opacity = 255;
    private WindowMode _mode = WindowMode.Normal;
    private ImageSource? _iconSource;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>Untruncated title used for preset pattern matching.</summary>
    public string FullTitle { get; set; } = string.Empty;

    public IntPtr Handle
    {
        get => _handle;
        set => SetProperty(ref _handle, value);
    }

    public byte Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public WindowMode Mode
    {
        get => _mode;
        set => SetProperty(ref _mode, value);
    }

    public ImageSource? IconSource
    {
        get => _iconSource;
        set => SetProperty(ref _iconSource, value);
    }

    public override string ToString() => Title;
}
