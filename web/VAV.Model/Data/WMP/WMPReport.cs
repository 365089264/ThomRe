using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.WMP
{
    public class WMPBankReport
    {
        public Nullable<long> SEQ { get; set; }
        public Nullable<System.DateTime> CTIME { get; set; }
        public Nullable<System.DateTime> MTIME { get; set; }
        public Nullable<decimal> ISVALID { get; set; }
        public Nullable<long> GENIUS_UID { get; set; }
        public long RPT_ID { get; set; }
        public string RPT_TITLE { get; set; }
        public string RPT_TYPE { get; set; }
        public string WRITEDATE { get; set; }
        public string RPT_SRC { get; set; }
        public string ACCE_ROUTE { get; set; }
        public Nullable<int> FIN_PRD_TYPE { get; set; }
    }
}
