using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension.Model
{
    public class FanYaStockData
    {
        //品种代码|仓库名称|库存量|库容量|库存总量|品种名称 
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string WareHouseName { get; set; }
        public string StockNum { get; set; }
        public string StockCapacity { get; set; }
        public string TotalStock { get; set; }
        public string ProductName { get; set; }
        public string StockTime { get; set; }
    }
}
