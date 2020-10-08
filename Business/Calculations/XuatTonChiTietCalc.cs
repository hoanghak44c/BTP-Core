using System;
using QLBH.Core.Exceptions;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Business.Calculations
{
    public class XuatTonChiTietCalc : TonChiTietCalc
    {
        public XuatTonChiTietCalc(HangHoa_ChiTietInfo tonChiTietInfo) : base(tonChiTietInfo) { }

        protected virtual void Minus(int soLuong)
        {
            TonChiTietInfo.SoLuong -= soLuong;
            TonChiTietInfo.DeltaSoLuong = -soLuong;
        }

        internal override void Calculate(int soLuong)
        {
            var tonChiTietTmpInfo = TonChiTietInfo.Clone() as HangHoa_ChiTietInfo;

            Minus(soLuong);

            if (soLuong != 0 && (TonChiTietInfo.SoLuong < 0))
            {
                var soTon = tonChiTietTmpInfo == null ? 0 : tonChiTietTmpInfo.SoLuong;

                var message = String.Format("Số lượng mã vạch '{0}' {1}, không thể thực hiện được.", tonChiTietTmpInfo.MaVach,
                                               soTon > 0 ? "chỉ còn " + soTon : "đã hết");
                message += "\nIdSanPham: " + tonChiTietTmpInfo.IdSanPham;
                message += "\nIdKho: " + tonChiTietTmpInfo.IdKho;
                message += "\nIdTrungTam: " + tonChiTietTmpInfo.IdTrungTam;
                message += "\nIdChiTiet: " + tonChiTietTmpInfo.IdChiTiet;
                throw new TinhTonException(message);
            }

            if(soLuong == 0) return;

            if (TonChiTietInfo.IdChiTiet != 0)
            {
                while (TblHangHoaChiTietDataProvider.Update(TonChiTietInfo) == 0)
                {
                    TonChiTietInfo = TblHangHoaChiTietDataProvider.
                        GetHangHoaChiTietByMaVach(TonChiTietInfo.IdKho,
                                                  TonChiTietInfo.IdSanPham,
                                                  TonChiTietInfo.MaVach,
                                                  TonChiTietInfo.IdTrungTam);

                    tonChiTietTmpInfo = TonChiTietInfo.Clone() as HangHoa_ChiTietInfo;

                    //TonChiTietInfo.DeltaSoLuong = tonChiTietTmpInfo.DeltaSoLuong;

                    Minus(soLuong);

                    if (soLuong != 0 && (TonChiTietInfo.SoLuong < 0))
                    {
                        var soTon = tonChiTietTmpInfo == null ? 0 : tonChiTietTmpInfo.SoLuong;

                        var message = String.Format("Số lượng mã vạch '{0}' {1}, không thể thực hiện được.", tonChiTietTmpInfo.MaVach,
                                                       soTon > 0 ? "chỉ còn " + soTon : "đã hết");
                        message += "\nIdSanPham: " + tonChiTietTmpInfo.IdSanPham;
                        message += "\nIdKho: " + tonChiTietTmpInfo.IdKho;
                        message += "\nIdTrungTam: " + tonChiTietTmpInfo.IdTrungTam;
                        message += "\nIdChiTiet: " + tonChiTietTmpInfo.IdChiTiet;
                        throw new TinhTonException(message);
                    }
                }
            }
        }
    }
}
