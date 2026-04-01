using System.Management;
using System.Security.Principal;

namespace ApparitionWM.Services;

/// <summary>
/// Monitors Windows process start/stop events via WMI and raises
/// an event so the window list can be refreshed.
/// </summary>
public sealed class ProcessWatcherService : IDisposable
{
    private ManagementEventWatcher? _startWatch;
    private ManagementEventWatcher? _stopWatch;
    private bool _isRunning;

    /// <summary>Raised when any process starts or stops.</summary>
    public event EventHandler? ProcessChanged;

    /// <summary>
    /// Starts monitoring. Requires administrator privileges for WMI process trace events.
    /// Returns true if watchers were successfully started.
    /// </summary>
    public bool Start()
    {
        if (_isRunning) return true;
        if (!IsUserAdministrator()) return false;

        try
        {
            _startWatch = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _startWatch.EventArrived += OnProcessEvent;
            _startWatch.Start();

            _stopWatch = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _stopWatch.EventArrived += OnProcessEvent;
            _stopWatch.Start();

            _isRunning = true;
            return true;
        }
        catch
        {
            Stop();
            return false;
        }
    }

    /// <summary>Stops monitoring and releases WMI resources.</summary>
    public void Stop()
    {
        _startWatch?.Stop();
        _startWatch?.Dispose();
        _startWatch = null;

        _stopWatch?.Stop();
        _stopWatch?.Dispose();
        _stopWatch = null;

        _isRunning = false;
    }

    public void Dispose() => Stop();

    private void OnProcessEvent(object sender, EventArrivedEventArgs e)
    {
        ProcessChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsUserAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
