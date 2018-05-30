using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Practices.Unity;
using VAV.DAL.Common;
using VAV.DAL.Services;
using VAV.Model.Data.Bond;
using VAV.Model.Data;
using VAV.Model.Data.ZCX;
using VAV.Web.Localization;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.Web.ViewModels.Bond;
using VAV.DAL.Report;
using VAV.DAL.Fundamental;
using VAV.DAL.ResearchReport;
using System.Data;
using VAV.Model.Chart;
using log4net;
using VAV.Entities;

namespace VAV.Web.Controllers
{
    /// <summary>
    /// Report Controller
    /// </summary>
    public class BondReportController : BaseController
    {

        [Dependency]
        public BondReportRepository BondReportRepository { get; set; }

        [Dependency]
        public ZCXRepository ZcxRepository { get; set; }

        [Dependency]
        public ResearchReportRepository CmaRepository { get; set; }

        [Dependency]
        public BondInfoRepository BondInfoRepository { get; set; }

        #region Issue Rates
        [Localization]
        public ActionResult GetRateOfIssuesReport(int id)
        {
            ReportInfo reportInfo = ReportService.GetReportInfoById(id);
            var startDate = new DateTime(DateTime.Now.Year - 1, DateTime.Now.Month, 1);
            var bondRateRepo = ReportService.GetBondIssueRatesRepo(new BondIssueParams
            {
                StartDate = startDate,
                BondType = "BTB",
                Term = "7Y",
                IsFloat = "n",
                Rating = "All"
            }).ToList();
            var bondRateRepo2 = ReportService.GetBondIssueRatesRepo(new BondIssueParams
            {
                StartDate = startDate,
                BondType = "PBB",
                Term = "7Y",
                IsFloat = "n",
                Rating = "All"
            }).ToList();

            var bondIssueReport = new BondIssueRateReport(reportInfo.ReportId)
            {
                Name = reportInfo.DisplayName,
            };

            bondIssueReport.InitOrUpdate(bondRateRepo);
            bondIssueReport.InitOrUpdate(bondRateRepo2);
            var themeName = ThemeHelper.GetTheme(Request);
            bondIssueReport.Chart.Theme = themeName;

            return View("RateOfIssues", bondIssueReport);
        }

        [Localization]
        public ActionResult AddBond(int reportId, string displayName, DateTime? date, string bondType, string term, string couponType, string rating, string grid)
        {
            var serializer = new JavaScriptSerializer();

            if (grid.Contains("&quot"))
                grid = System.Web.HttpUtility.HtmlDecode(grid);
            var g = serializer.Deserialize(grid, typeof(List<BondIssueRate>)) as List<BondIssueRate>;

            var param = new BondIssueParams
            {
                StartDate = date ?? new DateTime(DateTime.Now.Year, 1, 1),
                BondType = bondType,
                Term = term,
                IsFloat = couponType,
                Rating = rating,
            };

            var bondRateRepo = ReportService.GetBondIssueRatesRepo(param).ToList();

            var bondIssueReport = new BondIssueRateReport(reportId)
            {
                Name = displayName,
                Grid = g,
                IsAllRating = rating == "All"
            };

            bondIssueReport.InitOrUpdate(bondRateRepo);
            var themeName = ThemeHelper.GetTheme(Request);
            bondIssueReport.Chart.Theme = themeName;

            return PartialView("_IssueRatesStatistical", bondIssueReport);
        }

        [Localization]
        public ActionResult DeleteBond(int reportId, string displayName, DateTime? date, string bondType, string term, string couponType, string rating, string grid, string itemName)
        {
            var serializer = new JavaScriptSerializer();

            if (grid.Contains("&quot"))
                grid = System.Web.HttpUtility.HtmlDecode(grid);
            var g = serializer.Deserialize(grid, typeof(List<BondIssueRate>)) as List<BondIssueRate>;

            if (g != null)
                g.RemoveAll(b => b.ItemName == itemName);

            var bondIssueReport = new BondIssueRateReport(reportId)
            {
                Name = displayName,
                Grid = g,
                IsAllRating = rating == "All"
            };

            bondIssueReport.UpdateChartOnly(g);
            var themeName = ThemeHelper.GetTheme(Request);
            bondIssueReport.Chart.Theme = themeName;

            return PartialView("_IssueRatesStatistical", bondIssueReport);
        }

        #endregion

        ILog _log = LogManager.GetLogger(typeof(BondReportController));

