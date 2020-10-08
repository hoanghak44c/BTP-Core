namespace QLBH.Core.Printers.Godex
{
    //0:date 2015/01/01
    //1:model 0G23456789B
    //2:last segment item code 52223
    //3:serial 163045831239BHF152223345
    public class SmallSerial : BaseSerial
    {
        public string Date { get; set; }

        public string Model { get; set; }

        public string LsItemCode { get; set; }
    }
}