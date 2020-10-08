using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;
using ThreadState = System.Threading.ThreadState;

namespace QLBH.Core.Form
{
    public partial class frmProgress : DevExpress.XtraEditors.XtraForm
    {
        private delegate int GetIntValueDelegate();
        private delegate void SetIntValueDelegate(int value);
        private delegate string GetStringValueDelegate();
        private delegate void SetStringValueDelegate(string value);
        private delegate void VoidDelegate();

        public delegate void ProgressStart();
        public delegate void ParameterizedProgressStart(object parameter);

        private static frmProgress frmInstance;
        private bool isCompleted;
        private bool isHandleCreated;
        
        private struct ProgressStatus
        {
            public string Description;
            public string Caption;
            public int MaxValue;
            public int Value;
            public bool IsCompleted;
        }

        private List<ProgressStatus> statusStack;
        
        //private frmProgress(bool isVisible)
        //{
        //    InitializeComponent();
        //    this.Load += frmProgress_Load;
        //    this.HandleCreated += frmProgress_HandleCreated;
        //    this.HandleDestroyed += frmProgress_HandleDestroyed;
        //    Name = "Progress" + statusStack.Count;
        //    //Text = frmInstance.Text;
        //    this.StartPosition = FormStartPosition.Manual;
        //    this.Top = -1000;
        //    this.Left = -1000;

        //}

        public frmProgress()
        {
            //InitializeComponent();
            this.HandleCreated += frmProgress_HandleCreated;
            this.HandleDestroyed += frmProgress_HandleDestroyed;
            statusStack = new List<ProgressStatus>();
            this.FormClosing += frmProgress_FormClosing;
            this.Load += frmProgress_Load;
            //isCompleted = false;
            //UseWaitCursor = true;
            //pbStatus.Properties.Step = 1;
            //pbStatus.Properties.PercentView = true;
            //pbStatus.Properties.ShowTitle = true;

            //frmInstance.HandleCreated += frmProgress_HandleCreated;
            //frmInstance.HandleDestroyed += frmProgress_HandleDestroyed;
            //frmInstance.statusStack = new List<ProgressStatus>();
            //frmInstance.FormClosing += frmProgress_FormClosing;
            //frmInstance.Load += frmProgress_Load;

        }

        void frmProgress_HandleDestroyed(object sender, EventArgs e)
        {
            isHandleCreated = false;
        }

        void frmProgress_HandleCreated(object sender, EventArgs e)
        {
            isHandleCreated = true;
        }

        public new bool IsHandleCreated
        {
            get { return isHandleCreated; }
        }

        public static frmProgress Instance
        {
            get
            {
                if (frmInstance == null || frmInstance.IsDisposed)
                {
                    frmInstance = new frmProgress();
                    frmInstance.InitializeComponent();
                    frmInstance.isCompleted = false;
                    frmInstance.UseWaitCursor = true;
                    frmInstance.pbStatus.Properties.Step = 1;
                    frmInstance.pbStatus.Properties.PercentView = true;
                    frmInstance.pbStatus.Properties.ShowTitle = true;
                }

                return frmInstance;
            }
        }

        public void PushStatus()
        {
            statusStack.Add(new ProgressStatus
                                {
                                    Caption = Caption,
                                    Description = Description,
                                    MaxValue = MaxValue,
                                    Value = Value,
                                    IsCompleted = IsCompleted,
                                });
        }

        public void PopStatus()
        {
            if (statusStack.Count == 0) return;
            ProgressStatus progressStatus = statusStack[statusStack.Count - 1];
            Caption = progressStatus.Caption;
            Description = progressStatus.Description;
            MaxValue = progressStatus.MaxValue;
            Value = progressStatus.Value;
            IsCompleted = progressStatus.IsCompleted;
            statusStack.RemoveAt(statusStack.Count - 1);
        }

        public bool IsCompleted 
        { 
            get { return isCompleted; }
            set 
            {
                isCompleted = value;
            }
        }

        public bool IsRetryingConnect { get; set; }

        private int getMaxValue()
        {
            return Instance.pbStatus.Properties.Maximum;
        }

        private void setMaxValue(int value)
        {
            Instance.pbStatus.Properties.Maximum = value;
            //pbStatus.Refresh();
        }

