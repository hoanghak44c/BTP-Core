using System;
using System.Collections.Generic;
using System.Text;
using QLBH.Core.Data;
using QLBH.Core.Infors;

namespace QLBH.Core.DAO
{
    internal class DMSanPhamDAO : BaseDAO
    {
        private static DMSanPhamDAO instance;
        private DMSanPhamDAO()
        {
        }
        public static DMSanPhamDAO Instance
        {
            get
            {
                if (instance == null) instance = new DMSanPhamDAO();
                return instance;
            }
        }

        internal DMSanPhamInfo GetSanPhamByIdInfo(int idSanPham)
        {
            return GetObjectCommand<DMSanPhamInfo>(spSanPhamGetById, idSanPham);
        }

        internal List<DMSanPhamInfo> GetListSanPhamByCode(string maSanPhams)
        {
            return GetListCommand<DMSanPhamInfo>(spSanPhamGetByCodes, maSanPhams);
        }

        internal string GetModelById(int idSanPham)
        {
            return
                GetObjectCommand<string>(
                    @"select mo.ten
	                    from tbl_sanpham sp
                     inner join tbl_dm_loaisanpham lsp
		                    on lsp.idloaisp = sp.idcha
                     inner join tbl_dm_dl_model mo
		                    on lsp.model = mo.ma
	                     and mo.nhom = lsp.loai
                     where sp.idsanpham = :idsanpham",
                    idSanPham);
        }
    }   
}
