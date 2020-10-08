using System;
using System.Reflection;

namespace QLBH.Core.Infors
{
    [Serializable]
    [ObfuscationAttribute(Feature = "properties renaming")]
    public class MapChiTietBaseInfo : NotifyInfo
    {
        private int _idSanPham;
        
        private int _idChungTuChiTiet;

        private int _soLuong;

        public int IdSanPham
        {
            get { return _idSanPham; }
            set
            {
                if (_idSanPham != value) NotifyChange();
                _idSanPham = value;
            }
        }

        public int IdChungTuChiTiet
        {
            get { return _idChungTuChiTiet; }
            set
            {
                if (_idChungTuChiTiet != value) NotifyChange();
                _idChungTuChiTiet = value;
            }
        }

        public int SoLuong
        {
            get { return _soLuong; }
            set
            {
                if (_soLuong != value) NotifyChange();
                _soLuong = value;
            }
        }

        public bool IsAuto { get; set; }
    }
    
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    public class ChungTuChiTietBaseInfo : MapChiTietBaseInfo
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
        /// <summary>
        /// Duoc dung de dua ra cac thong bao den nguoi su dung
        /// </summary>
        public string TenSanPham { get; set; }

        public string MaSanPham { get; set; }

        public string TrangThai { get; set; }

    }
}
