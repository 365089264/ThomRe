using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Query.Dynamic;
using Microsoft.Practices.Unity;
using VAV.DAL.Common;
using VAV.DAL.IPP;
using VAV.Entities;
using VAV.Model.Chart;
using VAV.Model.Data;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.Web.Localization;
using VAV.DAL.Report;
using VAV.Model.Data.Bond;
using VAV.DAL.Services;
using VAV.Web.ViewModels.Bond;
using VAV.Web.ViewModels.BondInfo;
using VAV.DAL.ResearchReport;
using log4net;

namespace VAV.Web.Controllers
{
    public class BondInfoController : BaseController
    {
        ILog _log = LogManager.GetLogger(typeof(BondInfoController));

        [Dependency]
        public MenuService MenuService { get; set; }

        [Dependency]
        public BondInfoRepository BondInfoRepository { get; set; }

        [Dependency]
        public BondReportRepository BondReportRepository { get; set; }

        [Dependency]
        public ResearchReportRepository CMARepository { get; set; }

        [Dependency]
        public UserColumnService UserColumnService { get; set; }

        [Localization]
        public ActionResult GetScheduledIssueDetailData(string assetClass, string couponClass, string optionClass, int id,int startPage, int pageSize)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetScheduledIssueBondInfo(ArrayToString(assetClass), couponClass, optionClass, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), startPage, pageSize, out total);
            return Json(BuidJsonTable(dataTable, columns, total, startPage, pageSize), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetNewIssueDetailData(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string optionClass, string bondRating, string bondCode, string bondMarket, string bondTrustee, string isMD, string othBondClass, int id, int startPage, int pageSize)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetNewIssueDetails(startDate, endDate, ArrayToString(bondClass), couponClass, optionClass, bondRating, bondCode, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), isMD, othBondClass, bondMarket, bondTrustee, startPage, pageSize, out total);
            return Json(BuidJsonTable(dataTable, columns, total, startPage, pageSize), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetNewListDetailData(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string optionClass, string bondCode, string isMD, string othBondClass, int id, int startPage, int pageSize)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetNewListDetails(startDate, endDate, ArrayToString(bondClass), couponClass, optionClass, bondCode, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), isMD, othBondClass, startPage, pageSize, out total);
            return Json(BuidJsonTable(dataTable, columns, total, startPage, pageSize), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetQueueToListDetailData(string assetClass, string couponClass, string optionClass, int id, int startPage, int pageSize)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetQToListBondInfo(ArrayToString(assetClass), couponClass, optionClass, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), startPage, pageSize, out total);
            return Json(BuidJsonTable(dataTable, columns, total, startPage, pageSize), JsonRequestBehavior.AllowGet);
        }

        

        private JsonTable BuidJsonTableAbs(DataTable table, IEnumerable<REPORTCOLUMNDEFINITION> columns, int total = 0, int currentPage = 0, int pageSize = 0)
        {
            var jTable = new JsonTable();
            jTable.CurrentPage = currentPage;
            jTable.PageSize = pageSize;
            jTable.Total = total;
            foreach (var column in columns)
            {
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = column.COLUMN_NAME, ColumnType = column.COLUMN_TYPE, Name = column.DisplayName, ColumnStyle = column.COLUMN_STYLE ?? "" });
            }
            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                foreach (REPORTCOLUMNDEFINITION column in columns)
                {

                    if (!currentRow.Keys.Contains(column.COLUMN_NAME))
                        currentRow.Add(column.COLUMN_NAME, column.COLUMN_STYLE != "" && row[column.COLUMN_NAME].ToString().Length > 200 ? (row[column.COLUMN_NAME].ToString().Substring(0, 200) + "...") : UIGenerator.FormatCellValue(row, column));
                }
                if (!columns.Any(x => x.COLUMN_NAME == "BOND_UNI_CODE"))
                {
                    currentRow.Add("BOND_UNI_CODE", row["BOND_UNI_CODE"].ToString());
                }
                if (!columns.Any(x => x.COLUMN_NAME == "Issuer_ORG_UNI_CODE"))
                {
                    currentRow.Add("Issuer_ORG_UNI_CODE", row["Issuer_ORG_UNI_CODE"].ToString());
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        [Localization]
        public ActionResult ExportExcel(string asset, string coupon, string option, int id, int startPage=1, int pageSize=2000)
        {
            var reportName = MenuService.GetMenuNodeByReportId(id).DisplayName;
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetScheduledIssueBondInfo(ArrayToString(asset), coupon, option, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), startPage, pageSize, out total);
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.DisplayName).ToArray(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName);
        }

        [Localization]
        public ActionResult ExportExcelForNewIssue(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string optionClass, string bondRating, string bondCode, string bondMarket, string bondTrustee, string isMD, string othBondClass, int id, int startPage = 1, int pageSize = 2000)
        {
            var reportName = MenuService.GetMenuNodeByReportId(id).DisplayName;
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetNewIssueDetails(startDate, endDate, ArrayToString(bondClass), couponClass, optionClass, bondRating, bondCode, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), isMD, othBondClass, bondMarket, bondTrustee, startPage, pageSize, out total);
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.DisplayName).ToArray(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName);
        }

        [Localization]
        public ActionResult ExportExcelForNewList(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string optionClass, string bondCode, string isMD, string othBondClass, int id, int startPage = 1, int pageSize = 2000)
        {
            var reportName = MenuService.GetMenuNodeByReportId(id).DisplayName;
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetNewListDetails(startDate, endDate, ArrayToString(bondClass), couponClass, optionClass, bondCode, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), isMD, othBondClass, startPage, pageSize, out total);
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.DisplayName).ToArray(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName);
        }

        [Localization]
        public ActionResult ExportExcelForQueueToList(string asset, string coupon, string option, int id, int startPage = 1, int pageSize = 2000)
        {
            var reportName = MenuService.GetMenuNodeByReportId(id).DisplayName;
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetQToListBondInfo(ArrayToString(asset), coupon, option, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), startPage, pageSize,out total);
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.DisplayName).ToArray(), columns.Where(x => x.COLUMN_NAME != "BondRatingHist" && x.COLUMN_NAME != "BondIssuerRating").Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName);
        }

        #region Dim Sum Bond
        [Localization]
        public JsonResult GetDimSumData(DateTime start, DateTime end, string category, string unit, int id, string chartType, string dataType, string bottomRate, string bottomDataType, string term, int startPage = 1, int pageSize = 300, string rate = "m", bool isHTML = false)
        {
            
            _log.Error("Dim Start: " + DateTime.Now.ToString());

            var topTable = BondInfoRepository.GetDimSumBondSummary(start, end, category, unit, "all", "all");

            _log.Error("Dim topTable:" + DateTime.Now.ToString());
            var summary = new DimSumBondSummary(); //added total
            summary.Type = "all";
            summary.TypeName = summary.TypeName = Resources.Global.Total;
            summary.InitialBalance = topTable.Where(d => !d.IsParent).Sum(d => d.InitialBalance);
            summary.Issues = topTable.Where(d => !d.IsParent).Sum(d => d.Issues);
            summary.IssuesAmount = topTable.Where(d => !d.IsParent).Sum(d => d.IssuesAmount);
            summary.MaturityBonds = topTable.Where(d => !d.IsParent).Sum(d => d.MaturityBonds);
            summary.MaturityAmount = topTable.Where(d => !d.IsParent).Sum(d => d.MaturityAmount);
            summary.EndBalance = topTable.Where(d => !d.IsParent).Sum(d => d.EndBalance);
            summary.IssuesPercent = 100.00;
            summary.IssuesAmountPercent = 100.00;
            summary.MaturityBondsPercent = 100.00;
            summary.MaturityAmountPercent = 100.00;
            topTable.Add(summary);

            _log.Error("Dim typeValue: " + DateTime.Now.ToString());
            var typeValue = topTable.Count() == 0 ? "" : topTable.FirstOrDefault().Type;
            _log.Error("Dim bottomTable: " + DateTime.Now.ToString());
            var bottomTable = GetBottomTable(start, end, category, typeValue, unit, term, id, "", startPage, pageSize);
            _log.Error("Dim chart: " + DateTime.Now.ToString());
            Dictionary<string, IEnumerable<DimSumBondSummary>> yearsDicForBottomChart;
            var chart = GetChart(chartType, dataType, start, end, category, unit, rate, out yearsDicForBottomChart);
            _log.Error("Dim bottomChart: " + DateTime.Now.ToString());
            var bottomChart = GetDimsumBottomChartDataInit(bottomDataType,unit, bottomRate,
                topTable[0].Type, yearsDicForBottomChart);
            _log.Error("Dim dimSumData: " + DateTime.Now.ToString());
            var dimSumData =
                new
                {
                    TopTable = ConvertToDic(topTable),
                    Chart = chart,
                    BottomTable = bottomTable,
                    BottomChart = bottomChart
                };
            _log.Error("Dim Complete: " + DateTime.Now.ToString());
            return isHTML ? Json(dimSumData, "text/html", JsonRequestBehavior.AllowGet) : Json(dimSumData, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public JsonResult GetDimSumDetailData(DateTime start, DateTime end, string category, string typeValue, string unit, string term, int id, string order, int startPage, int pageSize)
        {
            var data = GetBottomTable(start, end, category, typeValue, unit, term, id, order, startPage, pageSize);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public JsonResult GetChartData(string chartType, string dataType, DateTime start, DateTime end, string category, string unit, string rate)
        {
            var yearsDicForBottomChart = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
            //var bondList = BondInfoRepository.GetDimSumBondList(start, end,category, CultureHelper.IsEnglishCulture());
            return Json(GetChart(chartType, dataType, start, end, category, unit, rate, out yearsDicForBottomChart), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetDimsumBottomChart(DateTime start, DateTime end, string dataType, string category, string unit, string rate, string typeValue)
        {
            //var bondList = BondInfoRepository.GetDimSumBondList(start, end,category, CultureHelper.IsEnglishCulture());
            var chartData = GetDimsumBottomChartData(dataType, start, end, category, unit, rate, typeValue);
            return Json(chartData, JsonRequestBehavior.AllowGet);
        }

        private object GetDimsumBottomChartData(string dataType, DateTime start,
            DateTime end, string category, string unit, string rate, string typeValue)
        {
            _log.Error("1: "+DateTime.Now);
            if (start > end) return null;
            var chartData = new ChartJsonData
            {
                YText = Resources.Global.Unit + "(" + HtmlUtil.GetUnitOptionByKey(unit) + ")"
            };
            switch (dataType)
            {
                case "DimSum_Column_IBalance":
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_Issues":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 0;
                    break;
                case "DimSum_Column_IssuesPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_IssueAmount":
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_IssueAmountPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_Maturities":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 0;
                    break;
                case "DimSum_Column_MaturitiesPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_MaturityAmount":
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_MaturityAmountPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_EndBalance":
                    chartData.Decimal = 2;
                    break;

            }
            _log.Error("2: " + DateTime.Now);
            var yearsDic = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
            var dates = PopulateRateTimes(start, end, rate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var summaryData =
                    BondInfoRepository.GetDimSumBondSummary(currentStart, currentEnd, category, unit, dataType, typeValue).ToList();

                yearsDic.Add(string.Format("{0}", currentEnd.ToString("yyyy-M")), summaryData);
            }
            _log.Error("3: " + DateTime.Now);
            //class/year/data
            var groupDic = new Dictionary<string, Dictionary<string, DimSumBondSummary>>();
            foreach (var keyValue in yearsDic)
            {
                foreach (var summary in keyValue.Value)
                {
                    if (summary.Type != typeValue)
                        continue;
                    if (!groupDic.ContainsKey(summary.TypeName))
                        groupDic.Add(summary.TypeName, new Dictionary<string, DimSumBondSummary>());
                    var currentTypeDic = groupDic[summary.TypeName];
                    if (currentTypeDic.ContainsKey(keyValue.Key))
                    {
                        currentTypeDic[keyValue.Key] = summary;
                    }
                    else
                    {
                        currentTypeDic.Add(keyValue.Key, summary);
                    }
                    break;
                }
            }
            _log.Error("4: " + DateTime.Now);
            var seriesDataList = new List<SeriesData>();
            var categories = new List<string>();
            foreach (var keyValue in groupDic)
            {
                var sData = new SeriesData { name = keyValue.Key };
                if (keyValue.Value != null)
                {
                    var dataPoints = new List<double>();
                    foreach (var yearSumaryPair in keyValue.Value)
                    {
                        categories.Add(rate == "y" ? yearSumaryPair.Value.CurrentDate.ToString("yyyy") : yearSumaryPair.Key);

                        var point = GetChartSeriesDataFromSummary(dataType, yearSumaryPair.Value);
                        dataPoints.Add(point ?? 0);
                    }
                    sData.data = dataPoints.ToArray();
                    seriesDataList.Add(sData);
                }
            }
            _log.Error("5: " + DateTime.Now);
            chartData.ColumnCategories = categories.ToArray();
            _log.Error("6: " + DateTime.Now);
            chartData.SeriesData = seriesDataList.ToArray();
            _log.Error("7: " + DateTime.Now);
            return chartData;
        }

        private object GetDimsumBottomChartDataInit(string dataType,string unit, string rate, string typeValue, Dictionary<string, IEnumerable<DimSumBondSummary>> yearsDic)
        {
            _log.Error("GetDimsumBottomChartDataInit: " + DateTime.Now);
            var chartData = new ChartJsonData
            {
                YText = Resources.Global.Unit + "(" + HtmlUtil.GetUnitOptionByKey(unit) + ")"
            };
            var groupDic = new Dictionary<string, Dictionary<string, DimSumBondSummary>>();
            foreach (var keyValue in yearsDic)
            {
                foreach (var summary in keyValue.Value)
                {
                    if (summary.Type != typeValue)
                        continue;
                    if (!groupDic.ContainsKey(summary.TypeName))
                        groupDic.Add(summary.TypeName, new Dictionary<string, DimSumBondSummary>());
                    var currentTypeDic = groupDic[summary.TypeName];
                    if (currentTypeDic.ContainsKey(keyValue.Key))
                    {
                        currentTypeDic[keyValue.Key] = summary;
                    }
                    else
                    {
                        currentTypeDic.Add(keyValue.Key, summary);
                    }
                    break;
                }
            }
            _log.Error("4: " + DateTime.Now);
            var seriesDataList = new List<SeriesData>();
            var categories = new List<string>();
            foreach (var keyValue in groupDic)
            {
                var sData = new SeriesData { name = keyValue.Key };
                if (keyValue.Value != null)
                {
                    var dataPoints = new List<double>();
                    foreach (var yearSumaryPair in keyValue.Value)
                    {
                        categories.Add(rate == "y" ? yearSumaryPair.Value.CurrentDate.ToString("yyyy") : yearSumaryPair.Key);

                        var point = GetChartSeriesDataFromSummary(dataType, yearSumaryPair.Value);
                        dataPoints.Add(point ?? 0);
                    }
                    sData.data = dataPoints.ToArray();
                    seriesDataList.Add(sData);
                }
            }
            _log.Error("5: " + DateTime.Now);
            chartData.ColumnCategories = categories.ToArray();
            _log.Error("6: " + DateTime.Now);
            chartData.SeriesData = seriesDataList.ToArray();
            _log.Error("7: " + DateTime.Now);
            return chartData;
        }

        [Localization]
        public ActionResult ExportExcelForDimSumSummary(DateTime start, DateTime end, string category, string unit, string reportName, int id)
        {
            //var bondList = BondInfoRepository.GetDimSumBondList(start, end,category, CultureHelper.IsEnglishCulture());
            var topTable = BondInfoRepository.GetDimSumBondSummary(start, end, category, unit, "all", "all");

            var summary = new DimSumBondSummary(); //added total
            summary.Type = "all";
            summary.TypeName = Resources.Global.Total;
            summary.InitialBalance = topTable.Where(d => !d.IsParent).Sum(d => d.InitialBalance);
            summary.Issues = topTable.Where(d => !d.IsParent).Sum(d => d.Issues);
            summary.IssuesAmount = topTable.Where(d => !d.IsParent).Sum(d => d.IssuesAmount);
            summary.MaturityBonds = topTable.Where(d => !d.IsParent).Sum(d => d.MaturityBonds);
            summary.MaturityAmount = topTable.Where(d => !d.IsParent).Sum(d => d.MaturityAmount);
            summary.EndBalance = topTable.Where(d => !d.IsParent).Sum(d => d.EndBalance);
            summary.IssuesPercent = 100.00;
            summary.IssuesAmountPercent = 100.00;
            summary.MaturityBondsPercent = 100.00;
            summary.MaturityAmountPercent = 100.00;
            topTable.Add(summary);

            ReportParameter reportParam = new ReportParameter
            {
                StartDate = start,
                EndDate = end,
                Unit = string.IsNullOrEmpty(unit) ? "100M" : unit,
            };

            return new ExcelResult(topTable.AsQueryable(), GetDimSumHeader(), GetDimSumColumns(), reportName, reportName, false, null, reportParam, false, specificDateFormat: "yyyy-MM-dd");
        }

        [Localization]
        public ActionResult ExportExcelForDimSumDetail(DateTime start, DateTime end, string category, string typeValue, string unit, string term, string reportName, int id, string order, int startPage, int pageSize)
        {
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var bottomTable = BondInfoRepository.GetDimSumBondDetail(start, end, category, typeValue, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), unit, term, order, startPage, pageSize, out total);

            ReportParameter reportParam = new ReportParameter
            {
                StartDate = start,
                EndDate = end,
                Unit = string.IsNullOrEmpty(unit) ? "100M" : unit,
            };

            return new ExcelResult(bottomTable.AsEnumerable().AsQueryable(), columns.Select(x => x.DisplayName).ToArray(), columns.Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName, false, null, reportParam, false, specificDateFormat: "yyyy-MM-dd");
        }

        [Localization]
        public ActionResult ExportExcelFroBottomChart(DateTime start, DateTime end, string dataType, string category, string unit, string rate, string typeValue)
        {
            var data = BuildBottomChartDataForExcel(start, end, dataType, category, unit, rate, typeValue);
            var jP = new JsonExcelParameter { Table = data, TableName = Resources.Global.Bond_DimSum_Class_Detail, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public ActionResult ExportExcelForSummaryChart(string dataType, DateTime start, DateTime end, string category, string unit, string rate)
        {
            var data = BuildSummaryChartDataForExcel(start, end, dataType, category, unit, rate);
            var jP = new JsonExcelParameter { Table = data, TableName = GetExcelTitleByDataType(dataType) + Resources.Global.Bond_DimSum_Summary, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        private JsonTable BuildSummaryChartDataForExcel(DateTime start, DateTime end, string dataType, string category, string unit, string rate)
        {
            var jTable = new JsonTable();
            //var bondList = BondInfoRepository.GetDimSumBondList(start, end,category, CultureHelper.IsEnglishCulture());

            if (start > end) return null;

            var yearsDic = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
            var dates = PopulateRateTimes(start, end, rate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var summaryData = BondInfoRepository.GetDimSumBondSummary(currentStart, currentEnd, category, unit,dataType,"all").Where(x => !x.IsParent);
                yearsDic.Add(string.Format("{0}", currentEnd.ToString("yyyy-M")), summaryData);
            }

            var typeList = new List<dynamic>();
            foreach (var dic in yearsDic)
            {
                typeList.AddRange(dic.Value.Select(f => new { f.Type, f.TypeName }));
            }
            var  types=typeList.Distinct();
            //var typeList = yearsDic.FirstOrDefault().Value.Select(f => new { f.Type, f.TypeName }).Distinct().ToList();

            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Date, ColumnName = "Date" });
            foreach (var t in types)
            {
                jTable.ColumTemplate.Add(new JsonColumn { Name = t.TypeName, ColumnName = t.Type + "_" + GetColumnNameByDataType(dataType) });
            }

            foreach (var f in yearsDic)
            {
                var currentRow = new Dictionary<string, string>();
                currentRow.Add("Date", f.Key);
                foreach (var v in f.Value)
                {
                    currentRow.Add(v.Type + "_" + GetColumnNameByDataType(dataType), GetColumnValueByDataType(v, dataType));
                }
                jTable.RowData.Add(currentRow);
            }

            return jTable;
        }

        private string GetExcelTitleByDataType(string dataType)
        {
            string title = "";

            switch (dataType)
            {
                case "DimSum_Column_IBalance":
                    title = Resources.Global.DimSum_Column_IBalance;
                    break;
                case "DimSum_Column_Issues":
                    title = Resources.Global.DimSum_Column_Issues;
                    break;
                case "DimSum_Column_IssuesPtn":
                    title = Resources.Global.DimSum_Column_IssuesPtn;
                    break;
                case "DimSum_Column_IssueAmount":
                    title = Resources.Global.DimSum_Column_IssueAmount;
                    break;
                case "DimSum_Column_IssueAmountPtn":
                    title = Resources.Global.DimSum_Column_IssueAmountPtn;
                    break;
                case "DimSum_Column_Maturities":
                    title = Resources.Global.DimSum_Column_Maturities;
                    break;
                case "DimSum_Column_MaturitiesPtn":
                    title = Resources.Global.DimSum_Column_MaturitiesPtn;
                    break;
                case "DimSum_Column_MaturityAmount":
                    title = Resources.Global.DimSum_Column_MaturityAmount;
                    break;
                case "DimSum_Column_MaturityAmountPtn":
                    title = Resources.Global.DimSum_Column_MaturityAmountPtn;
                    break;
                case "DimSum_Column_EndBalance":
                    title = Resources.Global.DimSum_Column_EndBalance;
                    break;
                default:
                    break;
            }

            return title;
        }

        private string GetColumnNameByDataType(string dataType)
        {
            string columnName = "";

            switch (dataType)
            {
                case "DimSum_Column_IBalance":
                    columnName = "InitialBalance";
                    break;
                case "DimSum_Column_Issues":
                    columnName = "Issues";
                    break;
                case "DimSum_Column_IssuesPtn":
                    columnName = "IssuesPercent";
                    break;
                case "DimSum_Column_IssueAmount":
                    columnName = "IssuesAmount";
                    break;
                case "DimSum_Column_IssueAmountPtn":
                    columnName = "IssuesAmountPercent";
                    break;
                case "DimSum_Column_Maturities":
                    columnName = "MaturityBonds";
                    break;
                case "DimSum_Column_MaturitiesPtn":
                    columnName = "MaturityBondsPercent";
                    break;
                case "DimSum_Column_MaturityAmount":
                    columnName = "MaturityAmount";
                    break;
                case "DimSum_Column_MaturityAmountPtn":
                    columnName = "MaturityAmountPercent";
                    break;
                case "DimSum_Column_EndBalance":
                    columnName = "EndBalance";
                    break;
                default:
                    break;
            }

            return columnName;
        }

        private string GetColumnValueByDataType(DimSumBondSummary summary, string dataType)
        {
            string columnValue = "";

            switch (dataType)
            {
                case "DimSum_Column_IBalance":
                    columnValue = summary.InitialBalance.ToString();
                    break;
                case "DimSum_Column_Issues":
                    columnValue = summary.Issues.ToString();
                    break;
                case "DimSum_Column_IssuesPtn":
                    columnValue = summary.IssuesPercent.ToString();
                    break;
                case "DimSum_Column_IssueAmount":
                    columnValue = summary.IssuesAmount.ToString();
                    break;
                case "DimSum_Column_IssueAmountPtn":
                    columnValue = summary.IssuesAmountPercent.ToString();
                    break;
                case "DimSum_Column_Maturities":
                    columnValue = summary.MaturityBonds.ToString();
                    break;
                case "DimSum_Column_MaturitiesPtn":
                    columnValue = summary.MaturityBondsPercent.ToString();
                    break;
                case "DimSum_Column_MaturityAmount":
                    columnValue = summary.MaturityAmount.ToString();
                    break;
                case "DimSum_Column_MaturityAmountPtn":
                    columnValue = summary.MaturityAmountPercent.ToString();
                    break;
                case "DimSum_Column_EndBalance":
                    columnValue = summary.EndBalance.ToString();
                    break;
                default:
                    break;
            }

            return columnValue;
        }

        private JsonTable BuildBottomChartDataForExcel(DateTime start, DateTime end, string dataType, string category, string unit, string rate, string typeValue)
        {
            var jTable = new JsonTable();
            //var bondList = BondInfoRepository.GetDimSumBondList(start, end,category, CultureHelper.IsEnglishCulture());

            if (start > end) return null;

            var yearsDic = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
            var dates = PopulateRateTimes(start, end, rate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var summaryData = BondInfoRepository.GetDimSumBondSummary(currentStart, currentEnd, category, unit, dataType, typeValue);

                var summary = new DimSumBondSummary(); //added total
                summary.Type = "all";
                summary.TypeName = Resources.Global.Total;
                summary.InitialBalance = summaryData.Where(d => !d.IsParent).Sum(d => d.InitialBalance);
                summary.Issues = summaryData.Where(d => !d.IsParent).Sum(d => d.Issues);
                summary.IssuesAmount = summaryData.Where(d => !d.IsParent).Sum(d => d.IssuesAmount);
                summary.MaturityBonds = summaryData.Where(d => !d.IsParent).Sum(d => d.MaturityBonds);
                summary.MaturityAmount = summaryData.Where(d => !d.IsParent).Sum(d => d.MaturityAmount);
                summary.EndBalance = summaryData.Where(d => !d.IsParent).Sum(d => d.EndBalance);

                yearsDic.Add(string.Format("{0}", currentEnd.ToString("yyyy-M")), summaryData);
            }

            var groupDic = new Dictionary<string, Dictionary<string, DimSumBondSummary>>();
            foreach (var keyValue in yearsDic)
            {
                foreach (var summary in keyValue.Value)
                {
                    if (summary.Type != typeValue)
                        continue;
                    if (!groupDic.ContainsKey(summary.TypeName))
                        groupDic.Add(summary.TypeName, new Dictionary<string, DimSumBondSummary>());

                    var currentTypeDic = groupDic[summary.TypeName];
                    if (currentTypeDic.ContainsKey(keyValue.Key))
                    {
                        currentTypeDic[keyValue.Key] = summary;
                    }
                    else
                    {
                        currentTypeDic.Add(keyValue.Key, summary);
                    }
                    break;
                }
            }

            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Date, ColumnName = "Date" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.DimSum_Column_IBalance, ColumnName = "InitialBalance" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.DimSum_Column_Issues, ColumnName = "Issues" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.DimSum_Column_IssueAmount, ColumnName = "IssuesAmount" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.DimSum_Column_Maturities, ColumnName = "MaturityBonds" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.DimSum_Column_MaturityAmount, ColumnName = "MaturityAmount" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.DimSum_Column_EndBalance, ColumnName = "EndBalance" });

            foreach (var f in groupDic)
            {
                foreach (var v in f.Value)
                {
                    var currentRow = new Dictionary<string, string>();
                    currentRow.Add("Date", v.Key);
                    currentRow.Add("InitialBalance", UIGenerator.FormatCellValue(v.Value.InitialBalance, "decimal"));
                    currentRow.Add("Issues", UIGenerator.FormatCellValue(v.Value.Issues, "decimal"));
                    currentRow.Add("IssuesAmount", UIGenerator.FormatCellValue(v.Value.IssuesAmount, "decimal"));
                    currentRow.Add("MaturityBonds", UIGenerator.FormatCellValue(v.Value.MaturityBonds, "decimal"));
                    currentRow.Add("MaturityAmount", UIGenerator.FormatCellValue(v.Value.MaturityAmount, "decimal"));
                    currentRow.Add("EndBalance", UIGenerator.FormatCellValue(v.Value.EndBalance, "decimal"));

                    jTable.RowData.Add(currentRow);
                }
            }

            return jTable;
        }

        private JsonTable GetBottomTable(DateTime start, DateTime end, string category, string typeValue, string unit, string term, int id, string order, int startPage, int pageSize)
        {
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var bottomTable = BondInfoRepository.GetDimSumBondDetail(start, end, category, typeValue, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), unit, term, order, startPage, pageSize, out total);
            var jsonTable = BuidJsonTable(bottomTable, columns, total, startPage, pageSize);
            List<Dictionary<string, string>> rowData = jsonTable.RowData;
            foreach (Dictionary<string, string> currentRow in rowData)
            {
                foreach (DataRow row in bottomTable.Rows)
                {
                    if (row["AssetId"].ToString() == currentRow["AssetId"])
                    {
                        if (!columns.Any(x => x.COLUMN_NAME == "IssuerOrgId"))
                        {
                            currentRow.Add("IssuerOrgId", row["IssuerOrgId"].ToString());
                        }
                        break;
                    }
                }
            }
            return jsonTable;
        }

        private ChartData GetChart(string chartType, string dataType, DateTime start, DateTime end, string category, string unit, string rate, out Dictionary<string, IEnumerable<DimSumBondSummary>> yearsDicForBottomChart)
        {
            if (start > end)
            {
                yearsDicForBottomChart = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
                return null;
            } 
            var chartData = new ChartData
            {
                ChartType = chartType,
                YText = Resources.Global.Unit + "(" + HtmlUtil.GetUnitOptionByKey(unit) + ")"
            };
            switch (dataType)
            {
                case "DimSum_Column_IBalance":
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_Issues":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 0;
                    break;
                case "DimSum_Column_IssuesPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_IssueAmount":
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_IssueAmountPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_Maturities":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 0;
                    break;
                case "DimSum_Column_MaturitiesPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_MaturityAmount":
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_MaturityAmountPtn":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "DimSum_Column_EndBalance":
                    chartData.Decimal = 2;
                    break;

            }
            if (chartType == "bar")
            {
                var yearsDic = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
                var dates = PopulateRateTimes(start, end, rate);
                for (var i = 0; i < dates.Count; i += 2)
                {
                    var currentStart = dates[i];
                    var currentEnd = dates[i + 1];
                    var summaryData = BondInfoRepository.GetDimSumBondSummary(currentStart, currentEnd, category, unit, dataType, "all");
                    yearsDic.Add(rate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M"), summaryData);
                }
                yearsDicForBottomChart = yearsDic;
                chartData.ColumnCategories = yearsDic.Keys.Select(x => x.ToString()).ToArray();
                var typeList = new List<dynamic>();
                foreach (var dic in yearsDic)
                {
                    typeList.AddRange(dic.Value.Select(f => new { f.Type, f.TypeName }));
                }
                var types = typeList.Distinct();
                //class/year/data
                var groupDic = new Dictionary<string, Dictionary<string, DimSumBondSummary>>();
                foreach (var keyValue in yearsDic)
                {
                    foreach (var type in types)
                    {
                        var summary = keyValue.Value.Where(re => re.Type == type.Type).FirstOrDefault();
                        if (summary==null)summary=new DimSumBondSummary(){Type = type.Type,TypeName = type.TypeName};
                        if (!groupDic.ContainsKey(summary.TypeName)) groupDic.Add(summary.TypeName, new Dictionary<string, DimSumBondSummary>());
                        var currentTypeDic = groupDic[summary.TypeName];
                        if (currentTypeDic.ContainsKey(keyValue.Key))
                        {
                            currentTypeDic[keyValue.Key] = summary;
                        }
                        else
                        {
                            currentTypeDic.Add(keyValue.Key, summary);
                        }
                    }
                }
                var seriesDataList = new List<SeriesData>();
                foreach (var keyValue in groupDic)
                {
                    var sData = new SeriesData { name = keyValue.Key };
                    if (keyValue.Value != null)
                    {
                        var dataPoints = new List<double>();
                        foreach (var yearSumaryPair in keyValue.Value)
                        {
                            var point = GetChartSeriesDataFromSummary(dataType, yearSumaryPair.Value);
                            dataPoints.Add(point ?? 0);
                        }
                        sData.data = dataPoints.ToArray();
                        seriesDataList.Add(sData);
                    }
                }
                chartData.ColumnSeriesData = seriesDataList.ToArray();
            }
            else
            {
                yearsDicForBottomChart = new Dictionary<string, IEnumerable<DimSumBondSummary>>();
                var groupData = BondInfoRepository.GetDimSumBondSummary( start, end, category, unit,dataType,"all").Where(x => !x.IsParent); ;
                var pieData = new List<PieSectionData>();
                foreach (var dimSumBondSummary in groupData)
                {
                    var currentSection = new PieSectionData { name = dimSumBondSummary.TypeName, y = GetChartSeriesDataFromSummary(dataType, dimSumBondSummary) ?? 0 };
                    pieData.Add(currentSection);
                }
                chartData.PieSeriesData = pieData.ToArray();
            }
            return chartData;
        }

       

        private static double? GetChartSeriesDataFromSummary(string dataType, DimSumBondSummary summary)
        {
            switch (dataType)
            {
                case "DimSum_Column_IBalance":
                    return summary.InitialBalance;
                case "DimSum_Column_Issues":
                    return summary.Issues;
                case "DimSum_Column_IssuesPtn":
                    return summary.IssuesPercent;
                case "DimSum_Column_IssueAmount":
                    return summary.IssuesAmount;
                case "DimSum_Column_IssueAmountPtn":
                    return summary.IssuesAmountPercent;
                case "DimSum_Column_Maturities":
                    return summary.MaturityBonds;
                case "DimSum_Column_MaturitiesPtn":
                    return summary.MaturityBondsPercent;
                case "DimSum_Column_MaturityAmount":
                    return summary.MaturityAmount;
                case "DimSum_Column_MaturityAmountPtn":
                    return summary.MaturityAmountPercent;
                case "DimSum_Column_EndBalance":
                    return summary.EndBalance;
            }
            return null;
        }


        private List<Dictionary<string, string>> ConvertToDic(IEnumerable<DimSumBondSummary> summaryList)
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

            foreach (var bondSummary in summaryList)
            {
                var currentRow = new Dictionary<string, string>();
                var indent = "";

                if (bondSummary.Type == "Gov" || bondSummary.Type == "Agen")
                    indent = "&nbsp;&nbsp;&nbsp;&nbsp;";

                currentRow.Add("Type", UIGenerator.FormatCellValue(bondSummary.Type, ""));
                currentRow.Add("TypeName", indent + UIGenerator.FormatCellValue(bondSummary.TypeName, ""));
                currentRow.Add("InitialBalance", UIGenerator.FormatCellValue(bondSummary.InitialBalance, "decimal"));
                currentRow.Add("Issues", UIGenerator.FormatCellValue(bondSummary.Issues, "decimal"));
                currentRow.Add("IssuesPercent", UIGenerator.FormatCellValue(bondSummary.IssuesPercent, "decimal"));
                currentRow.Add("IssuesAmount", UIGenerator.FormatCellValue(bondSummary.IssuesAmount, "decimal"));
                currentRow.Add("IssuesAmountPercent", UIGenerator.FormatCellValue(bondSummary.IssuesAmountPercent, "decimal"));
                currentRow.Add("MaturityBonds", UIGenerator.FormatCellValue(bondSummary.MaturityBonds, "decimal"));
                currentRow.Add("MaturityBondsPercent", UIGenerator.FormatCellValue(bondSummary.MaturityBondsPercent, "decimal"));
                currentRow.Add("MaturityAmount", UIGenerator.FormatCellValue(bondSummary.MaturityAmount, "decimal"));
                currentRow.Add("MaturityAmountPercent", UIGenerator.FormatCellValue(bondSummary.MaturityAmountPercent, "decimal"));
                currentRow.Add("EndBalance", UIGenerator.FormatCellValue(bondSummary.EndBalance, "decimal"));

                data.Add(currentRow);
            }
            return data;
        }

        private string[] GetDimSumHeader()
        {
            string[] header = null;

            header = new string[] {
                     Resources.Global.DimSum_Column_Type,
                     Resources.Global.DimSum_Column_IBalance,
                     Resources.Global.DimSum_Column_Issues,
                     Resources.Global.DimSum_Column_IssuesPtn,
                     Resources.Global.DimSum_Column_IssueAmount,
                     Resources.Global.DimSum_Column_IssueAmountPtn,
                     Resources.Global.DimSum_Column_Maturities,
                     Resources.Global.DimSum_Column_MaturitiesPtn,
                     Resources.Global.DimSum_Column_MaturityAmount,
                     Resources.Global.DimSum_Column_MaturityAmountPtn,
                     Resources.Global.DimSum_Column_EndBalance,
            };

            return header;
        }

        private string[] GetDimSumColumns()
        {
            string[] cloumns = null;

            cloumns = new string[] {
                        "TypeName",
                        "InitialBalance",
                        "Issues",
                        "IssuesPercent",
                        "IssuesAmount",
                        "IssuesAmountPercent",
                        "MaturityBonds",
                        "MaturityBondsPercent",
                        "MaturityAmount",
                        "MaturityAmountPercent",
                        "EndBalance"
            };

            return cloumns;
        }

        #endregion

        
        [Localization]
        public ActionResult DownloadComprehensiveAnalysisFile()
        {
            return File("~/App_Data/Document/Chinese Treasury Monitor.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Resources.Global.Bond_Future_Comprehensive_Analysis + ".xlsx");
        }

        /// <summary>
        /// Make sure every column is in []
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AddSquareBracket(string value)
        {
            string result = string.Empty;
            if (value.StartsWith("[") && value.EndsWith("]"))
                result = value;
            else
                result = "[" + value + "]";
            return result;
        }

        private string ArrayToString(string array)
        {
            string str = array.Replace("[", "(").Replace("]", ")").Replace("\"", "'");
            return str;
        }

        [Localization]
        public ActionResult GetCityAndLocalBondData(DateTime start, DateTime end, string unit, string ciBondFlag, string issOrMatFlag, string bottomRate, string provOrIssValue, int id, bool isHTML = false)
        {
            var provinceValue = "";
            var topGridEntity = GetCityBondGropData(start, end, unit, ciBondFlag, provOrIssValue);
            var topGrid = BuildCityAndLocalTopTable(topGridEntity,provOrIssValue);
            if (topGridEntity.Count > 0)
            {
                provinceValue = topGridEntity.First().ProvinceKey;
            }
            var bottomGrid = GetCityBondBottomTable(start, end, provinceValue, ciBondFlag, issOrMatFlag, provOrIssValue, id);
            var bottomDetailChart = GetCityBondBottomChartData(start, end, provinceValue, unit, ciBondFlag, bottomRate, provOrIssValue);
            return isHTML ? Json(new { topGrid, bottomGrid, bottomDetailChart }, "text/html", JsonRequestBehavior.AllowGet) : Json(new { topGrid, bottomGrid, bottomDetailChart }, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForCityBondTopData(DateTime start, DateTime end, string unit, string ciBondFlag, string provOrIssValue, int id)
        {
            var topGridEntity = GetCityBondGropData(start, end, unit, ciBondFlag, provOrIssValue);
            return new ExcelResult(topGridEntity.AsEnumerable().AsQueryable(),
                GetCityBondTopTableHeaders(provOrIssValue),
                GetCityBondTopTableColumns(),
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local,
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local);
        }

        [Localization]
        public ActionResult ExportExcelForCityBondBottomData(string provinceValue, DateTime start, DateTime end, string ciBondFlag, string issOrMatFlag, string provOrIssValue, int id)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            if (provinceValue == Resources.Global.Total) provinceValue = "";
            var dataTable = BondInfoRepository.GetCityBondBottomGrid(provinceValue, start, end, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), ciBondFlag, issOrMatFlag, provOrIssValue);
            return new ExcelResult(Resources.Global.Source
                , dataTable.AsEnumerable().AsQueryable(),
                columns.Select(x => x.DisplayName).ToArray(),
                columns.Select(x => x.COLUMN_NAME).ToArray(),
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local,
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local);
        }

        private List<BondCityAndLocal> GetCityBondGropData(DateTime start, DateTime end, string unit, string ciBondFlag, string provOrIssValue)
        {
            var table = DataTableSerializer.ToList<BondCityAndLocal>(BondInfoRepository.GetCityBondTopGrid(start, end, unit, ciBondFlag, provOrIssValue));
            if (table.Count > 0)
            {
                var sum = new BondCityAndLocal
                {
                    ProvinceKey = Resources.Global.Total,
                    ProvinceValue = Resources.Global.Total,
                    EndBalance = table.Sum(re => re.EndBalance),
                    InitialBalance = table.Sum(re => re.InitialBalance),
                    Issues = table.Sum(re => re.Issues),
                    IssuesPercent = 100,
                    IssuesAmount = table.Sum(re => re.IssuesAmount),
                    IssuesAmountPercent = 100,
                    MaturityBonds = table.Sum(re => re.MaturityBonds),
                    MaturityAmount = table.Sum(re => re.MaturityAmount),
                    EndIssues = table.Sum(re => re.EndIssues)
                };
                table.Add(sum);
            }
            return table;
        }

        private string[] GetCityBondTopTableColumns()
        {
            string[] headers = null;
            headers = new string[] {
                        "ProvinceValue",
                        "EndBalance",
                        "InitialBalance",
                        "Issues",
                        "IssuesPercent",
                        "IssuesAmount",
                        "IssuesAmountPercent",
                        "MaturityBonds",
                        "MaturityAmount",
                        "EndIssues"
            };
            return headers;
        }

        private string[] GetCityBondTopTableHeaders(string provOrIssValue)
        {
            string[] cloumns = null;
            cloumns = new string[] {
                        provOrIssValue=="Y"?Resources.Global.WMP_Region:Resources.Global.Bond_Issuer,
                        Resources.Global.BondIssue_End_Balance,
                        Resources.Global.BondIssue_Initial_Balance,
                        Resources.Global.BondIssue_Issues,
                        Resources.Global.BondIssue_Issues_PCT,
                        Resources.Global.BondIssue_Issue_Amt,
                        Resources.Global.BondIssue_Issues_Amt_PCT,
                        Resources.Global.BondIssue_Maturity_Bonds,
                        Resources.Global.BondIssue_Maturity_Amount,
                        Resources.Global.BondIssue_End_Issues
            };
            return cloumns;
        }

        private JsonTable BuildCityAndLocalTopTable(IEnumerable<BondCityAndLocal> table, string provOrIssValue)
        {
            var jTable = new JsonTable();
            var headers = GetCityBondTopTableHeaders(provOrIssValue);
            var columns = GetCityBondTopTableColumns();
            for (int i = 0; i < headers.Length; i++)
            {
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = columns[i], Name = headers[i] });
            }
            foreach (var row in table)
            {
                var currentRow = new Dictionary<string, string>
                {
                    {"ProvinceKey", row.ProvinceKey},
                    {"ProvinceValue", row.ProvinceValue},
                    {"EndBalance", UIGenerator.FormatCellValue(row.EndBalance, "decimal")},
                    {"InitialBalance", UIGenerator.FormatCellValue(row.InitialBalance, "decimal")},
                    {"Issues", row.Issues.ToString()},
                    {"IssuesPercent", UIGenerator.FormatCellValue(row.IssuesPercent, "decimal")},
                    {"IssuesAmount", UIGenerator.FormatCellValue(row.IssuesAmount, "decimal")},
                    {"IssuesAmountPercent", UIGenerator.FormatCellValue(row.IssuesAmountPercent, "decimal")},
                    {"MaturityBonds", row.MaturityBonds.ToString()},
                    {"MaturityAmount", UIGenerator.FormatCellValue(row.MaturityAmount, "decimal")},
                    {"EndIssues", row.EndIssues.ToString()}
                };
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        [Localization]
        public ActionResult GetCityBondBottomChart(DateTime start, DateTime end, string provinceKey, string unit, string ciBondFlag, string bottomRate, string provOrIssValue, int id)
        {
            var topGridDetails = GetCityBondBottomChartData(start, end, provinceKey, unit, ciBondFlag, bottomRate, provOrIssValue);
            return Json(topGridDetails, JsonRequestBehavior.AllowGet);
        }
        private object GetCityBondBottomChartData(DateTime start, DateTime end, string provinceKey, string unit, string ciBondFlag, string bottomRate, string provOrIssValue)
        {
            if (start > end) return null;
            var yearsDic = new Dictionary<string, Dictionary<string, string>>();
            var dates = PopulateRateTimes(start, end, bottomRate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var detailData = BondInfoRepository.GetCityBondTopGrid(currentStart, currentEnd, unit, ciBondFlag, provOrIssValue);
                if (!string.IsNullOrEmpty(provinceKey) && provinceKey != Resources.Global.Total)
                {
                    for (int j = 0; j < detailData.Rows.Count; j++)
                    {
                        if (detailData.Rows[j]["ProvinceKey"].ToString() != provinceKey) continue;
                        var currentRow = new Dictionary<string, string>
                    {
                        {"ProvinceKey", detailData.Rows[j]["ProvinceKey"].ToString()},
                        {"EndBalance", UIGenerator.FormatCellValue(detailData.Rows[j]["EndBalance"], "decimal")},
                        {"InitialBalance", UIGenerator.FormatCellValue(detailData.Rows[j]["InitialBalance"], "decimal")},
                        {"Issues", detailData.Rows[j]["Issues"].ToString()},
                        {"IssuesAmount", UIGenerator.FormatCellValue(detailData.Rows[j]["IssuesAmount"], "decimal")},
                        {"MaturityBonds", detailData.Rows[j]["MaturityBonds"].ToString()},
                        {"MaturityAmount", UIGenerator.FormatCellValue(detailData.Rows[j]["MaturityAmount"], "decimal")},
                        {"EndIssues", detailData.Rows[j]["EndIssues"].ToString()},
                    };
                        yearsDic.Add(string.Format("{0}", bottomRate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M")), currentRow);
                    }
                }
                else
                {
                    decimal endBalance = 0, initialBalance = 0, issuesAmount = 0, maturityAmount = 0;
                    int issues = 0, maturityBonds = 0, endIssues = 0;
                    for (int j = 0; j < detailData.Rows.Count; j++)
                    {
                        endBalance += Convert.ToDecimal(detailData.Rows[j]["EndBalance"]);
                        initialBalance += Convert.ToDecimal(detailData.Rows[j]["InitialBalance"]);
                        issuesAmount += Convert.ToDecimal(detailData.Rows[j]["IssuesAmount"]);
                        maturityAmount += Convert.ToDecimal(detailData.Rows[j]["MaturityAmount"]);
                        issues += Convert.ToInt32(detailData.Rows[j]["Issues"]);
                        endIssues += Convert.ToInt32(detailData.Rows[j]["EndIssues"]);
                        maturityBonds += Convert.ToInt32(detailData.Rows[j]["MaturityBonds"]);
                    }
                    var currentRow = new Dictionary<string, string>
                    {
                        {"ProvinceKey", ""},
                        {"EndBalance", UIGenerator.FormatCellValue(endBalance, "decimal")},
                        {"InitialBalance", UIGenerator.FormatCellValue(initialBalance, "decimal")},
                        {"Issues", issues.ToString()},
                        {"IssuesAmount", UIGenerator.FormatCellValue(issuesAmount, "decimal")},
                        {"MaturityBonds", maturityBonds.ToString()},
                        {"MaturityAmount", UIGenerator.FormatCellValue(maturityAmount, "decimal")},
                        {"EndIssues", endIssues.ToString()}
                    };
                    yearsDic.Add(string.Format("{0}", bottomRate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M")), currentRow);
                }
            }
            return yearsDic;
        }

        [Localization]
        public JsonResult GetCityBondDetailData(DateTime start, DateTime end, string provinceValue, string ciBondFlag, string issOrMatFlag, string provOrIssValue, int id)
        {
            var data = GetCityBondBottomTable(start, end, provinceValue, ciBondFlag, issOrMatFlag, provOrIssValue, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private JsonTable GetCityBondBottomTable(DateTime start, DateTime end, string provinceValue, string ciBondFlag, string issOrMatFlag, string provOrIssValue, int id)
        {
            if (provinceValue == Resources.Global.Total) provinceValue = "";
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var reportColumnDefinitions = columns as REPORTCOLUMNDEFINITION[] ?? columns.ToArray();
            var bottomTable = BondInfoRepository.GetCityBondBottomGrid(provinceValue, start, end, reportColumnDefinitions.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), ciBondFlag, issOrMatFlag, provOrIssValue);
            return BuidJsonTable(bottomTable, reportColumnDefinitions);
        }

        [Localization]
        public FileResult GetCityBondMap()
        {
            string path =
                Server.MapPath(Thread.CurrentThread.CurrentUICulture.Name != "zh-CN"
                    ? "/Scripts/D3/chinaEN.json"
                    : "/Scripts/D3/chinaCN.json");
            return File(path, "application/x-javascript");
        }

        [Localization]
        public ActionResult ExportExcelForCityBondStatisticsChart(DateTime start, DateTime end, string unit, string ciBondFlag, string itemValue, string itemName, string provOrIssValue, int id)
        {
            var topGridEntity = GetCityBondGropData(start, end, unit, ciBondFlag, provOrIssValue);
            return new ExcelResult(topGridEntity.AsEnumerable().AsQueryable(),
                GetCityBondStatisticsHeaders(itemName),
                GetCityBondStatisticsColumns(itemValue),
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local,
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local);
        }

        [JsonpFilter]
        public ActionResult GetBondBasicInfoById(string id, string culture)
        {
            return Json(BondInfoRepository.GetBondBasicInfoById(id, culture), JsonRequestBehavior.AllowGet);
        }

        [JsonpFilter]
        public ActionResult GetBondExchangeCodeById(string id)
        {
            return Json(BondInfoRepository.GetBondExchangeCodeById(id), JsonRequestBehavior.AllowGet);
        }

        private string[] GetCityBondStatisticsColumns(string itemValue)
        {
            string[] headers = null;
            headers = new string[] {
                        "ProvinceValue",
                        itemValue
            };
            return headers;
        }

        private string[] GetCityBondStatisticsHeaders(string itemName)
        {
            string[] cloumns = null;
            cloumns = new string[] {
                        Resources.Global.WMP_Region,
                        itemName
            };
            return cloumns;
        }

        [Localization]
        public ActionResult ExportExcelForCityBondGraph(DateTime start, DateTime end, string provinceValue, string unit, string ciBondFlag, string itemValue, string itemName, string bottomRate, string provOrIssValue)
        {
            var topGridDetails = GetCityBondBottomTableData(start, end, provinceValue, unit, ciBondFlag, bottomRate, itemValue, provOrIssValue);
            return new ExcelResult(Resources.Global.Source
                , topGridDetails.AsEnumerable().AsQueryable(),
                GetCityBondGraphHeaders(itemName),
                GetCityBondGraphColumns(itemValue),
                ciBondFlag == "Y" ? Resources.Global.CityBondInfo_City : Resources.Global.CityBondInfo_Local,
                provinceValue);
        }
        private string[] GetCityBondGraphColumns(string itemValue)
        {
            string[] headers = null;
            headers = new string[] {
                        "reDate",
                        itemValue
            };
            return headers;
        }

        private string[] GetCityBondGraphHeaders(string itemName)
        {
            string[] cloumns = null;
            cloumns = new string[] {
                        Resources.Global.Date,
                        itemName
            };
            return cloumns;
        }

        private DataTable GetCityBondBottomTableData(DateTime start, DateTime end, string provinceKey, string unit, string ciBondFlag, string bottomRate, string itemValue, string provOrIssValue)
        {
            if (start > end) return null;
            var yearsDic = new DataTable();
            yearsDic.Columns.Add("reDate");
            yearsDic.Columns.Add(itemValue);
            var dates = PopulateRateTimes(start, end, bottomRate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var detailData = BondInfoRepository.GetCityBondTopGrid(currentStart, currentEnd, unit, ciBondFlag, provOrIssValue);
                if (!string.IsNullOrEmpty(provinceKey) && provinceKey != Resources.Global.Total)
                {
                    for (int j = 0; j < detailData.Rows.Count; j++)
                    {
                        if (detailData.Rows[j]["ProvinceKey"].ToString() != provinceKey) continue;
                        DataRow dr = yearsDic.NewRow();
                        dr[0] = string.Format("{0}",
                            bottomRate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M"));
                        string item = "";
                        switch (itemValue)
                        {
                            case "EndBalance":
                                item = UIGenerator.FormatCellValue(detailData.Rows[j]["EndBalance"], "decimal");
                                break;
                            case "InitialBalance":
                                item = UIGenerator.FormatCellValue(detailData.Rows[j]["InitialBalance"], "decimal");
                                break;
                            case "Issues":
                                item = detailData.Rows[j]["Issues"].ToString();
                                break;
                            case "IssuesAmount":
                                item = UIGenerator.FormatCellValue(detailData.Rows[j]["IssuesAmount"], "decimal");
                                break;
                            case "MaturityBonds":
                                item = detailData.Rows[j]["MaturityBonds"].ToString();
                                break;
                            case "MaturityAmount":
                                item = UIGenerator.FormatCellValue(detailData.Rows[j]["MaturityAmount"], "decimal");
                                break;
                            case "EndIssues":
                                item = detailData.Rows[j]["EndIssues"].ToString();
                                break;
                        }
                        dr[1] = item;
                        yearsDic.Rows.Add(dr);
                    }
                }
                else
                {
                    string item = "";
                    decimal count = 0;
                    DataRow dr = yearsDic.NewRow();
                    dr[0] = string.Format("{0}",
                         bottomRate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M"));
                    for (int j = 0; j < detailData.Rows.Count; j++)
                    {
                        count += Convert.ToDecimal(detailData.Rows[j][itemValue]);
                    }
                    switch (itemValue)
                    {
                        case "EndBalance":
                        case "MaturityAmount":
                        case "InitialBalance":
                        case "IssuesAmount":
                            item = UIGenerator.FormatCellValue(count, "decimal");
                            break;
                        case "Issues":
                        case "EndIssues":
                        case "MaturityBonds":
                            item = count.ToString();
                            break;
                    }
                    dr[1] = item;
                    yearsDic.Rows.Add(dr);
                }
            }
            return yearsDic;
        }

        #region UnderWriterRanking

        [Localization]
        public ActionResult GetUnderWriterAnalysis(string bondClass, DateTime start, DateTime end, string unit, string order)
        {
            var topGridEntity = GetUnderWriterAnalysisData(bondClass, start, end, unit, order);
            var topGrid = BuildUnderWriterTopTable(topGridEntity);
            return Json(topGrid, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetUnderwritersBond(string undwrtId, string bondClass, DateTime start, DateTime end, int id)
        {

            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var dataTable = BondInfoRepository.GetUnderWriterBond(bondClass, start, end, undwrtId);
            return Json(BuidJsonTable(dataTable, columns), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetUnderwritersBottomChart(string undwrtId, string bondClass, DateTime start, DateTime end, string unit, string bottomRate)
        {
            var bottomDetailChart = GetUnderwritersBottomChartData(bondClass, start, end, undwrtId, unit, bottomRate);
            return Json(bottomDetailChart, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForUnderwritersAnalysis(string bondClass, DateTime start, DateTime end, string unit, string order)
        {
            var topGridEntity = GetUnderWriterAnalysisData(bondClass, start, end, unit, order);
            return new ExcelResult(topGridEntity.AsEnumerable().AsQueryable(),
                GetUnderWriterTopTableHeaders(),
                GetUnderWriterTopTableColumns(),
                Resources.Global.Underwriters_Title,
                Resources.Global.Underwriters_Title);
        }

        [Localization]
        public ActionResult ExportExcelForUnderwritersBond(string bondClass, DateTime start, DateTime end, string undwrtId, int id, string undwrtName)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var dataTable = BondInfoRepository.GetUnderWriterBond(bondClass, start, end, undwrtId);
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(), columns.Select(x => x.DisplayName).ToArray(), columns.Select(x => x.COLUMN_NAME).ToArray(), undwrtName, undwrtName);
        }

        [Localization]
        public ActionResult ExportExcelForUnderwritersGraph(string undwrtId, string undwrtName, string bondClass, DateTime start, DateTime end, string unit, string bottomRate, string itemValue, string itemName)
        {
            var graphDetails = GetUnderwritersDetailData(bondClass, start, end, undwrtId, unit, bottomRate, itemValue);
            return new ExcelResult(Resources.Global.Source
                , graphDetails.AsEnumerable().AsQueryable(),
                GetCityBondGraphHeaders(itemName),
                GetCityBondGraphColumns(itemValue),
                undwrtName,
                undwrtName);
        }

        private DataTable GetUnderwritersDetailData(string bondClass, DateTime start, DateTime end, string undwrtId,
            string unit, string bottomRate, string itemValue)
        {
            if (start > end) return null;
            var yearsDic = new DataTable();
            yearsDic.Columns.Add("reDate");
            yearsDic.Columns.Add(itemValue);
            var dates = PopulateRateTimes(start, end, bottomRate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var detailData = BondInfoRepository.GetUnderWriterDetail(bondClass, currentStart, currentEnd, unit,
                    undwrtId);
                for (int j = 0; j < detailData.Rows.Count; j++)
                {
                    DataRow dr = yearsDic.NewRow();
                    dr[0] = string.Format("{0}",
                        bottomRate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M"));
                    string item = "";
                    switch (itemValue)
                    {
                        case "UnderWr_Amount":
                            item = UIGenerator.FormatCellValue(detailData.Rows[j]["underWr_amount"], "decimal");
                            break;
                        case "Issues":
                            item = detailData.Rows[j]["issues"].ToString();
                            break;
                    }
                    dr[1] = item;
                    yearsDic.Rows.Add(dr);
                }

            }
            return yearsDic;
        }

        private object GetUnderwritersBottomChartData(string bondClass, DateTime start, DateTime end, string undwrtId,
            string unit, string bottomRate)
        {
            if (start > end) return null;
            var yearsDic = new Dictionary<string, Dictionary<string, string>>();
            var dates = PopulateRateTimes(start, end, bottomRate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var detailData = BondInfoRepository.GetUnderWriterDetail(bondClass, currentStart, currentEnd, unit,
                    undwrtId);

                for (int j = 0; j < detailData.Rows.Count; j++)
                {
                    var currentRow = new Dictionary<string, string>
                    {
                        {"UndwrtId", detailData.Rows[j]["undwrt_Id"].ToString()},
                        {"UnderWr_Amount", UIGenerator.FormatCellValue(detailData.Rows[j]["underWr_amount"], "decimal")},
                        {"Issues", detailData.Rows[j]["issues"].ToString()},

                    };
                    yearsDic.Add(
                        string.Format("{0}",
                            bottomRate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M")), currentRow);
                }
            }
            return yearsDic;
        }

        private List<UnderWriterRankingData> GetUnderWriterAnalysisData(string bondClass, DateTime start, DateTime end, string unit, string order)
        {
            int issues;
            var table = DataTableSerializer.ToList<UnderWriterRankingData>(BondInfoRepository.GetUnderWriterAnalysis(bondClass, start, end, unit, order, out issues));
            if (table.Count > 0)
            {
                var sum = new UnderWriterRankingData
                {
                    Undwrt_Id = Resources.Global.Total,
                    Undwrt_Long_Name = Resources.Global.Total,
                    UnderWr_Amount = table.Sum(re => re.UnderWr_Amount),
                    UnderWr_pert = 100,
                    Issues = issues
                };
                table.Add(sum);
            }
            return table;
        }

        private JsonTable BuildUnderWriterTopTable(IEnumerable<UnderWriterRankingData> table)
        {
            var jTable = new JsonTable();
            var headers = GetUnderWriterTopTableHeaders();
            var columns = GetUnderWriterTopTableColumns();
            for (int i = 0; i < headers.Length; i++)
            {
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = columns[i], Name = headers[i] });
            }
            foreach (var row in table)
            {
                var currentRow = new Dictionary<string, string>
                {
                    {"Undwrt_Id", row.Undwrt_Id},
                    {"Undwrt_Long_Name", row.Undwrt_Long_Name},
                    {"Row_Num", row.Row_Num.ToString()},
                    {"UnderWr_Amount", UIGenerator.FormatCellValue(row.UnderWr_Amount, "decimal")},
                    {"UnderWr_pert", UIGenerator.FormatCellValue(row.UnderWr_pert, "decimal")},
                    {"Issues", row.Issues.ToString()}
                };
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        private string[] GetUnderWriterTopTableColumns()
        {
            string[] headers = null;
            headers = new string[] {
                        "Undwrt_Long_Name",
                        "Row_Num", 
                        "UnderWr_Amount",
                        "UnderWr_pert",
                        "Issues"
            };
            return headers;
        }

        private string[] GetUnderWriterTopTableHeaders()
        {
            string[] cloumns = null;
            cloumns = new string[] {
                        Resources.Global.Underwriters_Title,
                        Resources.Global.Underwriters_Ranking,
                        Resources.Global.Underwriters_Amount,
                        Resources.Global.Underwriters_Amount_PCT,
                        Resources.Global.Underwriters_Issues
            };
            return cloumns;
        }
        #endregion

        #region AbsList

        [Localization]
        public ActionResult GetAbsListData(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string optionClass, string bondRating, string isBondCode, string bondCodeOrIss, int id, int startPage, int pageSize)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetAbsBondList(startDate, endDate, bondClass, couponClass, optionClass, bondRating, isBondCode, bondCodeOrIss, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), startPage, pageSize, out total);
            return Json(BuidJsonTableAbs(dataTable, columns, dataTable.Rows.Count == 0 ? 0 : total, startPage, pageSize), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForAbsList(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string optionClass, string bondRating, string isBondCode, string bondCodeOrIss, int id, int startPage = 1, int pageSize = 2000)
        {
            var reportName = MenuService.GetMenuNodeByReportId(id).DisplayName;
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var dataTable = BondInfoRepository.GetAbsBondList(startDate, endDate, bondClass, couponClass, optionClass, bondRating, isBondCode, bondCodeOrIss, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), startPage, pageSize, out total);
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(), columns.Select(x => x.DisplayName).ToArray(), columns.Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName);
        }


        #endregion

        #region ABS Detail
        [Localization]
        public ActionResult ABSDetail(long id)
        {
            var issuerRatingOrgHis =BondInfoRepository.GetIssuerRatingHisByUniCode(id);
            if (issuerRatingOrgHis.Count > 0)
            {
                var idlist =
                    issuerRatingOrgHis.Select(r => r.RATE_ID.ToString()).ToList().Aggregate((a, b) => a + "," + b).ToString();
                var dataTable = CMARepository.CheckCommonFileExsit(idlist, "RATE_REP_DATA", "RATE_ID");

                foreach (DataRow row in dataTable.Rows)
                    issuerRatingOrgHis.Where(r => r.RATE_ID == Convert.ToInt64(row["RATE_ID"]))
                        .ToList()
                        .ForEach(r => r.ContainFile = true);
            }

            var vBond = BondInfoRepository.GetBondInfoByUnicode(id);
            var ratingHistData = BondInfoRepository.GetBondrRatingHisByUniCode(id);
            var rateIdlist = ratingHistData.Select(r => r.RATE_ID.ToString()).ToList();

            if (rateIdlist.Count() != 0)
            {
                var ids = rateIdlist.Aggregate((a, b) => a + "," + b).ToString();
                var dataTable = CMARepository.CheckCommonFileExsit(ids, "RATE_REP_DATA", "RATE_ID");

                if (dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                        ratingHistData.Where(r => r.RATE_ID == Convert.ToInt64(row["RATE_ID"])).ToList().ForEach(r => r.ContainFile = true);
                }
            }
            var detailViewModel = new ABSDetailViewModel
            {
                VBondAbs = vBond,
                ListBondSizeChan = BondInfoRepository.GetBondSizeChans(id),
                ListRateOrgCredHis = issuerRatingOrgHis,
                BondRatingHist = ratingHistData
            };
            return PartialView(detailViewModel);
        }
        #endregion
    }
}
