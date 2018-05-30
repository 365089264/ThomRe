using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondIssueAmountExport
    {
        public string TypeName { get; set; }
        public double? LowestIssueRate { get; set; }
        public double? HighestIssueRate { get; set; }
        public int Issues { get; set; }
        public decimal IssuesPercent { get; set; }
        public decimal? IssuesAmount { get; set; }
        public decimal? IssuesAmountPercent { get; set; }
        public string Row_level { get; set; }
    }
}
