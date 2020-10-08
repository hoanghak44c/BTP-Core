using System;
using System.Reflection;

namespace QLBH.Core.Infors
{
    public interface ILockInfo
    {
        string ProcessId { get; set; }
        string LockAccount { get; set; }
        string LockMachine { get; set; }
        int LockId { get; set; }
        DateTime LastUpdatedDate { get; set; }        
    }

    [Serializable]
    [ObfuscationAttribute(Feature = "properties renaming")]
    public class ChungTuBaseLockInfo : ChungTuBaseInfo, ILockInfo
    {
        public string ProcessId { get; set; }
        public string LockAccount { get; set; }
        public string LockMachine { get; set; }
        public int LockId { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }

    [Serializable]
    [ObfuscationAttribute(Feature = "properties renaming")]
    public class ChungTuBaseInfo : NotifyInfo
    {
        private int _idChungTu;
        public int IdChungTu
        {
            get { return _idChungTu; }
            set
            {
                if (_idChungTu != value) NotifyChange();
                _idChungTu = value;
            }
        }

        private string _soChungTu;
        public string SoChungTu
        {
            get { return _soChungTu; }
            set
            {
                if (_soChungTu != value) NotifyChange();
                _soChungTu = value;
            }
        }

        private DateTime _ngayLap;
        public DateTime NgayLap
        {
            get { return _ngayLap; }
            set
            {
                if (_ngayLap != value) NotifyChange();
                _ngayLap = value;
            }
        }

        private int _loaiChungTu;
        public int LoaiChungTu
        {
            get { return _loaiChungTu; }
            set
            {
                if (_loaiChungTu != value) NotifyChange();
                _loaiChungTu = value;
            }
        }

        private int _idTrungTamHachToan;
        public int IdTrungTamHachToan
        {
            get { return _idTrungTamHachToan; }
            set
            {
                if (_idTrungTamHachToan != value) NotifyChange();
                _idTrungTamHachToan = value;
            }
        }

        private int _idTrungTam;
        public int IdTrungTam
        {
            get { return _idTrungTam; }
            set
            {
                if (_idTrungTam != value) NotifyChange();
                _idTrungTam = value;
            }
        }


        private int _idKho;
        public int IdKho
        {
            get { return _idKho; }
            set
            {
                if (_idKho != value) NotifyChange();
                _idKho = value;
            }
        }
        
        private int _idNhanVien;
        public int IdNhanVien
        {
            get { return _idNhanVien; }
            set
            {
                if (_idNhanVien != value) NotifyChange();
                _idNhanVien = value;
            }
        }

        private int _dongBo_ORC;
        public int DongBo_ORC
        {
            get { return _dongBo_ORC; }
            set
            {
                if (_dongBo_ORC != value) NotifyChange();
                _dongBo_ORC = value;
            }
        }

        private int _trangThai;
        public int TrangThai
        {
            get { return _trangThai; }
            set
            {
                if(_trangThai != value) NotifyChange();
                _trangThai = value;
            }
        }

        private DateTime _ngayHoaDon;
        public DateTime NgayHoaDon
        {
            get { return _ngayHoaDon; }
            set
            {
                if (_ngayHoaDon != value) NotifyChange();
                _ngayHoaDon = value;
            }
        }
        public string NguoiTao { get; set; }

        public DateTime ThoiGianTao { get; set; }

        public string TenMayTao { get; set; }

        public string NguoiSua { get; set; }

        public DateTime ThoiGianSua { get; set; }

        public string TenMaySua { get; set; }
        
    }
}
