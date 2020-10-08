namespace QLBH.Core.Interfaces
{
    public interface IBaseModel
    {
        void RegisterSidCode(int sid, string scode);
        bool CheckPrivileged(string scode);
        int GetSidNumber(string scode);
    }
}