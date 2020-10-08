using System;
using System.Reflection;

namespace QLBH.Core.Infors
{
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    public class ChungTuChiTietHangHoaBaseInfo : MapChiTietBaseInfo
    {
        private int _idChiTietHangHoa;

        //private int idChiTietChungTu;

        private string _maVach;

        public int IdChiTietHangHoa
        {
            get { return _idChiTietHangHoa; }
            set
            {
                if (_idChiTietHangHoa != value) NotifyChange();
                _idChiTietHangHoa = value;
            }
        }

        [Obsolete("Khong tiep tuc dung thuoc tinh nay nua. Hay chuyen sang dung IdChungTuChiTiet.")]
        public int IdChiTietChungTu
        {
            get { return IdChungTuChiTiet; }
            set
            {
                IdChungTuChiTiet = value;
            }
        }

        public string MaVach
        {
            get { return _maVach; }
            set
            {
                if (_maVach != value) NotifyChange();
                _maVach = value;
            }
        }
    }
}
