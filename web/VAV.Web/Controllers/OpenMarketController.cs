using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.Common;
using VAV.DAL.Services;
using VAV.Entities;
using VAV.Web.Localization;
using VAV.Web.ViewModels.OpenMarket;
using VAV.Model.Data;
using VAV.Model.Data.OpenMarket;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.DAL.Report;
using OpenMarketRepo = VAV.Model.Data.OpenMarket.OpenMarketRepo;

namespace VAV.Web.Controllers
{
    /// <summary>
    /// Open Market controller
    /// </summary>
    public class OpenMarketController : Controller
    {
        /// <summary>
        /// Report Generator
        /// </summary>
        [Dependency]
        public ReportService ReportService { get; set; }

        [Dependency]
        public OpenMarketReportRepository OpenMarketReportRepository { get; set; }

        /// <summary>
        /// _menuService
        /// </summary>
        [Dependency]
        public MenuService MenuService { get; set; }

        public DateTime DefaultStartDate
        {
            get { return new DateTime(DateTime.Now.Year - 1, DateTime.Now.Month, 1); }
        }

        [Localization]
        public ActionResult ExportOpenMarketDetailDataReport(int reportId, string type, string category, string startDate, string endDate, string unit, string includeExpired)
        {
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                Type = type,
                Group = category,
                Unit = unit,
                IncludeExpired = includeExpired == "true" ? true : false
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();
            //tempFlag is refecenced to sign whether category is used
            int tempFlag = 0;
            var result = VAV.Web.Extensions.OpenMarketExtension.GetCategorys(openMarketRepo, category, ref tempFlag);

            string[] rowKeys = { "Date", "IssueDate", "Direction", "OperationType", "Code", "OperationTerm", "Volume", "Amount", "PirceRate", "MaturityDate", "RefRate" };
            string[] headers = { Resources.Global.Date, Resources.Global.Issue_Date, Resources.Global.Operation_Direction, Resources.Global.Operation_Type, Resources.Global.Code, Resources.Global.Option_Term, Resources.Global.Volume, Resources.Global.Amount, Resources.Global.Price_Rate, Resources.Global.Maturity_Date, Resources.Global.Ref_Rate };
            var rows = result;
            string reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;

            string isSumRowColumnName = "IsSumItem";
            string sumGroupColumnName = "Category";

            return new ExcelResult(rows.AsQueryable(), headers, rowKeys, reportName, reportName, isSumRowColumnName: isSumRowColumnName, sumGroupColumnName: sumGroupColumnName);
        }

        private ImmaturityAmount GetImmaturityAmount(DateTime queryDate, string unit)
        {
            var table = OpenMarketReportRepository.GetImmaturityAmount(queryDate);
            var row = table.Rows[0];

            var amount = new ImmaturityAmount()
            {
                All = SwitchAmountUnit(unit, double.Parse(row["total"].ToString())),
                CBankBill = SwitchAmountUnit(unit,double.Parse(row["cbb"].ToString())),
                FMD = SwitchAmountUnit(unit, double.Parse(row["fmd"].ToString())),
                Repo = SwitchAmountUnit(unit, double.Parse(row["rp"].ToString())),
                ReverseRepo = SwitchAmountUnit(unit, double.Parse(row["rrp"].ToString())),
                Mlf = SwitchAmountUnit(unit, double.Parse(row["mlf"].ToString()))
            };
            return amount;
        }

        public double SwitchAmountUnit(string unit, double num)
        {
            int multiplier = 1;
            switch (unit)
            {
                case ConstValues.Unit_100M:
                    multiplier = 1;
                    break;
                case ConstValues.Unit_M:
                    multiplier = 100;
                    break;
                case ConstValues.Unit_10K:
                    multiplier = 10000;
                    break;
                case ConstValues.Unit_K:
                    multiplier = 100000;
                    break;
                default:
                    break;
            }
            return num * multiplier;
        }

