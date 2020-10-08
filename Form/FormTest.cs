using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QLBH.Core.Properties;
using QLBH.Core.UserControls;

namespace QLBH.Core.Form
{
    public partial class FormTest : System.Windows.Forms.Form
    {
        TaskbarNotifier TaskbarNotifier;

        public FormTest()
        {
            InitializeComponent();
            TaskbarNotifier = new TaskbarNotifier();
            TaskbarNotifier.SetBackgroundBitmap(Resources.skin2, Color.FromArgb(255, 0, 255));
            TaskbarNotifier.SetCloseBitmap(Resources.close2, Color.FromArgb(255, 0, 255), new Point(300, 74));
            TaskbarNotifier.TitleRectangle = new Rectangle(123, 80, 176, 16);
            TaskbarNotifier.ContentRectangle = new Rectangle(116, 97, 197, 22);
            TaskbarNotifier.TitleText = "haha title";
            TaskbarNotifier.ContentText = "hihi content";
        }

        private void ButtonShowPopup1_Click(object sender, EventArgs e)
        {
            TaskbarNotifier.Show(TaskbarNotifier.TitleText, TaskbarNotifier.ContentText, 500, 3000, 500);
        }
    }
}
