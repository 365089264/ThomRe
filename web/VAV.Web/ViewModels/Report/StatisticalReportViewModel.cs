using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using VAV.Model.Data;
using VAV.Web.Localization;

namespace VAV.Web.ViewModels.Report
{
    /// <summary>
    /// Statistical Report Model
    /// </summary>
    public class StatisticalReportViewModel : BasicReportViewModel
    {
        /// <summary>
        /// Gets or sets the top chart.
        /// </summary>
        /// <value>The top chart.</value>
        public ChartViewModel TopChart { get; set; }

        public ChartModel TopChartModel { get; set; }

        /// <summary>
        /// Gets or sets the buttom chart.
        /// </summary>
        /// <value>The buttom chart.</value>
        public ChartViewModel BottomChart { get; set; }

        public ChartModel BottomChartModel { get; set; }

        public StandardReport SumGrid { get; set; }

        public StandardReport DetailGrid { get; set; }

        public IEnumerable<string> StatisticalAspects { get; private set; }

        private List<object> _chartOnly { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticalReportViewModel"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public StatisticalReportViewModel(int id, StandardReport detailStandardReport)
            : base(id, detailStandardReport)
        {
            TopChart = new ChartViewModel { ChartName = "Top Chart" + id };
            BottomChart = new ChartViewModel { ChartName = "Bottom Chart" + id };
            SumGrid = new StandardReport(id);
            DetailGrid = detailStandardReport;
        }

        public StatisticalReportViewModel(int id, StandardReport sumStandardReport, StandardReport detailStandardReport)
            : base(id, detailStandardReport)
        {
            TopChart = new ChartViewModel { ChartName = "Top Chart" + id };
            BottomChart = new ChartViewModel { ChartName = "Bottom Chart" + id };
            SumGrid = sumStandardReport;
            DetailGrid = detailStandardReport;
        }

        /// <summary>
        /// Initializations this instance.
        /// </summary>
        public override void Initialization()
        {
            SumGrid.IsStatisticalReport = true;
            DetailGrid.IsStatisticalReport = false;
            SumGrid.ReportID = ID;
            DetailGrid.ReportID = ID;
            DetailGrid.Columns = DetailGrid.Columns.Where(c => c.IsDetailedColumn).ToList();
            DetailGrid.Columns.Insert(0, new Column
            {
                ColumnFormat = "{0:yyyy-MM}",
                ColumnName = "REDATE",
                ColumnHeaderCN = "月份",
                ColumnHeaderEN = "Month"
            });

            var serializer = new JavaScriptSerializer();
            base.Initialization();
            _chartOnly = SumGrid.ResultDataTable.AsEnumerable().Select(i => i.Field<object>("CHART_SOURCE")).ToList();

            StatisticalAspects = GetStatisticalAspects().Keys;
            TopChart.StatisticalAspects = GetStatisticalAspects();
            TopChart.ColumnValues = GetTopCols(TopChart.StatisticalAspects.Values);
            TopChart.ColumnName = GetStatisticalAspects().Keys.FirstOrDefault(); // need to modify
            TopChart.ReportId = ID;
            TopChart.DataCategories = this.GetTopColsByAspect("CHINESE_NAME");
            TopChart.IsTop = true;
            TopChart.ChartTypeOptions = new List<string>
                                            {
                                                Resources.Global.ChatType_Column,
                                                Resources.Global.ChatType_Bar,
                                                Resources.Global.ChatType_Pie
                                            };
            TopChart.Unit = Unit;
            TopChart.Title = Name.Replace(Resources.Global.Title_Statistics, Resources.Global.Title_StatisticsChart);
            TopChartModel = GetChartModel(TopChart);
            

            BottomChart.StatisticalAspects = GetStatisticalAspects();
            BottomChart.ColumnValues = GetBottomCols(BottomChart.StatisticalAspects.Values);
            BottomChart.ColumnName = GetStatisticalAspects().Keys.FirstOrDefault(); // need to modify
            BottomChart.ReportId = ID;
            BottomChart.DataCategories = this.GetBottomRowValues(); // TODO: english version
            BottomChart.IsTop = false;
            BottomChart.ChartTypeOptions = new List<string>
                                               {
                                                   Resources.Global.ChatType_Column,
                                                   Resources.Global.ChatType_Line,
                                               };
            BottomChart.Unit = Unit;
            BottomChart.Title = Name.Replace(Resources.Global.Title_Statistics, Resources.Global.Title_DetailChart);
            BottomChartModel = GetChartModel(BottomChart);
        }

        public IDictionary<string, List<object>> GetTopCols(IEnumerable<string> aspects)
        {
            return aspects.ToDictionary(aspect => aspect, GetTopColsByAspect);
        }

        public List<object> GetTopColsByAspect(string colName)
        {
            if (string.IsNullOrEmpty(colName))
                colName = TopChart.StatisticalAspects.FirstOrDefault().Value;
            var dict = SumGrid.ResultDataTable.AsEnumerable().Select(i => i.Field<object>(colName)).ToList();
            var result = GetChartNeedValues(dict);
            return result;
        }

        private List<object> GetChartNeedValues(List<object> dict)
        {
            var result = new List<object>();
            for (int i = 0; i < _chartOnly.Count; i++)
            {
                if (Boolean.Parse(_chartOnly[i] as string))
                {
                    result.Add(dict[i]);
                }
            }
            return result;
        }

        public IDictionary<string, List<object>> GetBottomCols(IEnumerable<string> aspects)
        {
            return aspects.ToDictionary(aspect => aspect, GetBottomColsByAspect);
        }

        public List<object> GetBottomColsByAspect(string colName)
        {
            if (this.DetailGrid == null || this.DetailGrid.Columns.Count == 0)
                return new List<object>();

            if (string.IsNullOrEmpty(colName))
                colName = this.DetailGrid.Columns[0].ColumnName;
            var dict = DetailGrid.ResultDataTable.AsEnumerable().Select(i => i.Field<object>(colName)).ToList();
            //var dict = standardReport.ResultDataTable.AsEnumerable().Select(i => i.Field<object>("volume")).ToList();

            return dict;
        }

        public List<object> GetBottomRowValues()
        {
            if (this.DetailGrid == null || this.DetailGrid.Columns.Count == 0)
                return new List<object>();

            var dict = DetailGrid.ResultDataTable.AsEnumerable().Select(i => ((DateTime)i.Field<object>(0)).ToString("yyyy-MM")).Cast<object>().ToList();
            //var dict = standardReport.ResultDataTable.AsEnumerable().Select(i => i.Field<object>("volume")).ToList();

            return dict;
        }

        private Dictionary<string, string> GetStatisticalAspects()
        {
            bool isInEnglish = CultureHelper.IsEnglishCulture();
            var pairs = from a in this.StandardReport.Columns.Where(c => c.IsDetailedColumn)
                        //where a.ColumnName
                        select new KeyValuePair<string, string>( isInEnglish ? a.ColumnHeaderEN : a.ColumnHeaderCN, a.ColumnName);

            //TODO: english version

            return pairs.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        }

        public ChartModel GetChartModel(ChartViewModel cols)
        {
            var selected = cols.StatisticalAspects.FirstOrDefault().Key;
            var values =
                cols.ColumnValues[cols.StatisticalAspects[selected]].ToArray().Select(v => v ?? 0);

            var categories = cols.DataCategories;
            var pairValues = new List<object>();
            //var serie = new List<Series>();
            for (int i = 0; i < values.Count(); i++)
            {
                //skip i which no need to render in chart 
                pairValues.Add(new object[] { categories[i], values.ToList()[i] });
                //serie.Add(new Series { Data = new Data(values.ToArray()), Name = selected });
            }

            var series = new Series {Data = new Data(pairValues.ToArray()), Name = selected, Type = ChartTypes.Column};


            var yText = cols.Unit != null
                            ? Resources.Global.Unit + "(" + cols.Unit + ")"
                            : Resources.Global.Unit + "(" + Resources.Global.Unit_Option_100M + ")";

            return new ChartModel(Resources.Global.SourceCCDC)
            {
                ChartName = "chart" + cols.ReportId + (cols.IsTop ? "top" : "bottom"),
                Series = new[] { series },
                Title = cols.Title,
                SubTitle = cols.ChartName,
                XAxisCategory = categories.Cast<string>(),
                ChartType = ChartTypes.Column,
                IsXAxisDate = !cols.IsTop,
                YAxisText = yText,
                Theme = cols.Theme,
                IsResizeable=true,
                ReportID = cols.ReportId
            };
        }
    }
}