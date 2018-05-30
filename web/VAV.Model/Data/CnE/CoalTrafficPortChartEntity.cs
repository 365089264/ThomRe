using System.Collections.Generic;

namespace VAV.Model.Data.CnE
{
    public class SingleLineChartEntity
    {
        public SingleLineChartEntity()
        {
            this.data = new List<object>();
        }
        public string name { get; set; }
        public List<object> data { get; set; }

    }
    public class CoalTrafficPortChartEntity
    {
        public string end_date { get; set; }
        public string PortName { get; set; }
        public decimal A004 { get; set; }
        public decimal A005 { get; set; }
        public decimal A007 { get; set; }
       
    }
}
