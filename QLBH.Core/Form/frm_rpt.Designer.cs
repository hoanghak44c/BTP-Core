namespace QLBH.Core.Form
{
    partial class frm_rpt
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
            this.crtView = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // crtView
            // 
            this.crtView.ActiveViewIndex = -1;
            this.crtView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crtView.DisplayGroupTree = false;
            this.crtView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crtView.Location = new System.Drawing.Point(0, 0);
            this.crtView.Name = "crtView";
            this.crtView.SelectionFormula = "";
            this.crtView.Size = new System.Drawing.Size(619, 377);
            this.crtView.TabIndex = 0;
            this.crtView.ViewTimeSelectionFormula = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkGreen;
            this.label1.Location = new System.Drawing.Point(186, 161);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(261, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Đang xử lý dữ liệu, xin vui lòng đợi giây lát...";
            // 
            // frm_rpt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 377);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.crtView);
            this.Name = "frm_rpt";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frm_rpt";
            this.Load += new System.EventHandler(this.frm_rpt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crtView;
        private System.Windows.Forms.Label label1;
    }
}