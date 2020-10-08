using System;
using QLBH.Core.DAO;

namespace QLBH.Core.Providers
{
    public class NetProvider
    {

        private static NetProvider instance;

        private NetProvider()
        {
        }

        public static NetProvider Instance
        {
            get { return instance ?? (instance = new NetProvider()); }
        }

        public void UpdatePortNumber(int portNum)
        {
            NetDAO.Instance.UpdatePortNumber(portNum);
        }

        public bool ExistPortNumber(int port)
        {
            return NetDAO.Instance.ExistPortNumber(port);
        }
    }
}