        #region Issue Amount Report
        [Localization]
        public ActionResult GetIssueAmountReport(int id)
        {
            _log.Error("GetIssueAmountReport start:"+DateTime.Now);
            ReportInfo reportInfo = ReportService.GetReportInfoById(id);
            _log.Error("GetIssueAmountReport 2:" + DateTime.Now);
            IEnumerable<BondIssueAmount> topGrid;
            IEnumerable<BondDetail> bottomGrid;
            GetIssueAmountReportData(null, out topGrid, out bottomGrid);
            _log.Error("GetIssueAmountReport 3:" + DateTime.Now);
            var bondIssueAmounts = topGrid as BondIssueAmount[] ?? topGrid.ToArray();
            var model = new BondIssueAmountReport(
                    reportInfo.ReportId,
                    reportInfo.DisplayName,
                    Resources.Global.Unit_Option_100M,
                    bondIssueAmounts, bottomGrid);
            _log.Error("GetIssueAmountReport 4:" + DateTime.Now);
            int year = DateTime.Now.Year;
            int month = 12;

            if (DateTime.Now.Month == 1)
                year = DateTime.Now.Year - 1;
            else
                month = DateTime.Now.Month - 1;

            ViewData["StartDate"] = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Type"] = ConstValues.Type_Bond_Class;
            ViewData["TypeList"] = bondIssueAmounts.Count() == 0 ? "" : string.Join(",", bondIssueAmounts.Select(re => re.Type).ToList());
            ViewData["SubType"] = "";
            ViewData["UseSubType"] = false;
            ViewData["UseSecType"] = "n";
            ViewData["Unit"] = ConstValues.Unit_100M;
            ViewData["TopGridName"] = model.TopGridName;
            ViewData["TypeValue"] = (model.TopGrid == null || model.TopGrid.Count() == 0) ? "" : model.TopGrid.Select(t => t.Type).First();
            ViewData["SubTypeValue"] = "";
            ViewData["TopGridName"] = model.TopGridName;
            ViewData["BottomGridName"] = model.BottomGridName;
            _log.Error("GetIssueAmountReport 5:" + DateTime.Now);
            return View("IssueAmountReport", model);
        }

        [Localization]
        public ActionResult GetIssueAmountReportContent(int reportId, string reportName, string type, string typeList, string useSubType, string subType, string startDate, string endDate, string unit)
        {
            var param = new BondIssueAmountParams
            {
                Type = type,
                TypeList = typeList.Split(',').ToList(),
                UseSubType = useSubType == "true",
                SubType = subType,
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                Unit = unit
            };

            IEnumerable<BondIssueAmount> topGrid;
            IEnumerable<BondDetail> bottomGrid;
            GetIssueAmountReportData(param, out topGrid, out bottomGrid);
            var model = new BondIssueAmountReport(
                    reportId,
                    reportName,
                    HtmlUtil.GetUnitOptionByKey(unit), // for display
                    topGrid, bottomGrid);

            if (topGrid == null || topGrid.Count() == 0)
            {
                return new EmptyResult();
            }

            ViewData["StartDate"] = param.StartDate;
            ViewData["EndDate"] = param.EndDate;
            ViewData["Type"] = param.Type;
            ViewData["TypeList"] = typeList;
            ViewData["SubType"] = param.SubType;
            ViewData["UseSubType"] = param.UseSubType && type != subType;
            ViewData["UseSecType"] = (param.UseSubType && type != subType) ? "y" : "n";
            ViewData["Unit"] = param.Unit;
            ViewData["TopGridName"] = model.TopGridName;
            ViewData["TypeValue"] = model.TopGrid.Select(t => t.Type).First();
            ViewData["SubTypeValue"] = "";
            ViewData["TopGridName"] = model.TopGridName;
            ViewData["BottomGridName"] = model.BottomGridName;

            return View("_IssueAmountReportContent", model);
        }

        [Localization]
        public ActionResult RefreshBondDetail(int reportId, string type, string typeValue, string subType, string subTypeValue, string useSubType, string isParent, string startDate, string endDate, string unit,  string itemList, int startPage, int pageSize)
        {
            var bottomGrid = ReportService.GetBondDetailByTypeAndSubType(
                new BondDetailParams
                {
                    Type = type,
                    TypeValue = typeValue,
                    SubType = subType,
                    SubTypeValue = subTypeValue,
                    UseSubType = useSubType == "true",
                    IsParent = isParent == "true",
                    StartDate = DateTime.Parse(startDate),
                    EndDate = DateTime.Parse(endDate),
                    ItemList = itemList,
                    StartPage = startPage,
                    PageSize = pageSize
                });
            if (bottomGrid == null || bottomGrid.Count() == 0)
            {
                return new EmptyResult();
            }
            return View("_BondDetail", new Tuple<IEnumerable<BondDetail>, int>(bottomGrid, reportId));
        }

        [Localization]
        public string UpdateSubTypeOptions(string type)
        {
            var options = HtmlUtil.GetSubTypeOptions2(type);
            return options;
        }

