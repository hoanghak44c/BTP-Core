using System;
using QLBH.Core.DAO;
using QLBH.Core.Data;
using QLBH.Core.Form;
using QLBH.Core.Infors;

namespace QLBH.Core.Business
{
    public abstract class ChungTuBusinessBase
    {
        protected ChungTuBaseInfo ChungTuInfo;
        private BusinessType businessType;
 
        //private int trangThaiOrigin;

        //protected internal int TrangThaiOrigin { get { return trangThaiOrigin; } }
        /// <summary>
        /// Read only to determine IN or OUT business
        /// </summary>
        public BusinessType BusinessType
        {
            get { return businessType; }
            protected set { businessType = value; }
        }

        private bool isOwnTran;

        public void SaveChungTu()
        {
            try
            {
                isOwnTran = false;
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    ConnectionUtil.Instance.BeginTransaction();
                    isOwnTran = true;
                }
                SaveChungTuInstance();
                if (ConnectionUtil.Instance.IsInTransaction && isOwnTran)
                    ConnectionUtil.Instance.CommitTransaction();
            }
            catch (Exception)
            {
                if (ConnectionUtil.Instance.IsInTransaction && isOwnTran)
                    ConnectionUtil.Instance.RollbackTransaction();
                throw;
            }
        }

        public void DeleteChungTu()
        {
            try
            {
                isOwnTran = false;
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    ConnectionUtil.Instance.BeginTransaction();
                    isOwnTran = true;
                }
                Prepare();
                DeleteChungTuInstance();
                if (ConnectionUtil.Instance.IsInTransaction && isOwnTran)
                    ConnectionUtil.Instance.CommitTransaction();
            }
            catch (Exception)
            {
                if (ConnectionUtil.Instance.IsInTransaction && isOwnTran)
                    ConnectionUtil.Instance.RollbackTransaction();
                throw;
            }            
        }

        //private const int STATE_NULL = 0xAEAF;
        //public int CancelState = STATE_NULL;

        public void CancelChungTu()
        {
            try
            {
                //if (CancelState == STATE_NULL)
                //    throw new Exception("CancelState has not been set.");

                //if (trangThaiOrigin == CancelState)
                //    throw new Exception("Chứng từ này đã được hủy.");

                isOwnTran = false;
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    ConnectionUtil.Instance.BeginTransaction();
                    isOwnTran = true;
                }

                Prepare();
                CancelChungTuInstance();
                if (ConnectionUtil.Instance.IsInTransaction && isOwnTran)
                    ConnectionUtil.Instance.CommitTransaction();
            }
            catch (Exception)
            {
                if (ConnectionUtil.Instance.IsInTransaction && isOwnTran)
                    ConnectionUtil.Instance.RollbackTransaction();
                throw;
            }
        }


        protected internal abstract void Prepare();
        protected internal abstract void SaveChungTuInstance();
        protected internal abstract void DeleteChungTuInstance();
        protected internal abstract void CancelChungTuInstance();

        public virtual ChungTuBusinessBase Clone()
        {
            ChungTuBusinessBase tmpBusiness = null;

            try
            {
                tmpBusiness = (ChungTuBusinessBase)Activator.CreateInstance(GetType(), null);
            }
            catch (MissingMethodException)
            {
                tmpBusiness = (ChungTuBusinessBase)Activator.CreateInstance(GetType(), ChungTuInfo.Clone());
            }

            tmpBusiness.ChungTuInfo = (ChungTuBaseInfo) ChungTuInfo.Clone();

            return tmpBusiness;
        }
    }

    public enum BusinessType
    {
        VIRTUAL_IN,
        VIRTUAL_OUT,
        REAL_IN,
        REAL_OUT
    }
}
