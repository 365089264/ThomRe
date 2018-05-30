using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAV.Model.Chart;

namespace VAV.Model.Data.Bond
{
    public class BondRatingHist
    {
        public long BOND_ID { get; set; }
        public string BOND_CODE { get; set; }
        public DateTime RATE_DATE { get; set; }
        public string RATE_TYPE { get; set; }
        public string RATE_CLS { get; set; }
        public string RATE_ORG { get; set; }
        public string RATE { get; set; }
        public string RATE_PROS { get; set; }
        public string RATE_TITLE { get; set; }
        public long RATE_ID { get; set; }
        public bool ContainFile { get; set; }
    }
}
