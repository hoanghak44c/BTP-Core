using System.Windows.Forms;

namespace QLBH.Core.Form
{
    partial class frmLookUp_BaseNew_1<T>
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.grcLookUp = new DevExpress.XtraGrid.GridControl();
            this.grvLookUp = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.txtLookUp = new QLBH.Core.Form.GtidTextBox();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grcLookUp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grvLookUp)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 472);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(531, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsStatus
            // 
            this.tsStatus.Name = "tsStatus";
            this.tsStatus.Size = new System.Drawing.Size(516, 17);
            this.tsStatus.Spring = true;
            this.tsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.grcLookUp);
            this.panel1.Controls.Add(this.txtLookUp);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(531, 472);
            this.panel1.TabIndex = 7;
            // 
            // grcLookUp
            // 
            this.grcLookUp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grcLookUp.Location = new System.Drawing.Point(3, 39);
            this.grcLookUp.MainView = this.grvLookUp;
            this.grcLookUp.Name = "grcLookUp";
            this.grcLookUp.Size = new System.Drawing.Size(525, 430);
            this.grcLookUp.TabIndex = 8;
            this.grcLookUp.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.grvLookUp});
            // 
            // grvLookUp
            // 
            this.grvLookUp.GridControl = this.grcLookUp;
            this.grvLookUp.Name = "grvLookUp";
            this.grvLookUp.OptionsCustomization.AllowGroup = false;
            this.grvLookUp.OptionsView.ShowAutoFilterRow = true;
            this.grvLookUp.OptionsView.ShowGroupPanel = false;
            this.grvLookUp.TopRowChanged += new System.EventHandler(this.grvLookUp_TopRowChanged);
            this.grvLookUp.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grvLookUp_KeyDown);
            this.grvLookUp.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.grvLookUp_KeyPress);
            this.grvLookUp.ColumnFilterChanged += new System.EventHandler(this.grvLookUp_ColumnFilterChanged);
            this.grvLookUp.DoubleClick += new System.EventHandler(this.grvLookUp_DoubleClick);
            // 
            // txtLookUp
            // 
            this.txtLookUp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLookUp.Location = new System.Drawing.Point(3, 12);
            this.txtLookUp.Name = "txtLookUp";
            this.txtLookUp.Size = new System.Drawing.Size(525, 21);
            this.txtLookUp.TabIndex = 7;
            this.txtLookUp.TextChanged += new System.EventHandler(this.txtLookUp_TextChanged);
            this.txtLookUp.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtLookUp_KeyDown);
            this.txtLookUp.Leave += new System.EventHandler(this.txtLookUp_Leave);
            this.txtLookUp.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLookUp_KeyPress);
            this.txtLookUp.Enter += new System.EventHandler(this.txtLookUp_Enter);
            // 
            // frmLookUp_BaseNew_1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 494);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "frmLookUp_BaseNew_1";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmLookUp_Base";
            this.Load += new System.EventHandler(this.frmLookUp_Base_Load);
            this.Activated += new System.EventHandler(this.frmLookUp_Base_Activated);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmLookUp_Base_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmLookUp_Base_KeyDown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grcLookUp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grvLookUp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        protected System.Windows.Forms.ToolStripStatusLabel tsStatus;
        private System.Windows.Forms.Panel panel1;
        protected GtidTextBox txtLookUp;
        protected DevExpress.XtraGrid.GridControl grcLookUp;
        protected DevExpress.XtraGrid.Views.Grid.GridView grvLookUp;

    }
}