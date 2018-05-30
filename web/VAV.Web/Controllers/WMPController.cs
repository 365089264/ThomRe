using System;
using System.IO;
using System.Web.Mvc;
using System.Data;
using System.Linq;
using Aspose.Cells;
using Microsoft.Practices.Unity;
using Resources;
using VAV.DAL.IPP;
using VAV.DAL.ResearchReport;
using VAV.Web.Common;
using VAV.Web.Localization;
using VAV.DAL.WMP;
using VAV.Model.Data.WMP;
using VAV.Web.ViewModels.WMP;
using VAV.DAL.Services;
using VAV.Model.Data;
using VAV.Entities;
using System.Collections.Generic;
using VAV.Web.Extensions;
using System.Configuration;
using VAV.Model.Chart;
using System.Diagnostics;

namespace VAV.Web.Controllers
{
    public class WMPController : BaseController
    {
        const int PAGE_SIZE = 50;

        [Dependency]
        public WMPRepository WMPRepository { get; set; }

        [Dependency]
        public ResearchReportRepository ResearchReportRepository { get; set; }

        [Dependency]
        public UserColumnService UserColumnService { get; set; }

        #region Bank WMP
        [Localization]
        public JsonResult GetWMPBankData(bool includeTimeSpan, string bankType, string bank, int currency, string yieldType, string prodSate,
                                        string term, string initAmount, string investType, string yield, DateTime startDate,
                                        DateTime endDate, string prodName, string IS_QDII,string area, string order, int id, int currentPage, int pageSize
            , bool isHTML = false)
        {

            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);

            int total = 0;
            var data = wmpRepository.GetWmpBankDataPaging(includeTimeSpan, bankType, string.IsNullOrEmpty(bank) ? "0" : bank, currency, yieldType, prodSate,
                                        term, initAmount, investType, yield, startDate,
                                        endDate, prodName.Trim(), IS_QDII, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                        order, currentPage, pageSize,area, out total);

            if (columns.Any(x => x.COLUMN_NAME.Equals("ACCE_ROUTE")))
            {
                var view = new DataView(data);
                var myRows = view.ToTable(false, "INNER_CODE").Select();
                var idlist = myRows.Select(x => x["INNER_CODE"]).ToList();
                if (idlist.Count > 0)
                {
                    var filecheckdt =
                        ResearchReportRepository.CheckBankFile(idlist.Aggregate((a, b) => a + "," + b).ToString());
                    data.PrimaryKey = new[] { data.Columns["INNER_CODE"] };
                    filecheckdt.PrimaryKey = new[] { filecheckdt.Columns["INNER_CODE"] };
                    data.Merge(filecheckdt, true, MissingSchemaAction.Ignore);
                }
            }

