using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondIssueParams
    {
        public DateTime? StartDate { get; set; }
        public string BondType { get; set; }
        public string Term { get; set; }
        public string IsFloat { get; set; }
        public string Rating { get; set; }
    }
}