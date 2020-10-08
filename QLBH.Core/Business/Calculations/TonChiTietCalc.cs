using QLBH.Core.Infors;

namespace QLBH.Core.Business.Calculations
{
    public abstract class TonChiTietCalc
    {
        internal HangHoa_ChiTietInfo TonChiTietInfo;

        protected TonChiTietCalc(HangHoa_ChiTietInfo tonChiTietInfo)
        {
            this.TonChiTietInfo = tonChiTietInfo;
        }

        internal abstract void Calculate(int soLuong);
    }
}