        public int MaxValue
        {
            get
            {
                if (InvokeRequired)
                {
                    var getMaxValueDelegate = new GetIntValueDelegate(getMaxValue);
                    return (int)Invoke(getMaxValueDelegate);
                }
                return Instance.pbStatus.Properties.Maximum;
            }
            set
            {
                if (InvokeRequired)
                {
                    var setMaxValueDelegate = new SetIntValueDelegate(setMaxValue);
                    Invoke(setMaxValueDelegate, value <= 0 ? 1 : value);
                    return;
                }
                Instance.pbStatus.Properties.Maximum = value <= 0 ? 1 : value;
                //pbStatus.Refresh();
            }
        }

        private int getMinValue()
        {
            return Instance.pbStatus.Properties.Minimum;
        }
        
        private void setMinValue(int value)
        {
            Instance.pbStatus.Properties.Minimum = value;
            //pbStatus.Refresh();
        }

        public int MinValue
        {
            get
            {
                if (InvokeRequired)
                {
                    var getMinValueDelegate = new GetIntValueDelegate(getMinValue);
                    return (int)Invoke(getMinValueDelegate);
                }
                return Instance.pbStatus.Properties.Minimum;
            }
            set
            {
                if (InvokeRequired)
                {
                    var setMinValueDelegate = new SetIntValueDelegate(setMinValue);
                    Invoke(setMinValueDelegate, value);
                    return;
                }
                Instance.pbStatus.Properties.Minimum = value;
                //pbStatus.Refresh();
            }
        }

        private int getValue()
        {
            if(Instance.pbStatus == null) Instance.InitializeComponent();

            return Convert.ToInt32(Instance.pbStatus.EditValue);
        }

        private void setValue(int value)
        {
            Instance.pbStatus.EditValue = value;
            //pbStatus.Refresh();
        }

        public int Value
        {
            get
            {
                if (InvokeRequired)
                {
                    var getValueDelegate = new GetIntValueDelegate(getValue);
                    return (int)Invoke(getValueDelegate);
                }

                return Convert.ToInt32(Instance.pbStatus.EditValue);
            }
            set
            {
                if (InvokeRequired)
                {
                    var setValueDelegate = new SetIntValueDelegate(setValue);
                    Invoke(setValueDelegate, value);
                    return;
                }
                Instance.pbStatus.EditValue = value;
                //pbStatus.Refresh();
            }
        }

        private string getCaption()
        {
            return Instance.Text;
        }

        private void setCaption(string value)
        {
            Instance.Text = value;
            //Refresh();
        }

        public string Caption
        {
            get
            {
                if (InvokeRequired)
                {
                    var getCaptionDelegate = new GetStringValueDelegate(getCaption);
                    return (string)Invoke(getCaptionDelegate);
                }

                return Instance.Text;
            }
            set
            {
                if (InvokeRequired)
                {
                    var setCaptionDelegate = new SetStringValueDelegate(setCaption);
                    Invoke(setCaptionDelegate, value);
                    return;
                }
                Instance.Text = value;
                //Refresh();
            }
        }

        private string getDescription()
        {
            return Instance.lblStatus.Text;
        }

        private void setDescription(string value)
        {
            Instance.lblStatus.Text = value;
            //lblStatus.Refresh();
        }

        public string Description
        {
            get
            {
                if (InvokeRequired)
                {
                    var getDescriptionDelegate = new GetStringValueDelegate(getDescription);
                    return (string)Invoke(getDescriptionDelegate);
                }

                return Instance.lblStatus.Text;
            }
            set
            {
                if (InvokeRequired)
                {
                    var setDescriptionDelegate = new SetStringValueDelegate(setDescription);
                    Invoke(setDescriptionDelegate, value);
                    return;
                }
                Instance.lblStatus.Text = value;
                //lblStatus.Refresh();
            }
        }

