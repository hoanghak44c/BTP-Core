using System.Data;

namespace QLBH.Core.Data
{
    internal interface IHydratable
    {
        int KeyID { set;get;}
        void Fill(IDataReader dr);
    }
}
