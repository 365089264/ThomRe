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
using Microsoft.Practices.Unity;
using VAV.DAL.Report;

namespace VAV.Web.ViewModels.OpenMarket
{
    /// <summary>
    /// Monetary and Return Analysis Report Model
    /// </summary>
    public class MaRAnalysisReportViewModel : BaseReportViewModel
    {
        public string SumGridDisplayName { get; set; }
        public string ChartDisplayName { get; set; }
        public string DetailedGridDisplayName { get; set; }
        public string ChartTheme { get; set; }
        public string OperationType { get; set; }

        public ChartModel Chart { get; set; }

        public List<MonetaryAndReturnAnalysisSummaryModel> SumGrid { get; set; }

        public string[] SumGridColumns;

        public List<OpenMarketRepo> DetailGrid { get; set; }

        private List<Column> summaryGridColumns = new List<Column>();
        public List<Column> SummaryGridColumns
        {
            get { return summaryGridColumns; }
            set { summaryGridColumns = value; }
        }

        public bool IsStatisticalReport { get; set; }

        public bool IsInEnglish { get; private set; }

        public ImmaturityAmount ImmaturityAmount { get; private set; }




        public MaRAnalysisReportViewModel(int id, List<MonetaryAndReturnAnalysisSummaryModel> sumReport, List<OpenMarketRepo> detailedReport, string theme, ImmaturityAmount immaturityAmount, string operationType = "CBB,RP,RRP,MLF")
            : base(id)
        {
            this.IsInEnglish = CultureHelper.IsEnglishCulture();

            ID = id;
            SumGridDisplayName = IsInEnglish ? "Monetary and Return Analysis" : "回笼投放资金分析";
            ChartDisplayName = IsInEnglish ? "Open Market - Monetary and Return Analysis Chart" : "公开市场-回笼投放资金分析图表";
            DetailedGridDisplayName = IsInEnglish ? "Open Market - Detailed Monetary and Return Analysis" : "公开市场-回笼投放资金分析明细";
            ChartTheme = theme;
            OperationType = operationType;

            SumGrid = sumReport;
            DetailGrid = detailedReport;
            InitializationSumGridColumn();
            foreach (var item in SummaryGridColumns)
            {
                item.DisplayHeader = IsInEnglish ? item.ColumnHeaderEN : item.ColumnHeaderCN;
            }
            Chart = InitialChart();

            ImmaturityAmount = immaturityAmount;

        }

