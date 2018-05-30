using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Model.Data;
using VAV.Web.Localization;
using VAV.DAL.Fundamental;
using VAV.Web.ViewModels.Fundamental;
using VAV.Model.Data.ZCX;
using System.Reflection;
using System.Linq;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.DAL.ResearchReport;
using System.Data;
using VAV.Entities;

namespace VAV.Web.Controllers
{
    public class FundamentalController : BaseController
    {
        [Dependency]
        public MenuService MenuService { get; set; }

        [Dependency]
        public ZCXRepository _repository { get; set; }

        [Dependency]
        public ResearchReportRepository CMARepository { get; set; }

        [Localization]
        public ActionResult NonlistIssuerDetailMain(string id)
        {
            var detailID = int.Parse(id.Replace("FundamentalDetail", string.Empty));
            var viewModel = new NonlistIssuerDetailViewModel(detailID, _repository);
            return PartialView(viewModel);
        }


        /// <summary>
        /// From bond code
        /// </summary>
        /// <param name="id">bond code</param>
        /// <returns></returns>
        [Localization]
        public ActionResult NonlistIssuerDetailMainFromCode(string id)
        {
            var code = id.Replace("FundamentalDetail", string.Empty);
            var comCode = _repository.GetComCodeFromBondCode(code);
            var viewModel = new NonlistIssuerDetailViewModel(Convert.ToInt32(comCode), _repository);
            return PartialView("NonlistIssuerDetailMain", viewModel);
        }

        /// <summary>
        /// Issuer rating partial page
        /// </summary>
        /// <param name="id">com code</param>
        /// <returns></returns>
        [Localization]
        public ActionResult NonlistIssuerRating(string id)
        {
            var model = _repository.GetIssuerRating(id);

            if (model.Count > 0)
            {
                var idlist =
                    model.Select(r => r.RATE_ID.ToString()).ToList().Aggregate((a, b) => a + "," + b).ToString();
                var dataTable = CMARepository.CheckCommonFileExsit(idlist, "RATE_REP_DATA", "RATE_ID");

                foreach (DataRow row in dataTable.Rows)
                    model.Where(r => r.RATE_ID == Convert.ToInt64(row["RATE_ID"]))
                        .ToList()
                        .ForEach(r => r.ContainFile = true);
            }
            return PartialView(model);
        }

