using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace QLBH.Core.Form
{
    public partial class frmPendingCallOut : DevExpress.XtraEditors.XtraForm
    {
        public frmPendingCallOut()
        {
            InitializeComponent();
            
            btnCancel.Click += btnCancel_Click;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            if(OnCancel != null) OnCancel.Invoke();
        }

        public PendingDelegate OnCancel { get; set; }

        public delegate void PendingDelegate();

        private delegate bool BoolValueDelegate();

        private delegate DialogResult DialogResultValueDelegate();

        private delegate int IntValueDelegate();

        public DialogResult DlgResult
        {
            get { return (DialogResult)Invoke((DialogResultValueDelegate)delegate { return DialogResult; }); }
            set { Invoke((MethodInvoker)delegate { DialogResult = value; }); }
        }

        public new bool Enabled
        {
            get { return (bool) Invoke((BoolValueDelegate) delegate { return btnCancel.Enabled; }); }
            set { Invoke((MethodInvoker) delegate { btnCancel.Enabled = value; }); }
        }

        public int MaxValue
        {
            get { return (int)Invoke((IntValueDelegate)delegate { return pbStatus.Properties.Maximum; }); }
            set { Invoke((MethodInvoker) delegate { pbStatus.Properties.Maximum = value; }); }
        }

        public int MinValue
        {
            get { return (int)Invoke((IntValueDelegate)delegate { return pbStatus.Properties.Minimum; }); }
            set { Invoke((MethodInvoker)delegate { pbStatus.Properties.Minimum = value; }); }
        }

        public int Value
        {
            get { return (int)Invoke((IntValueDelegate)delegate { return Convert.ToInt32(pbStatus.EditValue); }); }
            set { Invoke((MethodInvoker)delegate { pbStatus.EditValue = value; }); }
        }

        public void DoWork(PendingDelegate work)
        {
            var pendingCallOut = new Thread(CastDelegate<ThreadStart>(work));

            pendingCallOut.Start();

            ShowDialog();
        }

        private static T CastDelegate<T>(Delegate source, params object[] para) where T : class
        {
            if (source == null)
                return null;

            Delegate[] delegates = source.GetInvocationList();
            if (delegates.Length == 1)
            {
                if (para == null)
                    return Delegate.CreateDelegate(typeof(T), delegates[0].Target, delegates[0].Method) as T;
                return Delegate.CreateDelegate(typeof(T), delegates[0].Target, delegates[0].Method, false) as T;
            }

            for (int i = 0; i < delegates.Length; i++)
                delegates[i] = Delegate.CreateDelegate(typeof(T), delegates[i].Target, delegates[i].Method);

            return Delegate.Combine(delegates) as T;
        }
    }
}