using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondIssueAmountParams
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
        public List<string> TypeList { get; set; }
        public bool UseSubType { get; set; }
        public string SubType { get; set; }
        public string Unit { get; set; }
    }
}