        /// <summary>
        /// Initializations this instance.
        /// </summary>
        public void InitializationSumGridColumn()
        {
            var operations = OperationType.Split(',');
            //SummaryGridColumns.Add(new Column { ColumnHeaderCN = "种类", ColumnHeaderEN = "Category", ColumnName = "Category", ColumnIndex = 0, IsDetailedColumn = false });
            SummaryGridColumns.Add(new Column { ColumnHeaderCN = "开始日期", ColumnHeaderEN = "Start Date", ColumnName = "CategoryStartDate", ColumnIndex = 1, IsDetailedColumn = false, ColumnFormat = "Date" });
            SummaryGridColumns.Add(new Column { ColumnHeaderCN = "结束日期", ColumnHeaderEN = "End Date", ColumnName = "CategoryEndDate", ColumnIndex = 2, IsDetailedColumn = false, ColumnFormat = "Date" });
            if (OperationType == "ALL" || operations.Contains("RP"))
            {
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "正回购发行量", ColumnHeaderEN = "Repo Injection", ColumnName = "RepoInjection", ColumnIndex = 3, IsDetailedColumn = false, ColumnFormat = "MinusValue" });
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "正回购到期量", ColumnHeaderEN = "Repo Withdrawal", ColumnName = "RepoWithdrawal", ColumnIndex = 4, IsDetailedColumn = false });
            }
            if (OperationType == "ALL" || operations.Contains("RRP"))
            {
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "逆回购发行量", ColumnHeaderEN = "Reverse Repo Injection", ColumnName = "ReverseRepoInjection", ColumnIndex = 5, IsDetailedColumn = false });
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "逆回购到期量", ColumnHeaderEN = "Reverse Repo Withdrawal", ColumnName = "ReverseRepoWithdrawal", ColumnIndex = 6, IsDetailedColumn = false, ColumnFormat = "MinusValue" });
            }
            if (OperationType == "ALL" || operations.Contains("CBB"))
            {
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "央行票据发行量", ColumnHeaderEN = "Central Bank Bill Injection", ColumnName = "CbbInjection", ColumnIndex = 7, IsDetailedColumn = false, ColumnFormat = "MinusValue" });
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "央行票据到期量", ColumnHeaderEN = "Central Bank Bill Withdrawal", ColumnName = "CbbWithdrawal", ColumnIndex = 8, IsDetailedColumn = false });
            }
            if (OperationType == "ALL" || operations.Contains("MLF"))
            {
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "中期借贷便利发行量", ColumnHeaderEN = "Medium-term Lending Facility Injection", ColumnName = "MlfInjection", ColumnIndex = 8, IsDetailedColumn = false, ColumnFormat = "MinusValue" });
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "中期借贷便利到期量", ColumnHeaderEN = "Medium-term Lending Facility Withdrawal", ColumnName = "MlfWithdrawal", ColumnIndex = 9, IsDetailedColumn = false });
            }
            if (OperationType == "ALL" || operations.Contains("FMD"))
            {
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "中央国库现金管理发行量", ColumnHeaderEN = "Central treasury cash management Injection", ColumnName = "FmdInjection", ColumnIndex = 9, IsDetailedColumn = false });
                SummaryGridColumns.Add(new Column { ColumnHeaderCN = "中央国库现金管理到期量", ColumnHeaderEN = "Central treasury cash management Withdrawal", ColumnName = "FmdWithdrawal", ColumnIndex = 10, IsDetailedColumn = false, ColumnFormat = "MinusValue" });
            }
            SummaryGridColumns.Add(new Column { ColumnHeaderCN = "投放量(+)", ColumnHeaderEN = "Injection(+)", ColumnName = "NetInjection", ColumnIndex = 11, IsDetailedColumn = true });
            SummaryGridColumns.Add(new Column { ColumnHeaderCN = "回笼量(-)", ColumnHeaderEN = "Withdrawal(-)", ColumnName = "NetWithdrawal", ColumnIndex = 12, IsDetailedColumn = true });
            SummaryGridColumns.Add(new Column { ColumnHeaderCN = "净投放量(+)/净回笼量(-)", ColumnHeaderEN = "Net Injection(+)/Net Withdrawal(-)", ColumnName = "NetInjectionWithdrawal", ColumnIndex = 13, IsDetailedColumn = true });
            SummaryGridColumns.Add(new Column { ColumnHeaderCN = "总投放量(+)/总回笼量(-)", ColumnHeaderEN = "Sum Injection(+)/Sum Withdrawal(-)", ColumnName = "SumInjectionWithdrawal", ColumnIndex = 14, IsDetailedColumn = true });

            IsStatisticalReport = true;
        }

        /// <summary>
        /// Initial chart
        /// </summary>
        /// <returns></returns>
        public ChartModel InitialChart()
        {
            var series = new List<DotNet.Highcharts.Options.Series>();
            var seriesNames = SummaryGridColumns.Where(c => c.IsDetailedColumn).ToList();

            var dataNetInjection = from d in SumGrid select new object[] { d.CategoryStartDate.AddDays(1), d.NetInjection };
            series.Add(new Series
            {
                Name = this.IsInEnglish ? seriesNames[0].ColumnHeaderEN : seriesNames[0].ColumnHeaderCN,
                Data = new Data(dataNetInjection.OrderBy(d=>d[0]).ToArray()),
                Type = ChartTypes.Column
            });
            var dataNetWithdrawal = from d in SumGrid select new object[] { d.CategoryStartDate.AddDays(1), d.NetWithdrawal };
            series.Add(new Series
            {
                Name = this.IsInEnglish ? seriesNames[1].ColumnHeaderEN : seriesNames[1].ColumnHeaderCN,
                Data = new Data(dataNetWithdrawal.OrderBy(d => d[0]).ToArray()),
                Type = ChartTypes.Column
            });
            var dataNetInjectionWithdrawal = from d in SumGrid select new object[] { d.CategoryStartDate.AddDays(1), d.NetInjectionWithdrawal };
            series.Add(new Series
            {
                Name = this.IsInEnglish ? seriesNames[2].ColumnHeaderEN : seriesNames[2].ColumnHeaderCN,
                Data = new Data(dataNetInjectionWithdrawal.OrderBy(d => d[0]).ToArray()),
                Type = ChartTypes.Line
            });
            var dataSumInjectionWithdrawal = from d in SumGrid select new object[] { d.CategoryStartDate.AddDays(1), d.SumInjectionWithdrawal };
            series.Add(new Series
            {
                Name = this.IsInEnglish ? seriesNames[3].ColumnHeaderEN : seriesNames[3].ColumnHeaderCN,
                Data = new Data(dataSumInjectionWithdrawal.OrderBy(d => d[0]).ToArray()),
                Type = ChartTypes.Line
            });

            Chart = new ChartModel(Resources.Global.Source)
            {
                ChartName = "chart" + this.ID,
                Series = series.ToArray(),
                Title = ChartDisplayName,
                //TODO：global resource
                //ChartType = ChartTypes.Line,
                IsXAxisDate = true,
                YAxisText = this.IsInEnglish ? "Volume" : "面额",
                //TODO：global resource
                StartDate = SumGrid.Min(d => d.CategoryStartDate),
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
        //private IDictionary<string, List<object>> GetColumnValues()
        //{
        //    Dictionary<string, List<object>> dict = new Dictionary<string, List<object>>();
        //    dict.Add("NetInjection", SumGrid.Select(t => t.NetInjection).Cast<object>().ToList());
        //    dict.Add("NetWithdrawal", SumGrid.Select(t => t.NetWithdrawal).Cast<object>().ToList());
        //    dict.Add("NetInjectionWithdrawal", SumGrid.Select(t => t.NetInjectionWithdrawal).Cast<object>().ToList());
        //    dict.Add("SumInjectionWithdrawal", SumGrid.Select(t => t.SumInjectionWithdrawal).Cast<object>().ToList());
        //    return dict;
        //}

        //private Dictionary<string, string> GetStatisticalAspects()
        //{
        //    var pairs = from a in this.SummaryGridColumns.Where(c => c.IsDetailedColumn)
        //                //where a.ColumnName
        //                select new KeyValuePair<string, string>(IsInEnglish ? a.ColumnHeaderEN : a.ColumnHeaderCN, a.ColumnName);

        //    //TODO: english version

        //    return pairs.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        //}
    }

    public class ImmaturityAmount
    {
        public double All { get; set; }
        public double CBankBill { get; set; }
        public double FMD { get; set; }
        public double Repo { get; set; }
        public double ReverseRepo { get; set; }
        public double Mlf { get; set; }
    }
}