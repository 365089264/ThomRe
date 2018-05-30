using VAV.Model.Chart;

namespace VAV.Model.Data.WMP
{
    public class IssueTrendData
    {
        public JsonTable TopTable { get; set; }
        public object Chart { get; private set; }
        public JsonTable BottomTable { get; private set; }

        public IssueTrendData(JsonTable topTable, object chart, JsonTable bottomTable)
        {
            TopTable = topTable;
            Chart = chart;
            BottomTable = bottomTable;
        }
    }
}
