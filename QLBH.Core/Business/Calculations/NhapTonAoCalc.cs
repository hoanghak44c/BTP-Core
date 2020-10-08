using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    /// <summary>
    /// Chỉ tính tồn ảo
    /// </summary>
    public sealed class NhapTonAoCalc : NhapTonCalc
    {
        public NhapTonAoCalc(HangTonKhoInfo tonKhoInfo) : base(tonKhoInfo) { }

        protected internal override void Calculation(int deltaSoLuong)
        {
            TonKhoInfo.TonAo += deltaSoLuong;
            TonKhoInfo.DeltaTonAo = deltaSoLuong;
        }
    }
}
