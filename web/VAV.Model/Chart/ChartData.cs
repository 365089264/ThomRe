namespace VAV.Model.Chart
{
    public class ChartData
    {
        public string ChartType { get; set; }

        public string[] ColumnCategories { get; set; }

        public SeriesData[] ColumnSeriesData { get; set; }

        public PieSectionData[] PieSeriesData { get; set; }

        public string YText { get; set; }

        public int Decimal { get; set; }
    }
}
