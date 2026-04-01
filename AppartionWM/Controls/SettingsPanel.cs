using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowManager.Controls
{
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();

            this.ForeColor = this.groupBox1.ForeColor = this.groupBox2.ForeColor = Color.White;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.RestoreOnclose = this.restoreWindowsCB.Checked;
            Properties.Settings.Default.QuickTransparency = (int)this.quickTransparencyNUD.Value;
            Properties.Settings.Default.IconTransparency = (int)this.iconTransparencyNUD.Value;
            Properties.Settings.Default.Save();
        }

        public void UpdateSettings()
        {
            this.restoreWindowsCB.Checked = Properties.Settings.Default.RestoreOnclose;
            this.quickTransparencyNUD.Value = (decimal) Properties.Settings.Default.QuickTransparency;
            this.iconTransparencyNUD.Value = (decimal) Properties.Settings.Default.IconTransparency;
        }
    }
}