            var jtable = BuidJsonTable(data, columns, currentPage, pageSize, order);
            jtable.Total = total;
            return isHTML ? Json(jtable, "text/html", JsonRequestBehavior.AllowGet) : Json(jtable, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult BankWMPDetail(string id)
        {
            var innerCode = int.Parse(id.Replace("WMPD", ""));
            var viewModel = new BankWMPDetailViewModel(innerCode, WMPRepository);
            return PartialView(viewModel);
        }

        [Localization]
        public ActionResult DonwloadBankWMPFile(int id)
        {
            var file = WMPRepository.GetBankFileData(id);
            if (file == null)
                return HttpNotFound();
            if (file.Content == null)
                return HttpNotFound();
            var contentType = MimeHelper.GetMimeTypeByExt(file.FileType);
            return File(file.Content, contentType,
                Resources.Global.DownloadFileName + "." + file.FileType);
        }

        [Localization]
        public ActionResult GetBankOptionByType(string typeCode)
        {
            List<WMPBankOption> options = new List<WMPBankOption>() { new WMPBankOption { TypeCode = typeCode, BankId = "all", BankName = Resources.Global.WMP_All } };
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            IEnumerable<WMPBankOption> ret = wmpRepository.GetWmpBankOptionByType(typeCode);
            options = options.Union(ret.ToList()).ToList();
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetMultipleBankOptionByType(string typeCode)
        {
            List<WMPBankOption> options = new List<WMPBankOption>() { };
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            IEnumerable<WMPBankOption> ret = wmpRepository.GetWmpBankOptionByType(typeCode);
            options = options.Union(ret.ToList()).ToList();
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetMultipleCityOptionByType(string regionCode)
        {
            List<WMPCityOption> options = new List<WMPCityOption>() { };
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            IEnumerable<WMPCityOption> ret = wmpRepository.GetWmpCityOption(regionCode);
            options = options.Union(ret.ToList()).ToList();
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelFoWMPBankData(bool includeTimeSpan, string bankType, string bank, int currency, string yieldType, string prodSate,
                                        string term, string initAmount, string investType, string yield, DateTime startDate,
                                        DateTime endDate, string prodName, string IS_QDII, string order, int id, int currentPage,string area)
        {
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);

            int total = 0;
            var data = wmpRepository.GetWmpBankDataPaging(includeTimeSpan, bankType, bank, currency, yieldType, prodSate,
                                        term, initAmount, investType, yield, startDate,
                                        endDate, prodName, IS_QDII, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                        order, 1, 2000, area, out total);

            return new ExcelResult(data.AsEnumerable().AsQueryable(), columns.Select(x => x.DisplayName).ToArray(), columns.Select(x => x.COLUMN_NAME).ToArray(), Resources.Global.WMP_ExcelOutputTitle, Resources.Global.WMP_ExcelOutputTitle, false, null, null, false, specificDateFormat: "yyyy-MM-dd");
        }
        #endregion

        #region WMP Research Report
        [Localization]
        public ActionResult GetWMPReport(DateTime startDate, DateTime endDate, int type, int pageNo, int pageSize, bool isHTML = false)
        {
            int total;
            var reports = WMPRepository.GetWmpReport(startDate, endDate, type, pageNo, pageSize, out total);
            var result = new
            {
                Data = reports,
                Total = total,
                CurrentPage = pageNo,
                PageSize = pageSize
            };
            return isHTML ? Json(result, "text/html", JsonRequestBehavior.AllowGet) : Json(result, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForWMPReport(DateTime startDate, DateTime endDate, int type, string reportName)
        {
            int total;
            var data = WMPRepository.GetWmpReport(startDate, endDate, type, -1, 0, out total);
            return new ExcelResult(data.AsQueryable(), GetWMPResearchReportHeader(), GetWMPResearchReportHeaderColumns(), workSheetName: reportName);
        }

        public ActionResult DownloadResearchReport(int reportId)
        {
            var file = WMPRepository.GetReportData(reportId);
            if (file == null)
                return HttpNotFound();
            if (file.Content == null)
                return HttpNotFound();
            var contentType = MimeHelper.GetMimeTypeByExt(file.FileType);
            return File(file.Content, contentType,
                Resources.Global.DownloadFileName + "." + file.FileType);
        }

        private string[] GetWMPResearchReportHeader()
        {
            string[] header = null;

            header = new string[] {
                     Resources.Global.BankWMP_Research_ReportTitle,
                     Resources.Global.BankWMP_Research_ReportType,
                     Resources.Global.BankWMP_Research_ReportDate,
                     Resources.Global.BankWMP_Research_ReportAuthor,
            };

            return header;
        }

        private string[] GetWMPResearchReportHeaderColumns()
        {
            string[] cloumns = null;

            cloumns = new string[] {
                        "RPT_TITLE",
                        "RPT_TYPE",
                        "WRITEDATE",
                        "RPT_SRC",
            };

            return cloumns;
        }

        #endregion

        #region Issue Trend
        /// <summary>
        /// Gets the issue trend data.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="category">The category.EY=Expect Yield; IBT=Invest Bid Type; PT=ProdTerm; C=Currency; YT=Yield Type</param>
        /// <param name="issuer">The issuer.</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        /// <returns></returns>
        [Localization]
        public JsonResult GetIssueTrendData(DateTime start, DateTime end, string category, string issuer, string area, int id, bool isHTML = false)
        {
            var topTable = WMPRepository.GetAmountTrendData(start, end, category, issuer, area);
            var topJsonTable = BuildIssueTrendTopTable(topTable, category);
            var filter = topJsonTable.RowData[0]["Type"];
            int total;
            var bottomJsonTable = GetIssueTrendBottomTable(start, end, category, issuer, area, filter, 1, PAGE_SIZE, id, out total);
            bottomJsonTable.Total = total;
            var bottomChart = GetIssueTrendBottomChartJsonObject(start, end, category, issuer, area, "q");
            var data = new IssueTrendData(topJsonTable, bottomChart, bottomJsonTable);
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public JsonResult GetIssueTrendDetailTable(DateTime start, DateTime end, string category, string issuer, string area, string filter, int currentPage, int id)
        {
            int total;
            var table = GetIssueTrendBottomTable(start, end, category, issuer, area, filter, currentPage, PAGE_SIZE, id, out total);
            table.Total = total;
            return Json(table, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the issue trend bottom chart.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="category">The category.</param>
        /// <param name="issuer">The issuer.</param>
        /// <param name="period">Y=Year,H=Half Year,Q=Quarter,M=Month</param>
        /// <returns></returns>
        [Localization]
        public JsonResult GetIssueTrendBottomChart(DateTime start, DateTime end, string category, string issuer, string area, string period)
        {
            return Json(GetIssueTrendBottomChartJsonObject(start, end, category, issuer, area, period), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Return a chart object
        /// </summary>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date</param>
        /// <param name="category">Category</param>
        /// <param name="issuer"></param>
        /// <param name="period">m=month,q=quarter,h=half year,y=year</param>
        /// <returns></returns>
        private object GetIssueTrendBottomChartJsonObject(DateTime start, DateTime end, string category, string issuer, string area, string period)
        {
            var chartJsonData = new ChartJsonData();

            var yearsDic = new Dictionary<string, DataTable>();
            var dates = PopulateRateTimes(start, end, period);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var summaryData = WMPRepository.GetTrendCountData(currentStart, currentEnd, category, issuer, area);
                yearsDic.Add(string.Format("{0}", currentEnd.ToString("yyyy-M")), summaryData);
            }
            chartJsonData.ColumnCategories = yearsDic.Keys.Select(x => x.ToString()).ToArray();
            var groupDic = new Dictionary<string, Dictionary<string, int>>();
            foreach (var keyValue in yearsDic)
            {
                foreach (DataRow row in keyValue.Value.Rows)
                {
                    if (!groupDic.ContainsKey(row["Type"].ToString())) groupDic.Add(row["Type"].ToString(), new Dictionary<string, int>());
                    var currentTypeDic = groupDic[row["Type"].ToString()];
                    if (currentTypeDic.ContainsKey(keyValue.Key))
                    {
                        currentTypeDic[keyValue.Key] = Convert.ToInt32(row["Count"]);
                    }
                    else
                    {
                        currentTypeDic.Add(keyValue.Key, Convert.ToInt32(row["Count"]));
                    }
                }
            }
            var seriesDataList = new List<IntSeriesData>();
            foreach (var keyValue in groupDic)
            {
                var sData = new IntSeriesData { name = ResolveTypeNameForTrendTopTable(category, keyValue.Key) };
                if (keyValue.Value != null)
                {
                    var dataPoints = new List<int>();
                    foreach (var yearSumaryPair in keyValue.Value)
                    {
                        dataPoints.Add(yearSumaryPair.Value);
                    }
                    sData.data = dataPoints.ToArray();
                    seriesDataList.Add(sData);
                }
            }
            chartJsonData.SeriesData = seriesDataList.ToArray();

            return chartJsonData.ToJson();
        }

        private new List<DateTime> PopulateRateTimes(DateTime start, DateTime end, string rate)
        {
            var result = new List<DateTime>();
            if (rate.Equals("m"))
            {
                result.Add(start);
                start = start.AddDays(1 - start.Day);
                while (start.AddMonths(1) < end)
                {
                    result.Add(start.AddMonths(1).AddDays(-1));
                    result.Add(start.AddMonths(1));
                    start = start.AddMonths(1);
                }
                result.Add(end);
                return result;
            }
            if (rate.Equals("q"))
            {
                result.Add(start);
                start = start.AddDays(1 - start.Day).AddMonths(0 - (start.Month-1) % 3);
                while (start.AddMonths(3) < end)
                {
                    result.Add(start.AddMonths(3).AddDays(-1));
                    result.Add(start.AddMonths(3));
                    start = start.AddMonths(3);
                }
                result.Add(end);
                return result;
            }
            if (rate.Equals("h"))
            {
                result.Add(start);
                start = start.AddDays(1 - start.Day).AddMonths(0 - (start.Month - 1) % 6);
                while (start.AddMonths(6) < end)
                {
                    result.Add(start.AddMonths(6).AddDays(-1));
                    result.Add(start.AddMonths(6));
                    start = start.AddMonths(6);
                }
                result.Add(end);
                return result;
            }
            if (rate.Equals("y"))
            {
                result.Add(start);
                start = start.AddMonths(1 - start.Month).AddDays(1 - start.Day);
                while (start.AddYears(1) < end)
                {
                    result.Add(start.AddYears(1).AddDays(-1));
                    result.Add(start.AddYears(1));
                    start = start.AddYears(1);
                }
                result.Add(end);
                return result;
            }
            return result;
        }


        private JsonTable GetIssueTrendBottomTable(DateTime start, DateTime end, string category, string issuer, string area, string filter, int currentPage, int pageSize, int id, out int total)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            string yieldType = "all", term = "all", investType = "all", yield = "all", currency = "all";
            switch (category)
            {
                case "EY":
                    yield = filter;
                    break;
                case "IBT":
                    investType = filter;
                    break;
                case "PT":
                    term = filter;
                    break;
                case "C":
                    currency = filter;
                    break;
                case "YT":
                    yieldType = filter;
                    break;
                default:
                    break;
            }
            var bottomTableData =
                WMPRepository.GetTrendBottomTable(start,
                                                    end,
                                                    issuer,
                                                    area,
                                                    currency,
                                                    yieldType,
                                        term,
                                        investType,
                                        yield,
                                        columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                         currentPage, pageSize, out total);
            return BuidJsonTable(bottomTableData, columns, currentPage, pageSize, null);
        }

        private JsonTable BuildIssueTrendTopTable(DataTable table, string category)
        {
            var jTable = new JsonTable();

            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "TypeName", Name = Resources.Global.DimSum_Column_Type });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "Count", Name = Resources.Global.DimSum_Column_Issues });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "CountPercent", Name = Resources.Global.DimSum_Column_IssuesPtn });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "Amount", Name = Resources.Global.WMP_Trend_Column_IssueAmount });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "AmountPercent", Name = Resources.Global.DimSum_Column_IssueAmountPtn });

