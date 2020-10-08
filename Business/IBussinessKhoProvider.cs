using System.Collections.Generic;

namespace QLBH.Core.Business
{
    /// <summary>
    /// Ap dung voi chung tu chi tiet hang hoa
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TL"></typeparam>
    public interface IBussinessKhoProvider<T, TK, TL> : IBusinessBaseProvider<T, TK>
    {
        /// <summary>
        /// Chú ý khi thực thi hàm này phải set origin cho danh sách trả về
        /// </summary>
        /// <param name="idChungTu"></param>
        /// <returns></returns>
        List<TL> GetListChiTietHangHoaByIdChungTu(int idChungTu);
        void DeleteChiTietHangHoa(TL chiTietHangHoaInfo);
        void InsertChiTietHangHoa(TL chiTietHangHoaInfo);
        void UpdateChiTietHangHoa(TL chiTietHangHoaInfo);
    }

}
