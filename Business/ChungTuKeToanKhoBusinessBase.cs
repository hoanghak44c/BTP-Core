using System;
using System.Collections.Generic;
//using QLBH.Common;
using QLBH.Core.Business.Calculations;
using QLBH.Core.Exceptions;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Business
{

    /// <summary>
    /// Lập chứng từ kế toán có liên quan đến tồn kho
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK"></typeparam>
    public abstract class ChungTuKeToanKhoBusinessBase<T, TK> : ChungTuKeToanBusinessBase<T, TK> where T : class where TK : class
    {
        protected TonKhoCalc TonKhoCalc;

        protected readonly int IdTrungTam;

        protected ChungTuKeToanKhoBusinessBase()
        {
            IdTrungTam = TinhTonKhoTheoTrungTam();
            TonKhoCalc.TheKhoInfo = null;
        }

        protected ChungTuKeToanKhoBusinessBase(T chungTuBaseInfo) : base(chungTuBaseInfo)
        {
            IdTrungTam = TinhTonKhoTheoTrungTam();
            TonKhoCalc.TheKhoInfo = null;
        }

        protected IBussinessKeToanKhoProvider<T, TK> BusinessKeToanKhoProvider
        {
            get { return BusinessProvider as IBussinessKeToanKhoProvider<T, TK>; }
            set { BusinessProvider = value; }
        }

        protected internal override void Prepare()
        {
            ListChiTietChungTu = BusinessProvider.GetListChiTietChungTuByIdChungTu(ChungTuInfo.IdChungTu);
        }

        protected internal override void DeleteChungTuInstance()
        {
            if (IsEditMode)
            {
                CheckInvalidLock();

                if (((BusinessProviderBase)BusinessProvider).GetChungTuRefBySoChungTu<T>(ChungTuInfo.SoChungTu) != null)
                {
                    throw new Exception("Chứng từ này đã được sử dụng trong hệ thống, không thể xóa được.");
                }

                if (!((BusinessProviderBase)BusinessProvider).ChiTietHangHoaExists(ChungTuInfo.SoChungTu))
                {
                    SaveChiTietChungTu(ListChiTietChungTu, new List<TK>());

                    if (BusinessKeToanKhoProvider != null)
                    {
                        BusinessKeToanKhoProvider.DeleteChungTu(ChungTuInfo.IdChungTu);
                    }
                    else
                    {
                        BusinessProvider.UpdateChungTu(ChungTu);
                    }
                }
                else
                {
                    throw new Exception("Chứng từ này đã được bắn mã vạch, không thể xóa được.");
                }                
            }
        }

        protected internal override void CancelChungTuInstance()
        {
            if (IsEditMode)
            {
                CheckInvalidLock();

                if (!ChungTuInfo.IsOrigin)
                {
                    BusinessProvider.UpdateChungTu(ChungTu);
                }

                ChungTuChiTietBaseInfo chungTuChiTietInfo;

                foreach (TK originInfo in ListChiTietChungTu)
                {
                    chungTuChiTietInfo = ToBaseInfo(originInfo);

                    TinhTonKho(ChungTuInfo.IdKho, chungTuChiTietInfo.IdSanPham, -chungTuChiTietInfo.SoLuong, chungTuChiTietInfo.SoLuong);
                }

                if (ChungTuInfo.IdKho == 0) return;

                ((BusinessProviderBase)BusinessProvider).ChungTuSyncPushORC(this);

            }
        }

        /// <summary>
        /// Kiểm tra có cập nhật thì mới lưu.
        /// </summary>
        protected internal override void SaveChungTuInstance()
        {
            TK[] arrBackupChiTietChungTu = new TK[ListChiTietChungTu.Count];
            ListChiTietChungTu.CopyTo(arrBackupChiTietChungTu);
            int backupChungTu = ChungTuInfo.IdChungTu;

            try
            {
                if (ListChiTietChungTu.Count == 0) 
                    throw new ManagedException("Không có line chi tiết!");

                if (!IsOnTheSameAccountBook) 
                    throw new ManagedException("Không được ghi nhận trong cùng bộ sổ.");

                if (IsEditMode)
                {
                    if (!ChungTuInfo.IsOrigin)
                    {
                        if ((ChungTuOriginInfo as ChungTuBaseInfo).IdKho != ChungTuInfo.IdKho)
                        {
                            throw new ManagedException("Chứng từ đã được tạo, không thể thay đổi thông tin kho!");
                        }

                        if ((ChungTuOriginInfo as ChungTuBaseInfo).SoChungTu != ChungTuInfo.SoChungTu)
                        {
                            throw new ManagedException("Chứng từ đã được tạo, không thể thay đổi số chứng từ!");
                        }

                        CheckInvalidLock();

                        BusinessProvider.UpdateChungTu(ChungTu);
                    }
                }
                else
                {
                    ChungTuInfo.IdChungTu = BusinessProvider.InsertChungTu(ChungTu);
                }

                //if ((((BusinessProviderBase)BusinessProvider).GetChungTuBySoChungTu<T>(ChungTuInfo.SoChungTu) as ChungTuBaseInfo).TrangThai == TrangThaiHuy)
                //{
                //    throw new ManagedException("Chứng từ này đã bị hủy!");
                //}                    

                //check so chung tu bi lap
                if (BusinessProvider is BusinessProviderBase)
                {
                    if(((BusinessProviderBase)BusinessProvider).
                        DuplicateSoChungTu(ChungTuInfo.SoChungTu, ChungTuInfo.IdChungTu))
                    {
                        throw new ManagedException("Số chứng từ này đã được sử dụng!");                        
                    }
                }
                

                List<TK> listOrigin = BusinessProvider.GetListChiTietChungTuByIdChungTu(ChungTuInfo.IdChungTu);

                if (!IsKeToan && listOrigin.Count == 0) throw new Exception("Nghiệp vụ này không làm phát sinh nội dung chứng từ");

                ListChiTietChungTu.Sort(
                    delegate(TK cthh1, TK cthh2)
                        {
                            return ToBaseInfo(cthh1).IdSanPham.CompareTo(ToBaseInfo(cthh2).IdSanPham);
                        });

                SaveChiTietChungTu(listOrigin, ListChiTietChungTu);

                if (ChungTuInfo.IdKho == 0) return;

                ((BusinessProviderBase)BusinessProvider).ChungTuSyncPushORC(this);
            }
            catch (Exception ex)
            {
                ChungTuInfo.IdChungTu = backupChungTu;
                ListChiTietChungTu.Clear();
                ListChiTietChungTu.AddRange(arrBackupChiTietChungTu);
                var description = String.Empty;
                ListChiTietChungTu.ForEach(delegate (TK ctct)
                                               {
                                                   description += ToBaseInfo(ctct).IdSanPham + " " + ToBaseInfo(ctct).SoLuong + "\r\n";
                                               });
                description += ex.ToString();

                EventLogProvider.Instance.WriteOfflineLog(String.Format("{0}\r\n{1}", ChungTuInfo.SoChungTu, description), "Loi luu chung tu");
                throw;
            }

        }

        protected virtual bool IsOnTheSameAccountBook
        {
            get { return true; }
        }

        protected internal bool Calculable(int idSanPham)
        {
            DMSanPhamInfo dmSanPhamInfo = DmSanPhamProvider.Instance.GetSanPhamById(idSanPham);
            return dmSanPhamInfo != null && dmSanPhamInfo.ChietKhau == 0;
        }

        private void TinhTonKho(int idKho, int idSanPham, int deltaLuong, int soLuong)
        {
            
            if(!Calculable(idSanPham)) return;

            //nếu có số tồn đầu kỳ thì chắc chắn đã có số tồn kho

            HangTonKhoInfo hangTonKhoInfo = HangTonKhoDataProvider.GetHangTonKhoById(idKho, idSanPham, IdTrungTam) ??
                                            new HangTonKhoInfo {IdKho = idKho, IdSanPham = idSanPham, IdTrungTam = IdTrungTam};

            TonKhoCalc.TheKhoNeeded = IsThuKho;

            CreateTonKhoCalc(hangTonKhoInfo);

            if (deltaLuong == 0)
            {
                if (!TonKhoCalc.HasTheKho && IsThuKho)
                    deltaLuong = soLuong;
            }

            TonKhoCalc.IdChungTu = ChungTuInfo.IdChungTu;

            TonKhoCalc.Calculate(deltaLuong);
            
        }

        protected void SaveChiTietChungTu(List<TK> listOrigin, List<TK> listNew)
        {
            ChungTuChiTietBaseInfo chungTuChiTietInfo = null;
            try
            {
                foreach (TK originInfo in listOrigin)
                {
                    chungTuChiTietInfo = ToBaseInfo(originInfo);
                    if (!listNew.Exists(delegate(TK match)
                    {
                        return ToBaseInfo(match).IdChungTuChiTiet == chungTuChiTietInfo.IdChungTuChiTiet;
                    }))
                    {
                        if (TinhLaiChiTietChungTu())
                        {
                            if (IsKeToan) DeleteChiTietChungTu(originInfo);
                            //if (TrangThaiOrigin != CancelState)
                            TinhTonKho(ChungTuInfo.IdKho, chungTuChiTietInfo.IdSanPham, -chungTuChiTietInfo.SoLuong, chungTuChiTietInfo.SoLuong);                            
                        }
                    }
                }

                foreach (TK newInfo in listNew)
                {
                    chungTuChiTietInfo = ToBaseInfo(newInfo);

                    if (chungTuChiTietInfo== null) throw new Exception("Không đúng kiểu " + typeof(ChungTuChiTietBaseInfo));

                    TK originInfo = listOrigin.Find(
                                        delegate(TK match)
                                        {
                                            return ToBaseInfo(match).IdChungTuChiTiet == chungTuChiTietInfo.IdChungTuChiTiet;
                                        });

                    if (originInfo != null)
                    {
                        if (IsKeToan && !chungTuChiTietInfo.IsOrigin) UpdateChiTietChungTu(newInfo);
                        //TinhTonKho(ChungTuInfo.IdKho, chungTuChiTietInfo.IdSanPham,
                        //           TrangThaiOrigin != CancelState
                        //               ? chungTuChiTietInfo.SoLuong - ToBaseInfo(originInfo).SoLuong
                        //               : chungTuChiTietInfo.SoLuong,
                        //           chungTuChiTietInfo.SoLuong);
                        TinhTonKho(ChungTuInfo.IdKho, chungTuChiTietInfo.IdSanPham,
                                       chungTuChiTietInfo.SoLuong - ToBaseInfo(originInfo).SoLuong,
                                   chungTuChiTietInfo.SoLuong);
                    }
                    else
                    {
                        TinhTonKho(ChungTuInfo.IdKho, chungTuChiTietInfo.IdSanPham, chungTuChiTietInfo.SoLuong, chungTuChiTietInfo.SoLuong);

                        if (IsKeToan) InsertChiTietChungTu(newInfo);
                    }
                }
            }
            catch (TinhTonException ex)
            {
                if (chungTuChiTietInfo != null)
                    throw new TinhTonException(ex.Message + "\nMã sản phẩm: " + chungTuChiTietInfo.MaSanPham +
                        "\nTên sản phẩm: " + chungTuChiTietInfo.TenSanPham);
                throw;
            }
        }

        private void InsertChiTietChungTu(TK info)
        {
            ChungTuChiTietBaseInfo chungTuChiTietInfo = ToBaseInfo(info);
            //if (chungTuChiTietInfo.IdChungTu == 0)
            chungTuChiTietInfo.IdChungTu = ChungTuInfo.IdChungTu;
            chungTuChiTietInfo.IdChungTuChiTiet = BusinessKeToanKhoProvider.InsertChiTietChungTu(info);
        }

        private void UpdateChiTietChungTu(TK info)
        {
            BusinessKeToanKhoProvider.UpdateChiTietChungTu(info);
        }

        private void DeleteChiTietChungTu(TK info)
        {
            BusinessKeToanKhoProvider.DeleteChiTietChungTu(info);
        }

        protected abstract void CreateTonKhoCalc(HangTonKhoInfo tonKhoInfo);

        public virtual bool TinhLaiChiTietChungTu()
        {
            return true;
        }

        protected virtual int TinhTonKhoTheoTrungTam()
        {
            return 0;
        }

        protected virtual int? TrangThaiHuy
        {
            get { return null; }
        }

        public override ChungTuBusinessBase Clone()
        {
            TK[] arrClonedChiTietChungTu = new TK[ListChiTietChungTu.Count];
            ListChiTietChungTu.CopyTo(arrClonedChiTietChungTu);

            ChungTuKeToanKhoBusinessBase<T, TK> tmpBusiness = (ChungTuKeToanKhoBusinessBase<T, TK>)base.Clone();
            tmpBusiness.ListChiTietChungTu.Clear();
            tmpBusiness.ListChiTietChungTu.AddRange(arrClonedChiTietChungTu);
            return tmpBusiness;
        }

        public virtual void CheckInvalidLock()
        {
            var lockOriginInfo = ChungTuOriginInfo as ChungTuBaseLockInfo;
            var lockInfo = ChungTuInfo as ChungTuBaseLockInfo;
            if (lockOriginInfo != null && lockInfo != null)
            {
                if (lockOriginInfo.LockId == 1)
                {
                    if (lockOriginInfo.LockAccount != lockInfo.LockAccount ||
                    lockOriginInfo.LockMachine != lockInfo.LockMachine)
                        throw new ManagedException(
                            String.Format(
                                "Chứng từ đang bị lock bởi người dùng {0} tại máy {1}, không thể thực hiện giao dịch.",
                                lockOriginInfo.LockAccount,
                                lockOriginInfo.LockMachine));

                    if (lockOriginInfo.ProcessId != lockInfo.ProcessId)
                    {
                        throw new ManagedException("Chứng từ đang bị lock bởi ứng dụng khác, không thể thực hiện giao dịch.");
                    }
                }
            }
        }
    }
}
