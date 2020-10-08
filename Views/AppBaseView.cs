using System;
using System.Diagnostics;
using System.Windows.Forms;
using QLBH.Core.Controllers;
using QLBH.Core.Exceptions;
using QLBH.Core.Interfaces;

namespace QLBH.Core.Views
{
    public abstract class AppBaseView : DevExpress.XtraEditors.XtraForm
    {
        internal protected BaseController Controller { get; set; }

        public virtual bool Allow
        {
            get
            {
                return Controller.CheckPrivileged();
            }
        }

        public new IBaseViewA MdiParent
        {
            get { return base.MdiParent as IBaseViewA; }

            set { base.MdiParent = value as System.Windows.Forms.Form; }
        }

        public new DialogResult ShowDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;

            return base.ShowDialog();
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            StartPosition = FormStartPosition.CenterScreen;

            return base.ShowDialog(owner);
        }

        public new void Show()
        {
            StartPosition = FormStartPosition.CenterScreen;

            base.Show();
        }

        public new void Show(IWin32Window owner)
        {
            StartPosition = FormStartPosition.CenterScreen;

            base.Show(owner);
        }

        public new bool DesignMode
        {
            get { return (Process.GetCurrentProcess().ProcessName == "devenv"); }
        }

        public static IBaseViewA Instance(Type typeOfView)
        {
            if (AppViewManager.Instance.FindAll(typeOfView).Count > 1)
            {
                throw new ManagedException("Đang có nhiều view loại này, bạn phải xác định rõ cần lấy view nào");
            }

            IBaseViewA instance = AppViewManager.Instance.FindByTypeOfView(typeOfView);

            if (instance == null)
            {
                string defaultName = typeOfView.FullName;

                if (AppViewManager.Instance[defaultName] == null)
                {
                    instance = AppViewManager.Instance.CreateView(defaultName, typeOfView);
                }
                else
                    throw new ManagedException(
                        String.Format(
                            "Tên view '{0}' đã được sử dụng, không thể khởi tạo!",
                            defaultName));
            }

            return instance;
        }

