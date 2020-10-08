using System;
using QLBH.Core.Exceptions;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Business.Calculations
{
    /// <summary>
    /// Tính cả tồn thật và tồn ảo
    /// </summary>
    public class XuatTonCalc : TonKhoCalc
    {
        protected XuatTonCalc(HangTonKhoInfo tonKhoInfo) : base(tonKhoInfo) { }

        public XuatTonCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayChungTu) : base(tonKhoInfo, soChungTu, ngayChungTu){ }

        protected internal override void Calculation(int deltaSoLuong)
        {
            TonKhoInfo.SoLuong -= deltaSoLuong;
            TonKhoInfo.DeltaSoLuong = -deltaSoLuong;
            TonKhoInfo.TonAo -= deltaSoLuong;
            TonKhoInfo.DeltaTonAo = -deltaSoLuong;
        }

        internal sealed override void Calculate(int deltaSoLuong)
        {
            base.Calculate(deltaSoLuong);

            if (deltaSoLuong != 0)
            {
                if (TonKhoInfo.IdTonKho == 0)
                {
                    throw new ManagedException("Hàng không có trong kho, không thể thực hiện giao dịch.");
                }

                if(TonKhoInfo.SoLuong < 0 || TonKhoInfo.TonAo < 0)
                {
                    int soTon = TonKhoInfo.SoLuong < 0 ? TonKhoInfo.SoLuong : TonKhoInfo.TonAo;
                    throw new TinhTonException(String.Format("{0}, không thể thực hiện được.",
                                                             soTon > 0 ? "Số lượng tồn chỉ còn " + soTon : "Đã hết hàng"));                    
                }
            }
        }

        protected internal override void CreateTheKho(TheKhoInfo theKhoInfo)
        {
            TheKhoCalc = new TheXuat(theKhoInfo);
        }
    }
}
