using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.WMP
{
    public class BrokerFee
    {
        public long INNER_CODE { get; set; }
        public long FEE_TYPE { get; set; }
        public long SRL_NO { get; set; }
        public string UNT_NV { get; set; }
        public string ANNU_YLD { get; set; }
        public string FEE { get; set; }
        public string MNY { get; set; }
        public string TERM { get; set; }
        public string REMARK { get; set; }
    }
}
