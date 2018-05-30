using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class DimSumBondSummary : BaseModel
    {
        /// <summary>
        /// The type is used to identify each row
        /// </summary>
        public string Type { get; set; }

        public string TypeName { get; set; }

        public double? EndBalance { get; set; }
        public double? InitialBalance { get; set; }
        
        public int? Issues { get; set; }
        public double IssuesPercent { get; set; }
        
        public double? IssuesAmount { get; set; }
        public double? IssuesAmountPercent { get; set; }
        
        public int? MaturityBonds { get; set; }
        public double? MaturityBondsPercent { get; set; }

        public double? MaturityAmount { get; set; }
        public double? MaturityAmountPercent { get; set; }

        public int Order { get; set; }

        public bool IsParent { get; set; }

        public DateTime CurrentDate { get; set; }
    }
}
