namespace WindowManager
{
    partial class Apparition
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Apparition));
            this.windowsLB = new System.Windows.Forms.ListBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.modeCB = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // windowsLB
            // 
            this.windowsLB.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.windowsLB.FormattingEnabled = true;
            this.windowsLB.Location = new System.Drawing.Point(10, 6);
            this.windowsLB.Name = "windowsLB";
            this.windowsLB.Size = new System.Drawing.Size(276, 56);
            this.windowsLB.TabIndex = 0;
            this.windowsLB.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.windowsLB.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.windowsLB.DoubleClick += new System.EventHandler(this.windowsLB_DoubleClick);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(6, 68);
            this.trackBar1.Maximum = 255;
            this.trackBar1.Minimum = 120;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(184, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.Value = 255;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Apparition";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Click += new System.EventHandler(this.notifyIcon1_Click);
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // modeCB
            // 
            this.modeCB.FormattingEnabled = true;
            this.modeCB.Location = new System.Drawing.Point(196, 69);
            this.modeCB.Name = "modeCB";
            this.modeCB.Size = new System.Drawing.Size(90, 21);
            this.modeCB.TabIndex = 2;
            this.modeCB.SelectedIndexChanged += new System.EventHandler(this.modeCB_SelectedIndexChanged);
            // 
            // Apparition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(292, 110);
            this.Controls.Add(this.modeCB);
            this.Controls.Add(this.windowsLB);
            this.Controls.Add(this.trackBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Apparition";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Apparition Window Manager";
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.Apparition_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Apparition_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
            this.Resize += new System.EventHandler(this.Apparition_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox windowsLB;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ComboBox modeCB;
    }
}

