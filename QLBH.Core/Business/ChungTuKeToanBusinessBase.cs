using System.Collections.Generic;
using System.Diagnostics;
using QLBH.Core.Business.Calculations;
using QLBH.Core.DAO;
using QLBH.Core.Exceptions;
using QLBH.Core.Infors;

namespace QLBH.Core.Business
{
    /// <summary>
    /// Lập chứng từ không liên quan đến tồn kho. 
    /// Cách thực hiện: Xóa hết rồi insert lại
    /// </summary>
    /// <typeparam name="T">Kiểu chứng từ</typeparam>
    /// <typeparam name="TK">Kiểu chứng từ chi tiết</typeparam>
    public abstract class ChungTuKeToanBusinessBase<T, TK> : ChungTuBusinessBase where T : class
    {
        protected T ChungTuOriginInfo;

        /// <summary>
        /// Thông tin về chứng từ
        /// </summary>
        public T ChungTu
        {
            get { return ChungTuInfo as T; }
            set { ChungTuInfo = value as ChungTuBaseInfo; }
        }

        /// <summary>
        /// Các chi tiết của chứng từ
        /// </summary>
        public List<TK> ListChiTietChungTu;

        protected internal IBusinessBaseProvider<T, TK> BusinessProvider;

        protected internal bool IsKeToan, IsThuKho; 

        protected IBussinessKeToanProvider<T, TK> BussinessKeToanProvider
        {
            get { return BusinessProvider as IBussinessKeToanProvider<T, TK>; }
            set { BusinessProvider = value; }
        }

        protected ChungTuKeToanBusinessBase()
        {
            CreateBusinessProvider();

            IsKeToan = false;

            if (BusinessProvider is IBussinessKeToanKhoProvider<T, TK>)
            {
                IsKeToan = true;
            }
            
            ListChiTietChungTu = new List<TK>();
        }

        protected ChungTuKeToanBusinessBase(T chungTuBaseInfo)
        {
            ChungTu = chungTuBaseInfo;
            CreateBusinessProvider();

            IsKeToan = false;

            if (BusinessProvider is IBussinessKeToanKhoProvider<T, TK>)
            {
                IsKeToan = true;
            }

            if (ChungTuInfo.IdChungTu > 0)
            {
                ListChiTietChungTu = BusinessProvider.GetListChiTietChungTuByIdChungTu(ChungTuInfo.IdChungTu);
            }
            else
            {
                ListChiTietChungTu = new List<TK>();                
            }
        }

        protected internal bool IsEditMode
        {
            get
            {
                if (ChungTuInfo.IdChungTu != 0)
                {
                    ChungTuOriginInfo = TblChungTuDAO.Instance.GetChungTuByIdChungTu<T>(ChungTuInfo.IdChungTu);

                    return ChungTuOriginInfo != null;
                }
                
                ChungTuInfo.IdChungTu = 0;

                return false;
            }
        }

        /// <summary>
        /// Thực hiện xóa toàn bộ rồi insert mới
        /// </summary>
        protected internal override void SaveChungTuInstance()
        {
            if (IsEditMode)
            {
                BussinessKeToanProvider.DeleteChiTietChungTu(ChungTuInfo.IdChungTu);
                BussinessKeToanProvider.UpdateChungTu(ChungTu);
            }
            else
            {
                if(TblChungTuDAO.Instance.ChungTuExistBySoChungTu(ChungTuInfo.SoChungTu))
                {
                    throw new ManagedException("Số chứng từ " + ChungTuInfo.SoChungTu + " đã được sử dụng. Đề nghị hãy chọn số khác!");
                }
                ChungTuInfo.IdChungTu = BussinessKeToanProvider.InsertChungTu(ChungTu);
            }

            SaveChiTietChungTu();
        }

        protected internal override void DeleteChungTuInstance()
        {
            BussinessKeToanProvider.DeleteChiTietChungTu(ChungTuInfo.IdChungTu);
            BussinessKeToanProvider.DeleteChungTu(ChungTuInfo.IdChungTu);
        }

        protected MapChiTietBaseInfo ToBaseInfo<TT>(TT item)
        {
            return item as MapChiTietBaseInfo;
        }

        protected ChungTuChiTietBaseInfo ToBaseInfo(TK item)
        {
            return item as ChungTuChiTietBaseInfo;
        }

        private void SaveChiTietChungTu()
        {
            for (int i = 0; i < ListChiTietChungTu.Count; i++)
            {

                ToBaseInfo(ListChiTietChungTu[i]).IdChungTu = ChungTuInfo.IdChungTu;

                ToBaseInfo(ListChiTietChungTu[i]).IdChungTuChiTiet = BussinessKeToanProvider.InsertChiTietChungTu(ListChiTietChungTu[i]);

            }
        }

        protected abstract void CreateBusinessProvider();
    }
}
