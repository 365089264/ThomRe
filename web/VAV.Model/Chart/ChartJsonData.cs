namespace VAV.Model.Chart
{
    public class ChartJsonData : ChartData
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public object[] SeriesData { get; set; }


        public object ToJson()
        {
            var jsonObj = new
            {
                chart = new { type = ChartType },
                title = new { text = Title },
                subtitle = new { text = SubTitle },
                xAxis = new { categories = ColumnCategories },
                yAxis = new { min = 0, title = new { text = YText } },
                series = SeriesData
            };
            return jsonObj;
        }
    }
}
