using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DevExpress.XtraEditors.Repository;
using QLBH.Core.Data;

namespace QLBH.Core.Infors
{
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    class DMSanPhamInfo
    {
        [DefaultDisplay(false)]
        public int IdSanPham { get; set; }

        [DefaultDisplay(false)]
        public int IdCha { get; set; }

        [DefaultDisplay(false)]
        public string MaVach { get; set; }

        [CaptionColumn("Mã sản phẩm")]
        public string MaSanPham { get; set; }

        [CaptionColumn("Tên sản phẩm")]
        public string TenSanPham { get; set; }

        [DefaultDisplay(false)]
        public int IdDonViTinh { get; set; }

        [CaptionColumn("Giá nhập")]
        public int GiaNhap { get; set; }

        [CaptionColumn("Mô tả")]
        public string MoTa { get; set; }

        [CaptionColumn("Sử dụng"), XtraGridEditor(typeof(RepositoryItemCheckEdit))]
        public int SuDung { get; set; }

        [CaptionColumn("Trùng mã vạch"), XtraGridEditor(typeof(RepositoryItemCheckEdit))]
        public int TrungMaVach { get; set; }

        [CaptionColumn("Tên viết tắt")]
        public string TenVietTat { get; set; }

        [CaptionColumn("Chiết khấu"), XtraGridEditor(typeof(RepositoryItemCheckEdit))]
        public int ChietKhau { get; set; }

        [CaptionColumn("Bảo hành hãng")]
        public int BaoHanhHang { get; set; }

        [CaptionColumn("Bảo hành khách")]
        public int BaoHanhKhach { get; set; }

        [CaptionColumn("Loại sản phẩm")]
        public string TenLoaiSP { get; set; }

        [CaptionColumn("Ngành")]
        public string Nganh { get; set; }

        [CaptionColumn("Loại")]
        public string Loai { get; set; }

        [CaptionColumn("Hãng"), DefaultDisplay(false)]
        public string Hang { get; set; }
        /// <summary>
        /// IdTaxCode
        /// </summary>
        public int TyLeVAT { get; set; }

        [CaptionColumn("Phụ kiện")]
        public int PhuKien { get; set; }
        [CaptionColumn("Hàng khuyến mại")]
        public int HangKhuyenMai { get; set; }
    }
}
