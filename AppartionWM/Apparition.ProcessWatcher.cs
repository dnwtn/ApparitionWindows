using System;
using System.Management;
using System.Security.Principal;

namespace WindowManager
{
    public partial class Apparition
    {
        ManagementEventWatcher startWatch;
        ManagementEventWatcher stopWatch;

        public void SetupWatchers()
        {
            if (IsUserAdministrator())
            {
                startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
                startWatch.Start();

                stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                stopWatch.EventArrived += new EventArrivedEventHandler(stopWatch_EventArrived);
                stopWatch.Start();
            }
        }

        public void StopWatchers()
        {
            if (startWatch != null)
                startWatch.Stop();
            if (stopWatch != null)
                stopWatch.Stop();
        }

        private void stopWatch_EventArrived(object sender, EventArrivedEventArgs e) 
        {
            //MessageBox.Show(string.Format("Process stopped: {0}", e.NewEvent.Properties["ProcessName"].Value));
            LoadProcesses();
        }

        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e) 
        {
            //MessageBox.Show(string.Format("Process started: {0}", e.NewEvent.Properties["ProcessName"].Value));
            LoadProcesses();
        }

        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}