using QLBH.Core.Business.Calculations;
using QLBH.Core.Infors;

namespace QLBH.Core.Business
{
    public abstract class ChungTuXuatKhoBusinessBase<T, TK, TL> : ChungTuKhoBusinessBase<T, TK, TL> where T : class where TK : class where TL : class
    {
        protected ChungTuXuatKhoBusinessBase()
        {
            BusinessType = BusinessType.REAL_OUT;
        }

        protected ChungTuXuatKhoBusinessBase(T chungTuBaseInfo) : base(chungTuBaseInfo)
        {
            BusinessType = BusinessType.REAL_OUT;
        }

        protected override void CreateTonKhoCalc(HangTonKhoInfo tonKhoInfo)
        {
            TonKhoCalc = new XuatTonThatCalc(tonKhoInfo, ChungTuInfo.SoChungTu, ChungTuInfo.NgayLap);
        }

        protected override void CreateTonMaVachCalc(HangHoa_ChiTietInfo tonChiTietInfo)
        {
            TonMaVachCalc = new XuatTonChiTietCalc(tonChiTietInfo);
        }
        /// <summary>
        /// Kiểm tra ngay khi thủ kho cập nhật chi tiết mã vạch cho chứng từ
        /// </summary>
        /// <param name="idSanPham">Id sản phẩm được nhập chi tiết mã vạch</param>
        /// <param name="maVach">mã vạch sản phẩm vừa được nhập</param>
        /// <param name="soLuong">số lượng mã vạch vừa được nhập</param>
        /// <returns></returns>
        public bool TonMaVachKhongDuXuat(int idSanPham, string maVach, int soLuong)
        {
            return true;
        }
    }
}
