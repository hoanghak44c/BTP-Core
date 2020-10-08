using QLBH.Core.Business.Calculations;
using QLBH.Core.Infors;

namespace QLBH.Core.Business
{
    public abstract class ChungTuXuatKeToanBusinessBase<T, TK> : ChungTuKeToanKhoBusinessBase<T, TK> where T : class where TK : class
    {
        protected ChungTuXuatKeToanBusinessBase()
        {
            BusinessType = BusinessType.VIRTUAL_OUT;
        }

        protected ChungTuXuatKeToanBusinessBase(T chungTuBaseInfo) : base(chungTuBaseInfo)
        {
            BusinessType = BusinessType.VIRTUAL_OUT;
        }

        protected override void CreateTonKhoCalc(HangTonKhoInfo tonKhoInfo)
        {
            TonKhoCalc = new XuatTonAoCalc(tonKhoInfo);
        }
        /// <summary>
        /// Kiểm tra ngay khi kế toán chọn sản phẩm và nhập số lượng để lập phiếu xuất
        /// </summary>
        /// <param name="idSanPham">Id sản phẩm được chọn</param>
        /// <param name="soLuong">số lượng sản phẩm vừa được nhập</param>
        /// <returns></returns>
        public bool TonKhoKhongDuXuat(int idSanPham, int soLuong)
        {
            return true;
        }
    }
}
