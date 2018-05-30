using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    public class GlobalValueParams
    {
        public string Type_CBankBill { get; set; }
        public string Type_CBankBillIE { get; set; }
        public string Type_Repo { get; set; }
        public string Type_RepoIE { get; set; }
        public string Type_ReverseRepo { get; set; }
        public string Type_ReverseRepoIE { get; set; }
        public string Type_FMD { get; set; }
        public string Type_FMDIE { get; set; }
        public string Type_MLF { get; set; }
        public string Type_MLFIE { get; set; }
        public string Report_Expire { get; set; }
        public string Direction_injection { get; set; }
        public string Direction_withdrawal { get; set; }
        public string CurrentContext { get; set; }
    }
}