        [Localization]
        public ActionResult GetMonetaryAndReturnReport(int id)
        {
            ReportInfo reportInfo = ReportService.GetReportInfoById(id);
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = DefaultStartDate,
                EndDate = DateTime.Now,
                IncludeExpired = true,
                //Type = ConstValues.Type_All,
                Type = "CBB,RP,RRP,MLF",
                Unit = ConstValues.Unit_100M,
                Group = ""
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF =Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();

            ViewData["Category" + reportInfo.ReportId] = ConstValues.Type_All;
            ViewData["Type" + reportInfo.ReportId] = "Week";
            ViewData["StartDate" + reportInfo.ReportId] = DefaultStartDate;
            ViewData["EndDate" + reportInfo.ReportId] = DateTime.Now;
            ViewData["Unit" + reportInfo.ReportId] = ConstValues.Unit_100M;
            ViewData["OperationType" + reportInfo.ReportId] = "ALL";
            var result = GetMonetaryAndReturnSummaryModel(DefaultStartDate, DateTime.Now, "Week", openMarketRepo);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                //if (i == result.Count - 1) result[i].SumInjectionWithdrawal = result[i].NetInjectionWithdrawal;
                result[i].SumInjectionWithdrawal = i == result.Count - 1 ? result[i].NetInjectionWithdrawal : result[i + 1].SumInjectionWithdrawal + result[i].NetInjectionWithdrawal;
            }
            if (result.Count == 0)
            {
                return new EmptyResult();
            }
            return View("MonetaryAndReturnAnalysisReport", new MaRAnalysisReportViewModel(reportInfo.ReportId, result, result[0].OpenMarketRepoList, ThemeHelper.GetTheme(Request), GetImmaturityAmount(DateTime.Now, ConstValues.Unit_100M)));
        }

