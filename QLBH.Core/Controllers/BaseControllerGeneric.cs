using System;
using QLBH.Core.Exceptions;
using QLBH.Core.Interfaces;
using QLBH.Core.Views;

namespace QLBH.Core.Controllers
{
    public abstract class BaseController<TVInterface, TMInterface> : BaseController
        where TVInterface : class
        where TMInterface : class
    {
        protected BaseController(TVInterface view) : base(view as IBaseViewA) { }

        protected new TVInterface View
        {
            get
            {
                if (!AppViewManager.Instance.Contains(view))

                    return null;

                return view as TVInterface;
            }
        }

        protected int Sid
        {
            get
            {
                if (Model != null && Model is IBaseModel)
                {
                    int sid = ((IBaseModel)Model).GetSidNumber(Scode);

                    if (sid != 0) return sid;
                }

                return View.GetHashCode();
            }
        }

        protected string Scode
        {
            get { return View.GetType().FullName; }
        }

        protected virtual TMInterface Model
        {
            get { return null; }
        }

        protected virtual bool CheckPrivilegedOfController()
        {
            return ((IBaseModel)Model).CheckPrivileged(Scode); ;
        }

        internal override sealed bool CheckPrivileged()
        {
            if (Model != null && Model is IBaseModel)
            {
                if (!CheckPrivilegedOfController())
                {
                    ((IBaseModel)Model).RegisterSidCode(Sid, Scode);

                    return false;
                }

                return true;
            }

            return false;
        }

        internal protected override sealed void CheckPrivilegedAndExecute()
        {
            if (((IBaseViewA)View).Controls.Count > 0)

                throw new ManagedException(
                    String.Format("{0} : View này đã không được khởi tạo hợp lệ.",
                    Scode));

            if (View is AppBaseView && !(View as AppBaseView).DesignMode)
            {
                if (!CheckPrivileged())
                {
                    throw new ManagedException(String.Format(
                        "{0} - {1}: Bạn chưa được phân quyền để thực hiện chức năng này!",
                        Sid, Scode));
                }

                ((IBaseViewA)View).Initialize();

                DisplayViewInfo();
            }
        }
    }
}