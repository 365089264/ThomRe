using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAV.Model.Chart;

namespace VAV.Model.Data.Bond
{
    public class DimSumBondData
    {
        public List<Dictionary<string, string>> TopTable { get; set; }
        public ChartData Chart { get; private set; }
        public JsonTable BottomTable { get; private set; }

        public DimSumBondData(List<Dictionary<string, string>> topTable, ChartData chart, JsonTable bottomTable)
        {
            this.TopTable = topTable;
            this.Chart = chart;
            this.BottomTable = bottomTable;
        }
    }
}