        [Localization]
        public ActionResult GetMonetaryAndReturnReportContent(int reportId, string type, string category, string startDate, string endDate, string unit, string operationType )
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(type))
                return new EmptyResult();
            var startDateValue = DateTime.Parse(startDate);
            var endDateValue = DateTime.Parse(endDate);
            
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = startDateValue,
                EndDate = endDateValue,
                Unit = unit,
                IncludeExpired = true,
                Type = operationType
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext,
            }).ToList();
            ViewData["Type" + reportId] = type;
            ViewData["Category" + reportId] = category;
            ViewData["StartDate" + reportId] = startDate;
            ViewData["EndDate" + reportId] = endDate;
            ViewData["Unit" + reportId] = unit;
            ViewData["OperationType" + reportId] = operationType;
            var result = GetMonetaryAndReturnSummaryModel(startDateValue, endDateValue, type, openMarketRepo);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                //if (i == result.Count - 1) result[i].SumInjectionWithdrawal = result[i].NetInjectionWithdrawal;
                result[i].SumInjectionWithdrawal = i == result.Count - 1 ? result[i].NetInjectionWithdrawal : result[i + 1].SumInjectionWithdrawal + result[i].NetInjectionWithdrawal;
            }
            if (result.Count == 0)
            {
                return new EmptyResult();
            }
            return View("_MonetaryAndReturnAnalysisReportContent", new MaRAnalysisReportViewModel(reportId, result, result[0].OpenMarketRepoList, ThemeHelper.GetTheme(Request), GetImmaturityAmount(endDateValue, unit), operationType));
        }

        [Localization]
        public ActionResult ExportMaRSummaryReport(int reportId, string type, string startDate, string endDate, string unit, string operationType)
        {
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(type))
                return new EmptyResult();
            var startDateValue = DateTime.Parse(startDate);
            var endDateValue = DateTime.Parse(endDate);
            
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = startDateValue,
                EndDate = endDateValue,
                Unit = unit,
                IncludeExpired = true,
                Type = operationType
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();
            var result = GetMonetaryAndReturnSummaryModel(startDateValue, endDateValue, type, openMarketRepo);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                result[i].SumInjectionWithdrawal = i == result.Count - 1 ? result[i].NetInjectionWithdrawal : result[i + 1].SumInjectionWithdrawal + result[i].NetInjectionWithdrawal;
            }
            if (result.Count == 0)
            {
                return new EmptyResult();
            }

            var modelToExport = new MaRAnalysisReportViewModel(reportId, result, null, ThemeHelper.GetTheme(Request), GetImmaturityAmount(endDateValue, unit), operationType);

            var columnsToExport = modelToExport.SummaryGridColumns.ToArray();
            string[] headers = CultureHelper.IsEnglishCulture() ? columnsToExport.Select(c => c.ColumnHeaderEN).ToArray() : columnsToExport.Select(c => c.ColumnHeaderCN).ToArray();
            string[] rowKeys = columnsToExport.Select(c => c.ColumnName).ToArray();

            var rows = modelToExport.SumGrid.AsQueryable();
            string reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;

            return new ExcelResult(rows, headers, rowKeys, reportName, reportName);
        }

        [HttpPost]
        [Localization]
        public ActionResult GetOpenMarketMaRADetailedReport(int reportId, string category, string startDate, string endDate, string unit, string rowName, string operationType)
        {
            if (string.IsNullOrEmpty(rowName))
            {
                return new EmptyResult();
            }
            if (!rowName.Contains('@'))
            {
                return new EmptyResult();
            }
            
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                Unit = unit,
                IncludeExpired = true,
                //Type = ConstValues.Type_All
                Type = operationType 
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();
            ViewData["Report" + reportId] = reportId;
            ViewData["Type" + reportId] = category;
            ViewData["Category" + reportId] = rowName;
            ViewData["StartDate" + reportId] = startDate;
            ViewData["EndDate" + reportId] = endDate;
            ViewData["Unit" + reportId] = unit;
            ViewData["OperationType" + reportId] = operationType;
            var result = GetMonetaryAndReturnSummaryModel(DefaultStartDate, DateTime.Now, category, openMarketRepo);
            var detailedList = result.Where(s => string.Compare(s.Category, rowName, true) == 0).FirstOrDefault();
            if (detailedList == null || detailedList.OpenMarketRepoList.Count == 0)
            {
                return new EmptyResult();
            }
            return View("_DetailDataReportTable", new DetailDataReportViewModel { ID = reportId, Content = detailedList.OpenMarketRepoList });
        }

        [Localization]
        public ActionResult ExportOpenMarketMaRADetailedReport(int reportId, string category, string startDate, string endDate, string unit, string rowName, string operationType)
        {
            if (string.IsNullOrEmpty(rowName))
            {
                return new EmptyResult();
            }
            
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                Unit = unit,
                IncludeExpired = true,
                Type = operationType
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();
            var result = GetMonetaryAndReturnSummaryModel(DefaultStartDate, DateTime.Now, category, openMarketRepo);
            var detailedList = result.Where(s => string.Compare(s.Category, rowName, true) == 0).FirstOrDefault();

            if (detailedList == null || detailedList.OpenMarketRepoList.Count == 0)
            {
                return new EmptyResult();
            }

            string[] rowKeys = { "IssueDate", "Direction", "OperationType", "Code", "OperationTerm", "Volume", "Amount", "PirceRate", "MaturityDate", "RefRate" };
            string[] headers = { Resources.Global.Issue_Date, Resources.Global.Operation_Direction, Resources.Global.Operation_Type, Resources.Global.Code, Resources.Global.Option_Term, Resources.Global.Volume, Resources.Global.Amount, Resources.Global.Price_Rate, Resources.Global.Maturity_Date, Resources.Global.Ref_Rate };

            string reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;

            if (CultureHelper.IsEnglishCulture())
                reportName = "Detailed " + reportName;
            else
                reportName = reportName + "明细";

            return new ExcelResult(detailedList.OpenMarketRepoList.AsQueryable(), headers, rowKeys, reportName, reportName);
        }

        [Localization]
        public ActionResult GetRatesAnalysisReport(int id)
        {
            ReportInfo reportInfo = ReportService.GetReportInfoById(id);

            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = DefaultStartDate,
                EndDate = DateTime.Now,
                IncludeExpired = false,
                Type = ConstValues.Type_All,
                Unit = ConstValues.Unit_100M,
                Group = "",
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();

            var result = openMarketRepo.Where(o => o.TermEn == "7D" || o.TermEn == "14D").ToList();
            if (result.Count == 0)
            {
                return new EmptyResult();
            }

            var initViewModel = new RatesAnalysisReportViewModel(reportInfo.ReportId, result, ThemeHelper.GetTheme(Request));
            ViewData["Type" + reportInfo.ReportId] = result == null ? "-Reverse Repo_7D" : initViewModel.SelectTypeAndTerm;//result[0].OperationType + "_" + result[0].TermEn;
            ViewData["StartDate" + reportInfo.ReportId] = DefaultStartDate;
            ViewData["EndDate" + reportInfo.ReportId] = DateTime.Now;
            ViewData["Unit" + reportInfo.ReportId] = ConstValues.Unit_100M;
            return View("RatesAnalysisReport", initViewModel);
        }

        [Localization]
        public ActionResult RefreshRatesAnalysisReport(int reportId, string typeList, string startDate, string endDate, string unit)
        {
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                IncludeExpired = false,
                Type = ConstValues.Type_All,
                Unit = unit,
                Group = "",
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();

            List<OpenMarketRepo> filteredResult = new List<OpenMarketRepo>();

            //to do, switch select option value to @Resources.Global.Term_Day or Month and remove the following replace code.
            if (!CultureHelper.IsEnglishCulture())
            {
                typeList = typeList.Replace("Reverse Repo", "逆回购");
                typeList = typeList.Replace("Repo", "正回购");
                typeList = typeList.Replace("CBB", "中央银行票据");
                typeList = typeList.Replace("Fmd", "中央国库现金管理");
                typeList = typeList.Replace("Mlf", "中期借贷便利");
            }
            else
            {
                typeList = typeList.Replace("CBB", "Central Bank Bill");
            }
            var selectItemList = typeList.Split('-'); //ArrayToList(typeList);
            foreach (var item in selectItemList)
            {
                if (!string.IsNullOrEmpty(item) && item.Contains('_'))
                {
                    string selectedOperationType = item.Split('_')[0].Trim();
                    string selectedOperationTerm = item.Split('_')[1].Trim();
                    var tempResult=new List<OpenMarketRepo>();
                    if (selectedOperationType == "Mlf" || selectedOperationType == "中期借贷便利")
                    {
                        selectedOperationType = selectedOperationType.Replace("Mlf", "Medium-term Lending Facility");
                        tempResult = openMarketRepo.Where(re => re.OperationType.Contains(selectedOperationType) && string.Compare(re.TermEn, selectedOperationTerm, true) == 0).ToList();
                        foreach (var mlfItem in tempResult)
                        {
                            mlfItem.OperationType = selectedOperationType;
                            mlfItem.Category = selectedOperationType;
                        }
                    }
                    else
                    {
                        tempResult = openMarketRepo.Where(re => string.Compare(re.OperationType, selectedOperationType, true) == 0 && string.Compare(re.TermEn, selectedOperationTerm, true) == 0).ToList(); 
                    }
                    
                    foreach (var result in tempResult)
                    {
                        filteredResult.Add(result);
                    }
                }
            }
            if (filteredResult.Count == 0)
            {
                return new EmptyResult();
            }
            ViewData["Type" + reportId] = filteredResult[0].OperationType.Replace( "Medium-term Lending Facility","Mlf") + "_" + filteredResult[0].TermEn;
            ViewData["StartDate" + reportId] = startDate;
            ViewData["EndDate" + reportId] = endDate;
            ViewData["Unit" + reportId] = unit;
            return View("_RatesAnalysisContent", new RatesAnalysisReportViewModel(reportId, filteredResult.ToList(), ThemeHelper.GetTheme(Request)));
        }

        [Localization]
        public ActionResult ExportRatesAnalysisReport(int reportId, string typeList, string startDate, string endDate, string unit)
        {
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                IncludeExpired = false,
                Type = ConstValues.Type_All,
                Unit = unit,
                Group = "",
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();

            List<OpenMarketRepo> filteredResult = new List<OpenMarketRepo>();

            if (typeList.Contains('-') || typeList.Contains('_'))
            {
                string selectedOperationType = typeList.Contains('-') ? typeList.Split('-')[0].Trim() : typeList.Split('_')[0].Trim();
                string selectedOperationTerm = typeList.Contains('-') ? typeList.Split('-')[1].Trim() : typeList.Split('_')[1].Trim();
                if (selectedOperationType == "Mlf" || selectedOperationType == "中期借贷便利")
                {
                    selectedOperationType = selectedOperationType.Replace("Mlf", "Medium-term Lending Facility");
                    filteredResult = openMarketRepo.Where(re => re.OperationType.Contains(selectedOperationType) && (string.Compare(re.TermEn, selectedOperationTerm, true) == 0|| string.Compare(re.TermCn, selectedOperationTerm, true) == 0)).ToList();
                    foreach (var mlfItem in filteredResult)
                    {
                        mlfItem.OperationType = selectedOperationType;
                        mlfItem.Category = selectedOperationType;
                    }
                }
                else
                {
                    filteredResult = openMarketRepo.Where(re => string.Compare(re.OperationType, selectedOperationType, true) == 0 && (string.Compare(re.TermEn, selectedOperationTerm, true) == 0|| string.Compare(re.TermCn, selectedOperationTerm, true) == 0)).ToList();
                }
                
            }

            string[] rowKeys = { "Date", "IssueDate", "Direction", "OperationType", "Code", "OperationTerm", "Volume", "Amount", "PirceRate", "MaturityDate", "RefRate" };
            string[] headers = { Resources.Global.Date, Resources.Global.Issue_Date, Resources.Global.Operation_Direction, Resources.Global.Operation_Type, Resources.Global.Code, Resources.Global.Option_Term, Resources.Global.Volume, Resources.Global.Amount, Resources.Global.Price_Rate, Resources.Global.Maturity_Date, Resources.Global.Ref_Rate };

            string reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;


            if (filteredResult.Count > 0)
                return new ExcelResult(filteredResult.OrderByDescending(f => f.Date).AsQueryable(), headers, rowKeys, reportName, reportName);
            else
                return new EmptyResult();
        }


        [HttpPost]
        [Localization]
        public ActionResult GetOpenMarketRatesAnalysisDetailedReport(int reportId, string seriesName, string startDate, string endDate, string unit)
        {
            if (string.IsNullOrEmpty(seriesName))
            {
                return new EmptyResult();
            }
            string selectedOperationType = seriesName.Split('-')[0].Trim();
            string selectedOperationTerm = seriesName.Split('-')[1].Trim();
            if (seriesName.Contains("Medium-term Lending Facility"))
            {
                selectedOperationType = "Medium-term Lending Facility";
                selectedOperationTerm = seriesName.Split('-')[2].Trim();
            }
            var openMarketRepo = ReportService.GetOpenMarketRepo(new DetailDataReportParams
            {
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                IncludeExpired = false,
                Type = ConstValues.Type_All,
                Unit = unit,
                Group = "",
            }, new GlobalValueParams
            {
                Type_CBankBill = Resources.Global.Type_CBankBill,
                Type_CBankBillIE = Resources.Global.Type_CBankBillIE,
                Type_Repo = Resources.Global.Type_Repo,
                Type_RepoIE = Resources.Global.Type_RepoIE,
                Type_ReverseRepo = Resources.Global.Type_ReverseRepo,
                Type_ReverseRepoIE = Resources.Global.Type_ReverseRepoIE,
                Type_FMD = Resources.Global.Type_Fmd,
                Type_FMDIE = Resources.Global.Type_FMDIE,
                Type_MLF = Resources.Global.Type_MLF,
                Type_MLFIE = Resources.Global.Type_MLFIE,
                Report_Expire = Resources.Global.Report_Expire,
                Direction_injection = Resources.Global.Direction_injection,
                Direction_withdrawal = Resources.Global.Direction_withdrawal,
                CurrentContext = Resources.Global.CurrentContext
            }).ToList();

            ViewData["Type" + reportId] = seriesName;
            ViewData["StartDate" + reportId] = startDate;
            ViewData["EndDate" + reportId] = endDate;
            ViewData["Unit" + reportId] = unit;
            var result=new List<OpenMarketRepo>();
            if (selectedOperationType == "Mlf" || selectedOperationType == "中期借贷便利")
            {
                selectedOperationType = selectedOperationType.Replace("Mlf", "Medium-term Lending Facility");
                result = openMarketRepo.Where(re => re.OperationType.Contains(selectedOperationType) && (string.Compare(re.TermEn, selectedOperationTerm, true) == 0 || string.Compare(re.TermCn, selectedOperationTerm, true) == 0)).ToList();
                foreach (var mlfItem in result)
                {
                    mlfItem.OperationType = selectedOperationType;
                    mlfItem.Category = selectedOperationType;
                }
            }
            else
            {
                result = openMarketRepo.Where(re => string.Compare(re.OperationType, selectedOperationType, true) == 0 && string.Compare(re.OperationTerm, selectedOperationTerm, true) == 0).ToList();
            }
            if (result.Count == 0)
            {
                return new EmptyResult();
            }
            return View("_DetailDataReportTable", new DetailDataReportViewModel { ID = reportId, Content = result });
        }

        private List<MonetaryAndReturnAnalysisSummaryModel> GetMonetaryAndReturnSummaryModel(DateTime startDate, DateTime endDate, string category, List<OpenMarketRepo> openMarketRepoResult)
        {
            if (string.Compare(category, Constants.SummarizingFrequency.Day.ToString(), true) == 0)
            {
                foreach (var re in openMarketRepoResult)
                {
                    re.Category = GetDateCategory(Constants.SummarizingFrequency.Day, (DateTime)re.Date);
                }
            }
            else if (string.Compare(category, Constants.SummarizingFrequency.Week.ToString(), true) == 0)
            {
                foreach (var re in openMarketRepoResult)
                {
                    re.Category = GetDateCategory(Constants.SummarizingFrequency.Week, (DateTime)re.Date);
                }
            }
            else if (string.Compare(category, Constants.SummarizingFrequency.Month.ToString(), true) == 0)
            {
                foreach (var re in openMarketRepoResult)
                {
                    re.Category = GetDateCategory(Constants.SummarizingFrequency.Month, (DateTime)re.Date);
                }
            }
            else if (string.Compare(category, Constants.SummarizingFrequency.Quarter.ToString(), true) == 0)
            {
                foreach (var re in openMarketRepoResult)
                {
                    re.Category = GetDateCategory(Constants.SummarizingFrequency.Quarter, (DateTime)re.Date);
                }
            }
            else if (string.Compare(category, Constants.SummarizingFrequency.Year.ToString(), true) == 0)
            {
                foreach (var re in openMarketRepoResult)
                {
                    re.Category = GetDateCategory(Constants.SummarizingFrequency.Year, (DateTime)re.Date);
                }
            }
            var resultSet = from gr in openMarketRepoResult
                            group gr by gr.Category into grouped
                            select new MonetaryAndReturnAnalysisSummaryModel
                            {
                                Category = grouped.Key,
                                CategoryStartDate = DateTime.Parse(grouped.Key.Split('@')[0]) < startDate ? startDate : DateTime.Parse(grouped.Key.Split('@')[0]),
                                CategoryEndDate = DateTime.Parse(grouped.Key.Split('@')[1]) > endDate ? endDate : DateTime.Parse(grouped.Key.Split('@')[1]),
                                CbbInjection = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_CBankBill, true) == 0).Sum(s => s.Volume),
                                CbbWithdrawal = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_CBankBillIE, true) == 0).Sum(s => s.Volume),
                                RepoInjection = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_Repo, true) == 0).Sum(s => s.Volume),
                                RepoWithdrawal = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_RepoIE, true) == 0).Sum(s => s.Volume),
                                ReverseRepoInjection = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_ReverseRepo, true) == 0).Sum(s => s.Volume),
                                ReverseRepoWithdrawal = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_ReverseRepoIE, true) == 0).Sum(s => s.Volume),
                                FmdInjection = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_Fmd, true) == 0).Sum(s => s.Volume),
                                FmdWithdrawal = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_FMDIE, true) == 0).Sum(s => s.Volume),
                                MlfInjection = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_MLF, true) == 0 ).Sum(s => s.Volume),
                                MlfWithdrawal = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_MLFIE, true) == 0 ).Sum(s => s.Volume),
                                NetInjection = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_CBankBillIE, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_RepoIE, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_ReverseRepo, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_Fmd, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_MLF, true) == 0).Sum(s => s.Volume),
                                NetWithdrawal = grouped.Where(g => string.Compare(g.OperationType, Resources.Global.Type_CBankBill, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_Repo, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_ReverseRepoIE, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_FMDIE, true) == 0 ||
                                                                     string.Compare(g.OperationType, Resources.Global.Type_MLFIE, true) == 0).Sum(s => s.Volume),
                                NetInjectionWithdrawal = grouped.Sum(s => s.Volume),
                                OpenMarketRepoList = grouped.OrderByDescending(g => g.Date).ToList()
                            };

            return resultSet.OrderByDescending(r => r.CategoryEndDate).ToList();
        }

        private string GetDateCategory(Constants.SummarizingFrequency frequency, DateTime date)
        {
            DateTime startDate;
            DateTime endDate;
            switch (frequency)
            {
                case Constants.SummarizingFrequency.Day:
                    startDate = date;
                    endDate = date;
                    break;
                case Constants.SummarizingFrequency.Week:
                    startDate = date.GetTheFirstDayOfWeek();
                    endDate = startDate.AddDays(6);
                    break;
                //case Constants.SummarizingFrequency.Half:
                //    if (Convert.ToDateTime(categoryName).Month < 7)
                //        return "1H" + Convert.ToDateTime(categoryName).Year.ToString().Substring(2);
                //    else
                //        return "2H" + Convert.ToDateTime(categoryName).Year.ToString().Substring(2);
                case Constants.SummarizingFrequency.Month:
                    startDate = date.GetFirstDayOfMonth();
                    endDate = date.GetLastDayOfMonth();

                    break;
                case Constants.SummarizingFrequency.Quarter:
                    startDate = date.GetTheFirstDayOfQuarter();
                    endDate = startDate.AddMonths(2).GetLastDayOfMonth();
                    break;
                case Constants.SummarizingFrequency.Year:
                    startDate = date.GetFirstDayOfYear();
                    endDate = date.GetLastDayOfYear();
                    break;
                default:
                    startDate = date;
                    endDate = date;
                    break;
            }
            return string.Format("{0}@{1}", startDate.ToShortDateString(), endDate.ToShortDateString());
        }

        #region OpenMarketSearch

        [Localization]
        public ActionResult GetOpenMarketSearch(int id)
        {
            ViewBag.ID = id;
            return View("OpenMarketSearch");
        }

        [Localization]
        public JsonResult GetOpenMarketOperation(int reportId, string type, DateTime startDate, DateTime endDate, string unit, bool includeExpired, string categoryType)
        {
            var table = OpenMarketReportRepository.GetOpenMarketOperation(type, startDate, endDate, unit, includeExpired?1:0);
            if (categoryType == "OperationTerm")
            {
                var dv = table.DefaultView;
                dv.Sort = "TERM";
                table = dv.ToTable();
            }
            var columns = OpenMarketReportRepository.GetOpenMarketColumnDefinitionByReportId(reportId, "Operation");
            var jtable = BuidJsonTable(table, columns);
            return Json(jtable, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public ActionResult ExportExcelForOperation(int reportId, string type, DateTime startDate, DateTime endDate, string unit, bool includeExpired, string category)
        {
            var table = OpenMarketReportRepository.GetOpenMarketOperation(type, startDate, endDate, unit, includeExpired?1:0);
            if (category == "OperationTerm")
            {
                var dv = table.DefaultView;
                dv.Sort = "TERM";
                table = dv.ToTable();
            }
            var columns = OpenMarketReportRepository.GetOpenMarketColumnDefinitionByReportId(reportId, "Operation");
            var jtable = BuidJsonTable(table, columns);
            var reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;
            var totalColumns = new List<string>();
            totalColumns.Add("Amount");
            totalColumns.Add("Volume");
            var jP = new JsonExcelParameter { Table = jtable, TableName = reportName, Source = Resources.Global.Source, totalColumns = totalColumns, sumGroupColumnName = category, isTotal = true };
            return new JsonTableGroupExcelResult(jP);
        }
        private JsonTable BuidJsonTable(DataTable table, IEnumerable<REPORTCOLUMNDEFINITION> columns)
        {
            var jTable = new JsonTable();

            foreach (var column in columns)
            {
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
                currentRow.Add("AssetId", row["AssetId"].ToString());
                currentRow.Add("Category", row["CATEGORYTYPE"].ToString());
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }
        [Localization]
        public JsonResult GetOpenMarketSLO(int reportId, DateTime startDate, DateTime endDate, string unit, string includeExpired)
        {
            var table = OpenMarketReportRepository.GetOpenMarketSLO(startDate, endDate, unit);
            var columns = OpenMarketReportRepository.GetOpenMarketColumnDefinitionByReportId(reportId, "SLO");
            var jtable = BuidJsonTable(table, columns);
            return Json(jtable, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public JsonResult GetOpenMarketSLF(int reportId, DateTime startDate, DateTime endDate, string unit)
        {
            var table = OpenMarketReportRepository.GetOpenMarketSLF(startDate, endDate, unit);
            var columns = OpenMarketReportRepository.GetOpenMarketColumnDefinitionByReportId(reportId, "SLF");
            var jtable = BuidJsonTable(table, columns);
            return Json(jtable, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public ActionResult ExportExcelForSLO(int reportId, DateTime startDate, DateTime endDate, string unit, string includeExpired)
        {
            var table = OpenMarketReportRepository.GetOpenMarketSLO(startDate, endDate, unit);
            var columns = OpenMarketReportRepository.GetOpenMarketColumnDefinitionByReportId(reportId, "SLO");
            var jtable = BuidJsonTable(table, columns);
            var reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;
            reportName = Resources.Global.OpenMarketSLO;
            var totalColumns = new List<string>();
            totalColumns.Add("IssueAmount");
            var jP = new JsonExcelParameter { Table = jtable, TableName = reportName, Source = Resources.Global.Source, totalColumns = totalColumns, sumGroupColumnName = "Direction", isTotal = true };
            return new JsonTableGroupExcelResult(jP);
        }
        [Localization]
        public ActionResult ExportExcelForSLF(int reportId, DateTime startDate, DateTime endDate, string unit)
        {
            var table = OpenMarketReportRepository.GetOpenMarketSLF(startDate, endDate, unit);
            var columns = OpenMarketReportRepository.GetOpenMarketColumnDefinitionByReportId(reportId, "SLF");
            var jtable = BuidJsonTable(table, columns);
            var reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;
            reportName = Resources.Global.OpenMarketSLF;
            var jP = new JsonExcelParameter { Table = jtable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }
      
        #endregion
    }
}
