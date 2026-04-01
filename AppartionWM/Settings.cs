using System;
using System.Drawing;
using System.Windows.Forms;
using WindowManager.Controls;

namespace WindowManager
{
    public partial class Settings : Form
    {
        SettingsPanel settingsPanel;

        public Settings()
        {
            InitializeComponent();

            this.BackColor = Color.FromArgb(64, 64, 64);
            this.settingsBtn.ForeColor = this.aboutBtn.ForeColor = this.saveButton.ForeColor = this.closeButton.ForeColor = Color.White;
            //tabControl1.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem);

            settingsPanel = new SettingsPanel();
            AddSettingsPanel();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            if ((sender as Button) == saveButton)
            {
                settingsPanel.SaveSettings();
            }
            else if ((sender as Button) == closeButton)
            {
                this.Close();
            }
        }

        private void tabControl1_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            //TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            //Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Red);
                g.FillRectangle(Brushes.Gray, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                e.DrawBackground();
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)10.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            //g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }

        private void settingsBtn_Click(object sender, EventArgs e)
        {
            AddSettingsPanel();
        }

        private void AddSettingsPanel()
        {
            this.panel1.Controls.Clear();
            this.panel1.Controls.Add(settingsPanel);
            settingsPanel.Dock = DockStyle.Fill;
            this.saveButton.Visible = true;
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            AddAboutPanel();
        }

        private void AddAboutPanel()
        {
            this.panel1.Controls.Clear();
            this.saveButton.Visible = false;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            settingsPanel.UpdateSettings();
        }
    }
}
