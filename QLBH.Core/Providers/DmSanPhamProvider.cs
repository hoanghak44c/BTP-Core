using System;
using System.Collections.Generic;
using System.Text;
using QLBH.Core.DAO;
using QLBH.Core.Infors;

namespace QLBH.Core.Providers
{
    internal class DmSanPhamProvider 
    {
        private static DmSanPhamProvider instance;

        private DmSanPhamProvider()
        {
        }

        public static DmSanPhamProvider Instance
        {
            get
            {
                if (instance == null) instance = new DmSanPhamProvider();
                return instance;
            }
        }

        internal List<DMSanPhamInfo> GetListSanPhamByCode(string oids)
        {
            return DMSanPhamDAO.Instance.GetListSanPhamByCode(oids);
        }

        internal DMSanPhamInfo GetSanPhamById(int idSanPham)
        {
            return DMSanPhamDAO.Instance.GetSanPhamByIdInfo(idSanPham);
        }

        internal string GetModelById(int idSanPham)
        {
            return DMSanPhamDAO.Instance.GetModelById(idSanPham);
        }
    }
}
