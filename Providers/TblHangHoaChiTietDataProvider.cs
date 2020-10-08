using System;
using QLBH.Core.DAO;
using QLBH.Core.Infors;

namespace QLBH.Core.Providers
{
    public class TblHangHoaChiTietDataProvider
    {
        internal static int Insert(HangHoa_ChiTietInfo info)
        {
            return TblHanghoaChiTietDAO.Instance.Insert(info);
        }
        internal static int Update(HangHoa_ChiTietInfo info)
        {
            return TblHanghoaChiTietDAO.Instance.Update(info);
        }
        public static HangHoa_ChiTietInfo GetHangHoaChiTietByMaVach(int idKho, int idSanPham, string maVach, int idTrungTam)
        {
            return TblHanghoaChiTietDAO.Instance.GetHangHoaChiTietByMaVach(idKho, idSanPham, maVach, idTrungTam);
        }
        public static DateTime GetNgayBaoHanhByMaVach(string maVach, int idSanPham)
        {
            return TblHanghoaChiTietDAO.Instance.GetNgayBaoHanhByMaVach(maVach, idSanPham);
        }
        public static bool IsUsedForAnotherProduct(string maVach, int idSanPham)
        {
            return TblHanghoaChiTietDAO.Instance.IsUsedForAnotherProduct(maVach, idSanPham);
        }
        public static bool IsUniqueSerial(string maVach)
        {
            return TblHanghoaChiTietDAO.Instance.IsUiqueSerial(maVach);
        }
        
        public static bool IsNotInUniqueSerial(string maVach)
        {
            return TblHanghoaChiTietDAO.Instance.IsNotInUiqueSerial(maVach);
        }

        public static bool DaDungChoGiaoDichKhac(int idChiTietHangHoa)
        {
            return TblHanghoaChiTietDAO.Instance.DaDungChoGiaoDichKhac(idChiTietHangHoa);
        }

        public static void UpdateTuoiTonBaseInfo(string maVach, int idChungTu, HangHoa_ChiTietInfo hangHoaInfo)
        {
            TblHanghoaChiTietDAO.Instance.UpdateTuoiTonBaseInfo(maVach, idChungTu, hangHoaInfo);
        }

        public static void PendingXacDinhNguonGoc(string maVach, int idChungTu, string soChungTu, int idSanPham)
        {
            TblHanghoaChiTietDAO.Instance.PendingXacDinhNguonGoc(maVach, idChungTu, soChungTu, idSanPham);
        }
    }
}