        public static ViewCollection<IBaseViewA> Views(Type typeOfView)
        {
            return AppViewManager.Instance.FindAll(typeOfView);
        }

    }

    /// <summary>
    /// Application view
    /// </summary>
    /// <typeparam name="TController">Controller's type</typeparam>
    /// <typeparam name="TCInterface">Interface controller's type</typeparam>
    /// <typeparam name="TView">View's type</typeparam>
    /// <typeparam name="TVInterface">Interface view's type</typeparam>
    public abstract class AppBaseView<TController, TCInterface, TView, TVInterface> : AppBaseView
        where TVInterface : class
        where TView : class
        where TCInterface : class
    {
        public new TCInterface Controller
        {
            get { return base.Controller as TCInterface; }

            private set { base.Controller = value as BaseController; }
        }

        protected AppBaseView()
        {
            try
            {
                if (!DesignMode)
                {
                    if (!(this is TView) || !(this is TVInterface))
                    {
                        throw new ManagedException("Khai báo View không hợp lệ!");
                    }

                    Controller = (TCInterface)Activator.CreateInstance(typeof(TController), this as TVInterface);

                    Activated += BaseView_Activated;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is ManagedException)

                    throw ex.InnerException;

                throw;
            }
        }

        public static Type TypeOfInstance
        {
            get { return typeof(TView); }
        }

        public new static TView Instance
        {
            get
            {
                if (ViewCount > 1)
                {
                    throw new ManagedException("Đang có nhiều view loại này, bạn phải xác định rõ cần lấy view nào");
                }

                TView instance = AppViewManager.Instance.Find(delegate(TView match) { return true; });

                if (instance == null)
                {
                    string defaultName = typeof(TView).FullName;

                    if (AppViewManager.Instance[defaultName] == null)
                    {
                        instance = (TView)AppViewManager.Instance.CreateView<TView>(defaultName);
                    }
                    else
                        throw new ManagedException(
                            String.Format(
                                "Tên view '{0}' đã được sử dụng, không thể khởi tạo!",
                                defaultName));
                }

                return instance;
            }
        }

        public static int ViewCount
        {
            get
            {
                return AppViewManager.Instance.FindAll(
                    delegate(TView match)
                    {
                        return true;
                    }).Count;
            }
        }

        public static IBaseViewA CreateView(string viewName)
        {
            return AppViewManager.Instance.CreateView<TView>(viewName);
        }

        public static IBaseViewA CreateView(params  object[] parameters)
        {
            return AppViewManager.Instance.CreateView<TView>(typeof(TView).FullName, parameters);
        }

        public static IBaseViewA CreateView(string viewName, params  object[] parameters)
        {
            return AppViewManager.Instance.CreateView<TView>(viewName, parameters);
        }

        public new static ViewCollection<TView> Views
        {
            get
            {
                return AppViewManager.Instance.FindAll(
                    delegate(TView match)
                    {
                        return true;
                    });
            }
        }

        void BaseView_Activated(object sender, EventArgs e)
        {
            //WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.CenterScreen;

            BringToFront();
        }

        protected sealed override void OnLoad(EventArgs e)
        {
            try
            {
                if (!DesignMode && !AppViewManager.Instance.Contains((IBaseViewA)this))
                {
                    throw new ManagedException("View này không được tạo bởi AppViewManager!");
                }

                base.OnLoad(e);
            }
            catch (Exception ex)
            {
                if (ex is ManagedException)
                {
                    if (((ManagedException)ex).CanThrow)

                        throw new ManagedException(ex.Message);
                }
                else
                {
                    throw new ManagedException(ex.Message);
                }

                ShowMessage(ex.Message);

                Close();
            }
        }

        public string ViewName { get; set; }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public DialogResult ShowConfirmMessage(string message)
        {
            return MessageBox.Show(message, "Xác nhận",
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Question,
                                   MessageBoxDefaultButton.Button2);
        }

        public DialogResult ShowConfirmMessage(IWin32Window owner, string message)
        {
            return MessageBox.Show(owner, message, "Xác nhận",
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Question,
                                   MessageBoxDefaultButton.Button2);
        }

        protected virtual void OnClosed() { }

        protected sealed override void OnClosed(EventArgs e)
        {
            if (AppViewManager.Instance.Contains((IBaseViewA)this))
                AppViewManager.Instance.Remove((IBaseViewA)this);

            OnClosed();
        }

        public new bool IsHandleCreated
        {
            get
            {
                if (AppViewManager.Instance[ViewName] != null) return base.IsHandleCreated;

                return false;
            }
        }

        private string oldErrMsg = String.Empty;

        public void SafeMode(UnitWorkDelegate methodInvoker)
        {
            try
            {
                if (InvokeRequired)

                    Invoke(methodInvoker);

                else

                    methodInvoker();
            }
            catch (Exception ex)
            {
                if (ex is ManagedException)
                {
                    if (oldErrMsg != ex.Message)
                    {
                        oldErrMsg = ex.Message;

                        throw new ManagedException(ex.Message);
                    }

                    ShowMessage(ex.Message);

                    Environment.Exit(123);
                }

                oldErrMsg = ex.Message;

                throw new ManagedException(ex.Message);
            }
        }

        public void SafeMode<T>(ParameterizedUnitWorkDelegate<T> methodInvoker, T param)
        {
            try
            {
                if (InvokeRequired)

                    Invoke(methodInvoker, param);

                else

                    methodInvoker(param);
            }
            catch (Exception ex)
            {
                if (ex is ManagedException)
                {
                    if (oldErrMsg != ex.Message)
                    {
                        oldErrMsg = ex.Message;

                        throw new ManagedException(ex.Message);
                    }

                    ShowMessage(ex.Message);

                    Environment.Exit(123);
                }

                oldErrMsg = ex.Message;

                throw new ManagedException(ex.Message);
            }
        }

        public void SafeMode(ParameterizedUnitWorkDelegate methodInvoker, IWin32Window owner)
        {
            try
            {
                if (InvokeRequired)

                    Invoke(methodInvoker, owner);

                else

                    methodInvoker(owner);
            }
            catch (Exception ex)
            {
                if (ex is ManagedException)
                {
                    if (oldErrMsg != ex.Message)
                    {
                        oldErrMsg = ex.Message;

                        throw new ManagedException(ex.Message);
                    }

                    ShowMessage(ex.Message);

                    Environment.Exit(123);
                }

                oldErrMsg = ex.Message;

                throw new ManagedException(ex.Message);
            }
        }

        public object SafeMode(ParameterizedUnitWorkDelegate methodInvoker, params object[] args)
        {
            try
            {
                if (InvokeRequired)

                    return Invoke(methodInvoker, args);

                return methodInvoker(args);
            }
            catch (Exception ex)
            {
                if (ex is ManagedException)
                {
                    if (oldErrMsg != ex.Message)
                    {
                        oldErrMsg = ex.Message;

                        throw new ManagedException(ex.Message);
                    }

                    ShowMessage(ex.Message);

                    Environment.Exit(123);
                }

                oldErrMsg = ex.Message;

                throw new ManagedException(ex.Message);
            }
        }
    }
}