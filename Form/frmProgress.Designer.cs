using System.Windows.Forms;

namespace QLBH.Core.Form
{
    partial class frmProgress
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
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (System.Exception){
                //nothing to do here
            }
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
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(12, 27);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(268, 23);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.UseWaitCursor = true;
            // 
            // pbStatus
            // 
            this.pbStatus.Location = new System.Drawing.Point(12, 54);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(268, 30);
            this.pbStatus.TabIndex = 2;
            this.pbStatus.UseWaitCursor = true;
            // 
            // frmProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 96);
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)
                                 delegate
                                     {
                                         this.Controls.Add(this.pbStatus);
                                         this.Controls.Add(this.lblStatus);
                                     });
            }
            else
            {
                this.Controls.Add(this.pbStatus);
                this.Controls.Add(this.lblStatus);                
            }
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmProgress";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.UseWaitCursor = true;
            ((System.ComponentModel.ISupportInitialize)(this.pbStatus.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private DevExpress.XtraEditors.ProgressBarControl pbStatus;

    }
}