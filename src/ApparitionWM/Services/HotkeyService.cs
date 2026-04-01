using System.Windows.Interop;
using ApparitionWM.Helpers;
using ApparitionWM.Models;

namespace ApparitionWM.Services;

/// <summary>
/// Registers and manages global hotkeys using Win32 RegisterHotKey.
/// Requires an HwndSource (WPF window handle) to receive WM_HOTKEY messages.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private IntPtr _hwnd;
    private HwndSource? _source;
    private readonly Dictionary<int, HotkeyBinding> _registeredHotkeys = new();
    private int _nextId = 9000; // start IDs high to avoid conflicts

    /// <summary>Raised when a registered hotkey is pressed.</summary>
    public event Action<HotkeyAction>? HotkeyPressed;

    /// <summary>
    /// Initializes the service with a WPF window handle for receiving messages.
    /// Call this after the window is loaded.
    /// </summary>
    public void Initialize(IntPtr hwnd)
    {
        _hwnd = hwnd;
        _source = HwndSource.FromHwnd(hwnd);
        _source?.AddHook(WndProc);
    }

    /// <summary>Registers all hotkeys from the provided bindings list.</summary>
    public void RegisterAll(IEnumerable<HotkeyBinding> bindings)
    {
        UnregisterAll();

        foreach (var binding in bindings)
        {
            var id = _nextId++;
            if (NativeMethods.RegisterHotKey(_hwnd, id, binding.Modifiers | NativeMethods.MOD_NOREPEAT, binding.VirtualKey))
            {
                _registeredHotkeys[id] = binding;
            }
        }
    }

    /// <summary>Unregisters all currently registered hotkeys.</summary>
    public void UnregisterAll()
    {
        foreach (var id in _registeredHotkeys.Keys)
        {
            NativeMethods.UnregisterHotKey(_hwnd, id);
        }
        _registeredHotkeys.Clear();
        _nextId = 9000;
    }

    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_registeredHotkeys.TryGetValue(id, out var binding))
            {
                HotkeyPressed?.Invoke(binding.Action);
                handled = true;
            }
        }
        return IntPtr.Zero;
    }
}
