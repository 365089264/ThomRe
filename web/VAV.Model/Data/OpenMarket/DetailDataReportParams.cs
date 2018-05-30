using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.OpenMarket
{
    public class DetailDataReportParams
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Type { get; set; }
        public bool GroupIsUsed { get; set; }
        public string Group { get; set; }
        public string Unit { get; set; }
        public bool IncludeExpired { get; set; }
    }
}
