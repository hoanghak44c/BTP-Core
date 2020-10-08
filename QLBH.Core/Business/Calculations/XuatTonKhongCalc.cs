using System;
using System.Collections.Generic;
using System.Text;
using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    public sealed class XuatTonKhongCalc : XuatTonCalc
    {
        public XuatTonKhongCalc(HangTonKhoInfo tonKhoInfo) : base(tonKhoInfo) { }

        public XuatTonKhongCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayChungTu) : base(tonKhoInfo, soChungTu, ngayChungTu) { }

        protected internal override void Calculation(int deltaSoLuong)
        {
            return;
        }
    }
}
