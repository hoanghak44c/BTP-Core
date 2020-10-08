using System;
using QLBH.Core.Data;

namespace QLBH.Core.DAO
{
    internal class TblChungTuDAO : BaseDAO
    {
        private static TblChungTuDAO instance;

        private TblChungTuDAO() { }

        internal static TblChungTuDAO Instance
        {
            get
            {
                if (instance == null) instance = new TblChungTuDAO();
                return instance;
            }
        }

        public T GetChungTuBySoChungTu<T>(string soChungTu)
        {
            return GetObjectCommand<T>("sp_ChungTu_GetBySoChungTu", soChungTu);
        }

        public T GetChungTuByIdChungTu<T>(int idChungTu)
        {
            return GetObjectCommand<T>("sp_ChungTu_GetByIdChungTu", idChungTu);
        }

        public bool ChungTuExistByIdChungTu(int idChungTu)
        {
            ExecuteCommand("sp_ChungTu_ExistByIdChungTu", idChungTu);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) > 0;
        }

        public bool ChungTuExistBySoChungTu(string soChungTu)
        {
            ExecuteCommand("sp_ChungTu_ExistBySoChungTu", soChungTu);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) > 0;
        }

        public bool DuplicateSoChungTu(string soChungTu, int idChungTu)
        {
            ExecuteCommand("sp_ChungTu_DuplicateSoChungTu", soChungTu, idChungTu);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) > 0;
        }

        public T GetChungTuRefBySoChungTu<T>(string soChungTu)
        {
            return GetObjectCommand<T>("sp_ChungTu_GetRefBySoChungTu", soChungTu);
        }

        public bool ChiTietHangHoaExists(string soChungTu)
        {
            ExecuteCommand("sp_CTu_GetCTHangHoaBySoChungTu", soChungTu);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) > 0;
        }

        internal void SyncChungTu(int idChungTu)
        {
            ExecuteCommand("sp_ChungTuSync_PushORC", idChungTu);
        }

        public bool CheckSameAccountBookByIdNhanVienAndIdKho(int idNhanVien, int idKho)
        {
            object result = ExecuteScalar(
                @"select decode((select tt.ouid
		                from tbl_dm_nhanvien nv, tbl_dm_trungtam tt
	                 where idnhanvien = :idNhanVien
		                 and nv.idtrungtamhachtoan = tt.idtrungtam),
	                (select tt.ouid
		                 from tbl_dm_kho kho, tbl_dm_trungtam tt
		                where idkho = :idKho
			                and kho.idtrungtam = tt.idtrungtam),
	                1, 0) from dual", idNhanVien, idKho);

            return result == null ? false : Convert.ToInt32(result) == 1;
        }
    }
}