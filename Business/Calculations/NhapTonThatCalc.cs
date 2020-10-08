using System;
using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    public sealed class NhapTonThatCalc : NhapTonCalc
    {
        /// <summary>
        /// Chỉ tính tồn thật
        /// </summary>
        /// <param name="tonKhoInfo"></param>
        /// <param name="soChungTu"></param>
        /// <param name="ngayChungTu"></param>
        public NhapTonThatCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayChungTu) : base(tonKhoInfo, soChungTu, ngayChungTu) { }

        protected internal override void Calculation(int deltaSoLuong)
        {
            TonKhoInfo.SoLuong += deltaSoLuong;
            TonKhoInfo.DeltaSoLuong = deltaSoLuong;
        }
    }
}
