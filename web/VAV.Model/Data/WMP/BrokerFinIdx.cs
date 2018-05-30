using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.WMP
{
    public class BrokerFinIdx
    {
        public long INNER_CODE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string PROFIT { get; set; }
        public string RPT_SRC { get; set; }
        public string END_ASSET_VAL { get; set; }
        public string NET_GR { get; set; }
        public string UNIT_ACCUM_NET_GR { get; set; }   
    }
}
