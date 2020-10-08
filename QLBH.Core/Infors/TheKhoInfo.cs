using System;
using System.Reflection;

namespace QLBH.Core.Infors
{
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    public class TheKhoInfo
    {
        public int IdTheKho { get; set; }

        public string SoChungTu { get; set; }
        /// <summary>
        /// Tính chi tiết đến hh:mm:ss ttt
        /// </summary>
        public DateTime NgayChungTu { get; set; }

        public int IdKho { get; set; }

        public int IdSanPham { get; set; }

        public int Nhap { get; set; }

        public int Xuat { get; set; }

        public int Ton { get; set; }

        public string ETime { get; set; }

        public int IdTrungTam { get; set; }
    }
}
