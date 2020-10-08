using System;
using System.Windows.Forms;
using CrystalDecisions.Shared;
using System.Threading;

namespace QLBH.Core.Form
{
    public interface IToolStripItem
    {
        void RemoveReportStripItem(int key);
        void AddAppBackgroundStatus(int key, string appTitle, EventHandler handleClick);
        void ChangeReportStripItemStatus(int key);
    }

    public partial class frm_rpt : System.Windows.Forms.Form
    {
        object rpt;
        string _title = String.Empty;
        public bool isAsSyns = false;
        Thread runningThread;
        bool completed = false;
        ParameterFields Params=null;
        public frm_rpt()
        {
            InitializeComponent();
        }
        public frm_rpt(object pRpt)
        {
            InitializeComponent();
            rpt = pRpt;
        }
        public frm_rpt(object pRpt,ParameterFields pParams)
        {
            InitializeComponent();
            rpt = pRpt;
            Params = pParams;
        }
        public frm_rpt(object pRpt, ParameterFields pParams, string t)
        {
            InitializeComponent();
            rpt = pRpt;
            Params = pParams;
            this.Text = t;
        }
        public frm_rpt(string t, Thread wt)
        {
            InitializeComponent();
            //Common.LoadStyle(this);
            this.Text = t;
            this.runningThread = wt;
        }

        delegate void SetReportViewerDelegate(object pRpt, ParameterFields pParams);
        private void SetReportViewer(object pRpt, ParameterFields pParams)
        {            
            label1.Visible = false;
            if (pParams != null) crtView.ParameterFieldInfo = pParams;
            crtView.ReportSource = pRpt;
            SendKeys.Send("{ESC}");
            completed = true;
        }

        public void LoadReport(object pRpt, ParameterFields pParams)
        {            
            if (this.InvokeRequired) {
                this.Invoke(new SetReportViewerDelegate(SetReportViewer), new object[] { pRpt, pParams });
            }
            else {
                SetReportViewer(pRpt, pParams);
            }
        }

        private void frm_rpt_Load(object sender, EventArgs e)
        {
            if (!isAsSyns) {
                label1.Visible = false;
                if (Params != null) crtView.ParameterFieldInfo = Params;
                crtView.ReportSource = rpt;
            }
            completed = false;
        }

        public void CreateBackgroundHandle(){
            this.CreateHandle();
        }

        public void onClick(object sender, EventArgs e)
        {
            //if (this != null && !this.IsDisposed) {
            //    this.Show();
            //}
            //((sender as ToolStripItem).OwnerItem.Owner.Parent as IToolStripItem).RemoveReportStripItem(int.Parse((sender as ToolStripItem).Name));
        }

        delegate void CloseFormDelegate();
        public void CloseMe(){
            completed = true;
            if (this.InvokeRequired) {
                this.Invoke(new CloseFormDelegate(Close));
            }
            else {
                this.Close();
            }
        }
        public bool Completed{
            get { return completed; }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (runningThread != null && runningThread.IsAlive) {
                if (!completed) {
                    if (MessageBox.Show(String.Format("{0} đang chạy, bạn có muốn dừng lại không?", Text), "Xác nhận", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3) != DialogResult.Yes) {
                        e.Cancel = true;
                        return;
                    }
                }
                runningThread.Abort();
            }
            base.OnFormClosing(e);
        }

    }
}