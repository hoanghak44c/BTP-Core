using QLBH.Core.DAO;
using QLBH.Core.Infors;

namespace QLBH.Core.Providers
{
    public class TheKhoDataProvider
    {
        internal static void WriteLog(TheKhoInfo info)
        {
            TheKhoDAO.Instance.WriteLog(info);
        }
        internal static void Delete(int idKho,int idSanPham,string soPhieu, int idTrungTam)
        {
            TheKhoDAO.Instance.Delete(idKho,idSanPham,soPhieu, idTrungTam);
        }

        public static TheKhoInfo GetTheKhoBy(int idKho, int idSanPham, string soChungTu, int idTrungTam)
        {
            return TheKhoDAO.Instance.GetTheKhoBy(idKho, idSanPham, soChungTu, idTrungTam);
        }

        public static void GetSoTonTruoc(TheKhoInfo theKhoInfo)
        {
            theKhoInfo.Ton = TheKhoDAO.Instance.GetSoTonTruoc(theKhoInfo.IdKho, theKhoInfo.IdSanPham, theKhoInfo.ETime);
        }
    }
}
