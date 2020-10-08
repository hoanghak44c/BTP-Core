using System;
using System.Collections.Generic;
using System.Text;
using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    public sealed class NhapTonKhongCalc : NhapTonCalc
    {
        public NhapTonKhongCalc(HangTonKhoInfo tonKhoInfo) : base(tonKhoInfo) {}

        public NhapTonKhongCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayChungTu) : base(tonKhoInfo, soChungTu, ngayChungTu) {}

        protected internal override void Calculation(int deltaSoLuong)
        {
            return;
        }
    }
}
