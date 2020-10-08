using System;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Business.Calculations
{
    public class NhapTonChiTietCalc : TonChiTietCalc
    {
        public NhapTonChiTietCalc(HangHoa_ChiTietInfo tonChiTietInfo) : base(tonChiTietInfo) { }

        protected virtual void Plus(int soLuong)
        {
            TonChiTietInfo.SoLuong += soLuong;
            TonChiTietInfo.DeltaSoLuong = soLuong;
        }

        internal override void Calculate(int soLuong)
        {
            Plus(soLuong);

            if (TonChiTietInfo.IdChiTiet != 0)
            {
                var tonChiTietBckInfo = TonChiTietInfo.Clone() as HangHoa_ChiTietInfo;

                while (TblHangHoaChiTietDataProvider.Update(TonChiTietInfo) == 0)
                {
                    TonChiTietInfo = TblHangHoaChiTietDataProvider.
                        GetHangHoaChiTietByMaVach(TonChiTietInfo.IdKho,
                                                  TonChiTietInfo.IdSanPham,
                                                  TonChiTietInfo.MaVach,
                                                  TonChiTietInfo.IdTrungTam);
                    Plus(soLuong);
                    
                    //TonChiTietInfo.DeltaSoLuong = tonChiTietBckInfo.DeltaSoLuong;
                }
            }
            else //if (TonChiTietInfo.SoLuong > 0)
            {
                if(TblHangHoaChiTietDataProvider.IsUsedForAnotherProduct(TonChiTietInfo.MaVach, TonChiTietInfo.IdSanPham))
                    throw new Exception(String.Format("Mã vạch '{0}' đã được sử dụng cho loại sản phẩm khác!", TonChiTietInfo.MaVach));
                TonChiTietInfo.IdChiTiet = TblHangHoaChiTietDataProvider.Insert(TonChiTietInfo);
            }

        }
    }
}