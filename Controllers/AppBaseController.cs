using System;
using QLBH.Core.Exceptions;
using QLBH.Core.Interfaces;

namespace QLBH.Core.Controllers
{
    public abstract class AppBaseController<TVInterface> : BaseController<TVInterface, IBaseModel> where TVInterface : class
    {
        protected AppBaseController(TVInterface view)
            : base(view)
        {
            if (this.view != null && this.view is System.Windows.Forms.Form)

                (this.view as System.Windows.Forms.Form).Load += DisplayViewInfo;
        }

        private void DisplayViewInfo(object sender, EventArgs e)
        {
            try
            {
                CheckPrivilegedAndExecute();
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

                ((IBaseViewA)View).ShowMessage(ex.Message);

                ((IBaseViewA)View).Close();
            }
        }
    }

    public abstract class AppBaseController<TVInterface, TMInterface> : BaseController<TVInterface, TMInterface>
        where TVInterface : class
        where TMInterface : class
    {
        protected AppBaseController(TVInterface view)
            : base(view)
        {
            if (this.view != null && this.view is System.Windows.Forms.Form) 
                
                (this.view as System.Windows.Forms.Form).Load += DisplayViewInfo;
        }

        private void DisplayViewInfo(object sender, EventArgs e)
        {
            try
            {
                CheckPrivilegedAndExecute();
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

                ((IBaseViewA)View).ShowMessage(ex.Message);

                ((IBaseViewA)View).Close();
            }
        }
    }

    public abstract class AppBaseTrustedController<TVInterface> : AppBaseController<TVInterface> where TVInterface : class
    {
        protected AppBaseTrustedController(TVInterface view)
            : base(view)
        {
        }

        protected override IBaseModel Model
        {
            get { return new TrustedModel(); }
        }

        protected override bool CheckPrivilegedOfController()
        {
            return true;
        }

        protected internal class TrustedModel : IBaseModel
        {
            #region Implementation of IBaseModel

            public void RegisterSidCode(int sid, string scode)
            {
                //trusted controller doesn't need register to system.
            }

            public bool CheckPrivileged(string scode)
            {
                return true;
            }

            public int GetSidNumber(string scode)
            {
                return 0;
            }

            #endregion
        }
    }
}