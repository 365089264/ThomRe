using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using VAV.Model.Data;
using VAV.Web.Localization;
using VAV.Web.ViewModels.OpenMarket;
using VAV.Model.Data.OpenMarket;
using VAV.Web.ViewModels.Report;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Helpers;

namespace VAV.Web.ViewModels.OpenMarket
{
    /// <summary>
    /// Rates Analysis Report Model
    /// </summary>
    public class RatesAnalysisReportViewModel : BaseReportViewModel
    {
        public string ChartDisplayName { get; set; }
        public string GridDisplayName { get; set; }
        public string SelectTypeAndTerm { get; set; }
        public string ChartTheme { get; set; }

        public ChartModel Chart { get; set; }

        public string[] GridColumns;

        public List<OpenMarketRepo> GridData { get; set; }

        public bool IsStatisticalReport { get; set; }

        public bool IsInEnglish { get; private set; }

        public RatesAnalysisReportViewModel(int id, List<OpenMarketRepo> resultList,string theme):base(id)
        {
            this.IsInEnglish = CultureHelper.IsEnglishCulture();

            ID = id;
            GridDisplayName = IsInEnglish ? "Open Market -Rates Analysis" : "公开市场-利率走势分析";
            ChartDisplayName = IsInEnglish ? "Open Market - Rates Analysis Chart" : "公开市场-利率走势分析图表";
            ChartTheme = theme;
            var resultSet = (from gr in resultList.OrderBy(re=>re.Date)
                            group gr by gr.OperationType into grouped
                            select grouped).ToList();

            Dictionary<string, List<OpenMarketRepo>> groupedResultDict = new Dictionary<string, List<OpenMarketRepo>>();
            foreach (var item in resultSet)
            {
                var groupedResultSet = (from gr in item
                                 group gr by gr.OperationTerm into grouped
                                 select grouped).ToList();

                foreach (var grouped in groupedResultSet)
                {
                    groupedResultDict.Add(item.Key + " - " + grouped.Key, grouped.ToList());
                }
            }

            //GridData = resultList.Where(re => string.Compare(re.OperationType, selectedType, true) == 0 && string.Compare(re.OperationTerm, selectedTerm, true) == 0).ToList();
            GridData = groupedResultDict.FirstOrDefault().Value;
            SelectTypeAndTerm = groupedResultDict.FirstOrDefault().Key;
            Chart = InitialChart(groupedResultDict);
        }

        /// <summary>
        /// Initial chart
        /// </summary>
        /// <returns></returns>
        public ChartModel InitialChart(Dictionary<string, List<OpenMarketRepo>> groupedResultDict)
        {
            var series = new List<DotNet.Highcharts.Options.Series>();

            foreach (var item in groupedResultDict)
            {
                    var resultData = from re in item.Value select new object[] { ((DateTime)re.Date).AddDays(1), re.RefRate};
                    series.Add(new Series
                    {
                        Name = item.Key,
                        Data = new Data(resultData.ToArray())
                    });
            }

            //foreach (var groupByType in GridData.GroupBy(re => re.OperationType))
            //{
            //    foreach (var groupByTypeAndTerm in groupByType.GroupBy(re=>re.OperationTerm))
            //    {
            //        var resultList = groupByTypeAndTerm.Select(new OpenMarketRepo { });
            //        series.Add(new Series
            //        {
            //            Name = groupByTypeAndTerm.OperationType + " - " + groupByTypeAndTerm.OperationTerm,
            //            Data = new Data(new object[] { ((DateTime)groupByTypeAndTerm.Date).AddDays(1), groupByTypeAndTerm.RefRate })
            //        });
            //    }
            //}

            Chart = new ChartModel(Resources.Global.Source)
            {
                ChartName = "chart" + this.ID,
                Series = series.ToArray(),
                Title = ChartDisplayName,
                //TODO：global resource
                ChartType = ChartTypes.Line,
                IsXAxisDate = true,
                YAxisText = this.IsInEnglish ? "Yield(%)" : "收益率(%)",
                //TODO：global resource
                IsResizeable=true,
                ReportID = ID,
                Theme = ChartTheme
            };

            return Chart;
        }

        public override void Initialization()
        {
            throw new NotImplementedException();
        }
    }
}