        [Localization]
        public ActionResult ExportReport(string reportId, string startDate, string endDate, string type, string typeValue, string typeList, string subType, string subTypeValue, string useSecType, string unit, string isParent, string reportName, string reportType, int startPage=1)
        {
            IEnumerable<BondIssueAmountExport> summaryExport ;
            IEnumerable<BondDetail> detailExport ;
            BondIssueAmountParams summaryParam ;
            BondDetailParams detailParam ;

            string[] headers ;
            string[] rowKeys ;

            if (reportType == "summary")
            {
                summaryParam = new BondIssueAmountParams
                {
                    Type = type,
                    TypeList = string.IsNullOrEmpty(typeList) ? null : typeList.Split(',').ToList(),
                    UseSubType = useSecType == "y",
                    SubType = subType,
                    StartDate = DateTime.Parse(startDate),
                    EndDate = DateTime.Parse(endDate),
                    Unit = unit
                };

                var summaryReport = ReportService.GetIssueAmount(summaryParam);
                summaryExport = (from s in summaryReport
                                 select new BondIssueAmountExport
                                 {
                                     TypeName = s.IsParent==1 ? (string.IsNullOrEmpty(s.Type) ? Resources.Global.Tip_Other : s.TypeName) : (string.IsNullOrEmpty(s.SubType) ? Resources.Global.Tip_Other : s.SubTypName),
                                     Issues = s.Issues,
                                     IssuesPercent = s.IssuesPercent,
                                     IssuesAmount = s.IssuesAmount,
                                     IssuesAmountPercent = s.IssuesAmountPercent,
                                     LowestIssueRate = s.LowestIssueRate,
                                     HighestIssueRate = s.HighestIssueRate,
                                     Row_level = s.IsParent==1 ? "0" : "1"
                                 }).ToList();
                headers = GetIssueAmountHeader("summary");
                rowKeys = GetIssueAmountCloumns("summary");

                string sumGroupColumnName = "TypeName";
                string groupedRowLevelColumnName = "Row_level";
                try
                {
                    return new ExcelResult(summaryExport.AsQueryable(), headers, rowKeys, reportName, reportName, sumGroupColumnName: sumGroupColumnName, groupedRowLevelColumnName: groupedRowLevelColumnName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                detailParam = new BondDetailParams
                {
                    Type = type,
                    TypeValue = typeValue,
                    UseSubType = (useSecType == "y"), // && !string.IsNullOrEmpty(subTypeValue)) ? true : false,
                    SubType = subType,
                    SubTypeValue = subTypeValue,
                    IsParent = isParent == "true",
                    StartDate = DateTime.Parse(startDate),
                    EndDate = DateTime.Parse(endDate),
                    ItemList = typeList,
                    PageSize = 300,
                    StartPage = startPage
                    
                };

                detailExport = ReportService.GetBondDetailByTypeAndSubType(detailParam);
                headers = GetIssueAmountHeader("detail");
                rowKeys = GetIssueAmountCloumns("detail");
                var bondDetails = detailExport as BondDetail[] ?? detailExport.ToArray();
                bondDetails.ToList().ForEach(r => r.Term = r.Term + (r.OrigAvgLife < 1 ? Resources.Global.Time_Day : Resources.Global.Time_Year));

                string dateFormat = "yyyy-MM-dd";
                try
                {
                    return new ExcelResult(bondDetails.AsQueryable(), headers, rowKeys, reportName, reportName, specificDateFormat: dateFormat);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion

        [Localization]
        public ActionResult BondRatingHist(string id)
        {
            var bondCode = id.Replace("BondRating", "");
            ViewBag.ID = bondCode;
            var viewModel = new BondRatingHistViewModel(bondCode, BondReportRepository, CmaRepository);
            return PartialView(viewModel);
        }

        public ActionResult DownloadRatingFile(long id)
        {
            var file = ZcxRepository.GetRatingFileData(id);
            if (file == null)
                return HttpNotFound();
            if (file.Content == null)
                return HttpNotFound();
            var contentType = MimeHelper.GetMimeTypeByExt(file.FileType);
            return File(file.Content, contentType,
                Resources.Global.DownloadFileName + "." + file.FileType);
        }


        #region jsonp api rating history

        [JsonpFilter]
        public ActionResult GetRatingHistory(string id)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("zh-cn");
            var code = BondInfoRepository.GetBondCodeById(id);
            if (string.IsNullOrEmpty(code))
            {
                return Json(new
                {
                    IssuerRatingList = new List<RATE_ORG_CRED_HIS>(),
                    BondRatingList = new List<BondRatingHist>(),
                }, JsonRequestBehavior.AllowGet);
            }
            var comCode = ZcxRepository.GetComCodeFromBondCode(code).ToString();
            var issuerRatingHistory = ZcxRepository.GetIssuerRating(comCode).Select(i => new
            {
                RATE_DATE = i.RATE_WRIT_DATE.ToString("yyyy-MM-dd"),
                RATE_ORG = i.Org,
                RATE = i.ISS_CRED_LEVEL
            });
            var bondRatingHistory = BondReportRepository.GetBondRatingByCode(code).Select(b => new
            {
                RATE_DATE = b.RATE_DATE.ToString("yyyy-MM-dd"),
                RATE_ORG = b.RATE_ORG,
                RATE = b.RATE
            });

            return Json(new
            {
                IssuerRatingList = issuerRatingHistory,
                BondRatingList = bondRatingHistory
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private functions

        private void GetIssueAmountReportData(BondIssueAmountParams param, out IEnumerable<BondIssueAmount> topGrid, out IEnumerable<BondDetail> bottomGrid)
        {
            var detailParam = new BondDetailParams();
            if (param == null)
            {
                int year = DateTime.Now.Year;
                int month;

                if (DateTime.Now.Month == 1)
                {
                    year = DateTime.Now.Year - 1;
                    month = 12;
                }
                else
                    month = DateTime.Now.Month - 1;

                param = new BondIssueAmountParams();
                param.Type = "Bond_Class";
                param.TypeList = UIStaticDataCache.Instance.AssetClass.Select(re => re.Value).ToList();
                param.Unit = "100M";
                param.UseSubType = false;
                param.SubType = "";
                param.StartDate=new DateTime(year, month, 1);
                param.EndDate = DateTime.Now;

                _log.Error("GetIssueAmount Data 1:" + DateTime.Now);
                topGrid = ReportService.GetIssueAmount(param).ToList();
                _log.Error("GetIssueAmount Data 2:" + DateTime.Now);
                if (topGrid.Count() == 0)
                {
                    bottomGrid = new List<BondDetail>();
                }
                else {
                    detailParam.Type = ConstValues.Type_Bond_Class;
                    detailParam.TypeValue = topGrid.Select(t => t.Type).First();
                    detailParam.StartDate = new DateTime(year, month, 1);
                    detailParam.EndDate = DateTime.Now;
                    detailParam.UseSubType = false;
                    detailParam.ItemList = detailParam.TypeValue;
                    detailParam.StartPage = 1;
                    detailParam.PageSize = 300;
                    bottomGrid = ReportService.GetBondDetailByTypeAndSubType(detailParam);
                    _log.Error("GetIssueAmount Data 3:" + DateTime.Now);
                }
                
            }
            else
            {
                _log.Error("GetIssueAmount Data 4:" + DateTime.Now);
                topGrid = ReportService.GetIssueAmount(param);
                _log.Error("GetIssueAmount Data 5:" + DateTime.Now);
                var bondIssueAmounts = topGrid as BondIssueAmount[] ?? topGrid.ToArray();
                if (bondIssueAmounts.Count() == 0)
                {
                    bottomGrid = new List<BondDetail>();
                }
                else {
                    detailParam.Type = param.Type;
                    detailParam.TypeValue = bondIssueAmounts.Select(t => t.Type).First();
                    detailParam.StartDate = param.StartDate;
                    detailParam.EndDate = param.EndDate;
                    detailParam.UseSubType = false;
                    detailParam.PageSize = 300;
                    detailParam.StartPage = 1;
                    detailParam.ItemList = string.Join(",", param.TypeList);
                    bottomGrid = ReportService.GetBondDetailByTypeAndSubType(detailParam);
                }
               
                _log.Error("GetIssueAmount Data 6:" + DateTime.Now);
            }
        }


        private string[] GetIssueAmountHeader(string reportType)
        {
            string[] header = null;

            if (reportType == "summary")
            {
                header = new[] {
                            Resources.Global.BondIssue_Type,
                            Resources.Global.BondIssue_Issues,
                            Resources.Global.BondIssue_Issues_PCT,
                            Resources.Global.BondIssue_Issue_Amt,
                            Resources.Global.BondIssue_Issues_Amt_PCT,
                            Resources.Global.BondIssue_Lowest_Issue_Rate,
                            Resources.Global.BondIssue_Highest_Issue_Rate,
                };
            }
            else if (reportType == "detail")
            {
                header = GetBondDetailReportHeader();
            }

            return header;
        }

        private string[] GetIssueAmountCloumns(string reportType)
        {
            string[] cloumns = null;

            if (reportType == "summary")
            {
                cloumns = new[] {
                            "TypeName",
                            "Issues",
                            "IssuesPercent",
                            "IssuesAmount",
                            "IssuesAmountPercent",
                            "LowestIssueRate",
                            "HighestIssueRate"
                };
            }
            else if (reportType == "detail")
            {
                cloumns = GetBondDetailCloumns();
            }

            return cloumns;
        }

        private string[] GetBondDetailReportHeader()
        {
            return new[] {
                            Resources.Global.Bond_Code,
                            Resources.Global.Bond_Name,
                            Resources.Global.Bond_IssueDate,
                            Resources.Global.Bond_IssueAmount + "(" + Resources.Global.Unit_Option_K + ")",
                            Resources.Global.Bond_Coupon_class,
                            Resources.Global.Bond_Coupon_Frequency,
                            Resources.Global.Bond_Coupon_Rate,
                            Resources.Global.Bond_Value_Date,
                            Resources.Global.Bond_Maturity_Date,
                            Resources.Global.Bond_Listing_Date,
                            Resources.Global.Bond_Term,
                            Resources.Global.Bond_Issue_Price,
                            Resources.Global.Bond_Ref_Yield,
                            Resources.Global.Bond_Option,
                            Resources.Global.Bond_ISBN,
                            Resources.Global.Bond_Rating,
                            Resources.Global.Bond_Rating_Agency,
                            Resources.Global.Bond_Party_Rating,
                            Resources.Global.Bond_Party_Rating_Agency,
                            Resources.Global.Bond_CDC_Classify,
                            Resources.Global.Bond_Issuer,
                            Resources.Global.Bond_Float_Index,
                            Resources.Global.Bond_Spread,
                            Resources.Global.Bond_Day_Count,
                            Resources.Global.Bond_Currency,
                            Resources.Global.Bond_Seniority,
                            Resources.Global.Bond_IssueComment};
        }

        private string[] GetBondDetailCloumns()
        {
            return new[] {
                            "Code",
                            "BondName",
                            "IssueDate",
                            "IssueAmount",
                            "CouponClass",
                            "CouponFreq",
                            "CouponRate",
                            "ValueDate",
                            "MaturityDate",
                            "ListingDate",
                            "Term",
                            "IssuePrice",
                            "RefYield",
                            "Option",
                            "ISBN",
                            "BondRating",
                            "BondRatingAgency",
                            "PartyRating",
                            "PartyRatingAgency",
                            "CDCTypeName",
                            "Issuer",
                            "FloatIndex",
                            "Spread",
                            "DayCount",
                            "Currency",
                            "Seniority",
                            "issueComment"
                };
        }

        #endregion

        #region New Depository Blance
        [Localization]
        public JsonResult GetBondIssuMatures(DateTime start, DateTime end, string category, string itemList, int useSubType, string subType, string unit, int id, string rate, string chartType, string columnType)
        {
            var topTable = BondReportRepository.GetBondDepositoryBalanceNew(category, start, end, unit, itemList, useSubType, subType);
            var chart = GetIssuMaturesTopChart(rate, chartType, columnType, start, end, category, itemList, unit);
            var SumData =
                new
                {
                    TopTable = BuildBondIssuMaturesTopTable(topTable),
                    Chart = chart
                };
            return Json(SumData, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public JsonResult GetIssuMaturesTopChartData(string rate, string chartType, string columnType, DateTime start, DateTime end, string category, string itemList, string unit, int isUseSubCategory = 0, string subCategory = "Bond_Class", string subCategoryValue = "", string categoryValue = "")
        {
            if (categoryValue != "" && categoryValue != "Total")
                itemList = categoryValue;
            var chart = GetIssuMaturesTopChart(rate, chartType, columnType, start, end, category, itemList, unit, isUseSubCategory, subCategory, subCategoryValue, categoryValue=="Total");
            return Json(chart, JsonRequestBehavior.AllowGet);
        }
        private ChartData GetIssuMaturesTopChart(string rate, string chartType, string columnType, DateTime start, DateTime end, string category, string itemList, string unit, int isUseSubCategory = 0, string subCategory = "Bond_Class", string subCategoryValue = "",bool isTotal=false)
        {
            if (start > end) return new ChartData();
            var chartData = new ChartData
            {
                ChartType = chartType,
                YText = Resources.Global.Unit + "(" + HtmlUtil.GetUnitOptionByKey(unit) + ")"
            };
            switch (columnType)
            {
                case "InitialBalance":
                    chartData.Decimal = 2;
                    break;
                case "Issues":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 0;
                    break;
                case "IssuesPercent":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "IssuesAmount":
                    chartData.Decimal = 2;
                    break;
                case "IssuesAmountPercent":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;
                case "MaturityBonds":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 0;
                    break;
                case "MaturityAmount":
                    chartData.Decimal = 2;
                    break;
                case "EndBalance":
                    chartData.Decimal = 2;
                    break;
                case "EndIssuesPercent":
                    chartData.YText = string.Empty;
                    chartData.Decimal = 2;
                    break;

            }
            var typeName = "TypeCn";
            var subTypeName = "SubTypeCn";
            if (CultureHelper.IsEnglishCulture())
            {
                typeName = "TypeEn";
                subTypeName = "SubTypeEn";
            }
            if (chartType == "bar" || chartType == "line")
            {
                var yearsDic = new Dictionary<string, DataTable>();
                var dates = PopulateRateTimes(start, end, rate);
                for (var i = 0; i < dates.Count; i += 2)
                {
                    var currentStart = dates[i];
                    var currentEnd = dates[i + 1];
                    var summaryData = BondReportRepository.GetBondDepositoryBalanceChart(columnType, currentStart, currentEnd, category, itemList, unit, isUseSubCategory, subCategory, subCategoryValue);
                    yearsDic.Add(rate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M"), summaryData);
                }
                chartData.ColumnCategories = yearsDic.Keys.Select(x => x.ToString()).ToArray();
                var groupDic = new Dictionary<string, List<double>>();
                if (isTotal)
                {
                    groupDic.Add(Resources.Global.Total, new List<double>());
                    foreach (var keyValue in yearsDic)
                    {
                        double total = 0;
                        for (var j = 0; j < keyValue.Value.Rows.Count; j++)
                        {
                            total+=Convert.ToDouble(keyValue.Value.Rows[j][columnType]);
                        }
                        groupDic[Resources.Global.Total].Add(total);
                    }
                }
                else 
                {
                    foreach (var keyValue in yearsDic)
                    {
                        for (var j = 0; j < keyValue.Value.Rows.Count; j++)
                        {
                            var name = keyValue.Value.Rows[j][typeName].ToString();
                            if (string.IsNullOrEmpty(subCategoryValue))
                            {
                                if (!groupDic.ContainsKey(name))
                                {
                                    groupDic.Add(name, new List<double>());
                                }
                            }
                            else
                            {
                                name = string.Format("{0}({1})", keyValue.Value.Rows[j][typeName], keyValue.Value.Rows[j][subTypeName]);
                                if (!groupDic.ContainsKey(name))
                                {
                                    groupDic.Add(name, new List<double>());
                                }
                            }
                            groupDic[name].Add(Convert.ToDouble(keyValue.Value.Rows[j][columnType]));
                        }
                    }
                }
                var seriesDataList = new List<SeriesData>();
                foreach (var keyValue in groupDic)
                {
                    var sData = new SeriesData { name = keyValue.Key };
                    if (keyValue.Value != null)
                    {

                        sData.data = keyValue.Value.ToArray();
                        seriesDataList.Add(sData);
                    }
                }
                chartData.ColumnSeriesData = seriesDataList.ToArray();
            }
            else
            {
                var groupData = BondReportRepository.GetBondDepositoryBalanceChart(columnType, start, end, category, itemList, unit, isUseSubCategory, subCategory, subCategoryValue);
                var pieData = new List<PieSectionData>();
                for (int j = 0; j < groupData.Rows.Count; j++)
                {
                    var currentSection = new PieSectionData { name = groupData.Rows[j][typeName].ToString(), y = Convert.ToDouble(groupData.Rows[j][columnType]) };
                    pieData.Add(currentSection);
                }
                chartData.PieSeriesData = pieData.ToArray();
            }


            return chartData;
        }

        [Localization]
        public ActionResult ExportExcelForBondIm(DateTime start, DateTime end, string category, string itemList, int useSubType, string subType, string unit, string reportName)
        {
            var table = BondReportRepository.GetBondDepositoryBalanceNew(category, start, end, unit, itemList, useSubType, subType);
            var jtable = BuildBondIssuMaturesTopTable(table);
            var jP = new JsonExcelParameter { Table = jtable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public ActionResult ExportExcelForBondImChart(string rate, string columnType, DateTime start, DateTime end, string category, string itemList, string unit, string reportName, int isUseSubCategory = 0, string subCategory = "Bond_Class", string subCategoryValue = "", string categoryValue = "")
        {
            if (categoryValue != "" && categoryValue != "Total")
                itemList = categoryValue;
            var jtable = BuildBondIssuMaturesChartDataForExcel(rate, columnType, start, end, category, itemList, unit, isUseSubCategory, subCategory, subCategoryValue, categoryValue=="Total");
            var jP = new JsonExcelParameter { Table = jtable, TableName = GetExcelTitleByDataType(columnType) + Resources.Global.Bond_DimSum_Summary, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }
        private JsonTable BuildBondIssuMaturesChartDataForExcel(string rate, string columnType, DateTime start, DateTime end, string category, string itemList, string unit, int isUseSubCategory = 0, string subCategory = "Bond_Class", string subCategoryValue = "",bool isTotal=false)
        {
            var jTable = new JsonTable();
            var typeName = "TypeCn";
            var subTypeName = "SubTypeCn";
            if (CultureHelper.IsEnglishCulture())
            {
                typeName = "TypeEn";
                subTypeName = "SubTypeEn";
            }

            var yearsDic = new Dictionary<string, DataTable>();
            var dates = PopulateRateTimes(start, end, rate);
            for (var i = 0; i < dates.Count; i += 2)
            {
                var currentStart = dates[i];
                var currentEnd = dates[i + 1];
                var summaryData = BondReportRepository.GetBondDepositoryBalanceChart(columnType, currentStart, currentEnd, category, itemList, unit, isUseSubCategory, subCategory, subCategoryValue);
                yearsDic.Add(rate == "y" ? currentEnd.ToString("yyyy") : currentEnd.ToString("yyyy-M"), summaryData);
            }
            var rows = yearsDic.FirstOrDefault().Value.Rows;
            var typeList = new Dictionary<string, string>();
            string header;
            foreach (DataRow row in rows)
            {
                header = (string.IsNullOrEmpty(subCategoryValue) || subCategoryValue == "undefined")
                    ? row[typeName].ToString()
                    : String.Format("{0}({1})", row[typeName], row[subTypeName]);
                typeList.Add(row["Type"].ToString(), header);
            }

            jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Date, ColumnName = "Date" });
            if (!isTotal)
            {
                foreach (var t in typeList)
                {
                    jTable.ColumTemplate.Add(new JsonColumn { Name = t.Value, ColumnName = t.Key + "_" + t.Value });
                }
            }
            else {
                jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Total, ColumnName = "Total" });
            }
            

            foreach (var f in yearsDic)
            {
                var currentRow = new Dictionary<string, string>();
                currentRow.Add("Date", f.Key);
                if (isTotal)
                {
                    double total = 0;
                    foreach (DataRow v in f.Value.Rows)
                    {
                        total += Convert.ToDouble(v[columnType]);
                    }
                    currentRow.Add("Total", total.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    foreach (DataRow v in f.Value.Rows)
                    {
                        var name = (string.IsNullOrEmpty(subCategoryValue) || subCategoryValue == "undefined")
                            ? v[typeName]
                            : String.Format("{0}({1})", v[typeName], v[subTypeName]);
                        currentRow.Add(v["Type"] + "_" + name, v[columnType].ToString());
                    }
                }
                
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        private JsonTable BuildBondIssuMaturesTopTable(DataTable table)
        {
            var jTable = new JsonTable();
            var headers = GetIssuMaturesTopTableHeaders();
            var columns = GetIssuMaturesTopTableColumns();
            for (int i = 0; i < headers.Length; i++)
            {
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = columns[i], Name = headers[i] });
            }
            var typeName = "TypeCn";
            var subTypeName = "SubTypeCn";
            if (CultureHelper.IsEnglishCulture())
            {
                typeName = "TypeEn";
                subTypeName = "SubTypeEn";
            }
            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>
                {
                    {"TypeValue", row["Type"].ToString()},
                    {"Type", row[typeName].ToString()},
                    {"SubType", row[subTypeName].ToString()},
                    {"SubTypeValue", row["SubType"].ToString()},
                    {"isParent", row["isParent"].ToString()},
                    {"EndBalance", UIGenerator.FormatCellValue(row["EndBalance"], "decimal")},
                    {"InitialBalance", UIGenerator.FormatCellValue(row["InitialBalance"], "decimal")},
                    {"Issues", row["Issues"].ToString()},
                    {"IssuesPercent", UIGenerator.FormatCellValue(row["IssuesPercent"], "decimal")},
                    {"IssuesAmount", UIGenerator.FormatCellValue(row["IssuesAmount"], "decimal")},
                    {"IssuesAmountPercent", UIGenerator.FormatCellValue(row["IssuesAmountPercent"], "decimal")},
                    {"MaturityBonds", row["MaturityBonds"].ToString()},
                    {"MaturityAmount", UIGenerator.FormatCellValue(row["MaturityAmount"], "decimal")},
                    {"EndIssues", row["EndIssues"].ToString()},
                    {"EndIssuesPercent", UIGenerator.FormatCellValue(row["EndIssuesPercent"], "decimal")}
                };
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }
        private string[] GetIssuMaturesTopTableHeaders()
        {
            string[] cloumns= {
                        Resources.Global.BondIssue_Type,
                        Resources.Global.BondIssue_End_Balance,
                        Resources.Global.BondIssue_Initial_Balance,
                        Resources.Global.BondIssue_Issues,
                        Resources.Global.BondIssue_Issues_PCT,
                        Resources.Global.BondIssue_Issue_Amt,
                        Resources.Global.BondIssue_Issues_Amt_PCT,
                        Resources.Global.BondIssue_Maturity_Bonds,
                        Resources.Global.BondIssue_Maturity_Amount,
                        Resources.Global.BondIssue_End_Issues,
                        Resources.Global.BondIssue_End_Issues_PCT
            };
            return cloumns;
        }
        private string[] GetIssuMaturesTopTableColumns()
        {
            string[] headers = new[] {
                        "Type",
                        "EndBalance",
                        "InitialBalance",
                        "Issues",
                        "IssuesPercent",
                        "IssuesAmount",
                        "IssuesAmountPercent",
                        "MaturityBonds",
                        "MaturityAmount",
                        "EndIssues",
                        "EndIssuesPercent"
            };
            return headers;
        }
        private string GetExcelTitleByDataType(string dataType)
        {
            string title = "";

            switch (dataType)
            {
                case "EndBalance":
                    title = Resources.Global.BondIssue_End_Balance;
                    break;
                case "InitialBalance":
                    title = Resources.Global.BondIssue_Initial_Balance;
                    break;
                case "Issues":
                    title = Resources.Global.BondIssue_Issues;
                    break;
                case "IssuesPercent":
                    title = Resources.Global.BondIssue_Issues_PCT;
                    break;
                case "IssuesAmount":
                    title = Resources.Global.BondIssue_Issue_Amt;
                    break;
                case "IssuesAmountPercent":
                    title = Resources.Global.BondIssue_Issues_Amt_PCT;
                    break;
                case "MaturityBonds":
                    title = Resources.Global.BondIssue_Maturity_Bonds;
                    break;
                case "MaturityAmount":
                    title = Resources.Global.BondIssue_Maturity_Amount;
                    break;
                case "EndIssues":
                    title = Resources.Global.BondIssue_End_Issues;
                    break;
                case "EndIssuesPercent":
                    title = Resources.Global.BondIssue_End_Issues_PCT;
                    break;
            }

            return title;
        }
        [Localization]
        public string UpdateSubTypeOptions3(string type)
        {
            var options = HtmlUtil.GetSubTypeOptions3(type);
            return options;
        }
        #endregion

        #region Bond IssuanceMatures buttom detail
        [Localization]
        public JsonResult GetIssuanceMaturesDetailData(DateTime start, DateTime end, string type, string typeValue, int useSubType, string subType, string subTypeValue, string unit, string term, int id, string order, string itemList, int startPage, int pageSize)
        {
            var data = GetBottomTable(new BondDetailParams
            {
                Type = type,
                TypeValue = typeValue,
                UseSubType = useSubType == 1,
                SubType = subType,
                SubTypeValue = subTypeValue,
                StartDate = start,
                EndDate = end,
                Term = term,
                OrderBy = order,
                ItemList = itemList,
                StartPage = startPage,
                PageSize = pageSize
            }, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForIssuanceMaturesDetail(DateTime start, DateTime end, string type, string typeValue, int useSubType, string subType, string subTypeValue, string unit, string term, string reportName, int id, string order, string itemList, int startPage, int pageSize)
        {
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            var param = new BondDetailParams
            {
                Type = type,
                TypeValue = typeValue,
                UseSubType = useSubType == 1,
                SubType = subType,
                SubTypeValue = subTypeValue,
                StartDate = start,
                EndDate = end,
                Term = term,
                OrderBy = order,
                ItemList = itemList,
                StartPage = startPage,
                PageSize = pageSize
            };
            int total;
            var bottomTable = BondReportRepository.GetBondDetailByTypeNew(param, out total);

            ReportParameter reportParam = new ReportParameter
            {
                StartDate = start,
                EndDate = end,
                Unit = string.IsNullOrEmpty(unit) ? "100M" : unit,
            };

            var reportcolumndefinitions = columns as REPORTCOLUMNDEFINITION[] ?? columns.ToArray();
            return new ExcelResult(bottomTable.AsEnumerable().AsQueryable(), reportcolumndefinitions.Select(x => x.DisplayName).ToArray(), reportcolumndefinitions.Select(x => x.COLUMN_NAME).ToArray(), reportName, reportName, false, null, reportParam, false, specificDateFormat: "yyyy-MM-dd");
        }
        private JsonTable GetBottomTable(BondDetailParams param, int id)
        {
            var userColumnService = (UserColumnService)DependencyResolver.Current.GetService(typeof(UserColumnService));
            var columns = userColumnService.GetUserColumns(UserSettingHelper.GetUserId(Request), id);
            int total;
            var bottomTable = BondReportRepository.GetBondDetailByTypeNew(param,out total);
            var reportcolumndefinitions = columns as REPORTCOLUMNDEFINITION[] ?? columns.ToArray();
            var jsonTable = BuidJsonTable(bottomTable, reportcolumndefinitions, total, param.StartPage, param.PageSize);
            List<Dictionary<string, string>> rowData = jsonTable.RowData;
            foreach (Dictionary<string, string> currentRow in rowData)
            {
                foreach (DataRow row in bottomTable.Rows)
                {
                    if (row["AssetId"].ToString() == currentRow["AssetId"])
                    {
                        if (!reportcolumndefinitions.Any(x => x.COLUMN_NAME == "IssuerOrgId"))
                        {
                            currentRow.Add("IssuerOrgId", row["IssuerOrgId"].ToString());
                        }
                        break;
                    }
                }
            }
            return jsonTable;
        }
        #endregion

    }
}
