using System;
using System.Collections.Generic;
using QLBH.Core.Data;
using QLBH.Core.Infors;

namespace QLBH.Core.DAO
{
    public class DefinedTransactionDAO : BaseDAO
    {

        private static DefinedTransactionDAO instance;

        private DefinedTransactionDAO()
        {
            UseCaching = true;
            CRUDTableName = "tbl_defined_transactions";
        }

        public static DefinedTransactionDAO Instance
        {
            get
            {
                if (instance == null) instance = new DefinedTransactionDAO();
                return instance;
            }
        }

        public List<DefinedTransactionInfo> GetListOutDefinedTransaction()
        {
            return GetListAll<DefinedTransactionInfo>("sp_GetListOutDefinedTransation", CRUDTableName);
        }

        public List<DefinedTransactionInfo> GetListInDefinedTransaction()
        {
            return GetListAll<DefinedTransactionInfo>("sp_GetListInDefinedTransation", CRUDTableName);
        }
    }
}