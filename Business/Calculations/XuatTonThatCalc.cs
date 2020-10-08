using System;
using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    public sealed class XuatTonThatCalc : XuatTonCalc
    {
        /// <summary>
        /// Chỉ tính tồn thật
        /// </summary>
        /// <param name="tonKhoInfo"></param>
        /// <param name="soChungTu"></param>
        /// <param name="ngayLap"></param>
        public XuatTonThatCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayLap) : base(tonKhoInfo, soChungTu, ngayLap) { }

        protected internal override void Calculation(int deltaSoLuong)
        {
            TonKhoInfo.SoLuong -= deltaSoLuong;
            TonKhoInfo.DeltaSoLuong = -deltaSoLuong;
        }
    }
}
