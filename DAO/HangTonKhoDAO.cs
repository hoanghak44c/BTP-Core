using System;
using QLBH.Core.Data;
using QLBH.Core.Infors;

namespace QLBH.Core.DAO
{
    public class HangTonKhoDAO : BaseDAO
    {
        private static HangTonKhoDAO instance;
        private HangTonKhoDAO()
        {
            CRUDTableName = TBL_HANG_TON_KHO;
            UseCaching = true;
        }
        
        public static HangTonKhoDAO Instance
        {
            get { return instance ?? (instance = new HangTonKhoDAO()); }
        }

        internal void Insert(HangTonKhoInfo info)
        {
            ExecuteCommand(spHangTonKhoInsert2, info.IdKho, info.IdSanPham, info.SoLuong, info.GhiChu, info.TonAo, info.IdTrungTam);
            info.IdTonKho = Convert.ToInt32(Parameters["p_IdTonKho"].Value.ToString());

        }

        internal int Update(HangTonKhoInfo info, int idChungTu)
        {
            int resultAffected = ExecuteCommand(
                    @"UPDATE tbl_hangtonkho t1
                    SET t1.idkho = :pIdKho,
                        t1.idsanpham = :pIdSanPham,
                        t1.soluong = t1.soluong + :pDeltaSoLuong,
                        t1.ghichu = :pGhiChu,
                        t1.tonao = t1.tonao + :pDeltaTonAo,
                        t1.idtrungtam = :pIdTrungTam
                    WHERE t1.idtonkho = :pIdTonKho
                        and t1.last_update_date = :pLastUpdateDate",
                    info.IdKho, info.IdSanPham, info.DeltaSoLuong, info.GhiChu,
                    info.DeltaTonAo, info.IdTrungTam, info.IdTonKho, info.LastUpdateDate);

//            if (resultAffected != 0)
//            {
//                ExecuteCommand(
//                   @"INSERT INTO tbl_hangtonkho_chk
//	                    (idkho,
//	                     idsanpham,
//	                     soluong,
//	                     dlt_soluong,
//	                     tonao,
//	                     dlt_tonao,
//	                     last_update_date,
//	                     idtrungtam,
//                         idchungtu,
//                         created_date)
//                    VALUES
//	                    (:idKho,
//	                     :idSanPham,
//	                     :soluong,
//	                     :dlt_soluong,
//	                     :tonao,
//	                     :dlt_tonao,
//	                     :last_update_date,
//	                     :idtrungtam,
//                         :idchungtu,
//                         localtimestamp)",
//                   info.IdKho, info.IdSanPham, info.SoLuong, info.DeltaSoLuong, info.TonAo,
//                   info.DeltaTonAo, info.LastUpdateDate, info.IdTrungTam, idChungTu);
//            }

            return resultAffected;
        }

        internal int Update(HangTonKhoInfo info)
        {
            int resultAffected = ExecuteCommand(
                    @"UPDATE tbl_hangtonkho t1
                    SET t1.idkho = :pIdKho,
                        t1.idsanpham = :pIdSanPham,
                        t1.soluong = t1.soluong + :pDeltaSoLuong,
                        t1.ghichu = :pGhiChu,
                        t1.tonao = t1.tonao + :pDeltaTonAo,
                        t1.idtrungtam = :pIdTrungTam
                    WHERE t1.idtonkho = :pIdTonKho
                        and t1.last_update_date = :pLastUpdateDate",
                    info.IdKho, info.IdSanPham, info.DeltaSoLuong, info.GhiChu, 
                    info.DeltaTonAo, info.IdTrungTam, info.IdTonKho, info.LastUpdateDate);

//            if (resultAffected != 0)
//            {
//                ExecuteCommand(
//                   @"INSERT INTO tbl_hangtonkho_chk
//	                    (idkho,
//	                     idsanpham,
//	                     soluong,
//	                     dlt_soluong,
//	                     tonao,
//	                     dlt_tonao,
//	                     last_update_date,
//	                     idtrungtam)
//                    VALUES
//	                    (:idKho,
//	                     :idSanPham,
//	                     :soluong,
//	                     :dlt_soluong,
//	                     :tonao,
//	                     :dlt_tonao,
//	                     :last_update_date,
//	                     :idtrungtam)",
//                   info.IdKho, info.IdSanPham, info.SoLuong, info.DeltaSoLuong, info.TonAo,
//                   info.DeltaTonAo, info.LastUpdateDate, info.IdTrungTam);                
//            }

            return resultAffected;
        }

        public HangTonKhoInfo GetHangTonKhoById(int idKho, int idSanPham, int idTrungTam)
        {
            return
                GetObjectCommand<HangTonKhoInfo>(
                    @"SELECT t1.idtonkho,
						 t1.idkho,
						 t1.idsanpham,
						 t1.soluong,
						 t1.ghichu,
						 t1.tonao,
						 t1.idtrungtam,
						 t1.last_update_date lastupdatedate
			          FROM tbl_hangtonkho t1
		                 WHERE t1.idkho = :pIdKho
			                 and t1.idsanpham = :pIdSanPham
			                 and t1.idtrungtam = :pIdTrungTam",
                    idKho, idSanPham, idTrungTam);
        }

        public int GetTonDauKy(int idKho, int idSanPham)
        {
            ExecuteCommand(spHangTonKhoGetSoTonDauKy, idKho, idSanPham);
            return Convert.ToInt32(Parameters["p_TonDau"].Value);
        }
    }
}
