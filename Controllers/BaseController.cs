using System;
using System.Reflection;
using QLBH.Core.Exceptions;
using QLBH.Core.Interfaces;

namespace QLBH.Core.Controllers
{
    public abstract class BaseController : Parallel
    {
        protected readonly IBaseViewA view;

        protected BaseController(IBaseViewA view)
        {
            this.view = view;
        }

        internal virtual bool CheckPrivileged()
        {
            return false;
        }

        protected internal virtual void CheckPrivilegedAndExecute()
        {
            throw new ManagedException("Bạn chưa được phân quyền để thực hiện chức năng này!");
        }

        protected abstract void DisplayViewInfo();

        internal protected IBaseViewA View
        {
            get { return view; }
        }

        private static void Enabled(object control, object value)
        {
            control.GetType().InvokeMember("Enabled",
                BindingFlags.SetProperty, null, control, new[] { value });
        }

        private static void Click(object control, Delegate callerDelegate, params object[] parameters)
        {
            control.GetType().GetEvent("Click")
                .AddEventHandler(
                    control,
                    new EventHandler(delegate { callerDelegate.DynamicInvoke(parameters); }));
        }

        protected void Allow(object control, bool allow, Delegate displayBusinessView, params object[] parameters)
        {
            Enabled(control, allow);

            if (allow)

                Click(control, displayBusinessView, parameters);
        }
    }

    //public abstract class BaseController<TVInterface> where TVInterface : class
    //{
    //    internal readonly TVInterface view;

    //    protected TVInterface View
    //    {
    //        get
    //        {
    //            if(!AppViewManager.Instance.Contains((IBaseViewA)view))
    //                AppViewManager.Instance.Add((IBaseViewA)view);

    //            return view;
    //        }
    //    }

    //    protected BaseController(TVInterface view)
    //    {
    //        this.view = view;
    //    }

    //    protected abstract void DisplayViewInfo();

    //    protected void Delay(int miliseconds)
    //    {
    //        Thread.CurrentThread.Join(miliseconds);
    //    }

    //    private static Thread CreateThread(Delegate job)
    //    {
    //        return new Thread(Delegate.CreateDelegate(typeof (ThreadStart), job.Target, job.Method) as ThreadStart);
    //    }

    //    protected void DoASyncWork(UnitWorkDelegate job)
    //    {
    //        var asSyncThread = CreateThread(job);

    //        asSyncThread.Start();
    //    }

    //    protected void DoSyncWork(UnitWorkDelegate job)
    //    {
    //        var asSyncThread = CreateThread(job);

    //        asSyncThread.Start();

    //        asSyncThread.Join();
    //    }
    //}
}