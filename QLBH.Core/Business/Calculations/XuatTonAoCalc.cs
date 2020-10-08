using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    public sealed class XuatTonAoCalc : XuatTonCalc
    {
        /// <summary>
        /// Chỉ tính tồn ảo
        /// </summary>
        /// <param name="tonKhoInfo"></param>
        public XuatTonAoCalc(HangTonKhoInfo tonKhoInfo) : base(tonKhoInfo){ }

        protected internal override void Calculation(int deltaSoLuong)
        {
            TonKhoInfo.TonAo -= deltaSoLuong;
            TonKhoInfo.DeltaTonAo = -deltaSoLuong;
        }
    }
}
