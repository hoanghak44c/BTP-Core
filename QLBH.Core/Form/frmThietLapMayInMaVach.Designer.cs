namespace QLBH.Core.Form
{
    partial class frmThietLapMayInMaVach
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
            this.label6 = new System.Windows.Forms.Label();
            this.txtConfig = new QLBH.Core.Form.GtidTextBox();
            this.btnOK = new QLBH.Core.Form.GtidButton();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(268, 36);
            this.label6.TabIndex = 82;
            this.label6.Text = "Thiết lập máy in mã vạch, tên máy in phải trùng với tên được chia sẻ (Ví dụ: \\\\12" +
                "7.0.0.1\\GodexEz)";
            // 
            // txtConfig
            // 
            this.txtConfig.Location = new System.Drawing.Point(12, 48);
            this.txtConfig.Name = "txtConfig";
            this.txtConfig.Size = new System.Drawing.Size(268, 21);
            this.txtConfig.TabIndex = 85;
            this.txtConfig.Text = "\\\\127.0.0.1\\GodexEZ";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(193, 90);
            this.btnOK.Name = "btnOK";
            this.btnOK.ShortCutKey = System.Windows.Forms.Keys.None;
            this.btnOK.Size = new System.Drawing.Size(87, 25);
            this.btnOK.TabIndex = 86;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frmThietLapMayInMaVach
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 127);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtConfig);
            this.Controls.Add(this.label6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmThietLapMayInMaVach";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thiết lập máy in mã vạch";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label6;
        private QLBH.Core.Form.GtidTextBox txtConfig;
        private QLBH.Core.Form.GtidButton btnOK;
    }
}