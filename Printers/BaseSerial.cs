using System;
using QLBH.Core.Infors;
using QLBH.Core.Printers.Godex;

namespace QLBH.Core.Printers
{
    public abstract class BaseSerial
    {
        public int Pos { get; set; }

        public int Label { get; set; }

        public string Owner { get; set; }

        public string Serial { get; set; }

        public int Sequence { get; set; }

        public GodexPrinter Printer { get; set; }

        public ChungTuChiTietHangHoaBaseInfo Info { get; set; }
    }
}