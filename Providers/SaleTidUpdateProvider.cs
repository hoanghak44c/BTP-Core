using System;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;

namespace QLBH.Core.Providers
{
    public interface IUpVer
    {
        double GetVersion();

        string GetPath();

        void UpVer(string version);

        DateTime GetSysDate();
    }

    internal abstract class UpVerBaseProvider : IUpVer
    {
        internal protected IUpVer Dao;

        internal protected UpVerBaseProvider()
        {
            CreateDao();
        }

        internal protected virtual void CreateDao()
        {
            throw new ManagedException("Chưa thực hiện hàm CreateDao", false);
        }

        #region Implementation of IUpVer

        public double GetVersion()
        {
            return Dao.GetVersion();
        }

        public string GetPath()
        {
            return Dao.GetPath();
        }

        public void UpVer(string version)
        {
            Dao.UpVer(version); 
        }

        public DateTime GetSysDate()
        {
            return Dao.GetSysDate();
        }

        #endregion
    }

    internal class SaleTidUpVerProvider : UpVerBaseProvider
    {

        private static IUpVer instance;

        private SaleTidUpVerProvider() { }

        public static IUpVer Instance
        {
            get { return instance ?? (instance = new SaleTidUpVerProvider()); }
        }

        #region Overrides of UpdateProviderBase

        protected internal override void CreateDao()
        {
            Dao = SaleTidUpdateDao.Instance;
        }
        #endregion
    }

    internal class CrmTidUpVerProvider : UpVerBaseProvider
    {

        private static IUpVer instance;

        private CrmTidUpVerProvider() { }

        public static IUpVer Instance
        {
            get { return instance ?? (instance = new CrmTidUpVerProvider()); }
        }

        #region Overrides of UpdateProviderBase

        protected internal override void CreateDao()
        {
            Dao = CrmTidUpdateDao.Instance;
        }
        #endregion
    }

    internal abstract class UpVerBaseDao : BaseDAO, IUpVer
    {
        internal protected abstract string ProductType { get; }

        internal protected UpVerBaseDao() {}

        #region Implementation of IUpVer

        public double GetVersion()
        {
            return GetObjectCommand<double>("select version from tbl_thamso_chung where type = :pType", ProductType);
        }

        public string GetPath()
        {
            return GetObjectCommand<string>("select path from tbl_thamso_chung where type = :pType", ProductType);
        }

        public void UpVer(string version)
        {
            ExecuteCommand("update tbl_thamso_chung set version = :pVersion where type = :pType", version, ProductType);
        }

        public DateTime GetSysDate()
        {
            return GetObjectCommand<DateTime>("select sysdate from dual");
        }

        #endregion
    }

    internal class SaleTidUpdateDao : UpVerBaseDao
    {

        private static IUpVer instance;

        private SaleTidUpdateDao(){}

        public static IUpVer Instance
        {
            get { return instance ?? (instance = new SaleTidUpdateDao()); }
        }

        #region Implementation of IUpVer

        protected internal override string ProductType
        {
            get { return "PRODUCT"; }
        }

        #endregion
    }

    internal class CrmTidUpdateDao : UpVerBaseDao
    {

        private static IUpVer instance;

        private CrmTidUpdateDao(){}

        public static IUpVer Instance
        {
            get { return instance ?? (instance = new CrmTidUpdateDao()); }
        }

        #region Overrides of UpVerBaseDao

        protected internal override string ProductType
        {
            get { return "DEV"; }
        }

        #endregion
    }

}