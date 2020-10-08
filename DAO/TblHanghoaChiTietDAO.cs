using System;
using System.Collections.Generic;
using QLBH.Core.Data;
using QLBH.Core.Infors;

namespace QLBH.Core.DAO
{
    public class TblHanghoaChiTietDAO : BaseDAO
    {
        private static TblHanghoaChiTietDAO instance;
        
        private TblHanghoaChiTietDAO(){}

        public static TblHanghoaChiTietDAO Instance
        {
            get
            {
                if (instance == null) instance = new TblHanghoaChiTietDAO();
                return instance;
            }
        }
        internal int Insert(HangHoa_ChiTietInfo info)
        {            
            ExecuteCommand(spHangHoaChiTietInsert4, info.IdKho, info.IdSanPham, info.MaVach, info.SoLuong, info.GhiChu,
                info.IdTrungTam, info.BaoHanhHangTu, info.BaoHanhHangDen, info.IdPhieuNhap, info.IdPhieuXuat, 
                info.NgayNhapKho_DK, info.SoPhieuNhap_DK, info.SoPO_DK, info.MaNCC_DK);
            
            info.IdChiTiet = Convert.ToInt32(Parameters["p_IdChiTiet"].Value.ToString());

            return Convert.ToInt32(Parameters["p_IdChiTiet"].Value.ToString());
        }

        internal int Update(HangHoa_ChiTietInfo info)
        {
            var lstParam = new List<object>();

            string commandText =
                @"UPDATE tbl_hanghoa_chitiet t1
                     SET t1.soluong = t1.soluong + :pDeltaSoLuong,
                         t1.ghichu = :pGhiChu,
                         t1.idtrungtam = :pIdTrungTam,
                         t1.Baohanhhangtu = :pBaoHanhHangTu,
                         t1.Baohanhhangden = :pBaoHanhHangDen";

            lstParam.AddRange(new object[]
                                  {
                                      info.DeltaSoLuong, 
                                      info.GhiChu, 
                                      info.IdTrungTam, 
                                      info.BaoHanhHangTu,
                                      info.BaoHanhHangDen
                                  });

            if (info.NgayNhapKho_DK.Date != DateTime.MinValue.Date)
            {
                commandText += @",t1.ngaynhapkho_dk  = :pNgayNhapKhoDK,
	                         t1.sochungtugoc_dk = :pSoPhieuNhapDK,
	                         t1.soseri_dk = :pSoPODK,
	                         t1.mancc_dk = :pMaNCCDK";

                lstParam.AddRange(new object[]
                                  {
                                      info.NgayNhapKho_DK,
                                      info.SoPhieuNhap_DK,
                                      info.SoPO_DK,
                                      info.MaNCC_DK
                                  });
            }
            
            if (info.IdPhieuNhap > 0)
            {
                commandText += @",t1.idphieunhap = :pIdPhieuNhap";

                lstParam.Add(info.IdPhieuNhap);
            }

            if (info.IdPhieuXuat > 0)
            {
                commandText += @",t1.idphieuxuat = :pIdPhieuXuat";

                lstParam.Add(info.IdPhieuXuat);                
            }

            commandText += @" WHERE t1.idchitiet = :pIdChiTiet
			                 and t1.last_update_date = :pLastUpdateDate";
            
            lstParam.AddRange(new object[] {info.IdChiTiet, info.LastUpdateDate});

            return ExecuteCommand(commandText, lstParam.ToArray());

        }

        public HangHoa_ChiTietInfo GetHangHoaChiTietByMaVach(int idKho, int idSanPham, string maVach, int idTrungTam)
        {
            return GetObjectCommand<HangHoa_ChiTietInfo>(spHangHoaChiTietGetByMaVach1, idKho, idSanPham, maVach, idTrungTam);
        }

        public DateTime GetNgayBaoHanhByMaVach(string maVach, int idSanPham)
        {
            ExecuteCommand(spNgayBaoHanhGetByMaVach, maVach, idSanPham);
            
            if (Parameters["p_BaoHanhTu"].Value == DBNull.Value) return DateTime.MinValue;

            DateTime result;

            if (DateTime.TryParse(Parameters["p_BaoHanhTu"].Value.ToString(), out result))
            {
                return result;
            }

            return DateTime.MinValue;
        }

