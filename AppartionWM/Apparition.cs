using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowManager
{
    public partial class Apparition : Form
    {
        IDictionary<string, IntPtr> windows = new Dictionary<string, IntPtr>();
        
        private IntPtr handle;
        private int opacity = 255;
        private uint colorref;
        private uint flags;

        Timer windowFadeIn;
        Timer windowFadeOut;

        private List<string> modes = new List<string>{ "Normal", "Top Most", "Ghost" };

        public Apparition()
        {
            InitializeComponent();
            this.BackColor = this.windowsLB.BackColor = this.modeCB.BackColor = Color.FromArgb(64, 64, 64);
            this.modeCB.ForeColor = Color.White;
            this.modeCB.DataSource = modes;
            this.windowsLB.DrawMode = DrawMode.OwnerDrawFixed;

            windowFadeIn = new Timer();
            windowFadeIn.Tick += windowFadeIn_Tick;
            windowFadeOut = new Timer();
            windowFadeOut.Tick += windowFadeOut_Tick;
        }

        //global brushes with ordinary/selected colors
        private SolidBrush reportsForegroundBrushSelected = new SolidBrush(Color.White);
        private SolidBrush reportsForegroundBrush = new SolidBrush(Color.White);
        private SolidBrush reportsBackgroundBrushSelected = new SolidBrush(Color.FromKnownColor(KnownColor.Gray));
        private SolidBrush reportsBackgroundBrush1 = new SolidBrush(Color.FromArgb(64, 64, 64));
        private SolidBrush reportsBackgroundBrush2 = new SolidBrush(Color.FromArgb(64, 64, 64));

        //custom method to draw the items, don't forget to set DrawMode of the ListBox to OwnerDrawFixed
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);

            int index = e.Index;
            if (index >= 0 && index < windowsLB.Items.Count)
            {
                string text = windowsLB.Items[index].ToString();
                Graphics g = e.Graphics;

                //background:
                SolidBrush backgroundBrush;
                if (selected)
                    backgroundBrush = reportsBackgroundBrushSelected;
                //else if ((index % 2) == 0)
                    //backgroundBrush = reportsBackgroundBrush1;
                else
                    backgroundBrush = reportsBackgroundBrush2;
                g.FillRectangle(backgroundBrush, e.Bounds);

                //text:
                SolidBrush foregroundBrush = (selected) ? reportsForegroundBrushSelected : reportsForegroundBrush;
                g.DrawString(text, e.Font, foregroundBrush, windowsLB.GetItemRectangle(index).Location);
            }

            e.DrawFocusRectangle();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadProcesses();

            SetupWatchers();
        }

        public void LoadProcesses()
        {
            if (windowsLB.InvokeRequired)
                windowsLB.Invoke((MethodInvoker)delegate { windowsLB.Items.Clear(); });
            else
                windowsLB.Items.Clear();

            windows = OpenWindowGetter.GetOpenWindows();

            foreach (string s in windows.Keys)
            {
                if (s != this.Name && s != this.Owner.Name && s != "Apparition Window Manager")
                {
                    if (windowsLB.InvokeRequired)
                        windowsLB.Invoke((MethodInvoker) delegate { windowsLB.Items.Add(s); });
                    else
                        windowsLB.Items.Add(s);
                }
            }
            //if(windowsLB.InvokeRequired)
            //{
            //    windowsLB.Invoke((MethodInvoker)delegate { windowsLB.Height = windowsLB.PreferredHeight; });
            //}   
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            handle = windows[windowsLB.SelectedItem.ToString()];

            if (handle == this.Handle || handle == this.Owner.Handle)
                this.modeCB.Enabled = false;
            else
                this.modeCB.Enabled = true;

            byte bOpacity;
            var temp = GetLayeredWindowAttributes(handle, out colorref, out bOpacity, out flags);

            if (temp && bOpacity != 0)
                opacity = bOpacity;
            else
            {
                opacity = 255;
            }

            GetWindowMode();

            this.Deactivate -= Apparition_Deactivate;

            //Bring Window to front
            SetWindowPos(handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            //var hwnd = FindWindow(null, listBox1.SelectedItem.ToString());

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(handle, ref placement);

            switch (placement.showCmd)
            {
                case 1:
                    ShowWindowAsync(handle, SW_SHOW);
                    break;
                case 2:
                    ShowWindowAsync(handle, SW_RESTORE);
                    break;
                default:
                    ShowWindowAsync(handle, SW_SHOW);
                    break;
            }
            //Bring Window to front
            SetForegroundWindow(handle);

            this.Deactivate += new System.EventHandler(this.Apparition_Deactivate);
            //GetEventHandlers();
            if (opacity < trackBar1.Minimum)
                opacity = trackBar1.Minimum;
            else if (opacity > trackBar1.Maximum)
                opacity = trackBar1.Maximum;

            trackBar1.Value = opacity;
            this.Activate();
        }

        private void GetWindowMode()
        {
            int extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);

            int transparent = extendedStyle & WS_EX_TRANSPARENT;
            int topMost = extendedStyle & WS_EX_TOPMOST;

            this.modeCB.SelectedIndexChanged -= this.modeCB_SelectedIndexChanged;
            //Set ComboBox to previously selected mode
            if (transparent == WS_EX_TRANSPARENT)//Ghost
            {
                this.modeCB.SelectedItem = modes[2];
            }
            else if (topMost == WS_EX_TOPMOST)//Top Most
            {
                this.modeCB.SelectedItem = modes[1];
            }
            else//Normal
            {
                this.modeCB.SelectedItem = modes[0];
            }

            this.modeCB.SelectedIndexChanged += this.modeCB_SelectedIndexChanged;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            opacity = trackBar1.Value;

            if (handle != null)
            {
                int extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);
                SetLayeredWindowAttributes(handle, 0, (byte)opacity, LWA_ALPHA);
            }
        }

        private void Apparition_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopWatchers();
            if (Properties.Settings.Default.RestoreOnclose)
            {
                foreach (KeyValuePair<string, IntPtr> window in windows)
                {
                    makeNormal(window.Value);
                    SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    SetLayeredWindowAttributes(window.Value, 0, (byte)255, LWA_ALPHA);
                }
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Cursor.Current = Cursors.SizeAll;
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Apparition_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Screen s = Screen.FromPoint((e as MouseEventArgs).Location);

            if (!this.Visible)
            {
                //int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                //int y = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
                int x = s.WorkingArea.Width - this.Width;
                int y = s.WorkingArea.Height - this.Height;
                this.Location = new Point(x, y);
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void modeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMode = this.modeCB.SelectedItem as string;

            SetWindowMode(selectedMode);
        }

        private void SetWindowMode(string selectedMode, bool keepOpacity = false)
        {
            this.Deactivate -= Apparition_Deactivate;
            if (selectedMode == modes[0])//Normal
            {
                makeNormal(handle);
                SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            else if (selectedMode == modes[1])//Top Most
            {
                if (!keepOpacity)
                    makeNormal(handle);
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                //SetWindowPos(handle, this.Owner.Handle, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                if (this.Owner != null)
                    SetWindowPos(this.Owner.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            else if (selectedMode == modes[2])//Ghost
            {
                makeTransparent(handle);
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            this.Deactivate += Apparition_Deactivate;
            this.Activate();
        }

        private void windowsLB_DoubleClick(object sender, EventArgs e)
        {
            if(windowsLB.SelectedItem != null)
                handle = windows[windowsLB.SelectedItem.ToString()];

            byte bOpacity;
            var temp = GetLayeredWindowAttributes(handle, out colorref, out bOpacity, out flags);

            if (temp && bOpacity != 255)
            {
                opacity = 255;
                //Set window mode to zero
                SetWindowMode(modes[0]);
            }
            else
            {
                opacity = (int) (Properties.Settings.Default.QuickTransparency/100*255);

                if (handle != null)
                {
                    int extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
                    SetWindowLong(handle, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);
                    //Set window mode
                    //TODO: Set as default from settings
                    SetWindowMode(modes[1], true);
                }
            }
            
            var val = SetLayeredWindowAttributes(handle, 0, (byte)opacity, LWA_ALPHA);
            trackBar1.Value = opacity;

            GetWindowMode();

            this.Activate();
        }

        private void Apparition_Deactivate(object sender, EventArgs e)
        {
            if (this.Visible)
                this.FadeOut();
        }

        void windowFadeIn_Tick(object sender, EventArgs e)
        {
            if (!windowFadeOut.Enabled)
            {
                if (this.Opacity < 1)
                    this.Opacity += .25;
                else
                {
                    windowFadeIn.Stop();
                    this.Activate();
                }
            }
        }

        public void FadeIn()
        {
            windowFadeIn.Start();
        }

        void windowFadeOut_Tick(object sender, EventArgs e)
        {
            if (!windowFadeIn.Enabled)
            {
                if (this.Opacity > 0)
                    this.Opacity -= .25;
                else
                {
                    windowFadeOut.Stop();
                    this.Hide();
                }
            }
            else
            {
                windowFadeOut.Stop();
            }
        }

        public void FadeOut()
        {
            windowFadeOut.Start();
        }
        }
}
