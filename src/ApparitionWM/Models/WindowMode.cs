namespace ApparitionWM.Models;

/// <summary>
/// Window behavior modes that can be applied to any managed window.
/// </summary>
public enum WindowMode
{
    /// <summary>Default window behavior.</summary>
    Normal,

    /// <summary>Window stays above all non-topmost windows.</summary>
    TopMost,

    /// <summary>Always-on-top and click-through — mouse events pass through.</summary>
    Ghost
}
