using System.Collections.Generic;
using QLBH.Core.DAO;
using QLBH.Core.Infors;

namespace QLBH.Core.Providers
{
    public class DefinedTransactionProvider
    {

        private static DefinedTransactionProvider instance;

        private DefinedTransactionProvider()
        {
        }

        public static DefinedTransactionProvider Instance
        {
            get
            {
                if (instance == null) instance = new DefinedTransactionProvider();
                return instance;
            }
        }

        public List<DefinedTransactionInfo> GetListOutDefinedTransaction()
        {
            return DefinedTransactionDAO.Instance.GetListOutDefinedTransaction();
        }

        public List<DefinedTransactionInfo> GetListInDefinedTransaction()
        {
            return DefinedTransactionDAO.Instance.GetListInDefinedTransaction();
        }
    }
}