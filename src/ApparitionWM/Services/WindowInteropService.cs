using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApparitionWM.Helpers;
using ApparitionWM.Models;

namespace ApparitionWM.Services;

/// <summary>
/// Provides Win32 interop operations for managing window transparency,
/// z-order, and click-through behavior.
/// </summary>
public sealed class WindowInteropService
{
    /// <summary>
    /// Enumerates all visible top-level windows, excluding shell and
    /// the Start menu, returning them as ManagedWindow instances.
    /// </summary>
    public List<ManagedWindow> GetOpenWindows(IntPtr excludeHandle1 = default, IntPtr excludeHandle2 = default)
    {
        var shellWindow = NativeMethods.GetShellWindow();
        var windows = new List<ManagedWindow>();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (hWnd == shellWindow) return true;
            if (!NativeMethods.IsWindowVisible(hWnd)) return true;
            if (hWnd == excludeHandle1 || hWnd == excludeHandle2) return true;

            int length = NativeMethods.GetWindowTextLength(hWnd);
            if (length == 0) return true;

            var builder = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(hWnd, builder, length + 1);
            var fullTitle = builder.ToString();

            if (fullTitle is "Start" or "Apparition Window Manager" or "Apparition")
                return true;

            // Truncate for display
            var displayTitle = fullTitle.Length > 45
                ? string.Concat(fullTitle.AsSpan(0, 42), "...")
                : fullTitle;

            var window = new ManagedWindow
            {
                Title = displayTitle,
                FullTitle = fullTitle,
                Handle = hWnd,
                Opacity = GetWindowOpacity(hWnd),
                Mode = GetWindowMode(hWnd),
                IconSource = GetWindowIcon(hWnd)
            };

            windows.Add(window);
            return true;
        }, 0);

        return windows;
    }

    /// <summary>Extracts the icon for a window, trying WM_GETICON then process exe.</summary>
    public static ImageSource? GetWindowIcon(IntPtr hWnd)
    {
        try
        {
            // Try WM_GETICON (small icon first)
            var iconHandle = NativeMethods.SendMessagePtr(hWnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_SMALL, IntPtr.Zero);
            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessagePtr(hWnd, NativeMethods.WM_GETICON, (IntPtr)NativeMethods.ICON_BIG, IntPtr.Zero);

            if (iconHandle != IntPtr.Zero)
            {
                var source = Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                source.Freeze();
                return source;
            }

            // Fallback: get icon from the process executable
            NativeMethods.GetWindowThreadProcessId(hWnd, out uint pid);
            var process = Process.GetProcessById((int)pid);
            var exePath = process.MainModule?.FileName;
            if (exePath != null)
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon != null)
                {
                    var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    source.Freeze();
                    icon.Dispose();
                    return source;
                }
            }
        }
        catch
        {
            // Access denied for some system processes — silently skip
        }

        return null;
    }

    /// <summary>Gets the current opacity of a window (0-255).</summary>
    public byte GetWindowOpacity(IntPtr handle)
    {
        bool result = NativeMethods.GetLayeredWindowAttributes(handle, out _, out byte opacity, out _);
        return result && opacity != 0 ? opacity : (byte)255;
    }

    /// <summary>Sets the opacity of a window (0-255).</summary>
    public void SetWindowOpacity(IntPtr handle, byte opacity)
    {
        int style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE, style | NativeMethods.WS_EX_LAYERED);
        NativeMethods.SetLayeredWindowAttributes(handle, 0, opacity, NativeMethods.LWA_ALPHA);
    }

    /// <summary>Detects the current mode of a window from its extended styles.</summary>
    public WindowMode GetWindowMode(IntPtr handle)
    {
        int style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);

        if ((style & NativeMethods.WS_EX_TRANSPARENT) == NativeMethods.WS_EX_TRANSPARENT)
            return WindowMode.Ghost;
        if ((style & NativeMethods.WS_EX_TOPMOST) == NativeMethods.WS_EX_TOPMOST)
            return WindowMode.TopMost;

        return WindowMode.Normal;
    }

    /// <summary>Applies a window mode (Normal, TopMost, or Ghost).</summary>
    public void SetWindowMode(IntPtr handle, WindowMode mode)
    {
        switch (mode)
        {
            case WindowMode.Normal:
                RemoveTransparent(handle);
                RemoveLayered(handle);
                NativeMethods.SetWindowPos(handle, NativeMethods.HWND_NOTOPMOST,
                    0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);
                break;

            case WindowMode.TopMost:
                RemoveTransparent(handle);
                NativeMethods.SetWindowPos(handle, NativeMethods.HWND_TOPMOST,
                    0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);
                break;

            case WindowMode.Ghost:
                MakeTransparent(handle);
                NativeMethods.SetWindowPos(handle, NativeMethods.HWND_TOPMOST,
                    0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);
                break;
        }
    }

    /// <summary>Brings a window to the foreground, restoring it if minimized.</summary>
    public void BringToFront(IntPtr handle)
    {
        NativeMethods.SetWindowPos(handle, NativeMethods.HWND_TOP,
            0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);

        var placement = new NativeMethods.WINDOWPLACEMENT();
        placement.length = System.Runtime.InteropServices.Marshal.SizeOf(placement);
        NativeMethods.GetWindowPlacement(handle, ref placement);

        uint showCmd = placement.showCmd switch
        {
            2 => NativeMethods.SW_RESTORE,  // minimized → restore
            _ => NativeMethods.SW_SHOW
        };

        NativeMethods.ShowWindowAsync(handle, showCmd);
        NativeMethods.SetForegroundWindow(handle);
    }

    /// <summary>Resets a window to fully opaque, normal mode.</summary>
    public void RestoreWindow(IntPtr handle)
    {
        RemoveTransparent(handle);
        RemoveLayered(handle);
        NativeMethods.SetWindowPos(handle, NativeMethods.HWND_NOTOPMOST,
            0, 0, 0, 0, NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_SHOWWINDOW);
        SetWindowOpacity(handle, 255);
    }

    /// <summary>Gets the handle of the currently focused foreground window.</summary>
    public IntPtr GetForegroundWindow() => NativeMethods.GetForegroundWindow();

    // ── Private helpers ─────────────────────────────────────────────────

    private static void MakeTransparent(IntPtr handle)
    {
        int style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE,
            style | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED);
    }

    private static void RemoveTransparent(IntPtr handle)
    {
        int style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE,
            style & ~NativeMethods.WS_EX_TRANSPARENT);
    }

    private static void RemoveLayered(IntPtr handle)
    {
        int style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE,
            style & ~NativeMethods.WS_EX_LAYERED);
        NativeMethods.SetLayeredWindowAttributes(handle, 0, 255, 0);
    }
}