        public bool IsUsedForAnotherProduct(string maVach, int idSanPham)
        {
            return false; //ma vach co the dung cho san pham khac neu khong con ton.
            ExecuteCommand(spHangHoaChiTietIsUsedForAnother, maVach, idSanPham);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) == 1;
        }

        public bool IsUiqueSerial(string maVach)
        {
            ExecuteCommand(spHangHoaChiTietIsUniqueSerial, maVach);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) == 0;
        }

        public bool IsNotInUiqueSerial(string maVach)
        {
            ExecuteCommand(spHangHoaChiTietIsNotInUniqueSerial, maVach);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) == 0;
        }

        public bool DaDungChoGiaoDichKhac(int idChiTietHangHoa)
        {
            ExecuteCommand(spHangHoaChiTietDaDungChoGiaoDichKhac, idChiTietHangHoa);
            return Convert.ToInt32(Parameters["p_Count"].Value.ToString()) > 1;
        }

        public void UpdateTuoiTonBaseInfo(string maVach, int idChungTu, HangHoa_ChiTietInfo hangHoaInfo)
        {
            ExecuteCommand(spHangHoaChiTietSetTuoiTon, maVach, idChungTu, hangHoaInfo.IdSanPham);
            hangHoaInfo.IdPhieuNhap = Parameters["p_IdPhieuNhap"].Value == DBNull.Value ||
            Parameters["p_IdPhieuNhap"].Value.ToString() == "null" ? 0 : Convert.ToInt32(Parameters["p_IdPhieuNhap"].Value.ToString());
            hangHoaInfo.IdPhieuXuat = Parameters["p_IdPhieuXuat"].Value == DBNull.Value ||
            Parameters["p_IdPhieuXuat"].Value.ToString() == "null" ? 0 : Convert.ToInt32(Parameters["p_IdPhieuXuat"].Value.ToString());
            hangHoaInfo.NgayNhapKho_DK = Parameters["p_NgayNhapKho"].Value == DBNull.Value ||
            Parameters["p_NgayNhapKho"].Value.ToString() == "null" ? DateTime.MinValue : Convert.ToDateTime(Parameters["p_NgayNhapKho"].Value.ToString());
            hangHoaInfo.SoPhieuNhap_DK = Parameters["p_SoPhieuNhap"].Value == DBNull.Value ||
            Parameters["p_SoPhieuNhap"].Value.ToString() == "null" ? String.Empty : Parameters["p_SoPhieuNhap"].Value.ToString();
            hangHoaInfo.SoPO_DK = Parameters["p_SoPO"].Value == DBNull.Value ||
            Parameters["p_SoPO"].Value.ToString() == "null" ? String.Empty : Parameters["p_SoPO"].Value.ToString();
            hangHoaInfo.MaNCC_DK = Parameters["p_MaNhaCC"].Value == DBNull.Value ||
            Parameters["p_MaNhaCC"].Value.ToString() == "null" ? String.Empty : Parameters["p_MaNhaCC"].Value.ToString();
        }

        public void PendingXacDinhNguonGoc(string maVach, int idChungTu, string soChungTu, int idSanPham)
        {
            bool notExisted = ExecuteScalar(
                @"select idchungtu from tbl_mavach_set_tuoiton_pending
                where idchungtu = :idChungTu and sochungtu = :soChungTu 
                and mavach = :maVach and idsanpham = :idSanPham",
                idChungTu, soChungTu, maVach, idSanPham) == null;

            if (notExisted)
                ExecuteCommand(
                    @"insert into tbl_mavach_set_tuoiton_pending(idchungtu, reason, 
                    createdate, sochungtu, mavach, idsanpham) values (:idChungTu, 'UNKNOWN', 
                    sysdate, :soChungTu, :maVach, :idSanPham)",
                    idChungTu, soChungTu, maVach, idSanPham);
        }
    }
}
