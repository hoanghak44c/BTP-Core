using System;
using System.Collections.Generic;
using QLBH.Core.Business;
using QLBH.Core.DAO;
using QLBH.Core.Data;
using QLBH.Core.Infors;

namespace QLBH.Core.Providers
{
    public class BusinessProviderBase
    {
        public List<T> Origin<T>(List<T> list)
        {
            foreach (var item in list)
            {
                (item as NotifyInfo).SetOrigin();
            }

            return list;
        }

        public T Origin<T>(T item)
        {
            (item as NotifyInfo).SetOrigin();

            return item;
        }

        public T GetChungTuBySoChungTu<T>(string soChungTu)
        {
            return TblChungTuDAO.Instance.GetChungTuBySoChungTu<T>(soChungTu);
        }

        public bool DuplicateSoChungTu(string soChungTu, int idChungTu)
        {
            return TblChungTuDAO.Instance.DuplicateSoChungTu(soChungTu, idChungTu);
        }

        public T GetChungTuRefBySoChungTu<T>(string soChungTu)
        {
            return TblChungTuDAO.Instance.GetChungTuRefBySoChungTu<T>(soChungTu);
        }

        public bool ChiTietHangHoaExists(string soChungTu)
        {
            return TblChungTuDAO.Instance.ChiTietHangHoaExists(soChungTu);
        }

        public bool CheckSameAccountBookByIdNhanVienAndIdKho(int idNhanVien, int idKho)
        {
            return TblChungTuDAO.Instance.CheckSameAccountBookByIdNhanVienAndIdKho(idNhanVien, idKho);
        }

        public bool ChungTuSyncPushORC<T, TK>(ChungTuKeToanKhoBusinessBase<T, TK> business)
            where T : class
            where TK : class
        {
            ChungTuBaseInfo chungTuBaseInfo = business.ChungTu as ChungTuBaseInfo;
            try
            {

                if (chungTuBaseInfo == null) throw new ArgumentException("Không phải kiểu ChungTuBaseInfo.");

                TblChungTuDAO.Instance.SyncChungTu(chungTuBaseInfo.IdChungTu);

                return true;
            }
            catch (Exception ex)
            {
                //khong throw exception, co the thuc hien sync lai chung tu nay sau.
                if (chungTuBaseInfo != null)
                    EventLogProvider.Instance.WriteLog(ex +
                                                       String.Format("\nSoGiaoDich:{0}\nIdChungTu:{1}", chungTuBaseInfo.SoChungTu, chungTuBaseInfo.IdChungTu), "Synch Push ORC");
                else
                    EventLogProvider.Instance.WriteLog(ex.ToString(), "Synch Push ORC");

                throw ex;
            }
        }
    }
}
