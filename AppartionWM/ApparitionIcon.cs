 using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowManager
{
    public partial class ApparitionIcon: Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        Apparition window;
        double iconTransparency;
        Timer fadeOut;
        Timer fadeIn;

        public ApparitionIcon()
        {
            InitializeComponent();
            
                window = new Apparition();
                window.Owner = this;
                window.StartPosition = FormStartPosition.Manual;

                iconTransparency = Properties.Settings.Default.IconTransparency/100;

                BackColor = Color.LightGray;
                TransparencyKey = Color.LightGray;
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                //TransparencyKey = this.BackColor;
                this.ShowInTaskbar = false;

                this.contextMenuStrip.BackColor = Color.FromArgb(64, 64, 64);
                this.contextMenuStrip.ForeColor = Color.White;

                this.window.Deactivate += Window_Deactivate;

                fadeIn = new Timer();
                fadeIn.Tick += FadeIn_Tick;
                fadeOut = new Timer();
                fadeOut.Tick += FadeOut_Tick;

        }

        ////draw image wherever you want
        //protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        //{
        //    e.Graphics.DrawImage(this.BackgroundImage, 0, 0, 55, 55);
        //}

        private void Window_Deactivate(object sender, EventArgs e)
        {
            fadeOut.Start();
        }

        private void FadeIn_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1)
                this.Opacity += .25;
            else
                fadeIn.Stop();
        }

        private void FadeOut_Tick(object sender, EventArgs e)
        {
            if (!this.window.Visible)
            {
                if (this.Opacity > iconTransparency)
                    this.Opacity -= .1;
                else
                    fadeOut.Stop();
            }
            else
            {
                fadeOut.Stop();
            }
        }

        bool isXVisibleOnAScreen(Point p)
        {
            Screen s = Screen.FromControl(this);
            if (p.X < s.Bounds.Right && p.X > s.Bounds.Left)
                return true;
         
            return false;
        }

        bool isYVisibleOnAScreen(Point p)
        {
            Screen s = Screen.FromControl(this);
            if (p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                return true;

            return false;
        }

        bool isPointVisibleOnAScreen(Point p)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (p.X > s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                    return true;
            }
            return false;
        }

        private void ApparitionIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (window.Visible)
                window.FadeOut();

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void ApparitionIcon_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Size = new Size(55,55);
            Location = Properties.Settings.Default.Location;
        }

        private void ApparitionIcon_Click(object sender, EventArgs e)
        {
            if ((e as MouseEventArgs).Button == MouseButtons.Right)
            {
                this.contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ApparitionIcon_MouseLeave(object sender, EventArgs e)
        {
            if(!fadeIn.Enabled)
                fadeOut.Start();
        }

        private void ApparitionIcon_MouseEnter(object sender, EventArgs e)
        {
            if(!fadeOut.Enabled)
                fadeIn.Start();
        }

        private void ApparitionIcon_MouseHover(object sender, EventArgs e)
        {
            if (!window.Visible || window.Opacity == 0)
            {
                Point p = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
                
                if(!isXVisibleOnAScreen(new Point(p.X + window.Width, p.Y)))
                {
                    p = new Point(p.X - window.Width, p.Y);
                }

                if (!isYVisibleOnAScreen(new Point(p.X, p.Y + window.Height)))
                {
                    p = new Point(p.X, p.Y - window.Height);
                }

                window.Location = p;
                window.LoadProcesses();
                window.Opacity = 0;
                window.Show(this);
                window.FadeIn();
            }
        }

        private void ApparitionIcon_Enter(object sender, EventArgs e)
        {
            if (window.Visible)
                window.Hide();
        }

        private void ApparitionIcon_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Location = Location;
            Properties.Settings.Default.Save();
        }

        private void settingsStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settingsPanel = new Settings();
            settingsPanel.Show();
        }
    }
}
