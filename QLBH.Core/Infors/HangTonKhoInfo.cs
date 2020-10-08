using System;
using System.Reflection;

namespace QLBH.Core.Infors
{
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    public class HangTonKhoInfo : ICloneable
    {
        public int IdTonKho { get; set; }

        public int IdKho { get; set; }

        public int IdSanPham { get; set; }

        public int SoLuong { get; set; }

        internal int DeltaSoLuong { get; set; }

        public string GhiChu { get; set; }

        public int TonAo { get; set; }

        internal int DeltaTonAo { get; set; }

        public int IdTrungTam { get; set; }

        public DateTime LastUpdateDate { get; set; }

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new HangTonKhoInfo
                       {
                           IdTonKho = IdTonKho,
                           IdKho = IdKho,
                           IdSanPham = IdSanPham,
                           SoLuong = SoLuong,
                           DeltaSoLuong = DeltaSoLuong,
                           GhiChu = GhiChu,
                           TonAo = TonAo,
                           DeltaTonAo = DeltaTonAo,
                           IdTrungTam = IdTrungTam,
                           LastUpdateDate = LastUpdateDate
                       };
        }

        #endregion
    }
}
