using System;
using System.Diagnostics;
using QLBH.Core.Data;
using QLBH.Core.Infors;

namespace QLBH.Core.DAO
{
    public class TheKhoDAO : BaseDAO
    {
        private static TheKhoDAO instance;
        private TheKhoDAO()
        {
            //CRUDTableName = Declare.TableNamespace.tbl_The_Kho;
            //UseCaching = true;
        }

        public static TheKhoDAO Instance
        {
            get
            {
                if (instance == null) instance = new TheKhoDAO();
                return instance;
            }
        }

        internal void WriteLog(TheKhoInfo info)
        {
            Debug.Print(
                String.Format(
                    "{0}, IdTheKho:{1}, SoChungTu:{2}, NgayChungTu:{3}, IdKho:{4}, IdSanPham:{5}, Nhap:{6}, Xuat:{7}, IdTrungTam:{8}",
                    spTheKhoWriteLog, info.IdTheKho, info.SoChungTu, info.NgayChungTu.ToString("dd/MM/yyyy hh:mm:ss tt"), 
                    info.IdKho, info.IdSanPham, info.Nhap, info.Xuat, info.IdTrungTam));

            ExecuteCommand(spTheKhoWriteLog, info.IdTheKho, info.SoChungTu, info.NgayChungTu, info.IdKho, 
                info.IdSanPham, info.Nhap, info.Xuat, info.IdTrungTam);
        }

        internal void Delete(int idKho, int idSanPham, string soPhieu, int idTrungTam)
        {
            ExecuteCommand(spTheKhoDelete, idKho, idSanPham, soPhieu, idTrungTam);
        }

        public TheKhoInfo GetTheKhoBy(int idKho, int idSanPham, string soChungTu, int idTrungTam)
        {
            return GetObjectCommand<TheKhoInfo>(spTheKhoGetBy, idKho, idSanPham, soChungTu, idTrungTam);
        }

        public int GetSoTonTruoc(int idKho, int idSanPham, string eTime)
        {
            ExecuteCommand(spTheKhoTonTruoc, idKho, idSanPham, eTime);
            return Convert.ToInt32(Parameters["p_TonTruoc"].Value.ToString());
        }
    }
}