            int totalCount = 0;
            decimal totalAmount = 0;

            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                currentRow.Add("Type", row["Type"].ToString());
                currentRow.Add("TypeName", ResolveTypeNameForTrendTopTable(category, row["Type"].ToString()));
                currentRow.Add("Count", Convert.ToInt32(row["Count"]).ToString("n0"));
                currentRow.Add("CountPercent", string.Format("{0:0.00}", row["CountPercent"]));
                currentRow.Add("Amount", ((decimal)row["Amount"]).ToString("n2"));
                currentRow.Add("AmountPercent", string.Format("{0:0.00}", row["AmountPercent"]));
                jTable.RowData.Add(currentRow);
                totalCount += Convert.ToInt32(row["Count"]);
                totalAmount += (decimal)row["Amount"];
            }

            var totalRow = new Dictionary<string, string>();
            totalRow.Add("Type", "all");
            totalRow.Add("TypeName", Resources.Global.Total);
            totalRow.Add("Count", totalCount.ToString("n0"));
            totalRow.Add("CountPercent", "100");
            totalRow.Add("Amount", totalAmount.ToString("n2"));
            totalRow.Add("AmountPercent", "100");
            jTable.RowData.Add(totalRow);

            return jTable;
        }

        private string ResolveTypeNameForTrendTopTable(string category, string type)
        {
            return Resources.Global.ResourceManager.GetString(string.Format("WMP_Trend_{0}_{1}", category, type));
        }

        [Localization]
        public ActionResult ExportExcelWMPIssueTrendSummary(DateTime start, DateTime end, string category, string issuer, string area, string reportName)
        {
            var topTable = WMPRepository.GetAmountTrendData(start, end, category, issuer, area);
            var topJsonTable = BuildIssueTrendTopTable(topTable, category);
            var jP = new JsonExcelParameter { Table = topJsonTable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public ActionResult ExportExcelWMPIssueTrendDetails(DateTime start, DateTime end, string category, string issuer, string area, string reportName, string filter, int id, int currentPage)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            string yieldType = "all", term = "all", investType = "all", yield = "all", currency = "all";
            switch (category)
            {
                case "EY":
                    yield = filter;
                    break;
                case "IBT":
                    investType = filter;
                    break;
                case "PT":
                    term = filter;
                    break;
                case "C":
                    currency = filter;
                    break;
                case "YT":
                    yieldType = filter;
                    break;
                default:
                    break;
            }
            int total;
            var data =
                WMPRepository.GetTrendBottomTable(start,
                                                    end,
                                                    issuer,
                                                    area,
                                                    currency,
                                                    yieldType,
                                        term,
                                        investType,
                                        yield,
                                        columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                         1, 2000, out total);

            return new ExcelResult(data.AsEnumerable().AsQueryable(), columns.Select(x => x.DisplayName).ToArray(), columns.Select(x => x.COLUMN_NAME).ToArray(), Resources.Global.WMP_ExcelOutputTitle, Resources.Global.WMP_ExcelOutputTitle, false, null, null, false, specificDateFormat: "yyyy-MM-dd");
        }


        #endregion

        #region Yield Trend

        /// <summary>
        /// Get yield trend chart and detail data
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="bankType">Bank type</param>
        /// <param name="bank">Bank</param>
        /// <param name="currency">Currency</param>
        /// <param name="yieldType">Yield type</param>
        /// <param name="investType">Invest type</param>
        /// <param name="term">Term,default: check 1,show detail</param>
        /// <param name="id">Page id for cloumns</param>
        /// <param name="currentPage">page number</param>
        /// <param name="isHTML">if set to <c>true</c> [is HTML].</param>
        /// <returns>
        /// Json object contains high chart data and data detail
        /// </returns>
        [Localization]
        public ActionResult GetYieldTrendData(DateTime start, DateTime end, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType, string term, int id, int currentPage = 1, bool isHTML = false)
        {
            int total;
            var jsonTable = GetYieldTrendTable(start, end, bankType, bank, prodName.Trim(), area, currency, yieldType, investType, term.Split(',').First(), id, currentPage, PAGE_SIZE, out total);
            jsonTable.Total = total;
            var chartJsonData = GetYieldTrendChart(start, end, bankType, bank,prodName,area, currency, yieldType, investType, term.Split(','));
            var data = new { chart = chartJsonData, table = jsonTable };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get yield trend chart data
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="bankType">Bank type</param>
        /// <param name="bank">Bank</param>
        /// <param name="currency">Currency</param>
        /// <param name="yieldType">Yield type</param>
        /// <param name="investType">Invest type</param>
        /// <param name="term">Term</param>
        /// <returns>Json object contains high chart data</returns>
        [Localization]
        public ActionResult GetYieldTrendChartData(DateTime start, DateTime end, string term, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType)
        {
            var chartJsonData = GetYieldTrendChart(start, end, bankType, bank,prodName,area, currency, yieldType, investType, term.Split(','));
            return Json(chartJsonData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get yield trend chart and detail data
        /// </summary>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <param name="bankType">Bank type</param>
        /// <param name="bank">Bank</param>
        /// <param name="currency">Currency</param>
        /// <param name="yieldType">Yield type</param>
        /// <param name="investType">Invest type</param>
        /// <param name="term">Term</param>
        /// <param name="id">Page id for cloumns</param>
        /// <param name="currentPage">page number</param>
        /// <param name="pageSize">default 50</param>
        /// <returns>Json object contains data detail</returns>
        [Localization]
        public ActionResult GetYieldTrendDetail(DateTime start, DateTime end, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType, string term, int id, int currentPage)
        {
            int total;
            var jsonTable = GetYieldTrendTable(start, end, bankType, bank, prodName, area, currency, yieldType, investType, term, id, currentPage, PAGE_SIZE, out total);
            jsonTable.Total = total;
            return Json(jsonTable, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="bankType"></param>
        /// <param name="bank"></param>
        /// <param name="currency"></param>
        /// <param name="yieldType"></param>
        /// <param name="investType"></param>
        /// <param name="term"></param>
        /// <param name="id"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Localization]
        public ActionResult ExportExcelForYieldTrendDetail(DateTime start, DateTime end, string bankType, string bank, string currency, string yieldType, string investType, string term, int id, int currentPage, string prodName,string area)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var data =
                WMPRepository.GetYieldTrendDetail(start, end, bankType, bank, prodName, area, currency, yieldType, investType, term, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), 1, 2000, out total);
            return new ExcelResult(data.AsEnumerable().AsQueryable(), columns.Select(x => x.DisplayName).ToArray(), columns.Select(x => x.COLUMN_NAME).ToArray(), Resources.Global.WMP_ExcelOutputTitle, Resources.Global.WMP_ExcelOutputTitle, false, null, null, false, specificDateFormat: "yyyy-MM-dd"); ;
        }

        /// <summary>
        /// Get detail
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="bankType"></param>
        /// <param name="bank"></param>
        /// <param name="currency"></param>
        /// <param name="yieldType"></param>
        /// <param name="investType"></param>
        /// <param name="term"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private JsonTable GetYieldTrendTable(DateTime start, DateTime end, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType, string term, int id, int currentPage, int pageSize,out int total)
        {
            var columns = UserColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var tableData =
                WMPRepository.GetYieldTrendDetail(start, end, bankType, bank, prodName, area, currency, yieldType, investType, term, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)), currentPage, pageSize, out total);
            return BuidJsonTable(tableData, columns, currentPage, pageSize, null);
        }

        /// <summary>
        /// Get chart
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="bankType"></param>
        /// <param name="bank"></param>
        /// <param name="currency"></param>
        /// <param name="yieldType"></param>
        /// <param name="investType"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        private object GetYieldTrendChart(DateTime start, DateTime end, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType, string[] term)
        {
            var chart = new ChartJsonData();
            var dataSeries = new List<object>();
            foreach (var item in term)
            {
                if (string.IsNullOrEmpty(item))
                    break;
                var table = WMPRepository.GetYieldTrendChartData(start, end, bankType, bank,prodName,area ,currency, yieldType, investType, item);
                //if (chart.ColumnCategories == null)
                //{
                //    var categories = new List<string>();
                //    foreach (DataRow row in table.Rows)
                //    {
                //        categories.Add(((DateTime)row["end"]).ToString("yy-MM"));
                //    }
                //    chart.ColumnCategories = categories.ToArray();
                //}
                var data = new List<object>();
                foreach (DataRow row in table.Rows)
                {
                    //data.Add((double)row["avg"]);
                    var point = new List<object>();
                    point.Add(((DateTime)row["endDate"]).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
                    point.Add(Convert.ToDouble(row["avgCount"]));
                    data.Add(point.ToArray());
                }
                dataSeries.Add(new { data = data.ToArray(), name = Resources.Global.ResourceManager.GetString(string.Format("WMP_Trend_PT_{0}", item)) });
            }
            chart.SeriesData = dataSeries.ToArray();
            return chart.ToJson();
        }

        #endregion

        #region Trust products

        [Localization]
        public JsonResult GetWMPTrustData(bool includeTimeSpan, string queryDateType, string orgType,
                                       string org, string trustType, string InvField, string yield,
                                       string prodState, string term, string minCap, string issueAmount, DateTime startDate,
                                       DateTime endDate, string prodName, string is_pe, string is_tot,
                                       string order, int startPage, int id, bool isHTML = false)
        {
            int total;
            var jtable = BuildJTableTrustData(includeTimeSpan, queryDateType, orgType, org, trustType, InvField, yield,
                                       prodState, term, issueAmount, minCap, startDate, endDate, prodName, is_pe, is_tot, order, startPage, id,out total);
            jtable.Total = total;
            return isHTML ? Json(jtable, "text/html", JsonRequestBehavior.AllowGet) : Json(jtable, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelWMPTrustData(bool includeTimeSpan, string queryDateType, string orgType,
                                       string org, string trustType, string InvField, string yield,
                                       string prodState, string term, string minCap, string issueAmount, DateTime startDate,
                                       DateTime endDate, string prodName, string is_pe, string is_tot,
                                       string order, int startPage, int id)
        {
            int total;
            var jTable = BuildJTableTrustData(includeTimeSpan, queryDateType, orgType, org, trustType, InvField, yield,
                                       prodState, term, issueAmount, minCap, startDate, endDate, prodName, is_pe, is_tot, order, 1, id, out total, 2000);

            var jP = new JsonExcelParameter { Table = jTable, TableName = ReportService.GetReportInfoById(id).DisplayName, Source = Resources.Global.Source }; ;
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public string UpdateWmpTrustCompany(string type)
        {
            var options = HtmlUtil.GetWmpTrustCompanyHtml(type);
            return options;
        }

        private JsonTable BuildJTableTrustData(bool includeTimeSpan, string queryDateType, string orgType,
                                      string org, string trustType, string InvField, string yield,
                                      string prodState, string term, string issueAmount, string minCap, DateTime startDate,
                                      DateTime endDate, string prodName, string is_pe, string is_tot,
                                      string order, int startPage, int id, out int total, int pageSize = PAGE_SIZE)
        {
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var data = wmpRepository.GetWmpTrustPaging(includeTimeSpan, queryDateType, orgType, org, trustType, InvField,
                                        yield, prodState, term, issueAmount, minCap, startDate,
                                        endDate, prodName.Trim(), is_pe, is_tot, columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                        order, startPage, pageSize, out total);
            return BuidJsonTable(data, columns, startPage, pageSize, order);
        }

        [Localization]
        public ActionResult TrustWMPDetail(string id)
        {
            var innerCode = int.Parse(id.Replace("WMPD", ""));
            var viewModel = new TrustWMPDetailViewModel(innerCode, WMPRepository);
            return PartialView(viewModel);
        }

        #endregion

        #region Bank Products Compare
        [Localization]
        public JsonResult GetBankWMPCompareData(string ids, bool isHTML = false)
        {
            var jTable = GetWMPCompareJtable(ids ?? "");

            return isHTML ? Json(jTable, "text/html", JsonRequestBehavior.AllowGet) : Json(jTable, JsonRequestBehavior.AllowGet); ;
        }

        [Localization]
        public ActionResult ExportExelForWMPCompare(string ids, string reportName)
        {
            var jTable = GetWMPCompareJtable(ids ?? "");
            var jP = new JsonExcelParameter { Table = jTable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        private JsonTable GetWMPCompareJtable(string ids)
        {
            var idstrings = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var products = idstrings.Select(id => WMPRepository.GetViewProdByInnerCode(int.Parse(id))).Where(product => product != null).ToList();

            var jTable = new JsonTable();
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "c0", Name = Global.WMP_ProductName });
            var row1 = new Dictionary<string, string>();
            row1.Add("c0", Resources.Global.WMP_ProductCode);
            var row2 = new Dictionary<string, string>();
            row2.Add("c0", Resources.Global.WMP_CommissionedCurrency);
            var row3 = new Dictionary<string, string>();
            row3.Add("c0", Resources.Global.WMP_BankName);
            var row4 = new Dictionary<string, string>();
            row4.Add("c0", Resources.Global.WMP_Yield_Type);
            var row5 = new Dictionary<string, string>();
            row5.Add("c0", Resources.Global.WMP_DelegatedAdministrationPeriod);
            var row6 = new Dictionary<string, string>();
            row6.Add("c0", Resources.Global.WMP_TheHighestExpectedRevenue);
            var row7 = new Dictionary<string, string>();
            row7.Add("c0", Resources.Global.WMP_SalesStartDate);
            var row8 = new Dictionary<string, string>();
            row8.Add("c0", Resources.Global.WMP_SalesEndDate);
            var row9 = new Dictionary<string, string>();
            row9.Add("c0", Resources.Global.WMP_Investment_Amount);
            var row10 = new Dictionary<string, string>();
            row10.Add("c0", Resources.Global.WMP_InterestPaymentPeriod);
            var row11 = new Dictionary<string, string>();
            row11.Add("c0", Resources.Global.WMP_Revenue_Start_Date);
            var row12 = new Dictionary<string, string>();
            row12.Add("c0", Resources.Global.WMP_Revenue_End_Date);
            var row13 = new Dictionary<string, string>();
            row13.Add("c0", Resources.Global.WMP_Sales_Region);
            var row14 = new Dictionary<string, string>();
            row14.Add("c0", Resources.Global.WMP_ReturnOfPrincipalAndInterestWay);
            var row15 = new Dictionary<string, string>();
            row15.Add("c0", Resources.Global.WMP_Brokerage);
            var row16 = new Dictionary<string, string>();
            row16.Add("c0", Resources.Global.WMP_ExpertComments);
            var row17 = new Dictionary<string, string>();
            row17.Add("c0", Resources.Global.WMP_ProductFeatures);
            var row18 = new Dictionary<string, string>();
            row18.Add("c0", Resources.Global.WMP_TypeOfInvestmentTargets);
            var row20 = new Dictionary<string, string>();
            row20.Add("c0", Resources.Global.WMP_ActualAnnualRateOfReturn);
            var row21 = new Dictionary<string, string>();
            row21.Add("c0", Resources.Global.WMP_YieldDescription);
            var row22 = new Dictionary<string, string>();
            row22.Add("c0", Resources.Global.WMP_IncomeCalculatedOnTheBasis);
            var row23 = new Dictionary<string, string>();
            row23.Add("c0", Resources.Global.WMP_RiskRating);
            var row34 = new Dictionary<string, string>();
            row34.Add("c0", Resources.Global.WMP_LiquidityRating);
            var row35 = new Dictionary<string, string>();
            row35.Add("c0", Resources.Global.WMP_RiskWarning);
            var row24 = new Dictionary<string, string>();
            row24.Add("c0", Resources.Global.WMP_CommissionedStartingAmount);
            var row25 = new Dictionary<string, string>();
            row25.Add("c0", Resources.Global.WMP_CommissionAmountIncrements);
            var row26 = new Dictionary<string, string>();
            row26.Add("c0", Resources.Global.WMP_TheConditionsOfPurchaseDescription);
            var row27 = new Dictionary<string, string>();
            row27.Add("c0", Resources.Global.WMP_EarlyTerminationAndExtensionInstructions);
            var row28 = new Dictionary<string, string>();
            row28.Add("c0", Resources.Global.WMP_EarlyTerminationCondition);
            var row29 = new Dictionary<string, string>();
            row29.Add("c0", Resources.Global.WMP_EarlyTerminationDateDescription);
            var row30 = new Dictionary<string, string>();
            row30.Add("c0", Resources.Global.WMP_RedemptionDateDescription);
            var row31 = new Dictionary<string, string>();
            row31.Add("c0", Resources.Global.WMP_BanksMayBeTerminated);
            var row32 = new Dictionary<string, string>();
            row32.Add("c0", Resources.Global.WMP_TheCustomerCanRedeem);
            var row33 = new Dictionary<string, string>();
            row33.Add("c0", Resources.Global.WMP_Pledged);
            foreach (v_WMP_BANK_PROD prod in products)
            {
                var columnName = "prod" + prod.INNER_CODE.ToString();
                //inner code stores in "ColumnType"
                jTable.ColumTemplate.Add(new JsonColumn() { ColumnName = columnName, Name = prod.PRD_NAME, Sort = prod.INNER_CODE.ToString() });
                row1.Add(columnName, prod.PRD_CODE ?? "");
                row2.Add(columnName, prod.ENTR_CURNCY_NAME ?? "");
                row3.Add(columnName, prod.BANK_NAME ?? "");
                row4.Add(columnName, prod.PRD_TYPE_NAME ?? "");
                row5.Add(columnName, prod.PRD_SYS ?? "");
                row6.Add(columnName, UIGenerator.FormatCellValue(prod.PRD_MAX_YLD_DE, "decimal"));
                row7.Add(columnName, UIGenerator.FormatDateTime(prod.SELL_ORG_DATE));
                row8.Add(columnName, UIGenerator.FormatDateTime(prod.SELL_END_DATE));
                row9.Add(columnName, prod.ENTR_MIN_CURNCY.ToString());
                row10.Add(columnName, prod.PAY_CYC_NAME ?? "");
                row11.Add(columnName, UIGenerator.FormatDateTime(prod.INC_ORG_DATE));
                row12.Add(columnName, UIGenerator.FormatDateTime(prod.END_DATE));
                row13.Add(columnName, prod.ISS_AREA ?? "");
                row14.Add(columnName, prod.RETN_TYPE ?? "");
                row15.Add(columnName, prod.AC_ORG ?? "");
                row16.Add(columnName, prod.EXP_VIEW ?? "");
                row17.Add(columnName, prod.FEAT ?? "");
                row18.Add(columnName, prod.INV_OBJ ?? "");
                row20.Add(columnName, UIGenerator.FormatCellValue(prod.MAT_ACTU_YLD, "decimal"));
                row21.Add(columnName, prod.YLD_DET ?? "");
                row22.Add(columnName, prod.INC_CLA_BASIS ?? "");
                row23.Add(columnName, prod.RIST_RTNG_DTL ?? "");
                row34.Add(columnName, prod.LIQ_RTNG_DTL ?? "");
                row35.Add(columnName, prod.RIST_WARE ?? "");
                row24.Add(columnName, UIGenerator.FormatCellValue(prod.ENTR_MIN_CURNCY, "decimal"));
                row25.Add(columnName, UIGenerator.FormatCellValue(prod.CURNCY_INCR_UNIT, "decimal"));
                row26.Add(columnName, prod.AD_PRC_CND ?? "");
                row27.Add(columnName, prod.TRM_DLY_REMARK ?? "");
                row28.Add(columnName, prod.AD_TRM_CND ?? "");
                row29.Add(columnName, prod.AD_TRM_DATE_DTL ?? "");
                row30.Add(columnName, prod.CALL_DATE_DTL ?? "");
                row31.Add(columnName, prod.IS_BNK_BRG_FRD_NAME ?? "");
                row32.Add(columnName, prod.IS_CST_BRG_FRD_NAME ?? "");
                row33.Add(columnName, prod.IS_PLEDGE_NAME ?? "");
            }
            jTable.RowData.Add(row1);
            jTable.RowData.Add(row2);
            jTable.RowData.Add(row3);
            jTable.RowData.Add(row4);
            jTable.RowData.Add(row5);
            jTable.RowData.Add(row6);
            jTable.RowData.Add(row7);
            jTable.RowData.Add(row8);
            jTable.RowData.Add(row9);
            jTable.RowData.Add(row10);
            jTable.RowData.Add(row11);
            jTable.RowData.Add(row12);
            jTable.RowData.Add(row13);
            jTable.RowData.Add(row14);
            jTable.RowData.Add(row15);
            jTable.RowData.Add(row16);
            jTable.RowData.Add(row17);
            jTable.RowData.Add(row18);
            jTable.RowData.Add(row20);
            jTable.RowData.Add(row21);
            jTable.RowData.Add(row22);
            jTable.RowData.Add(row23);
            jTable.RowData.Add(row34);
            jTable.RowData.Add(row35);
            jTable.RowData.Add(row24);
            jTable.RowData.Add(row25);
            jTable.RowData.Add(row26);
            jTable.RowData.Add(row27);
            jTable.RowData.Add(row28);
            jTable.RowData.Add(row29);
            jTable.RowData.Add(row30);
            jTable.RowData.Add(row31);
            jTable.RowData.Add(row32);
            jTable.RowData.Add(row33);
            return jTable;
        }

        #endregion

        #region trust product compare

        [Localization]
        public JsonResult GetTrustWMPCompareData(string ids, bool isHTML = false)
        {
            var jTable = GetTrustWMPCompareJtable(ids ?? "");

            return isHTML ? Json(jTable, "text/html", JsonRequestBehavior.AllowGet) : Json(jTable, JsonRequestBehavior.AllowGet); ;
        }

        [Localization]
        public ActionResult ExportExelForTrustWMPCompare(string ids, string reportName)
        {
            var jTable = GetTrustWMPCompareJtable(ids ?? "");
            var jP = new JsonExcelParameter { Table = jTable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        private JsonTable GetTrustWMPCompareJtable(string ids)
        {
            var idstrings = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var products = idstrings.Select(id => WMPRepository.GetTrustWmpDetailByInnerCode(int.Parse(id))).Where(product => product != null).ToList();

            var jTable = new JsonTable();
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "c0", Name = Global.WMP_Trust_PRD_NAME });
            var row0 = new Dictionary<string, string>();
            row0.Add("c0", Resources.Global.WMP_Trust_PROM_STARTDATE);
            var row1 = new Dictionary<string, string>();
            row1.Add("c0", Resources.Global.WMP_Trust_TRUST_SNAME);
            var row2 = new Dictionary<string, string>();
            row2.Add("c0", Resources.Global.WMP_Trust_PROM_ENDDATE);
            var row3 = new Dictionary<string, string>();
            row3.Add("c0", Resources.Global.WMP_Trust_ORGNAME);
            var row4 = new Dictionary<string, string>();
            row4.Add("c0", Resources.Global.WMP_Trust_FIN_CYC);
            var row5 = new Dictionary<string, string>();
            row5.Add("c0", Resources.Global.WMP_Trust_INV_FLD);
            var row6 = new Dictionary<string, string>();
            row6.Add("c0", Resources.Global.WMP_Trust_BUILD_DATE);
            var row7 = new Dictionary<string, string>();
            row7.Add("c0", Resources.Global.WMP_Trust_TRUST_MNG);
            var row8 = new Dictionary<string, string>();
            row8.Add("c0", Resources.Global.WMP_Trust_ENDDATE);
            var row9 = new Dictionary<string, string>();
            row9.Add("c0", Resources.Global.WMP_Trust_TRUST_TYPE);
            var row10 = new Dictionary<string, string>();
            row10.Add("c0", Resources.Global.WMP_Trust_PLAN_ISS_SIZE);
            var row11 = new Dictionary<string, string>();
            row11.Add("c0", Resources.Global.WMP_Trust_IS_STRU);
            var row12 = new Dictionary<string, string>();
            row12.Add("c0", Resources.Global.WMP_Trust_ACTU_ISS_SIZE);
            var row13 = new Dictionary<string, string>();
            row13.Add("c0", Resources.Global.WMP_Trust_EXP_YLD);
            var row14 = new Dictionary<string, string>();
            row14.Add("c0", Resources.Global.WMP_Trust_IS_MERGE);
            var row15 = new Dictionary<string, string>();
            row15.Add("c0", Resources.Global.WMP_Trust_ACT_YLD);
            var row16 = new Dictionary<string, string>();
            row16.Add("c0", Resources.Global.WMP_Trust_MIN_CAP);
            var row17 = new Dictionary<string, string>();
            row17.Add("c0", Resources.Global.WMP_Trust_PERI_TYPE_NAME);
            var row18 = new Dictionary<string, string>();
            row18.Add("c0", Resources.Global.WMP_Trust_OPEN_REMARK);
            var row19 = new Dictionary<string, string>();
            row19.Add("c0", Resources.Global.WMP_Trust_IS_BRG_FRD_NAME);
            var row20 = new Dictionary<string, string>();
            row20.Add("c0", Resources.Global.WMP_Trust_IS_DLY_NAME);
            var row21 = new Dictionary<string, string>();
            row21.Add("c0", Resources.Global.WMP_Trust_IS_PE_NAME);
            var row22 = new Dictionary<string, string>();
            row22.Add("c0", Resources.Global.WMP_Trust_IS_CURNCY_NAME);
            var row23 = new Dictionary<string, string>();
            row23.Add("c0", Resources.Global.WMP_Trust_IS_TOT);
            var row24 = new Dictionary<string, string>();
            row24.Add("c0", Resources.Global.WMP_Trust_SEC_INVEST_TYPE);
            var row25 = new Dictionary<string, string>();
            row25.Add("c0", Resources.Global.WMP_Trust_CURNCY_NAME);
            var row26 = new Dictionary<string, string>();
            row26.Add("c0", Resources.Global.WMP_Trust_INCOME_TYPE);
            var row27 = new Dictionary<string, string>();
            row27.Add("c0", Resources.Global.WMP_Trust_ADD_CAP);
            var row28 = new Dictionary<string, string>();
            row28.Add("c0", Resources.Global.WMP_Trust_PRD_STATUS);
            var row29 = new Dictionary<string, string>();
            row29.Add("c0", Resources.Global.WMP_Trust_INVEST_REMARK);
            var row30 = new Dictionary<string, string>();
            row30.Add("c0", Resources.Global.WMP_Trust_FEE_REMARK);
            var row31 = new Dictionary<string, string>();
            row31.Add("c0", Resources.Global.WMP_Trust_RIST_WARE);
            var row32 = new Dictionary<string, string>();
            row32.Add("c0", Resources.Global.WMP_Trust_ALERT_LINE);
            var row33 = new Dictionary<string, string>();
            row33.Add("c0", Resources.Global.WMP_Trust_ALERT_LINE_REMARK);
            var row34 = new Dictionary<string, string>();
            row34.Add("c0", Resources.Global.WMP_Trust_STOP_LINE);
            var row35 = new Dictionary<string, string>();
            row35.Add("c0", Resources.Global.WMP_Trust_STOP_LINE_REMARK);
            var row36 = new Dictionary<string, string>();
            row36.Add("c0", Resources.Global.WMP_Trust_REMARK);
            var row37 = new Dictionary<string, string>();
            row37.Add("c0", Resources.Global.WMP_Trust_PRD_REMARK);
            var row38 = new Dictionary<string, string>();
            row38.Add("c0", Resources.Global.WMP_Trust_EXP_YLD_REMARK);
            var row39 = new Dictionary<string, string>();
            row39.Add("c0", Resources.Global.WMP_Trust_ACT_YLD_REMARK);
            var row40 = new Dictionary<string, string>();
            row40.Add("c0", Resources.Global.WMP_Trust_MIN_CAP_REMARK);
            var row41 = new Dictionary<string, string>();
            row41.Add("c0", Resources.Global.WMP_Trust_CRED_ENHA_MODE);
            var row42 = new Dictionary<string, string>();
            row42.Add("c0", Resources.Global.UnlistIsser_Company_Name);

            foreach (v_WMP_TRUST ViewProd in products)
            {
                var columnName = "prod" + ViewProd.INNER_CODE.ToString();
                //inner code stores in "ColumnType"
                jTable.ColumTemplate.Add(new JsonColumn() { ColumnName = columnName, Name = ViewProd.PRD_NAME, Sort = ViewProd.INNER_CODE.ToString() });
                row0.Add(columnName, UIGenerator.FormatDateTime(ViewProd.PROM_STARTDATE));
                row1.Add(columnName, ViewProd.TRUST_SNAME ?? "");
                row2.Add(columnName, UIGenerator.FormatDateTime(ViewProd.PROM_ENDDATE));
                row3.Add(columnName, ViewProd.ORGNAME ?? "");
                row4.Add(columnName, UIGenerator.FormatCellValue(ViewProd.FIN_CYC, "decimal"));
                row5.Add(columnName, ViewProd.INV_FLD_NAME ?? "");
                row6.Add(columnName, UIGenerator.FormatDateTime(ViewProd.BUILD_DATE));
                row7.Add(columnName, ViewProd.TRUST_MNG ?? "");
                row8.Add(columnName, UIGenerator.FormatDateTime(ViewProd.ENDDATE));
                row9.Add(columnName, ViewProd.TRUST_TYPE_NAME ?? "");
                row10.Add(columnName, UIGenerator.FormatCellValue(ViewProd.PLAN_ISS_SIZE, "decimal"));
                row11.Add(columnName, ViewProd.IS_STRU_NAME ?? "");
                row12.Add(columnName, UIGenerator.FormatCellValue(ViewProd.ACTU_ISS_SIZE, "decimal"));
                row13.Add(columnName, UIGenerator.FormatCellValue(ViewProd.EXP_YLD, "decimal"));
                row14.Add(columnName, ViewProd.IS_MERGE_NAME ?? "");
                row15.Add(columnName, UIGenerator.FormatCellValue(ViewProd.ACT_YLD, "decimal"));
                row16.Add(columnName, UIGenerator.FormatCellValue(ViewProd.MIN_CAP, "decimal"));
                row17.Add(columnName, ViewProd.PERI_TYPE_NAME ?? "");
                row18.Add(columnName, ViewProd.OPEN_REMARK ?? "");
                row19.Add(columnName, ViewProd.IS_BRG_FRD_NAME ?? "");
                row20.Add(columnName, ViewProd.IS_DLY_NAME ?? "");
                row21.Add(columnName, ViewProd.IS_PE_NAME ?? "");
                row22.Add(columnName, ViewProd.IS_CURNCY_NAME ?? "");
                row23.Add(columnName, ViewProd.IS_TOT_NAME ?? "");
                row24.Add(columnName, ViewProd.SEC_INVEST_TYPE ?? "");
                row25.Add(columnName, ViewProd.CURNCY_NAME ?? "");
                row26.Add(columnName, ViewProd.INCOME_TYPE_NAME ?? "");
                row27.Add(columnName, UIGenerator.FormatCellValue(ViewProd.ADD_CAP, "decimal"));
                row28.Add(columnName, ViewProd.PRD_STATUS_NAME ?? "");
                row29.Add(columnName, ViewProd.INVEST_REMARK ?? "");
                row30.Add(columnName, ViewProd.FEE_REMARK ?? "");
                row31.Add(columnName, ViewProd.RIST_WARE ?? "");
                row32.Add(columnName, UIGenerator.FormatCellValue(ViewProd.ALERT_LINE, "decimal"));
                row33.Add(columnName, ViewProd.ALERT_LINE_REMARK ?? "");
                row34.Add(columnName, UIGenerator.FormatCellValue(ViewProd.STOP_LINE, "decimal"));
                row35.Add(columnName, ViewProd.STOP_LINE_REMARK ?? "");
                row36.Add(columnName, ViewProd.REMARK ?? "");
                row37.Add(columnName, ViewProd.PRD_REMARK ?? "");
                row38.Add(columnName, ViewProd.EXP_YLD_REMARK ?? "");
                row39.Add(columnName, ViewProd.ACT_YLD_REMARK ?? "");
                row40.Add(columnName, ViewProd.MIN_CAP_REMARK ?? "");
                row41.Add(columnName, ViewProd.CRED_ENHA_MODE ?? "");
                row42.Add(columnName, ViewProd.ORGNAME ?? "");
            }
            jTable.RowData.Add(row0);
            jTable.RowData.Add(row1);
            jTable.RowData.Add(row2);
            jTable.RowData.Add(row3);
            jTable.RowData.Add(row4);
            jTable.RowData.Add(row5);
            jTable.RowData.Add(row6);
            jTable.RowData.Add(row7);
            jTable.RowData.Add(row8);
            jTable.RowData.Add(row9);
            jTable.RowData.Add(row10);
            jTable.RowData.Add(row11);
            jTable.RowData.Add(row12);
            jTable.RowData.Add(row13);
            jTable.RowData.Add(row14);
            jTable.RowData.Add(row15);
            jTable.RowData.Add(row16);
            jTable.RowData.Add(row17);
            jTable.RowData.Add(row18);
            jTable.RowData.Add(row19);
            jTable.RowData.Add(row20);
            jTable.RowData.Add(row21);
            jTable.RowData.Add(row22);
            jTable.RowData.Add(row23);
            jTable.RowData.Add(row24);
            jTable.RowData.Add(row25);
            jTable.RowData.Add(row26);
            jTable.RowData.Add(row27);
            jTable.RowData.Add(row28);
            jTable.RowData.Add(row29);
            jTable.RowData.Add(row30);
            jTable.RowData.Add(row31);
            jTable.RowData.Add(row32);
            jTable.RowData.Add(row33);
            jTable.RowData.Add(row34);
            jTable.RowData.Add(row35);
            jTable.RowData.Add(row36);
            jTable.RowData.Add(row37);
            jTable.RowData.Add(row38);
            jTable.RowData.Add(row39);
            jTable.RowData.Add(row40);
            jTable.RowData.Add(row41);
            jTable.RowData.Add(row42);
            return jTable;
        }

        #endregion

        #region Broker WMP

        [Localization]
        public ActionResult BrokerWMPDetail(string id)
        {
            var innerCode = int.Parse(id.Replace("WMPD", ""));
            var viewModel = new BrokerWMPDetailViewModel(innerCode, WMPRepository);
            return PartialView(viewModel);
        }

        [Localization]
        public ActionResult GetBrokerWMPData(string orgs, string prodType, string investType, string lowest, string isQdii, string prodState
            , string banks, bool includeDate, string queryDateType, DateTime startDate, DateTime endDate, string prodName, string pmName,
            string order, int startPage, int id, bool isHTML = false)
        {
            int total;
            var jtable = BuildBrokerJTable(orgs, prodType, investType, lowest, isQdii, prodState
            , banks, includeDate, queryDateType, startDate, endDate, prodName, pmName, order, startPage, id, out total);
            jtable.Total = total;
            return isHTML ? Json(jtable, "text/html", JsonRequestBehavior.AllowGet) : Json(jtable, JsonRequestBehavior.AllowGet);
        }

        private JsonTable BuildBrokerJTable(string orgs, string prodType, string investType, string lowest, string isQdii, string prodState, string banks,
            bool includeDate, string queryDateType, DateTime startDate, DateTime endDate, string prodName, string pmName,
            string order, int startPage, int id, out int total, int pageSize = PAGE_SIZE)
        {
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var data = wmpRepository.GetWmpBrokerPaging(orgs, prodType, investType, lowest, isQdii, prodState, banks,
                includeDate, queryDateType, startDate, endDate, prodName, pmName,
                columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                        order, startPage, pageSize,out total);
            return BuidJsonTable(data, columns, startPage, pageSize, order);
        }

        [Localization]
        public ActionResult ExportExcelWMPBrokerData(string orgs, string prodType, string investType, string lowest, string isQdii, string prodState
            , string banks, bool includeDate, string queryDateType, DateTime startDate, DateTime endDate, string prodName, string pmName,
            string order, int startPage, int id, bool isHTML = false)
        {
            int total;
            var jTable = BuildBrokerJTable(orgs, prodType, investType, lowest, isQdii, prodState, banks,
                            includeDate, queryDateType, startDate, endDate, prodName, pmName, order, 1, id,out total, 2000);

            var jP = new JsonExcelParameter { Table = jTable, TableName = ReportService.GetReportInfoById(id).DisplayName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public ActionResult BrokerFee(int id)
        {
            ViewBag.ID = id;
            ViewBag.Name = Resources.WMP.Broker_Product_Rate;
            var viewModel = new BrokerWMPFeeViewModel(id, WMPRepository);
            return PartialView(viewModel);
        }

        [Localization]
        public ActionResult BrokerNetWorth(int id)
        {
            ViewBag.ID = id;
            ViewBag.Name = Resources.WMP.Broker_Net_Worth_History;
            return PartialView();
        }

        /// <summary>
        /// 产品分红
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Localization]
        public ActionResult BrokerBonus(int id)
        {
            var model = WMPRepository.GetCfpProfitsheets(id);
            return PartialView(model);
        }

        /// <summary>
        /// 财务指标
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Localization]
        public ActionResult BrokerFinIdx(int id)
        {
            ViewBag.ID = id;
            ViewBag.Name = Resources.WMP.Broker_Financial_Indicator;
            var viewModel = new BrokerWMPFinIdxViewModel(id, WMPRepository);
            return PartialView(viewModel);
        }

        /// <summary>
        /// 产品公告
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Localization]
        public ActionResult BrokerContent(int id)
        {
            var model = DataTableSerializer.ToList<BrokerContentModel>(WMPRepository.GetDiscContentById(id));
            return PartialView(model);
        }

        [Localization]
        public ActionResult DonwloadBrokerContent(string id)
        {
            var file = WMPRepository.GetBrokerDisc(id);
            if (file == null)
                return HttpNotFound();
            if (file.Content == null)
                return HttpNotFound();
            var contentType = MimeHelper.GetMimeTypeByExt(file.FileType);
            return File(file.Content, contentType,
                Resources.Global.DownloadFileName + "." + file.FileType);
        }

        [Localization]
        public ActionResult GetBrokerNetWorthData(int id, DateTime startDate, DateTime endDate)
        {
            var data = BuildBrokerNetWorthData(id, startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportBrokerNetWorthData(int id, DateTime startDate, DateTime endDate, string reportName)
        {
            var data = BuildBrokerNetWorthData(id, startDate, endDate);
            var jP = new JsonExcelParameter { Table = data, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResultWithCustomFormat(jP);
        }

        [Localization]
        public ActionResult ExportBrokerFinIdxData(int id)
        {
            var data = BuildBrokerFinIdxData(id);
            var jP = new JsonExcelParameter { Table = data, TableName = Resources.WMP.Broker_Financial_Indicator, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        private JsonTable BuildBrokerFinIdxData(int id)
        {
            var jTable = new JsonTable();
            var data = WMPRepository.GetWmpBrokerFinIdxById(id);

            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_End_Date, ColumnName = "ENDDATE" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_Report_Source, ColumnName = "RPT_SRC" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_Profit_of_Period, ColumnName = "PROFIT" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_Ending_Net_Asset_Value, ColumnName = "END_ASSET_VAL" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_Net_Growth_Rate, ColumnName = "NET_GR" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_Accumulated_Net_Growth_Rate, ColumnName = "UNIT_ACCUM_NET_GR" });

            foreach (var f in data)
            {
                var currentRow = new Dictionary<string, string>();
                currentRow.Add("ENDDATE", f.ENDDATE.ToString("yyyy-MM-dd"));
                currentRow.Add("RPT_SRC", f.RPT_SRC);
                currentRow.Add("PROFIT", f.PROFIT);
                currentRow.Add("END_ASSET_VAL", f.END_ASSET_VAL);
                currentRow.Add("NET_GR", f.NET_GR);
                currentRow.Add("UNIT_ACCUM_NET_GR", f.UNIT_ACCUM_NET_GR);
                jTable.RowData.Add(currentRow);
            }

            return jTable;
        }

        private JsonTable BuildBrokerNetWorthData(int id, DateTime startDate, DateTime endDate)
        {
            var jTable = new JsonTable();
            var data = WMPRepository.GetWmpBrokerNetWorthData(id, startDate, endDate);
            var containGrowth = data.Columns.Contains("Growth");

            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Date, ColumnName = "Date" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = containGrowth ? Resources.WMP.Broker_UNT_NET : Resources.WMP.thousands_of_income, ColumnName = "Unit" });
            jTable.ColumTemplate.Add(new JsonColumn { Name = containGrowth ? Resources.WMP.Broker_UNT_ACCUM_NET : Resources.WMP.Seven_day_annualized_yield, ColumnName = "Acc" });
            if (containGrowth)
            {
                jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.WMP.Broker_Daily_Growth, ColumnName = "Growth" });
            }

            for (int i = 0, j = data.Rows.Count; i < j; i++)
            {
                DataRow row = data.Rows[i];

                var currentRow = new Dictionary<string, string>();
                currentRow.Add("Unit", row["Unit"] == DBNull.Value ? "" : ((double)row["Unit"]).ToString("N4"));
                currentRow.Add("Acc", row["Acc"] == DBNull.Value ? "" : ((double)row["Acc"]).ToString("N4"));
                currentRow.Add("Date", ((DateTime)row["Dates"]).ToString("yyyy-MM-dd"));
                if (containGrowth)
                {
                    if (i != j - 1)
                    {
                        var growth = ((double)data.Rows[i]["Unit"] - (double)data.Rows[i + 1]["Unit"]) / (double)data.Rows[i + 1]["Unit"];
                        currentRow.Add("Growth", growth.ToString("P"));
                    }
                    else
                    {
                        currentRow.Add("Growth", "-");
                    }
                }
                jTable.RowData.Add(currentRow);
            }

            return jTable;
        }


        #endregion

        #region wmp broker compare

        [Localization]
        public JsonResult GetBrokerWMPCompareData(string ids, bool isHTML = false)
        {
            var jTable = GetBrokerWMPCompareJtable(ids ?? "");

            return isHTML ? Json(jTable, "text/html", JsonRequestBehavior.AllowGet) : Json(jTable, JsonRequestBehavior.AllowGet); ;
        }

        [Localization]
        public ActionResult ExportExelForBrokerWMPCompare(string ids, string reportName)
        {
            var jTable = GetBrokerWMPCompareJtable(ids ?? "");
            var jP = new JsonExcelParameter { Table = jTable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        private JsonTable GetBrokerWMPCompareJtable(string ids)
        {
            var idstrings = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var products = idstrings.Select(id => WMPRepository.GetBrokerWmpDetailByInnerCode(int.Parse(id))).Where(product => product != null).ToList();

            var jTable = new JsonTable();
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "c0", Name = Global.WMP_ProductName });
            var row1 = new Dictionary<string, string>();
            row1.Add("c0", Resources.WMP.Broker_CFPSNAME);
            var row2 = new Dictionary<string, string>();
            row2.Add("c0", Resources.WMP.Broker_CFPNAME);
            var row3 = new Dictionary<string, string>();
            row3.Add("c0", Resources.WMP.Broker_INVEST_CLS);
            var row4 = new Dictionary<string, string>();
            row4.Add("c0", Resources.WMP.Broker_CFP_TYPE);
            var row5 = new Dictionary<string, string>();
            row5.Add("c0", Resources.WMP.Broker_Orgname);
            var row6 = new Dictionary<string, string>();
            row6.Add("c0", Resources.WMP.Broker_PROD_Manager);
            var row7 = new Dictionary<string, string>();
            row7.Add("c0", Resources.WMP.Broker_ESTAB_DATE);
            var row8 = new Dictionary<string, string>();
            row8.Add("c0", Resources.WMP.Broker_TOT_VAL);
            var row9 = new Dictionary<string, string>();
            row9.Add("c0", Resources.WMP.Broker_LOWEST_VAL);
            var row10 = new Dictionary<string, string>();
            row10.Add("c0", Resources.WMP.Broker_ADD_UNIT_LOW);
            var row11 = new Dictionary<string, string>();
            row11.Add("c0", Resources.WMP.Broker_DURATION_SIZE_LOW);
            var row12 = new Dictionary<string, string>();
            row12.Add("c0", Resources.WMP.Broker_QUIT_UNIT_LOW);
            var row13 = new Dictionary<string, string>();
            row13.Add("c0", Resources.WMP.Broker_IS_QDII);
            var row14 = new Dictionary<string, string>();
            row14.Add("c0", Resources.WMP.Broker_DECLAREDATE);
            var row15 = new Dictionary<string, string>();
            row15.Add("c0", Resources.WMP.Broker_PROD_STATE);
            var row16 = new Dictionary<string, string>();
            row16.Add("c0", Resources.WMP.Broker_FIN_CYC);
            var row17 = new Dictionary<string, string>();
            row17.Add("c0", Resources.WMP.Broker_STARTDATE);
            var row18 = new Dictionary<string, string>();
            row18.Add("c0", Resources.WMP.Broker_ENDDATE);
            var row19 = new Dictionary<string, string>();
            row19.Add("c0", Resources.WMP.Broker_EXPE_ENDDATE);
            var row20 = new Dictionary<string, string>();
            row20.Add("c0", Resources.WMP.Broker_ACTU_ENDDATE);
            var row21 = new Dictionary<string, string>();
            row21.Add("c0", Resources.WMP.Broker_OPEN_PRD);
            var row22 = new Dictionary<string, string>();
            row22.Add("c0", Resources.WMP.Broker_CLOSED_PERD);
            var row23 = new Dictionary<string, string>();
            row23.Add("c0", Resources.WMP.Broker_INVEST_TARGET);
            var row24 = new Dictionary<string, string>();
            row24.Add("c0", Resources.WMP.Broker_INVEST_RANGE);
            var row25 = new Dictionary<string, string>();
            row25.Add("c0", Resources.WMP.Broker_INVEST_STRA);
            var row26 = new Dictionary<string, string>();
            row26.Add("c0", Resources.WMP.Broker_INVEST_BENCH);
            var row27 = new Dictionary<string, string>();
            row27.Add("c0", Resources.WMP.Broker_INVEST_TENET);
            var row28 = new Dictionary<string, string>();
            row28.Add("c0", Resources.WMP.Broker_PROD_CHAR);
            var row29 = new Dictionary<string, string>();
            row29.Add("c0", Resources.WMP.Broker_FORE_YIELD);
            var row30 = new Dictionary<string, string>();
            row30.Add("c0", Resources.WMP.Broker_PERFORM_BENCH);
            var row31 = new Dictionary<string, string>();
            row31.Add("c0", Resources.WMP.Broker_HUGE_QUIT_TERM);
            foreach (v_WMP_CFP prod in products)
            {
                var columnName = "prod" + prod.INNER_CODE.ToString();
                //inner code stores in "ColumnType"
                jTable.ColumTemplate.Add(new JsonColumn()
                {
                    ColumnName = columnName,
                    Name = prod.CFPNAME,
                    Sort = prod.INNER_CODE.ToString()
                });
                row1.Add(columnName, prod.CFPSNAME ?? "");
                row2.Add(columnName, prod.CFPNAME ?? "");
                row3.Add(columnName, prod.INVEST_NAME ?? "");
                row4.Add(columnName, prod.PROD_TYPE_NAME ?? "");
                row5.Add(columnName, prod.ORGNAME ?? "");
                row6.Add(columnName, prod.INV_MNG ?? "");
                row7.Add(columnName, UIGenerator.FormatDateTime(prod.ESTAB_DATE));
                row8.Add(columnName, UIGenerator.FormatCellValue(prod.TOT_VAL, "decimal"));
                row9.Add(columnName, UIGenerator.FormatCellValue(prod.LOWEST_VAL, "decimal"));
                row10.Add(columnName, UIGenerator.FormatCellValue(prod.ADD_UNIT_LOW, "decimal"));
                row11.Add(columnName, UIGenerator.FormatCellValue(prod.DURATION_SIZE_LOW, "decimal"));
                row12.Add(columnName, UIGenerator.FormatCellValue(prod.QUIT_UNIT_LOW, "decimal"));
                row13.Add(columnName, prod.IS_QDII_NAME ?? "");
                row14.Add(columnName, UIGenerator.FormatDateTime(prod.DECLAREDATE));
                row15.Add(columnName, prod.PROD_STATE ?? "");
                row16.Add(columnName, prod.FIN_CYC);
                row17.Add(columnName, UIGenerator.FormatDateTime(prod.STARTDATE));
                row18.Add(columnName, UIGenerator.FormatDateTime(prod.ENDDATE));
                row19.Add(columnName, UIGenerator.FormatDateTime(prod.EXPE_ENDDATE));
                row20.Add(columnName, UIGenerator.FormatDateTime(prod.ACTU_ENDDATE));
                row21.Add(columnName, prod.OPEN_PRD ?? "");
                row22.Add(columnName, prod.CLOSED_PERD ?? "");
                row23.Add(columnName, prod.INVEST_TARGET ?? "");
                row24.Add(columnName, prod.INVEST_RANGE ?? "");
                row25.Add(columnName, prod.INVEST_STRA ?? "");
                row26.Add(columnName, prod.INVEST_BENCH ?? "");
                row27.Add(columnName, prod.INVEST_TENET ?? "");
                row28.Add(columnName, prod.PROD_CHAR ?? "");
                row29.Add(columnName, prod.FORE_YIELD ?? "");
                row30.Add(columnName, prod.PERFORM_BENCH ?? "");
                row31.Add(columnName, prod.HUGE_QUIT_TERM ?? "");
            }
            jTable.RowData.Add(row1);
            jTable.RowData.Add(row2);
            jTable.RowData.Add(row3);
            jTable.RowData.Add(row4);
            jTable.RowData.Add(row5);
            jTable.RowData.Add(row6);
            jTable.RowData.Add(row7);
            jTable.RowData.Add(row8);
            jTable.RowData.Add(row9);
            jTable.RowData.Add(row10);
            jTable.RowData.Add(row11);
            jTable.RowData.Add(row12);
            jTable.RowData.Add(row13);
            jTable.RowData.Add(row14);
            jTable.RowData.Add(row15);
            jTable.RowData.Add(row16);
            jTable.RowData.Add(row17);
            jTable.RowData.Add(row18);
            jTable.RowData.Add(row19);
            jTable.RowData.Add(row20);
            jTable.RowData.Add(row21);
            jTable.RowData.Add(row22);
            jTable.RowData.Add(row23);
            jTable.RowData.Add(row24);
            jTable.RowData.Add(row25);
            jTable.RowData.Add(row26);
            jTable.RowData.Add(row27);
            jTable.RowData.Add(row28);
            jTable.RowData.Add(row29);
            jTable.RowData.Add(row30);
            jTable.RowData.Add(row31);
            return jTable;
        }

        #endregion

        private JsonTable BuidJsonTable(DataTable table, IEnumerable<REPORTCOLUMNDEFINITION> columns, int currentPage, int pageSize, string order)
        {
            var jTable = new JsonTable();

            jTable.CurrentPage = currentPage;
            jTable.PageSize = pageSize;
            if (table.Rows.Count > 0 && table.Columns.Contains("TOTAL"))
            {
                jTable.Total = Convert.ToInt32(table.Rows[0]["TOTAL"]);
            }
            foreach (var column in columns)
            {
                //if refresh header, the sell org date and prd_sys need to be desc
                if (string.IsNullOrEmpty(order) && (column.COLUMN_NAME == "SELL_ORG_DATE" ||  column.COLUMN_NAME == "DECLAREDATE" || column.COLUMN_NAME == "WEEK_GR"))
                    jTable.ColumTemplate.Add(new JsonColumn { ColumnName = column.COLUMN_NAME, ColumnType = column.COLUMN_TYPE, Name = column.DisplayName, Sort = "DESC" });
                else
                    jTable.ColumTemplate.Add(new JsonColumn { ColumnName = column.COLUMN_NAME, ColumnType = column.COLUMN_TYPE, Name = column.DisplayName, Sort = "" });
            }

            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                foreach (REPORTCOLUMNDEFINITION column in columns)
                {

                    if (!currentRow.Keys.Contains(column.COLUMN_NAME))
                        currentRow.Add(column.COLUMN_NAME, UIGenerator.FormatCellValue(row, column));
                }
                if (!columns.Any(x => x.COLUMN_NAME == "INNER_CODE"))
                {
                    currentRow.Add("INNER_CODE", row["INNER_CODE"].ToString());
                }
                if (!columns.Any(x => x.COLUMN_NAME == "PRD_NAME") && row.Table.Columns.Contains("PRD_NAME"))
                {
                    currentRow.Add("PRD_NAME", row["PRD_NAME"].ToString());
                }
                if (!columns.Any(x => x.COLUMN_NAME == "CFPNAME") && row.Table.Columns.Contains("CFPNAME"))
                {
                    currentRow.Add("CFPNAME", row["CFPNAME"].ToString());
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }
    }
}
