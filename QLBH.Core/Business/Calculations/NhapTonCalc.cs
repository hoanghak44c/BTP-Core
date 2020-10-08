using System;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Business.Calculations
{
    /// <summary>
    /// Tính cả tồn thật và tồn ảo
    /// </summary>
    public class NhapTonCalc : TonKhoCalc
    {
        protected NhapTonCalc(HangTonKhoInfo tonKhoInfo) : base(tonKhoInfo) { }

        public NhapTonCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayChungTu) : base(tonKhoInfo, soChungTu, ngayChungTu) { }

        protected internal override void Calculation(int deltaSoLuong)
        {
            TonKhoInfo.SoLuong += deltaSoLuong;
            TonKhoInfo.DeltaSoLuong = deltaSoLuong;
            TonKhoInfo.TonAo += deltaSoLuong;
            TonKhoInfo.DeltaTonAo = deltaSoLuong;
        }

        internal sealed override void Calculate(int deltaSoLuong)
        {
            base.Calculate(deltaSoLuong);
            
            if (TonKhoInfo.IdTonKho == 0)
            {
                if (TonKhoInfo.SoLuong > 0 || TonKhoInfo.TonAo > 0)

                    HangTonKhoDataProvider.Insert(TonKhoInfo);
                
                //else

                //    throw new ManagedException(
                //        String.Format("Sản phẩm không có trong kho, không thể thực hiện được giao dịch."));
            }
            
        }

        protected internal override void CreateTheKho(TheKhoInfo theKhoInfo)
        {
            TheKhoCalc = new TheNhap(theKhoInfo);
        }
    }
}
