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
    /// Nghiệp vụ lập chứng từ kho:
    /// Do thủ kho thực hiện (dựa trên chứng từ kế toán)
    /// </summary>
    /// <typeparam name="T">Kiểu chứng từ</typeparam>
    /// <typeparam name="TK">Kiểu chứng từ chi tiết</typeparam>
    /// <typeparam name="TL">Kiểu chứng từ chi tiết hàng hóa</typeparam>
    public abstract class ChungTuKhoBusinessBase<T, TK, TL> : ChungTuKeToanKhoBusinessBase<T, TK> where T : class where TK : class where TL : class
    {
        protected TonChiTietCalc TonMaVachCalc;

        /// <summary>
        /// Các chi tiết hàng hóa của chứng từ
        /// </summary>
        public List<TL> ListChiTietHangHoa;

        protected ChungTuKhoBusinessBase()
        {
            ListChiTietHangHoa = new List<TL>();
            
            IsThuKho = true;
        }

        protected ChungTuKhoBusinessBase(T chungTuBaseInfo)
            : base(chungTuBaseInfo)
        {
            if (ChungTuInfo.IdChungTu > 0)
                ListChiTietHangHoa = BusinessKhoProvider.GetListChiTietHangHoaByIdChungTu(ChungTuInfo.IdChungTu);
            else
                ListChiTietHangHoa = new List<TL>();

            IsThuKho = true;
        }

        protected static ChungTuChiTietHangHoaBaseInfo ToBaseInfo(TL item)
        {
            return item as ChungTuChiTietHangHoaBaseInfo;
        }

        /// <summary>
        /// Kiểm tra có cập nhật mới lưu.
        /// </summary>
        protected internal override void SaveChungTuInstance()
        {            
            TL[] arrBackupChiTietHangHoa = new TL[ListChiTietHangHoa.Count];
            ListChiTietHangHoa.CopyTo(arrBackupChiTietHangHoa);

            try
            {
                if (ListChiTietHangHoa.Count == 0) throw new ManagedException("Không có line hàng!");

                string sMaSanPham = String.Empty;

                foreach (var k in ListChiTietChungTu)
                {
                    if (String.IsNullOrEmpty(ToBaseInfo(k).MaSanPham))
                    {
                        throw new ManagedException("Không xác định được mã sản phẩm!");
                    }
                    if (!(sMaSanPham + ",").Contains("," + ToBaseInfo(k).MaSanPham + ","))
                        sMaSanPham += "," + ToBaseInfo(k).MaSanPham;
                }

                List<DMSanPhamInfo> listSanPham = DmSanPhamProvider.Instance.GetListSanPhamByCode(sMaSanPham);

                if (listSanPham.Count == 0) throw new ManagedException("Không xác định được mã sản phẩm!");

                string message;

                ListChiTietHangHoa.Sort(
                    delegate(TL cthh1, TL cthh2)
                        {
                            return ToBaseInfo(cthh1).IdSanPham.CompareTo(ToBaseInfo(cthh2).IdSanPham);
                        });

                for (int i = 0; i < ListChiTietHangHoa.Count; i++)
                {
                    message = String.Empty;
                    if (ListChiTietChungTu.Find(delegate(TK match)
                    {
                        message = ToBaseInfo(match).MaSanPham;
                        return Conjunction(match, ListChiTietHangHoa[i]);
                        //return (match as ChungTuChiTietBaseInfo).IdSanPham == ToBaseInfo(ListChiTietHangHoa[i]).IdSanPham;
                    }) == null)
                    {
                        DMSanPhamInfo item = listSanPham.Find(delegate(DMSanPhamInfo match)
                                                                  {
                                                                      return match.IdSanPham ==
                                                                             ToBaseInfo(ListChiTietHangHoa[i]).IdSanPham;
                                                                  });
                        if (item == null || item.ChietKhau == 0)
                            throw new ManagedException(String.Format("Sản phẩm '{0}' không tìm thấy mã vạch '{1}' tương ứng.", message, ToBaseInfo(ListChiTietHangHoa[i]).MaVach));
                        //ListChiTietHangHoa.RemoveAt(i);
                        //i--;
                    }
                }

                for (int i = 0; i < ListChiTietChungTu.Count; i++)
                {
                    if(ToBaseInfo(ListChiTietChungTu[i]).IdSanPham == 0)
                    {
                        throw new ManagedException(String.Format("Mã sản phẩm '{0}' chưa tồn tại trên hệ thống!",ToBaseInfo(ListChiTietChungTu[i]).MaSanPham));
                    }

                    DMSanPhamInfo item = listSanPham.Find(delegate(DMSanPhamInfo match)
                    {
                        return match.IdSanPham ==
                               ToBaseInfo(ListChiTietChungTu[i]).IdSanPham;
                    });

                    if (item.ChietKhau == 1) continue;

                    message = String.Empty;
                    if (ListChiTietHangHoa.Find(delegate(TL match)
                    {
                        message = ToBaseInfo(match).MaVach;
                        return Conjunction(ListChiTietChungTu[i], match);
                        //return (match as ChungTuChiTietBaseInfo).IdSanPham == ToBaseInfo(ListChiTietChungTu[i]).IdSanPham;
                    }) == null)
                    {
                        throw new ManagedException(String.Format("Mã vạch '{0}' không tìm thấy sản phẩm '{1}' tương ứng.", message, ToBaseInfo(ListChiTietChungTu[i]).MaSanPham));
                        //ListChiTietChungTu.RemoveAt(i);
                        //i--;
                    }
                }

                int slChiTietChungTu = 0;
                foreach (TK k in ListChiTietChungTu)
                {
                    DMSanPhamInfo item = listSanPham.Find(delegate(DMSanPhamInfo match)
                    {
                        return match.IdSanPham ==
                               ToBaseInfo(k).IdSanPham;
                    });

                    if(item.ChietKhau == 1) continue;

                    List<TL> listTmp = ListChiTietHangHoa.FindAll(delegate(TL match)
                    {
                        return Conjunction(k, match);
                    });
                    slChiTietChungTu = 0;
                    foreach (TL l in listTmp)
                    {
                        slChiTietChungTu += ToBaseInfo(l).SoLuong;
                    }
                    if (slChiTietChungTu != ToBaseInfo(k).SoLuong)
                    {
                        throw new ManagedException(
                            String.Format("Số lượng hàng '{0}' trong phiếu không khớp với số lượng mã vạch.",
                                          ToBaseInfo(k).MaSanPham)
                            );
                    }
                }

                slChiTietChungTu = 0;
                foreach (TK k in ListChiTietChungTu)
                {
                    DMSanPhamInfo item = listSanPham.Find(delegate(DMSanPhamInfo match)
                    {
                        return match.IdSanPham ==
                               ToBaseInfo(k).IdSanPham;
                    });

                    if (item.ChietKhau == 1) continue;

                    slChiTietChungTu += ToBaseInfo(k).SoLuong;
                }

                int slChiTietMaVach = 0;
                foreach (TL l in ListChiTietHangHoa)
                {
                    DMSanPhamInfo item = listSanPham.Find(delegate(DMSanPhamInfo match)
                    {
                        return match.IdSanPham ==
                               ToBaseInfo(l).IdSanPham;
                    });

                    if (item.ChietKhau == 1) continue;

                    slChiTietMaVach += ToBaseInfo(l).SoLuong;
                }

                if (slChiTietChungTu != slChiTietMaVach)
                {
                    throw new ManagedException("Số lượng hàng trong phiếu không khớp với số lượng mã vạch.");
                }

                slChiTietMaVach = 0;
                foreach (TL l in ListChiTietHangHoa)
                {
                    DMSanPhamInfo item = listSanPham.Find(delegate(DMSanPhamInfo match)
                    {
                        return match.IdSanPham ==
                               ToBaseInfo(l).IdSanPham;
                    });

                    if (item.ChietKhau == 1 || item.TrungMaVach == 1) continue;

                    slChiTietMaVach = 0;
                    ListChiTietHangHoa.ForEach(
                        delegate(TL match)
                            {
                                if (ToBaseInfo(l).MaVach == ToBaseInfo(match).MaVach)
                                {
                                    slChiTietMaVach += ToBaseInfo(match).SoLuong;
                                }
                            });
                    if(item.TrungMaVach == 0 && slChiTietMaVach > 1)
                    {
                        throw new ManagedException(String.Format("Mã vạch {0} đã bị nhập trùng.", ToBaseInfo(l).MaVach));
                    }
                }

                base.SaveChungTuInstance();

                List<TL> listOrigin = BusinessKhoProvider.GetListChiTietHangHoaByIdChungTu(ChungTuInfo.IdChungTu);

                SaveChiTietHangHoa(listOrigin, ListChiTietHangHoa);

            }
            catch (Exception ex)
            {
                ListChiTietHangHoa.Clear();
                ListChiTietHangHoa.AddRange(arrBackupChiTietHangHoa);
                var description = String.Empty;
                ListChiTietHangHoa.ForEach(delegate(TL ctct)
                {
                    description += ToBaseInfo(ctct).IdSanPham + " " + ToBaseInfo(ctct).MaVach + " " + ToBaseInfo(ctct).SoLuong + "\r\n";
                });

                description += ex.ToString();

                EventLogProvider.Instance.WriteOfflineLog(String.Format("{0}\r\n{1}", ChungTuInfo.SoChungTu, description), "Loi luu chung tu");
                throw;
            }
        }

        protected void SaveChiTietHangHoa(List<TL> listOrigin, List<TL> listNew)
        {
            ChungTuChiTietHangHoaBaseInfo chiTietHangHoaInfo = null;
            try
            {
                foreach (TL originInfo in listOrigin)
                {
                    chiTietHangHoaInfo = ToBaseInfo(originInfo);

                    if (chiTietHangHoaInfo == null) throw new Exception("Không đúng kiểu " + typeof(ChungTuChiTietHangHoaBaseInfo));

                    if (!listNew.Exists(delegate(TL match)
                    {
                        return ToBaseInfo(match).IdChiTietHangHoa == chiTietHangHoaInfo.IdChiTietHangHoa &&
                            ToBaseInfo(match).IdChungTuChiTiet == chiTietHangHoaInfo.IdChungTuChiTiet;
                    }))
                    {
                        DeleteChiTietHangHoa(originInfo);
                        //+ ton cu
                        //if(TrangThaiOrigin != CancelState)
                        TinhTonKho(ChungTuInfo.IdKho, chiTietHangHoaInfo.IdSanPham, -chiTietHangHoaInfo.SoLuong,
                                   chiTietHangHoaInfo.MaVach);
                    }
                }

                foreach (TL newInfo in listNew)
                {
                    chiTietHangHoaInfo = ToBaseInfo(newInfo);

                    if (chiTietHangHoaInfo == null) throw new Exception("Không đúng kiểu " + typeof(ChungTuChiTietHangHoaBaseInfo));
                    
                    if (!chiTietHangHoaInfo.IsOrigin)
                    {
                        TL originInfo =
                            listOrigin.Find(
                                delegate(TL match)
                                    {
                                        return ToBaseInfo(match).IdChiTietHangHoa == chiTietHangHoaInfo.IdChiTietHangHoa &&
                                               ToBaseInfo(match).IdChungTuChiTiet == chiTietHangHoaInfo.IdChungTuChiTiet;
                                    });

                        if (originInfo != null)
                        {
                            
                            UpdateChiTietHangHoa(newInfo);
                            //+ ton cu - ton moi
                            //TinhTonKho(ChungTuInfo.IdKho, chiTietHangHoaInfo.IdSanPham,
                            //           TrangThaiOrigin != CancelState
                            //               ? chiTietHangHoaInfo.SoLuong - ToBaseInfo(originInfo).SoLuong
                            //               : chiTietHangHoaInfo.SoLuong,
                            //           chiTietHangHoaInfo.MaVach);
                            TinhTonKho(ChungTuInfo.IdKho, chiTietHangHoaInfo.IdSanPham,
                                           chiTietHangHoaInfo.SoLuong - ToBaseInfo(originInfo).SoLuong,
                                       chiTietHangHoaInfo.MaVach);
                        }
                        else
                        {
                            //if (chiTietHangHoaInfo.SoLuong > 0)
                            //{
                                //- ton moi;
                                int idChiTietHangHoa = TinhTonKho(ChungTuInfo.IdKho, chiTietHangHoaInfo.IdSanPham,
                                           chiTietHangHoaInfo.SoLuong,
                                           chiTietHangHoaInfo.MaVach);
                                //need to set idchitiethanghoa before insert
                                if (chiTietHangHoaInfo.IdChiTietHangHoa != idChiTietHangHoa && idChiTietHangHoa != 0)
                                {
                                    chiTietHangHoaInfo.IdChiTietHangHoa = idChiTietHangHoa;
                                    //if not discountable - TuanLM bo ra ngoai 20/04/2013
                                    //InsertChiTietHangHoa(newInfo);                                   
                                }
                                //if not discountable
                                InsertChiTietHangHoa(newInfo);

                            //}
                        }
                    }
                }
            }
            catch (TinhTonException ex)
            {
                if(chiTietHangHoaInfo != null)
                {
                    string message = String.Empty;

                    ChungTuChiTietBaseInfo chungTuChiTietInfo = ToBaseInfo(ListChiTietChungTu.Find(delegate(TK match)
                    { return ToBaseInfo(match).IdSanPham == chiTietHangHoaInfo.IdSanPham; }));

                    if (chungTuChiTietInfo != null)
                    {
                        message = "\nMã sản phẩm: " + chungTuChiTietInfo.MaSanPham;
                        message += "\nTên sản phẩm: " + chungTuChiTietInfo.TenSanPham;
                    }
                    
                    message += "\nIdSanPham: " + chiTietHangHoaInfo.IdSanPham;

                    throw new TinhTonException(ex.Message + message);
                }
                throw;
            }
        }

        private int TinhTonKho(int idKho, int idSanPham, int soLuong, string maVach)
        {
            var dmSanPhamInfo = DmSanPhamProvider.Instance.GetSanPhamById(idSanPham);

            if (String.IsNullOrEmpty(maVach))
            {
                throw new ManagedException(String.Format("Sản phẩm '{0}' không có mã vạch!", dmSanPhamInfo.MaSanPham));
            }

            var hangHoaInfo =
                TblHangHoaChiTietDataProvider.GetHangHoaChiTietByMaVach(idKho, idSanPham, maVach, IdTrungTam) ??
                new HangHoa_ChiTietInfo { IdKho = idKho, IdSanPham = idSanPham, MaVach = maVach, IdTrungTam = IdTrungTam };

            if (soLuong > 0 && BusinessType == BusinessType.REAL_IN)
            {
                if (dmSanPhamInfo.TrungMaVach == 0)
                {
                    if (!TblHangHoaChiTietDataProvider.IsUniqueSerial(hangHoaInfo.MaVach))

                        throw new ManagedException(String.Format("Mã vạch '{0}' đã tồn tại trong hệ thống!", hangHoaInfo.MaVach));

                    if (soLuong > 1)

                        throw new ManagedException(String.Format("Mã vạch '{0}' không được phép trùng, số lượng không được lớn hơn 1!", hangHoaInfo.MaVach));                    
                } 
                else
                {
                    if (!TblHangHoaChiTietDataProvider.IsNotInUniqueSerial(hangHoaInfo.MaVach))

                        throw new ManagedException(String.Format("Mã vạch '{0}' đã tồn tại trong hệ thống!", hangHoaInfo.MaVach));                    
                }
                //if(Chit)
            }

            if (dmSanPhamInfo == null || dmSanPhamInfo.ChietKhau != 0) return hangHoaInfo.IdChiTiet;

            if (soLuong == 0) return hangHoaInfo.IdChiTiet; //MINHPN cần nhập với số lượng = 0

            var soTon = hangHoaInfo.SoLuong;

            //if (hangHoaInfo.IdChiTiet == 0) //có thể trong hệ thống đã có mã vạch này 
            {
                //ChungTuInfo.LoaiChungTu == Convert.ToInt32(TransactionType.NHAP_PO) ||
                    //ChungTuInfo.LoaiChungTu == Convert.ToInt32(TransactionType.NHAP_NOIBO)

                if (ThietLapBaoHanhHang())
                {
                    hangHoaInfo.BaoHanhHangTu = ChungTuInfo.NgayLap;
                    hangHoaInfo.BaoHanhHangDen = hangHoaInfo.BaoHanhHangTu.AddMonths(dmSanPhamInfo.BaoHanhHang);
                }
                else
                {
                    hangHoaInfo.BaoHanhHangTu = TblHangHoaChiTietDataProvider.GetNgayBaoHanhByMaVach(maVach, idSanPham);
                    hangHoaInfo.BaoHanhHangDen = hangHoaInfo.BaoHanhHangTu.AddMonths(dmSanPhamInfo.BaoHanhHang);
                }

                if (BusinessType == BusinessType.REAL_IN)
                {
                    if (!ThietLapTuoiTon(hangHoaInfo))
                        TblHangHoaChiTietDataProvider.UpdateTuoiTonBaseInfo(maVach, ChungTuInfo.IdChungTu, hangHoaInfo);

                    if(hangHoaInfo.IdPhieuNhap == 0 && hangHoaInfo.NgayNhapKho_DK == DateTime.MinValue)
                    {
                        TblHangHoaChiTietDataProvider.PendingXacDinhNguonGoc(maVach, ChungTuInfo.IdChungTu, ChungTuInfo.SoChungTu, idSanPham);
                    }
                }
            }

            CreateTonMaVachCalc(hangHoaInfo);

            TonMaVachCalc.Calculate(soLuong);

            if (hangHoaInfo.SoLuong < 0)
            {
                var message = String.Format("Số lượng mã vạch '{0}' {1}, không thể thực hiện được.", maVach,
                                               soTon > 0 ? "chỉ còn " + soTon : "đã hết");
                message += "\nIdSanPham: " + idSanPham;
                message += "\nIdKho: " + idKho;
                message += "\nIdTrungTam: " + IdTrungTam;
                throw new TinhTonException(message);
            }

            if (dmSanPhamInfo.TrungMaVach == 0 && BusinessType == BusinessType.REAL_IN)
            {
                if (soLuong > 0 && hangHoaInfo.SoLuong > 1)
                {
                    var message = String.Format("Mã vạch '{0}' đã tồn tại.", maVach);
                    message += "\nIdSanPham: " + idSanPham;
                    message += "\nIdKho: " + idKho;
                    message += "\nIdTrungTam: " + IdTrungTam;
                    throw new TinhTonException(message);
                }
            }

            return hangHoaInfo.IdChiTiet;

        }

        public virtual bool Conjunction(TK chiTietChungTuInfo, TL chiTietHangHoaInfo)
        {
            return ToBaseInfo(chiTietChungTuInfo).IdSanPham == ToBaseInfo(chiTietHangHoaInfo).IdSanPham;
        }

        private void InsertChiTietHangHoa(TL info)
        {
            var chiTietHangHoaInfo = ToBaseInfo(info);
            //if (chiTietHangHoaInfo.IdChungTuChiTiet == 0)
            //{
                //chiTietHangHoaInfo.IdChungTuChiTiet =
                //    (ListChiTietChungTu.Find(delegate(TK match)
                //                                 {
                //                                     return ToBaseInfo(match).IdSanPham == chiTietHangHoaInfo.IdSanPham;
                //                                 }) as ChungTuChiTietBaseInfo).IdChungTuChiTiet;

                chiTietHangHoaInfo.IdChungTuChiTiet =
                    (ListChiTietChungTu.Find(delegate(TK match)
                                                 {
                                                     return Conjunction(match, info);
                                                 }) as ChungTuChiTietBaseInfo).IdChungTuChiTiet;
            //}
            BusinessKhoProvider.InsertChiTietHangHoa(info);

            if (chiTietHangHoaInfo.IsAuto && chiTietHangHoaInfo.SoLuong > 0) 

                AutoGenSerialProvider.Instance.UseSerial(chiTietHangHoaInfo.MaVach);
        }

        private void DeleteChiTietHangHoa(TL info)
        {
            BusinessKhoProvider.DeleteChiTietHangHoa(info);
        }

        protected void UpdateChiTietHangHoa(TL info)
        {
            BusinessKhoProvider.UpdateChiTietHangHoa(info);
        }

        protected internal override void Prepare()
        {
            base.Prepare();
            
            ListChiTietHangHoa = BusinessKhoProvider.GetListChiTietHangHoaByIdChungTu(ChungTuInfo.IdChungTu);
        }

        protected internal override void DeleteChungTuInstance()
        {
            SaveChiTietHangHoa(ListChiTietHangHoa, new List<TL>());

            base.DeleteChungTuInstance();
        }

        protected internal override void CancelChungTuInstance()
        {
            base.CancelChungTuInstance();

            ChungTuChiTietHangHoaBaseInfo chiTietHangHoaInfo = null;
            
            foreach (TL originInfo in ListChiTietHangHoa)
            {
                chiTietHangHoaInfo = ToBaseInfo(originInfo);

                TinhTonKho(ChungTuInfo.IdKho, chiTietHangHoaInfo.IdSanPham, -chiTietHangHoaInfo.SoLuong,
                           chiTietHangHoaInfo.MaVach);
            }

        }

        protected IBussinessKhoProvider<T, TK, TL> BusinessKhoProvider
        {
            get
            {
                return BusinessProvider as IBussinessKhoProvider<T, TK, TL>;
            }
            set { BusinessProvider = value; }
        }

        /// <summary>
        /// Trả về danh sách mã vạch của một loại sản phẩm
        /// </summary>
        /// <param name="idSanPham"></param>
        /// <returns></returns>
        public List<TL> GetListChiTietHangHoaByIdSanPham(int idSanPham)
        {
            return ListChiTietHangHoa.FindAll(delegate(TL match)
              {
                  return ToBaseInfo(match).IdSanPham == idSanPham;
              });
        }

        /// <summary>
        /// Trả về danh sách mã vạch của một loại sản phẩm
        /// </summary>
        /// <param name="idSanPham"></param>
        /// <returns></returns>
        public List<TL> GetListChiTietHangHoaByConjunction(Predicate<TL> matchEx)
        {
            return ListChiTietHangHoa.FindAll(matchEx);
        }

        /// <summary>
        /// Lưu lại một danh sách mã vạch vừa được quét
        /// </summary>
        /// <param name="listChiTietHangHoa"></param>
        public void MergeChiTietHangHoa(List<TL> listChiTietHangHoa)
        {
            if(listChiTietHangHoa == null || listChiTietHangHoa.Count == 0) return;

            int idSanPham = ToBaseInfo(listChiTietHangHoa[0]).IdSanPham;

            if (!listChiTietHangHoa.TrueForAll(delegate(TL match)
                                                   {
                                                       return ToBaseInfo(match).IdSanPham == idSanPham;
                                                   }))
            {
                throw new Exception("Danh sách này không cùng loại sản phẩm");
            }

            ListChiTietHangHoa.RemoveAll(delegate(TL match)
                                             {
                                                 return ToBaseInfo(match).IdSanPham == idSanPham;
                                             });
            
            ListChiTietHangHoa.AddRange(listChiTietHangHoa);

        }

        /// <summary>
        /// Lưu lại một danh sách mã vạch vừa được quét
        /// </summary>
        /// <param name="listChiTietHangHoa"></param>
        public void MergeChiTietHangHoa(List<TL> listChiTietHangHoa, Predicate<TL> matchEx)
        {
            if (listChiTietHangHoa == null || listChiTietHangHoa.Count == 0) return;

            int idSanPham = ToBaseInfo(listChiTietHangHoa[0]).IdSanPham;

            if (!listChiTietHangHoa.TrueForAll(delegate(TL match)
            {
                return ToBaseInfo(match).IdSanPham == idSanPham;
            }))
            {
                throw new Exception("Danh sách này không cùng loại sản phẩm");
            }

            ListChiTietHangHoa.RemoveAll(matchEx);

            ListChiTietHangHoa.AddRange(listChiTietHangHoa);

        }

        protected abstract void CreateTonMaVachCalc(HangHoa_ChiTietInfo tonChiTietInfo);

        protected virtual bool ThietLapBaoHanhHang()
        {
            return false;
        }

        protected virtual bool ThietLapTuoiTon(HangHoa_ChiTietInfo hangHoaInfo)
        {
            return false;
        }

        public override ChungTuBusinessBase Clone()
        {
            TL[] arrClonedChiTietHangHoa = new TL[ListChiTietHangHoa.Count];
            ListChiTietHangHoa.CopyTo(arrClonedChiTietHangHoa);
            ChungTuKhoBusinessBase<T, TK, TL> tmpBusiness = (ChungTuKhoBusinessBase<T, TK, TL>)base.Clone();
            tmpBusiness.ListChiTietHangHoa.Clear();
            tmpBusiness.ListChiTietHangHoa.AddRange(arrClonedChiTietHangHoa);
            return tmpBusiness;
        }
    }
}
