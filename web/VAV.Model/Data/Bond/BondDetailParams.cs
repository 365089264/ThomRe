using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondDetailParams
    {
        public string Type { get; set; }
        public string TypeValue { get; set; }
        public string SubType { get; set; }
        public string SubTypeValue { get; set; }
        public bool UseSubType {get; set;}
        public bool IsParent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ColumnList { get; set; }
        public string ItemList { get; set; }
        public string Term { get; set; }
        public string OrderBy { get; set; }
        public int StartPage { get; set; }
        public int PageSize { get; set; }
    }
}