        private void closeSelf()
        {
            Close();

            //Dispose(true);

            //frmInstance = null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool checkPending(ProgressStart progressStart)
        {
            if (progressStart.Method.Name == "RetryConnect")
            {
                if (!IsRetryingConnect)
                {
                    IsRetryingConnect = true;
                }
                else
                {
                    while (IsRetryingConnect)
                    {
                        Thread.CurrentThread.Join(1000);
                    }

                    return true;
                }
            }

            return false;
        }

        public void DoWork(ProgressStart progressStart)
        {
            if (ConnectionUtil.Instance.IsTimeOutApp) return;

            if (!isHandleCreated)
            {
                this.Value = 0;

                IsCompleted = false;

                var progressWorker = new Thread(CastDelegate<ThreadStart>(progressStart));

                //Debug.Print("{0} Thread Id: {1} create new thread {2} to async execute {3}...", DateTime.Now, Thread.CurrentThread.ManagedThreadId, progressWorker.ManagedThreadId, progressStart.Method.Name);

                progressWorker.Start();

                if (Environment.UserInteractive && !Visible)
                {
                    ShowDialog();
                }
                else
                {
                    progressWorker.Join();
                    
                    Thread.CurrentThread.Join(3000);
                }
            }
            else
            {
                if(checkPending(progressStart)) return;

                PushStatus();

                Value = 0;

                IsCompleted = false;

                var progressWorker = new Thread(CastDelegate<ThreadStart>(progressStart));

                //Debug.Print("{0} Thread Id: {1} create new thread {2} to async execute {3}...", DateTime.Now, Thread.CurrentThread.ManagedThreadId, progressWorker.ManagedThreadId, progressStart.Method.Name);
                
                progressWorker.Start();
                
                progressWorker.Join();
                
                Thread.CurrentThread.Join(3000);
            }
        }

        public void DoWork(IWin32Window owner, ProgressStart progressStart)
        {
            if (ConnectionUtil.Instance.IsTimeOutApp) return;
            this.Value = 0;
            IsCompleted = false;
            var progressWorker = new Thread(CastDelegate<ThreadStart>(progressStart));
            progressWorker.Start();
            if (Environment.UserInteractive)
                this.ShowDialog(owner);
            else
            {
                progressWorker.Join();
                Thread.CurrentThread.Join(2500);
            }
        }

        public void DoWork(ParameterizedProgressStart parameterizedProgressStart, object parameter)
        {
            if (ConnectionUtil.Instance.IsTimeOutApp) return;
            this.Value = 0;
            IsCompleted = false;
            var progressWorker = new Thread(CastDelegate<ParameterizedThreadStart>(parameterizedProgressStart, parameter));
            //var stopWorker = new Thread(CloseWork);
            progressWorker.Start(parameter);
            //stopWorker.Start();
            if (!isHandleCreated) 
            {
                if (Environment.UserInteractive)
                    ShowDialog();
                else
                {
                    progressWorker.Join();
                    Thread.CurrentThread.Join(2500);
                }
            }
        }

        public void DoWork(IWin32Window owner, ParameterizedProgressStart parameterizedProgressStart, object parameter)
        {
            if (ConnectionUtil.Instance.IsTimeOutApp) return;
            this.Value = 0;
            IsCompleted = false;
            var progressWorker = new Thread(CastDelegate<ParameterizedThreadStart>(parameterizedProgressStart, parameter));
            progressWorker.Start(parameter);
            if (!isHandleCreated)
            {
                //var stopWorker = new Thread(CloseWork);
                //stopWorker.Start();

                if (Environment.UserInteractive)
                    this.ShowDialog(owner);
                else
                {
                    progressWorker.Join();
                    Thread.CurrentThread.Join(2500);
                }
            }
        }

        private void CloseWork()
        {
            while (isHandleCreated)
            {
                Thread.Sleep(2500);

                UseWaitCursor = IsCompleted != true;

                if (isCompleted && MaxValue > 0 && Value == MaxValue)
                {
                    if (statusStack.Count == 0)
                    {
                        if (InvokeRequired)
                        {
                            var closeDelegate = new VoidDelegate(closeSelf);

                            Invoke(closeDelegate);
                        }
                        else
                        {
                            Close();

                            //Dispose(true);

                            //frmInstance = null;
                        }

                        isHandleCreated = false;

                        return;
                    }

                    PopStatus();
                }
            }
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

        void frmProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (((e.CloseReason == CloseReason.UserClosing) || 
                (e.CloseReason == CloseReason.TaskManagerClosing)) &&
                (Convert.ToInt32(pbStatus.EditValue) < pbStatus.Properties.Maximum ||
                !IsCompleted || (statusStack != null && statusStack.Count > 0)))
            {
                e.Cancel = true;
                return;
            }
            
            if (statusStack != null) statusStack.Clear();

            IsCompleted = false;
        }

        private void frmProgress_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Caption)) Caption = "Quản lý bán hàng";

            if (String.IsNullOrEmpty(Description)) Description = "Đang xử lý dữ liệu...";

            var stopWorker = new Thread(CloseWork);
            
            stopWorker.Start();
        }
    }
}
