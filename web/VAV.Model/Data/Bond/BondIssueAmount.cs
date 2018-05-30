using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VAV.Model.Data.Bond
{
    public class BondIssueAmount : BaseModel
    {

        public int IsParent { get; set; }
        public string Type { get; set; }
        public string TypeEn { get; set; }
        public string TypeCn { get; set; }
        public string SubType { get; set; }
       
        public string SubTypeEn { get; set; }
        public string SubTypeCn { get; set; }
        public double? LowestIssueRate { get; set; }
        public double? HighestIssueRate { get; set; }
        public int Issues { get; set; }
        public decimal IssuesPercent { get; set; }
        public decimal? IssuesAmount { get; set; }
        public decimal? IssuesAmountPercent { get; set; }
        public int Order { get; set; }
        public int SubOrder { get; set; }
        
        
        public string TypeName 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(typeName))
                    typeName = Culture == "zh-CN" ? TypeCn : TypeEn;
                return typeName;
            } 
            set
            { 
                typeName = value; 
            } 
        }
        public string SubTypName { get { return Culture == "zh-CN" ? SubTypeCn : SubTypeEn; } }
        private string typeName;

    }
}
