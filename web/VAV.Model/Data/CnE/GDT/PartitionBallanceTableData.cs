using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VAV.Model.Data.CnE.GDT
{
    public class PartitionBallanceTableData
    {
        public string cn1 { get; set; }
        public string cn2 { get; set; }
        public string cn3 { get; set; }//usd3
        public string usd3 { get; set; }
        public string usd2 { get; set; }
        public string usd1 { get; set; }
        public string SupplyDemand_CN { get; set; }
        public string SupplyDemand_EN { get; set; }
        public string SupplyDemand { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? SupplyDemand_CN : SupplyDemand_EN; } }
    }
}
