using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Windows.Forms;
using QLBH.Core.Exceptions;
using QLBH.Core.Form;
using QLBH.Core.Infors;
using QLBH.Core.Providers;

namespace QLBH.Core.Printers.Godex
{
    public abstract class AutoGenSerial<T, T1> where T1 : class
    {
        private DateTime sysDate;

        private DMSanPhamInfo sanPhamInfo;

        protected T ChungTuChiTietInfo { get; private set; }

        private List<T1> lstChiTiet;

        protected string MaTrungTam { get; set; }

        protected int IdNhanVien { get; private set; }

        private string model;

        private readonly GodexPrinter smallPrinter, bigPrinter;

        protected AutoGenSerial(string maTrungTam, int idNhanVien)
        {
            sysDate = CommonProvider.Instance.GetSysDate();

            MaTrungTam = maTrungTam;

            IdNhanVien = idNhanVien;

            smallPrinter = new SmallSerialPrinter();

            bigPrinter = new BigSerialPrinter2();
        }

        protected virtual T1 CreateSerial(string maVach)
        {
            var serial = QLBHUtils.GetObject<T1>() as ChungTuChiTietHangHoaBaseInfo;

            if (serial == null) throw new ManagedException("Không đúng kiểu dữ liệu!");

            serial.IdChungTuChiTiet = (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IdChungTuChiTiet;

            serial.MaVach = maVach;

            serial.SoLuong = sanPhamInfo.TrungMaVach == 1 ? (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).SoLuong : 1;

            serial.IdSanPham = (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IdSanPham;

            serial.IsAuto = (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IsAuto && sanPhamInfo.TrungMaVach == 0;

            if (sanPhamInfo.TrungMaVach == 1)
            {
                for (var i = 0; i < serial.SoLuong; i++)
                {
                    smallPrinter.Add(new SmallSerial
                                         {
                                             Date = sysDate.ToString("yyyy/MM/dd"),
                                             Serial = maVach,
                                             Info = serial,
                                             Model = model,
                                             LsItemCode = sanPhamInfo.MaSanPham.Substring(sanPhamInfo.MaSanPham.LastIndexOf(".") + 1)
                                         });
                }
            } 
            else
            {
                bigPrinter.Add(new BigSerial
                                   {
                                       Date = sysDate.ToString("yyyy/MM/dd"),
                                       Serial = maVach,
                                       Info = serial,
                                       Model = model,
                                       LsItemCode = sanPhamInfo.MaSanPham.Substring(sanPhamInfo.MaSanPham.LastIndexOf(".") + 1),
                                       //TenSanPham = sanPhamInfo.TenSanPham.Replace(model, String.Empty).Replace("()", String.Empty).Trim()
                                   });
            }

            return serial as T1;
        }

        protected virtual bool MatchRemove(T1 match)
        {
            if (!(match is ChungTuChiTietHangHoaBaseInfo)) throw new ManagedException("Không đúng kiểu dữ liệu!");

            return (match as ChungTuChiTietHangHoaBaseInfo).IdChiTietHangHoa == 0 &&

                   (match as ChungTuChiTietHangHoaBaseInfo).IdSanPham ==
                   (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IdSanPham &&

                   (match as ChungTuChiTietHangHoaBaseInfo).IdChungTuChiTiet ==
                   (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IdChungTuChiTiet;
        }

        private bool MatchRemove(BaseSerial serial)
        {
            return MatchRemove(serial.Info as T1);
        }

        public void GenSerial(T chungTuChiTietInfo, List<T1> lstChiTiet)
        {
            ChungTuChiTietInfo = chungTuChiTietInfo;

            this.lstChiTiet = lstChiTiet;

            lstChiTiet.RemoveAll(MatchRemove);

            bigPrinter.RemoveAll(MatchRemove);

            smallPrinter.RemoveAll(MatchRemove);

            model = DmSanPhamProvider.Instance.GetModelById((ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IdSanPham);

            GenSerial();
        }

        private void GenSerial()
        {
            var result = new List<string>();

            var ngayThang = sysDate.ToString("yyMMdd");

            if (!(ChungTuChiTietInfo is ChungTuChiTietBaseInfo)) throw new ManagedException("Không đúng kiểu dữ liệu!");

            sanPhamInfo = DmSanPhamProvider.Instance.GetSanPhamById((ChungTuChiTietInfo as ChungTuChiTietBaseInfo).IdSanPham);

            var sanPham = sanPhamInfo.MaSanPham.Substring(sanPhamInfo.MaSanPham.LastIndexOf(".") + 1);

            if (sanPhamInfo.TrungMaVach == 1)
            {
                var s = String.Format("{0} {1} {2}", sanPham, MaTrungTam, ngayThang);

                var count = 0;

                while (!TblHangHoaChiTietDataProvider.IsNotInUniqueSerial(s) || lstChiTiet.Exists(delegate(T1 match)
                {
                    return match is ChungTuChiTietHangHoaBaseInfo &&

                        (match as ChungTuChiTietHangHoaBaseInfo).MaVach == s &&

                        (match as ChungTuChiTietHangHoaBaseInfo).IdSanPham != sanPhamInfo.IdSanPham;
                }))
                {
                    s = String.Format("{0} {1} {2}-{3}", sanPham, MaTrungTam, ngayThang, count);

                    count++;
                }

                lstChiTiet.Add(CreateSerial(s));

                frmProgress.Instance.Value += 1;

                return;
            }

            //kiểm tra xem nhân viên này đã dùng hết số serial tự sinh chưa
            //nếu chưa, nếu số lượng serial chưa dùng >= số lượng cần sinh thì không sinh thêm nữa, 
            //  ngược lại thì tính lại số lượng serial cần sinh
            
            bool isNotUnique;

            do
            {
                isNotUnique = false;

                var soLuongCanSinhThem = (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).SoLuong -

                    AutoGenSerialProvider.Instance.CountNotUsed(sanPham, MaTrungTam, ngayThang, IdNhanVien);

                if (soLuongCanSinhThem > 0)
                {
                    for (var i = 0; i < soLuongCanSinhThem; i++)
                    {
                        var success = false;

                        while (!success)
                            try
                            {
                                //tăng số nhảy
                                var seqNum = AutoGenSerialProvider.Instance.IncreaseCount(sanPham, MaTrungTam);

                                var serial = String.Format("{0} {1} {2} {3}", sanPham, MaTrungTam, ngayThang, seqNum);

                                if (!TblHangHoaChiTietDataProvider.IsUniqueSerial(serial) || lstChiTiet.Exists(delegate(T1 match)
                                {
                                    return match is ChungTuChiTietHangHoaBaseInfo &&
                                        
                                        (match as ChungTuChiTietHangHoaBaseInfo).MaVach == serial;
                                }))
                                {
                                    throw new ManagedException("Mã vạch này đã được sử dụng!");
                                }

                                //lưu vào bảng quản lý
                                AutoGenSerialProvider.Instance.SaveToManaged(sanPham, MaTrungTam, ngayThang, IdNhanVien, seqNum, 0);

                                frmProgress.Instance.Value += 1;

                                success = true;
                            }
                            catch (Exception)
                            {
                                success = false;
                            }
                    }

                    foreach (var genCodeInfo in AutoGenSerialProvider.Instance.GetListGenCodeInfo(sanPham, MaTrungTam, IdNhanVien))
                    {
                        result.Add(String.Format("{0} {1} {2} {3}", genCodeInfo.SanPham, genCodeInfo.TrungTam,
                                                 
                            genCodeInfo.NgayTao, genCodeInfo.GenCode));
                    }
                }
                else
                {
                    foreach (var genCodeInfo in AutoGenSerialProvider.Instance.GetListGenCodeInfoInNumRow(sanPham, MaTrungTam, IdNhanVien, 
                        
                        (ChungTuChiTietInfo as ChungTuChiTietBaseInfo).SoLuong))
                    {
                        result.Add(String.Format("{0} {1} {2} {3}", genCodeInfo.SanPham, genCodeInfo.TrungTam,
                                                 
                            genCodeInfo.NgayTao, genCodeInfo.GenCode));
                    }
                }

                foreach (var s in result)
                {
                    if (TblHangHoaChiTietDataProvider.IsUniqueSerial(s)) continue;

                    isNotUnique = true;

                    AutoGenSerialProvider.Instance.DeleteSerial(s);

                    frmProgress.Instance.Value -= 1;
                }

            } while (isNotUnique);

            foreach (var s in result)
            {
                lstChiTiet.Add(CreateSerial(s));
            }
        }        

        public void PrintSerial()
        {
            frmProgress.Instance.Caption = "In mã vạch";

            frmProgress.Instance.MaxValue = 3;

            frmProgress.Instance.Value = 0;

            frmProgress.Instance.DoWork(delegate
                                            {
                                                frmProgress.Instance.Description = "Đang kết nối máy in ...";

                                                var printed = false;

                                                while (!printed)
                                                {
                                                    try
                                                    {
                                                        printed = bigPrinter.Available();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        MessageBox.Show(ex.Message);
                                                    }

                                                    bigPrinter.PrinterPath = ConfigPrinter(bigPrinter.PrinterPath);
                                                }
                                                
                                                frmProgress.Instance.Value += 1;

                                                frmProgress.Instance.Description = "Đang chuẩn bị in ...";

                                                bigPrinter.Merge(smallPrinter);

                                                frmProgress.Instance.Value += 1;

                                                frmProgress.Instance.Description = "Bắt đầu in ...";

                                                bigPrinter.Print();

                                                frmProgress.Instance.Value += 1;

                                                frmProgress.Instance.Description = "Đã xong ...";

                                                frmProgress.Instance.Value = frmProgress.Instance.MaxValue;

                                                frmProgress.Instance.IsCompleted = true;
                                            });
        }

        private string ConfigPrinter(string printerPath)
        {
            var frmConfig = new frmThietLapMayInMaVach(printerPath);

            return frmConfig.ShowDialog() == DialogResult.OK ? frmConfig.ConfigValue : printerPath;
        }
    }

    public class SmallSerialPrinter : GodexPrinter
    {
        #region Overrides of GodexPrinter

        public SmallSerialPrinter()
        {
            NumOfSerialPerLabel = 2;

            Presentations = new[]
                                {
                                    "AT,15,0,7,16,1,0,0,1,{0}-{1}\r\nAT,215,0,7,16,1,0,0,1,{2}\r\nBP,15,20,1,5,3,0,0,{3}\r\nAT,15,45,7,20,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,15,77,7,16,1,0,0,1,{0}-{1}\r\nAT,215,77,7,16,1,0,0,1,{2}\r\nBP,15,97,1,5,3,0,0,{3}\r\nAT,15,122,7,20,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,295,0,7,16,1,0,0,1,{0}-{1}\r\nAT,495,0,7,16,1,0,0,1,{2}\r\nBP,295,20,1,5,3,0,0,{3}\r\nAT,295,45,7,20,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,295,77,7,16,1,0,0,1,{0}-{1}\r\nAT,495,77,7,16,1,0,0,1,{2}\r\nBP,295,97,1,5,3,0,0,{3}\r\nAT,295,122,7,20,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,575,0,7,16,1,0,0,1,{0}-{1}\r\nAT,775,0,7,16,1,0,0,1,{2}\r\nBP,575,20,1,5,3,0,0,{3}\r\nAT,575,45,7,20,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,575,77,7,16,1,0,0,1,{0}-{1}\r\nAT,775,77,7,16,1,0,0,1,{2}\r\nBP,575,97,1,5,3,0,0,{3}\r\nAT,575,122,7,20,1,0,0,1,{3}\r\n"
                                };
        }

        protected override object[] GetPerceptives(BaseSerial serial)
        {
            var smallSerial = serial as SmallSerial;

            return smallSerial == null ? null

                       : new object[] { smallSerial.Date, smallSerial.Model, smallSerial.LsItemCode, String.Format("{0} {1}", smallSerial.Serial, serial.Pos) };
        }

        #endregion

        protected internal override bool IsStrong(BaseSerial serial)
        {
            return !(serial is BigSerial) && (serial is SmallSerial);
        }

        protected internal override bool IsStrong(GodexPrinter printer)
        {
            return printer is SmallSerialPrinter;
        }

        protected internal override int Comp(BaseSerial serial1, BaseSerial serial2)
        {
            if (serial1.Printer.NumOfSerialPerLabel > serial2.Printer.NumOfSerialPerLabel)

                return 1;

            if (serial1.Printer.NumOfSerialPerLabel < serial2.Printer.NumOfSerialPerLabel)

                return -1;

            return 0;
        }
    }

    public class BigSerialPrinter : GodexPrinter
    {
        #region Overrides of GodexPrinter

        public BigSerialPrinter()
        {
            NumOfSerialPerLabel = 1;

            Presentations = new[]
                                {
                                    "AT,15,0,7,16,1,0,0,1,{0}\r\nAT,215,0,7,16,1,0,0,1,{1}\r\nBP,15,20,1,15,65,0,0,{2}\r\nAT,15,85,7,20,1,0,0,1,{2}\r\nAT,15,110,7,16,1,0,0,1,{3}\r\nAT,15,130,7,16,1,E,0,1,{4}\r\n"
                                    ,
                                    "AT,295,0,7,16,1,0,0,1,{0}\r\nAT,495,0,7,16,1,0,0,1,{1}\r\nBP,295,20,1,15,65,0,0,{2}\r\nAT,295,85,7,20,1,0,0,1,{2}\r\nAT,295,110,7,16,1,0,0,1,{3}\r\nAT,295,130,7,16,1,E,0,1,{4}\r\n"
                                    ,
                                    "AT,575,0,7,16,1,0,0,1,{0}\r\nAT,775,0,7,16,1,0,0,1,{1}\r\nBP,575,20,1,15,65,0,0,{2}\r\nAT,575,85,7,20,1,0,0,1,{2}\r\nAT,575,110,7,16,1,0,0,1,{3}\r\nAT,575,130,7,16,1,E,0,1,{4}\r\n"
                                };
        }

        protected override object[] GetPerceptives(BaseSerial serial)
        {
            var bigSerial = serial as BigSerial;

            return bigSerial == null ? null

                       : new object[] { bigSerial.Date, bigSerial.LsItemCode, bigSerial.Serial, bigSerial.Model, bigSerial.TenSanPham };
        }

        #endregion

        protected internal override bool IsStrong(BaseSerial serial)
        {
            return serial is BigSerial;
        }

        protected internal override bool IsStrong(GodexPrinter printer)
        {
            return printer is BigSerialPrinter;
        }

        protected internal override int Comp(BaseSerial serial1, BaseSerial serial2)
        {
            if (serial1.Printer.NumOfSerialPerLabel < serial2.Printer.NumOfSerialPerLabel)

                return 1;

            if (serial1.Printer.NumOfSerialPerLabel > serial2.Printer.NumOfSerialPerLabel)

                return -1;

            return 0;
        }
    }

    /// <summary>
    /// Không có tên sản phẩm
    /// </summary>
    public class BigSerialPrinter2 : GodexPrinter
    {
        #region Overrides of GodexPrinter

        public BigSerialPrinter2()
        {
            NumOfSerialPerLabel = 1;

            Presentations = new[]
                                {
                                    "AT,15,0,7,16,1,0,0,1,{0}\r\nAT,215,0,7,16,1,0,0,1,{1}\r\nBP,15,20,1,15,65,0,0,{2}\r\nAT,15,85,7,20,1,0,0,1,{2}\r\nAT,15,110,7,16,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,295,0,7,16,1,0,0,1,{0}\r\nAT,495,0,7,16,1,0,0,1,{1}\r\nBP,295,20,1,15,65,0,0,{2}\r\nAT,295,85,7,20,1,0,0,1,{2}\r\nAT,295,110,7,16,1,0,0,1,{3}\r\n"
                                    ,
                                    "AT,575,0,7,16,1,0,0,1,{0}\r\nAT,775,0,7,16,1,0,0,1,{1}\r\nBP,575,20,1,15,65,0,0,{2}\r\nAT,575,85,7,20,1,0,0,1,{2}\r\nAT,575,110,7,16,1,0,0,1,{3}\r\n"
                                };
        }

        protected override object[] GetPerceptives(BaseSerial serial)
        {
            var bigSerial = serial as BigSerial;

            return bigSerial == null ? null

                       : new object[] { bigSerial.Date, bigSerial.LsItemCode, bigSerial.Serial, bigSerial.Model };
        }

        #endregion

        protected internal override bool IsStrong(BaseSerial serial)
        {
            return serial is BigSerial;
        }

        protected internal override bool IsStrong(GodexPrinter printer)
        {
            return printer is BigSerialPrinter2 || printer is BigSerialPrinter;
        }

        protected internal override int Comp(BaseSerial serial1, BaseSerial serial2)
        {
            if (serial1.Printer.NumOfSerialPerLabel < serial2.Printer.NumOfSerialPerLabel)

                return 1;

            if (serial1.Printer.NumOfSerialPerLabel > serial2.Printer.NumOfSerialPerLabel)

                return -1;

            return 0;
        }
    }

    public abstract class GodexPrinter : List<BaseSerial>
    {
        public string PrinterPath = @"\\127.0.0.1\GODEXEZ";

        private string buffer;

        protected internal const int LABEL_PER_FEED = 3;

        protected internal string[] Presentations;

        protected internal int CurrentLabel = -1, NextLabel = 0;

        public int NumOfSerialPerLabel = 0;

        private bool ordered = true;

        private int getNextLabel()
        {
            return (CurrentLabel + 1)%LABEL_PER_FEED;
        }

        protected internal abstract bool IsStrong(BaseSerial serial);

        protected internal abstract bool IsStrong(GodexPrinter printer);

        protected internal abstract int Comp(BaseSerial serial1, BaseSerial serial2);

        public new void Add(BaseSerial serial)
        {
            //add
            if (serial.Printer == null)
            {
                if(!IsStrong(serial)) throw new ManagedException("Không đúng kiểu mã vạch");

                serial.Printer = this;

                serial.Pos = serial.Printer.Count % serial.Printer.Presentations.Length;

                base.Add(serial);
            } 
            //merge
            else {

                if(Count == 0)
                {
                    serial.Pos = 0;
                } 
                else
                {
                    if (this[Count - 1].Printer.IsStrong(serial))
                    {
                        serial.Pos = (this[Count - 1].Pos + 1) % this[Count - 1].Printer.Presentations.Length;
                    } 
                    else
                    {
                        serial.Pos = getNextLabel() * serial.Printer.NumOfSerialPerLabel;

                        ordered = false;
                    }
                }

                base.Add(serial);
            }

            CurrentLabel = serial.Label = serial.Pos / serial.Printer.NumOfSerialPerLabel;

        }

        public void Merge(GodexPrinter godexPrinter)
        {
            if(godexPrinter.Count == 0) return;

            foreach (var serial in godexPrinter)
            {
                Add(serial);
            }

            if (ordered) return;

            Sort(Comp);

            //reindex

            for (var i = 0; i < Count; i++)
            {
                if (i == 0)
                {
                    this[i].Pos = 0;
                }
                else
                {
                    if (this[i - 1].Printer.IsStrong(this[i]))
                    {
                        this[i].Pos = (this[i - 1].Pos + 1) % this[i - 1].Printer.Presentations.Length;
                    }
                    else
                    {
                        this[i].Pos = getNextLabel() * this[i].Printer.NumOfSerialPerLabel;
                    }
                }
                CurrentLabel = this[i].Label = this[i].Pos / this[i].Printer.NumOfSerialPerLabel;
            }

            ordered = true;
        }

        private static string GetFormated(BaseSerial serial)
        {
            return String.Format(serial.Printer.Presentations[serial.Pos], serial.Printer.GetPerceptives(serial));
        }

        protected abstract object[] GetPerceptives(BaseSerial serial);

        public void Print()
        {
            if (Count == 0) return;

            var content = GetFormated(this[0]);

            for (var i = 1; i < Count; i++)
            {
                if (this[i - 1].Label > this[i].Label)
                {
                    Feed(ref content);
                }

                content += GetFormated(this[i]);
            }

            Feed(ref content);

            PrintA();

            Clear();
        }

        private void PrintA()
        {
            var bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());

            var fileName = Path.GetRandomFileName();

            var filePath = String.Format("{0}\\{1}", Application.StartupPath, fileName);

            var f = File.Create(filePath);

            f.Write(bytes, 0, bytes.Length);

            f.Close();

            File.Copy(filePath, PrinterPath);

            File.Delete(filePath);

            buffer = String.Empty;
        }

        private void Feed(ref string content)
        {
            if (String.IsNullOrEmpty(content)) return;

            buffer += String.Format("^H15\r\n^S2\r\n^Q25,3,6\r\n^W106\r\n^L\r\n^R0\r\n{0}\r\nE\r\n^E24\r\n", content);

            content = String.Empty;
        }

        private string getLoop(string content, int roundNum)
        {
            return String.Format(LOOP, content, roundNum);
        }

        #region "Content"

        private const string LOOP = "{0}\r\n~P{1}\r\n";

        #endregion

        internal bool Available()
        {
            // Set management scope
            var scope = new ManagementScope(@"\root\cimv2");

            scope.Connect();

            // Select Printers from WMI Object Collections
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

            foreach (ManagementObject printer in searcher.Get())
            {
                var printerName = printer["Name"].ToString().ToUpper();

                if (!PrinterPath.Contains(printerName)) continue;

                if (printer["WorkOffline"].ToString().ToLower().Equals("false"))
                {
                    return true;
                }
            }

            throw new ManagedException("Máy in mã vạch chưa sẵn sàng! Đề nghị bạn thiết lập lại máy in");
        }

    }
}