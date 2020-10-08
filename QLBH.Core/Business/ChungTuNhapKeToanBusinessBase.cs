using QLBH.Core.Business.Calculations;
using QLBH.Core.Infors;

namespace QLBH.Core.Business
{
    public abstract class ChungTuNhapKeToanBusinessBase<T, TK> : ChungTuKeToanKhoBusinessBase<T, TK> where T : class where TK : class
    {
        protected ChungTuNhapKeToanBusinessBase()
        {
            BusinessType = BusinessType.VIRTUAL_IN;
        }

        protected ChungTuNhapKeToanBusinessBase(T chungTuBaseInfo) : base(chungTuBaseInfo)
        {
            BusinessType = BusinessType.VIRTUAL_IN;
        }

        protected override void CreateTonKhoCalc(HangTonKhoInfo tonKhoInfo)
        {
            TonKhoCalc = new NhapTonAoCalc(tonKhoInfo);
        }
    }

}
