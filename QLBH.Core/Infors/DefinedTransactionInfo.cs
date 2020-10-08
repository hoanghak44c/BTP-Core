using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QLBH.Core.Infors
{
    [Serializable]
    [Obfuscation(Feature = "properties renaming")]
    public class DefinedTransactionInfo
    {
        public int TransNum { get; set; }
        public string TransType { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public int Virtual { get; set; }
        public int Real { get; set; }
        public string TransName { get; set; }
        public int R_Receipt { get; set; }
        public int Source { get; set; }
        public string SourceName { get; set; }
    }
}
