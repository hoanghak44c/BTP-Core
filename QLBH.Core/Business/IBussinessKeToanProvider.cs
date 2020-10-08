using System;
using System.Collections.Generic;

namespace QLBH.Core.Business
{

    public interface IBusinessBaseProvider<T, TK>
    {
        void UpdateChungTu(T chungTu);
        /// <summary>
        /// Chú ý: khi implement hàm insert này, cần set lại 
        /// các thuộc tính idChungTu, soChungTu cho object chungTu
        /// trước khi return.
        /// </summary>
        /// <param name="chungTu">Chứng từ info</param>
        /// <returns>ID chứng từ</returns>
        int InsertChungTu(T chungTu);
        void DeleteChungTu(int idChungTu);
        /// <summary>
        /// Chú ý: khi thực thi hàm này phải set origin cho danh sách trả về.
        /// </summary>
        /// <param name="idChungTu"></param>
        /// <returns></returns>
        List<TK> GetListChiTietChungTuByIdChungTu(int idChungTu);        
    }
    
    public interface IBussinessKeToanProvider<T, TK> : IBusinessBaseProvider<T, TK>
    {
        int InsertChiTietChungTu(TK chiTietChungTu);
        void DeleteChiTietChungTu(int idChungTu);
    }

    public interface IBussinessKeToanKhoProvider<T, TK> : IBusinessBaseProvider<T, TK>
    {
        int InsertChiTietChungTu(TK chiTietChungTu);
        /// <summary>
        /// Chú ý khi implement hàm này là chỉ xóa theo IdChungTuChiTiet
        /// </summary>
        void DeleteChiTietChungTu(TK chiTietChungTu);
        void UpdateChiTietChungTu(TK chiTietChungTu);        
    }
}
