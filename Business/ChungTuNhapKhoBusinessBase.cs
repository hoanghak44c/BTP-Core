using QLBH.Core.Business.Calculations;
using QLBH.Core.Infors;

namespace QLBH.Core.Business
{
    public abstract class ChungTuNhapKhoBusinessBase<T, TK, TL> : ChungTuKhoBusinessBase<T, TK, TL> where T : class where TK : class where TL : class
    {
        protected ChungTuNhapKhoBusinessBase()
        {
            BusinessType = BusinessType.REAL_IN;
        }

        protected ChungTuNhapKhoBusinessBase(T chungTuBaseInfo) : base(chungTuBaseInfo)
        {
            BusinessType = BusinessType.REAL_IN;
        }

        protected override void CreateTonKhoCalc(HangTonKhoInfo tonKhoInfo)
        {
            TonKhoCalc = new NhapTonThatCalc(tonKhoInfo, ChungTuInfo.SoChungTu, ChungTuInfo.NgayLap);
        }

        protected override void CreateTonMaVachCalc(HangHoa_ChiTietInfo tonChiTietInfo)
        {
            TonMaVachCalc = new NhapTonChiTietCalc(tonChiTietInfo);
        }
        /// <summary>
        /// Kiểm tra ngay khi thủ kho cập nhật chi tiết cho chứng từ
        /// </summary>
        /// <param name="idSanPham">Id sản phẩm được nhập mã vạch</param>
        /// <param name="maVach">mã vạch vừa được nhập</param>
        /// <returns></returns>
        public bool MaVachDaDungChoSanPhamKhac(int idSanPham, string maVach)
        {
            return true;
        }
    }
}
