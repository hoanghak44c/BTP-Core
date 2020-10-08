using System;
using System.Collections.Generic;
using QLBH.Core.DAO;

namespace QLBH.Core.Providers
{
    public class AutoGenSerialProvider
    {
        private static AutoGenSerialProvider instance;

        private AutoGenSerialProvider()
        {
        }

        public static AutoGenSerialProvider Instance
        {
            get { return instance ?? (instance = new AutoGenSerialProvider()); }
        }

        public string GetSerial(string sanPham, string maTrungTam, string ngayThang, int idNhanVien)
        {
            //kiểm tra xem nhân viên này đã dùng hết mã serial tự sinh chưa
            var isUsing = AutoGenSerialDao.Instance.IsUsing(sanPham, maTrungTam, ngayThang, idNhanVien);
            //nếu chưa thì trả về mã serial đã sinh
            //nếu hết rồi thì sinh serial mới
            return String.Empty;
        }

        public int CountNotUsed(string sanPham, string maTrungTam, string ngayThang, int idNhanVien)
        {
            return AutoGenSerialDao.Instance.CountNotUsed(sanPham, maTrungTam, ngayThang, idNhanVien);
        }

        public int IncreaseCount(string sanPham, string maTrungTam)
        {
            return AutoGenSerialDao.Instance.IncreaseCount(sanPham, maTrungTam);
        }

        public void SaveToManaged(string sanPham, string maTrungTam, string ngayThang, int idNhanVien, int seqNum, int isUsed)
        {
            AutoGenSerialDao.Instance.SaveToManaged(sanPham, maTrungTam, ngayThang, idNhanVien, seqNum, isUsed);
        }

        public List<AutoGenSerialDao.GenCodeInfo> GetListGenCodeInfo(string sanPham, string maTrungTam, int idNhanVien)
        {
            return AutoGenSerialDao.Instance.GetListGenCodeInfo(sanPham, maTrungTam, idNhanVien);
        }

        public IEnumerable<AutoGenSerialDao.GenCodeInfo> GetListGenCodeInfoInNumRow(string sanPham, string maTrungTam, int idNhanVien, int numRow)
        {
            return AutoGenSerialDao.Instance.GetListGenCodeInfoInNumRow(sanPham, maTrungTam, idNhanVien, numRow);
        }

        public void DeleteSerial(string s)
        {
            AutoGenSerialDao.Instance.DeleteSerial(s);
        }

        public void UseSerial(string s)
        {
            AutoGenSerialDao.Instance.UseSerial(s);
        }
    }
}