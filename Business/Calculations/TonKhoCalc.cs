using System;
using System.Collections.Generic;
//using QLBH.Common;
using QLBH.Core.Exceptions;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Business.Calculations
{
    public abstract class TonKhoCalc
    {
        protected HangTonKhoInfo TonKhoInfo;
        protected HangTonKhoInfo TonKhoInfoBck;
        internal static TheKhoInfo TheKhoInfo;
        protected TheKho TheKhoCalc;
        private static bool? hasTheKho;
        internal int IdChungTu;

        protected TonKhoCalc(HangTonKhoInfo tonKhoInfo)
        {
            TonKhoInfo = tonKhoInfo;            
        }

        protected TonKhoCalc(HangTonKhoInfo tonKhoInfo, string soChungTu, DateTime ngayChungTu)
        {
            TonKhoInfo = tonKhoInfo;
            
            if(TheKhoNeeded)
            {
                if (TheKhoInfo == null || TheKhoInfo.SoChungTu != soChungTu || TheKhoInfo.IdSanPham != tonKhoInfo.IdSanPham)
                    hasTheKho = null;

                TheKhoInfo =
                    TheKhoDataProvider.GetTheKhoBy(tonKhoInfo.IdKho, tonKhoInfo.IdSanPham, soChungTu, tonKhoInfo.IdTrungTam) ??
                    new TheKhoInfo
                    {
                        IdKho = tonKhoInfo.IdKho,
                        IdSanPham = tonKhoInfo.IdSanPham,
                        NgayChungTu = ngayChungTu,
                        SoChungTu = soChungTu,
                        IdTrungTam = tonKhoInfo.IdTrungTam
                    };

                TheKhoInfo.NgayChungTu = ngayChungTu;

                if(hasTheKho == null)
                {
                    hasTheKho = TheKhoInfo.IdTheKho != 0;
                }

                CreateTheKho(TheKhoInfo);                
            }
        }

        protected internal abstract void Calculation(int deltaSoLuong);

        internal bool HasTheKho
        {
            get
            {
                if (hasTheKho != null) return (bool) hasTheKho;

                return false;
            }
        }

        internal static bool TheKhoNeeded { get; set; }

        protected internal abstract void CreateTheKho(TheKhoInfo theKhoInfo);

        internal virtual void Calculate(int deltaSoLuong)
        {
            try
            {
                TonKhoInfoBck = TonKhoInfo.Clone() as HangTonKhoInfo;

                Calculation(deltaSoLuong);

                //TonKhoInfoBck = TonKhoInfo.Clone() as HangTonKhoInfo;

                if (deltaSoLuong != 0 && (TonKhoInfo.SoLuong < 0 || TonKhoInfo.TonAo < 0))
                {
                    int soTon = TonKhoInfo.SoLuong < 0 ? TonKhoInfoBck.SoLuong : TonKhoInfoBck.TonAo;
                    throw new TinhTonException(String.Format("{0}, không thể thực hiện được.",
                                                             soTon > 0 ? "Số lượng tồn chỉ còn " + soTon : "Đã hết hàng"));
                }

                //if (TheKhoCalc == null || deltaSoLuong == 0) return;

                if (TheKhoCalc != null)
                {
                    if (TheKhoInfo.IdTheKho == 0)
                    {
                        //dong lai tam thoi
                        //if (deltaSoLuong < 0)
                        //    throw new TinhTonException("Không thể ghi thẻ có xuất/nhập âm");
                    }

                    TheKhoCalc.GhiXuatNhap(deltaSoLuong);

                    TheKhoDataProvider.WriteLog(TheKhoInfo);

                    //hasTheKho = true;                    
                }

                if (deltaSoLuong == 0) return;

                if (TonKhoInfo.IdTonKho != 0)
                {
                    while (HangTonKhoDataProvider.Update(TonKhoInfo, IdChungTu) == 0)
                    {
                        TonKhoInfo = HangTonKhoDataProvider.GetHangTonKhoById(TonKhoInfo.IdKho, TonKhoInfo.IdSanPham,
                                                                              TonKhoInfo.IdTrungTam);
                        
                        //TonKhoInfo.DeltaSoLuong = TonKhoInfoBck.DeltaSoLuong;
                        //TonKhoInfo.DeltaTonAo = TonKhoInfoBck.DeltaTonAo;
                        
                        TonKhoInfoBck = TonKhoInfo.Clone() as HangTonKhoInfo;

                        Calculation(deltaSoLuong);
                        
                        if (deltaSoLuong != 0 && (TonKhoInfo.SoLuong < 0 || TonKhoInfo.TonAo < 0))
                        {
                            int soTon = TonKhoInfo.SoLuong < 0 ? TonKhoInfoBck.SoLuong : TonKhoInfoBck.TonAo;
                            throw new TinhTonException(String.Format("{0}, không thể thực hiện được.",
                                                                     soTon > 0 ? "Số lượng tồn chỉ còn " + soTon : "Đã hết hàng"));
                        }
                    }
                }

            }
            catch (TinhTonException ex)
            {
                if (TonKhoInfo != null)
                    throw new TinhTonException(ex.Message + "\nIdTrungTam: " + TonKhoInfo.IdTrungTam +
                    "\nIdKho: " + TonKhoInfo.IdKho);
                throw;
            }
        }


        internal protected abstract class TheKho
        {
            protected TheKhoInfo TheKhoInfo;

            protected internal TheKho(TheKhoInfo theKhoInfo)
            {
                TheKhoInfo = theKhoInfo;
            }

            internal abstract void GhiXuatNhap(int soLuong);
        }

        internal sealed class TheXuat : TheKho
        {
            internal TheXuat(TheKhoInfo theKhoInfo) : base(theKhoInfo) { }

            internal override void GhiXuatNhap(int soLuong)
            {
                TheKhoInfo.Xuat += soLuong;
            }

        }

        internal sealed class TheNhap : TheKho
        {
            internal TheNhap(TheKhoInfo theKhoInfo) : base(theKhoInfo) { }

            internal override void GhiXuatNhap(int soLuong)
            {
                TheKhoInfo.Nhap += soLuong;
            }

        }

    }
}
