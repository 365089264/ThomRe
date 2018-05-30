using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension.Model
{
    public class FanYaExponentData
    {
        //品种代码|指数|昨收|涨跌|涨跌幅|品种名称 
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string Exponent { get; set; }
        public string YesSettle { get; set; }
        public string UpsAndDown { get; set; }
        public string RiseAndFall { get; set; }
        public string ProductName { get; set; }
        public string StockTime { get; set; }
    }
}
