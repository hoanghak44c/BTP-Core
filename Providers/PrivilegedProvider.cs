using System.Collections.Generic;

namespace QLBH.Core.Providers
{
    public class PrivilegedProvider
    {

        private static PrivilegedProvider instance;

        private PrivilegedProvider()
        {
        }

        public static PrivilegedProvider Instance
        {
            get { return instance ?? (instance = new PrivilegedProvider()); }
        }

        public List<string> CurrentPrivileges;

        public bool IsSupperUser { get; set; }
    }
}