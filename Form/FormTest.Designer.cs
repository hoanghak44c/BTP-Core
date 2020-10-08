namespace QLBH.Core.Form
{
    partial class FormTest
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
            this.ButtonShowPopup1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonShowPopup1
            // 
            this.ButtonShowPopup1.Location = new System.Drawing.Point(102, 125);
            this.ButtonShowPopup1.Name = "ButtonShowPopup1";
            this.ButtonShowPopup1.Size = new System.Drawing.Size(88, 23);
            this.ButtonShowPopup1.TabIndex = 1;
            this.ButtonShowPopup1.Text = "Show popup 1";
            this.ButtonShowPopup1.Click += new System.EventHandler(this.ButtonShowPopup1_Click);
            // 
            // FormTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.ButtonShowPopup1);
            this.Name = "FormTest";
            this.Text = "FormTest";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonShowPopup1;
    }
}