        [Localization]
        public ActionResult GetNonlistIssuer(long parCode, string name, string bond, bool hideNodata, string order, int currentPage, int pageSize, bool isHTML = false)
        {
            int total;
            var issuers = _repository.GetNonlistIssuer(parCode, name ?? string.Empty, bond ?? string.Empty, hideNodata, order, currentPage, pageSize, out total);
            var result = new
            {
                Data = issuers,
                Total = total,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
            return isHTML ? Json(result, "text/html", JsonRequestBehavior.AllowGet) : Json(result, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForNonlistIssuer(long parCode, string name, string bond, bool hideNodata, string reportName)
        {
            var data = _repository.GetAllNonlistIssuer(parCode, name, bond, hideNodata);

            return new ExcelResult(data.AsQueryable(), GetNonlistIssuerHeader(), GetNonlistIssuerColumns(), workSheetName: reportName);
        }

        private string[] GetNonlistIssuerHeader()
        {
            string[] header = null;

            header = new string[] {
                     Resources.Global.UnlistIsser_Company_Name,
                     Resources.Global.UnlistIsser_Abbreviation,
                     Resources.Global.UnlistIsser_Com_Type,
                     Resources.Global.UnlistIsser_COM_CON_PER,
                     Resources.Global.UnlistIsser_COM_TEL,
                     Resources.Global.UnlistIsser_COM_FAX,
                     Resources.Global.UnlistIsser_OFFI_ADDR,
                     Resources.Global.UnlistIsser_OFFI_ADDR_POST,
                     Resources.Global.UnlistIsser_REG_ADDR,
                     Resources.Global.UnlistIsser_Company_Website,
                     Resources.Global.UnlistIsser_MAIL_ADDR,
            };

            return header;
        }

        private string[] GetNonlistIssuerColumns()
        {
            string[] cloumns = null;

            cloumns = new string[] {
                        "COM_NAME",
                        "COM_SHORT_NAME",
                        "TYPE_BIG",
                        "COM_CON_PER",
                        "COM_TEL",
                        "COM_FAX",
                        "OFFI_ADDR",
                        "OFFI_ADDR_POST",
                        "REG_ADDR",
                        "COM_WEB",
                        "MAIL_ADDR",
            };

            return cloumns;
        }


        [Localization]
        public ActionResult NonlistIssuerDetailTable(string id, string page)
        {
            ViewBag.ID = id;
            ViewBag.Page = page;
            return PartialView();
        }

        /// <summary>
        /// Gets the non list table data.
        /// </summary>
        /// <param name="period">The period.Y,Q</param>
        /// <param name="viewBy">The view by.O,C,T,H</param>
        /// <param name="years">The years.1,2,3,4,5,all</param>
        /// <param name="unit">The unit.100M,M,10K,K</param>
        /// <param name="id">The id.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        [Localization]
        public JsonResult GetNonListTableData(string period, string viewBy, int years, string unit, int id, string page)
        {
            var jTable = BuildDetailJsonTable(period, viewBy, years, unit, id, page);
            return Json(jTable, JsonRequestBehavior.AllowGet);
        }

        private JsonTable BuildDetailJsonTable(string period, string viewBy, int years, string unit, int id, string page)
        {
            var issuerFundamental = _repository.GetFoundamentalData(page, id, period, viewBy, years, unit);
            if (issuerFundamental == null || issuerFundamental.Count() == 0)
                return new JsonTable();

            var startYear = issuerFundamental.Min(f => f.EndDate).Year;
            var endYear = issuerFundamental.Max(f => f.EndDate).Year;
            var jTable = new JsonTable();
            jTable.RowData = ConvertToRowData(issuerFundamental, page, period);
            if (period == "Y")
            {
                jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Fundation_PeriodEndDate, ColumnName = "Item" });
                for (int i = endYear; i >= startYear; i--)
                {
                    var endDates = issuerFundamental.Where(f => f.EndDate.Year == i).Select(f => f.EndDate).FirstOrDefault();
                    if (endDates == null)
                        continue;
                    jTable.ColumTemplate.Add(new JsonColumn { Name = endDates.ToString("yyyy-MM-dd"), ColumnName = "Y" + i.ToString() });
                }
            }
            else
            {
                jTable.ExtraHeaders = new List<JsonExtraColumn> { new JsonExtraColumn { Name = string.Empty, ColSpan = 1 } };
                jTable.ColumTemplate.Add(new JsonColumn { Name = Resources.Global.Fundation_PeriodEndDate, ColumnName = "Item" });
                for (var i = endYear; i >= startYear; i--)
                {
                    var endDates = issuerFundamental.Where(f => f.EndDate.Year == i).OrderByDescending(f => f.EndDate).Select(f => f.EndDate).ToList();
                    if (endDates == null)
                        continue;

                    jTable.ExtraHeaders.Add(new JsonExtraColumn { Name = i.ToString(), ColSpan = endDates.Count() });
                    for (var j = 0; j < endDates.Count(); j++)
                    {
                        jTable.ColumTemplate.Add(new JsonColumn
                                                     {
                                                         Name = endDates.ElementAt(j).ToString("yyyy-MM-dd"),
                                                         ColumnName =
                                                             "Y" + i + "Q" +
                                                             (Convert.ToInt32(endDates.ElementAt(j).Month) / 3)
                                                     });
                    }
                }
            }
            return jTable;
        }

        [Localization]
        public ActionResult GetDetailTableExcel(string period, string viewBy, int years, string unit, int id, string page, string reportName)
        {
            var jTable = BuildDetailJsonTable(period, viewBy, years, unit, id, page);
            var jP = new JsonExcelParameter { Table = jTable, TableName = reportName, Source = Resources.Global.Source };
            var unitText = unit == "P" ? ExcelUtil.PercentageUnit : string.Format(ExcelUtil.Unit, CultureHelper.IsEnglishCulture() ? unit : ExcelUtil.GetUnitChineseString(unit));
            jP.SubTitle.Add(new Tuple<int, string>(0, unitText));
            return new JsonTableExcelResult(jP);
        }


        private List<Dictionary<string, string>> ConvertToRowData(IEnumerable<IssuerFundamental> issuerFundamental, string fundamentalType, string reportType)
        {
            //PID = parent id, IP = is parent id, 0 or 1,IL= indent level , 0, 1, 2, CR= chart row
            List<Dictionary<string, string>> rowData = new List<Dictionary<string, string>>();
            var fieldMapping = _repository.GetFundamentalFiledMapping(fundamentalType);

            PropertyInfo[] properties = typeof(IssuerFundamental).GetProperties();
            var startYear = issuerFundamental.Min(f => f.EndDate).Year;
            string chartRow = "";

            if (fundamentalType == "tabb")
                chartRow = "Field60";
            else if (fundamentalType == "tacb")
                chartRow = "Field62";
            else if (fundamentalType == "tapb")
                chartRow = "Field50";

            foreach (PropertyInfo p in properties)
            {
                //fields that needn't displaying
                var fieldName = CultureHelper.IsEnglishCulture() ?
                                fieldMapping.Where(f => f.FIELD_NAME == p.Name && f.FUNDAMENATAL_TYPE == fundamentalType).Select(f => f.ENGLISH_NAME).FirstOrDefault()
                                : fieldMapping.Where(f => f.FIELD_NAME == p.Name && f.FUNDAMENATAL_TYPE == fundamentalType).Select(f => f.CHINESE_NAME).FirstOrDefault();

                if (string.IsNullOrEmpty(fieldName))
                    continue;

                var indentLevel = fieldMapping.Where(f => f.FIELD_NAME == p.Name && f.FUNDAMENATAL_TYPE == fundamentalType).Select(f => f.INDENT_LEVEL).FirstOrDefault() ?? 0;
                var is_Parent = fieldMapping.Where(f => f.FIELD_NAME == p.Name && f.FUNDAMENATAL_TYPE == fundamentalType).Select(f => f.IS_PARENT).FirstOrDefault().ToString();
                var pId = fieldMapping.Where(f => f.FIELD_NAME == p.Name && f.FUNDAMENATAL_TYPE == fundamentalType).Select(f => f.PARENT_ID).FirstOrDefault().ToString();

                Dictionary<string, string> row = new Dictionary<string, string>();
                row.Add("Item", SetIndent(indentLevel) + fieldName); //add the field name, the first left name column
                row.Add("PID", pId);
                row.Add("IP", is_Parent);
                row.Add("CR", p.Name == chartRow ? (fundamentalType == "tapb" ? fieldName.Substring(2, 3) : fieldName) : "0");

                if (reportType == "Y") //year report
                {
                    for (var i = DateTime.Now.Year - 1; i >= startYear; i--)
                    {
                        var fund = issuerFundamental.Where(f => f.EndDate.Year == i).FirstOrDefault();
                        var value = fund == null ? null : p.GetValue(fund, null);
                        row.Add("Y" + i.ToString(), value == null ? "" : UIGenerator.FormatCellValue(value, "decimal"));
                    }
                    rowData.Add(row);
                }
                else
                {
                    for (var i = DateTime.Now.Year; i >= startYear; i--)
                    {
                        var funds = issuerFundamental.Where(f => f.EndDate.Year == i).OrderByDescending(f => f.EndDate).ToList(); //add in a year

                        for (var j = 0; j < funds.Count(); j++)
                        {
                            int quarterIndex = 0;
                            int quarter = Convert.ToInt32(funds.ElementAt(j).EndDate.Month) / 3;

                            switch (quarter)
                            {
                                case 1:
                                    quarterIndex = 3;
                                    break;
                                case 2:
                                    quarterIndex = 2;
                                    break;
                                case 3:
                                    quarterIndex = 6;
                                    break;
                                case 4:
                                    quarterIndex = 1;
                                    break;
                                default:
                                    break;
                            }

                            var fund = funds.Where(f => f.ReportType == quarterIndex).Select(f => f).FirstOrDefault();
                            var value = fund == null ? null : p.GetValue(fund, null);

                            row.Add("Y" + i.ToString() + "Q" + quarter.ToString(), value == null ? "" : UIGenerator.FormatCellValue(value, "decimal"));
                        }
                    }
                    rowData.Add(row);
                }
            }

            return rowData;
        }


        private string SetIndent(Decimal indentLevel)
        {
            string indent = "";

            for (int i = 0; i <= indentLevel * 6; i++)
                indent += "&nbsp;";

            return indent;
        }


        /// <summary>
        /// Download rating file
        /// </summary>
        /// <param name="id">Rate id</param>
        /// <returns>File</returns>
        public ActionResult DownloadRatingFile(long id)
        {
            var file = _repository.GetRatingFileData(id);
            if (file == null)
                return HttpNotFound();
            if (file.Content == null)
                return HttpNotFound();
            var contentType = MimeHelper.GetMimeTypeByExt(file.FileType);
            return File(file.Content, contentType,
                Resources.Global.DownloadFileName + "." + file.FileType);
        }
    }
}
