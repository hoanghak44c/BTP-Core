using System;
using System.Reflection;

namespace QLBH.Core.Infors
{
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    public class HangHoa_ChiTietInfo : ICloneable
    {
        public int IdChiTiet { get; set; }

        public int IdKho { get; set; }

        public int IdSanPham { get; set; }

        public string MaSanPham { get; set; }

        public string MaVach { get; set; }

        public int SoLuong { get; set; }

        internal int DeltaSoLuong { get; set; }

        public string GhiChu { get; set; }

        public int IdTrungTam { get; set; }

        public DateTime BaoHanhHangTu { get; set; }
        
        public DateTime BaoHanhHangDen { get; set; }

        public int IdPhieuNhap { get; set; }

        public int IdPhieuXuat { get; set; }

        public DateTime NgayNhapKho_DK { get; set; }

        public string SoPhieuNhap_DK { get; set; }

        public string SoPO_DK { get; set; }

        public string MaNCC_DK { get; set; }

        public DateTime LastUpdateDate { get; set; }

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new HangHoa_ChiTietInfo
                       {
                           BaoHanhHangDen = BaoHanhHangDen,
                           BaoHanhHangTu = BaoHanhHangTu,
                           DeltaSoLuong = DeltaSoLuong,
                           GhiChu = GhiChu,
                           IdChiTiet = IdChiTiet,
                           IdKho = IdKho,
                           IdPhieuNhap = IdPhieuNhap,
                           IdPhieuXuat = IdPhieuXuat,
                           IdSanPham = IdSanPham,
                           IdTrungTam = IdTrungTam,
                           LastUpdateDate = LastUpdateDate,
                           MaNCC_DK = MaNCC_DK,
                           MaSanPham = MaSanPham,
                           MaVach = MaVach,
                           NgayNhapKho_DK = NgayNhapKho_DK,
                           SoLuong = SoLuong,
                           SoPhieuNhap_DK = SoPhieuNhap_DK,
                           SoPO_DK = SoPO_DK
                       };
        }

        #endregion
    }
}
