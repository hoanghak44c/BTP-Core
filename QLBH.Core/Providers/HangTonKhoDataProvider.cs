using QLBH.Core.DAO;
using QLBH.Core.Infors;

namespace QLBH.Core.Providers
{
    public class HangTonKhoDataProvider
    {
        internal static void Insert(HangTonKhoInfo info)
        {
            HangTonKhoDAO.Instance.Insert(info);
        }
        internal static int Update(HangTonKhoInfo info)
        {
            return HangTonKhoDAO.Instance.Update(info);
        }
        internal static int Update(HangTonKhoInfo info, int idChungTu)
        {
            return HangTonKhoDAO.Instance.Update(info, idChungTu);
        }
        public static HangTonKhoInfo GetHangTonKhoById(int idKho, int idSanPham)
        {
            return HangTonKhoDAO.Instance.GetHangTonKhoById(idKho, idSanPham, 0);
        }

        public static HangTonKhoInfo GetHangTonKhoById(int idKho, int idSanPham, int idTrungTam)
        {
            return HangTonKhoDAO.Instance.GetHangTonKhoById(idKho, idSanPham, idTrungTam);
        }

        public static int GetSoTonDauKy(int idKho, int idSanPham)
        {
            return HangTonKhoDAO.Instance.GetTonDauKy(idKho, idSanPham);
        }
    }
}
