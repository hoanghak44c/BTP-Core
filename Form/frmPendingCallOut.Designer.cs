namespace QLBH.Core.Form
{
    partial class frmPendingCallOut
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.pbStatus = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnCancel = new QLBH.Core.Form.GtidButton();
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(12, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(268, 23);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Đang chờ xác nhận cuộc gọi";
            this.lblStatus.UseWaitCursor = true;
            // 
            // pbStatus
            // 
            this.pbStatus.Location = new System.Drawing.Point(12, 35);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(268, 30);
            this.pbStatus.TabIndex = 3;
            this.pbStatus.UseWaitCursor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(195, 71);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.ShortCutKey = System.Windows.Forms.Keys.None;
            this.btnCancel.Size = new System.Drawing.Size(85, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Hủy";
            // 
            // frmPendingCallOut
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 104);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pbStatus);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPendingCallOut";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chờ xác nhận cuộc gọi";
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private DevExpress.XtraEditors.ProgressBarControl pbStatus;
        private GtidButton btnCancel;
    }
}