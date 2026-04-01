namespace WindowManager.Controls
{
    partial class SettingsPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.restoreWindowsCB = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.quickTransparencyNUD = new System.Windows.Forms.NumericUpDown();
            this.iconTransparencyNUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.quickTransparencyNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconTransparencyNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // restoreWindowsCB
            // 
            this.restoreWindowsCB.AutoSize = true;
            this.restoreWindowsCB.Location = new System.Drawing.Point(4, 4);
            this.restoreWindowsCB.Name = "restoreWindowsCB";
            this.restoreWindowsCB.Size = new System.Drawing.Size(156, 17);
            this.restoreWindowsCB.TabIndex = 0;
            this.restoreWindowsCB.Text = "Restore Windows On Close";
            this.restoreWindowsCB.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.iconTransparencyNUD);
            this.groupBox1.Location = new System.Drawing.Point(3, 86);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(243, 53);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Icon Transparency";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.quickTransparencyNUD);
            this.groupBox2.Location = new System.Drawing.Point(4, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(243, 53);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Quick Transparency Setting";
            // 
            // quickTransparencyNUD
            // 
            this.quickTransparencyNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.quickTransparencyNUD.Location = new System.Drawing.Point(7, 20);
            this.quickTransparencyNUD.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.quickTransparencyNUD.Name = "quickTransparencyNUD";
            this.quickTransparencyNUD.Size = new System.Drawing.Size(120, 16);
            this.quickTransparencyNUD.TabIndex = 0;
            this.quickTransparencyNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.quickTransparencyNUD.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // iconTransparencyNUD
            // 
            this.iconTransparencyNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.iconTransparencyNUD.Location = new System.Drawing.Point(8, 20);
            this.iconTransparencyNUD.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.iconTransparencyNUD.Name = "iconTransparencyNUD";
            this.iconTransparencyNUD.Size = new System.Drawing.Size(120, 16);
            this.iconTransparencyNUD.TabIndex = 0;
            this.iconTransparencyNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.iconTransparencyNUD.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // SettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.restoreWindowsCB);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(250, 150);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.quickTransparencyNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iconTransparencyNUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox restoreWindowsCB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown iconTransparencyNUD;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown quickTransparencyNUD;
    }
}
