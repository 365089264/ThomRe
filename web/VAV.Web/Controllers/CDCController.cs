using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Model.Data;
using VAV.Model.Data.Bond;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.Web.Localization;
using VAV.Web.ViewModels.Report;
using Global = Resources.Global;

namespace VAV.Web.Controllers
{
    /// <summary>
    ///     Report Controller
    /// </summary>
    public class CDCController : Controller
    {
        /// <summary>
        ///     Report Generator
        /// </summary>
        [Dependency]
        public ReportService ReportService { get; set; }

        /// <summary>
        ///     _menuService
        /// </summary>
        [Dependency]
        public MenuService MenuService { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CDCController" /> class.
        /// </summary>
        [Localization]
        public ActionResult GetReport(int id)
        {
            var reportInfo = ReportService.GetReportInfoById(id);
            BaseReportViewModel viewModel = null;

            var viewModelType = reportInfo == null ? null : Type.GetType(reportInfo.ViewModelName);
            
            if (viewModelType != null && viewModelType == typeof (StatisticalReportViewModel)){
                var reportData = ReportService.GetReportById(reportInfo.ReportId, new ReportParameter { TableName = reportInfo.TableName, ColumnList = reportInfo.ColumnList ?? "a.*" });
                viewModel = new StatisticalReportViewModel(reportInfo.ReportId, (StandardReport) reportData,
                    (StandardReport)
                        ReportService.GetReportById(reportInfo.ReportId, new ReportParameter { RowName = "1", TableName = reportInfo.TableName, ColumnList = reportInfo.ColumnList }));
            }
            else {
                viewModel = new BasicReportViewModel(reportInfo.ReportId, new StandardReport(id));
            }

            viewModel.Name = reportInfo.DisplayName;
            viewModel.Initialization();

            if (viewModel is StatisticalReportViewModel)
            {
                ViewData["IsStatisticReport_" + reportInfo.ReportId] = false;
                var themeName = ThemeHelper.GetTheme(Request);
                ((StatisticalReportViewModel) viewModel).TopChartModel.Theme = themeName;
                ((StatisticalReportViewModel) viewModel).BottomChartModel.Theme = themeName;
            }
            return View(reportInfo == null ? "BasicReport" : reportInfo.ViewName, viewModel);
        }

        [HttpPost]
        [Localization]
        public ActionResult GetReportTable(string reportType, int reportId, string startDate, string endDate,
            string unit)
        {
            var reportInfo = ReportService.GetReportInfoById(reportId);
            var reportData = ReportService.GetReportById(reportId,
                new ReportParameter
                {
                    StartDate =
                        string.IsNullOrEmpty(startDate) ? (DateTime?) null : DateTime.Parse(startDate),
                    EndDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate),
                    Unit = unit,
                    TableName = reportInfo.TableName,
                    ColumnList = string.IsNullOrEmpty(endDate)?null:reportInfo.ColumnList
                });

            var viewModel = reportType == "basic"
                ? new BasicReportViewModel(reportId, (StandardReport) reportData)
                : new StatisticalReportViewModel(
                    reportId,
                    (StandardReport) reportData,
                    (StandardReport) ReportService.GetReportById(
                        reportId,
                        new ReportParameter
                        {
                            StartDate =
                                string.IsNullOrEmpty(startDate)
                                    ? (DateTime?)null 
                                    : DateTime.Parse(startDate),
                            EndDate =
                                string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate),
                            Unit = unit,
                            RowName = "1",
                            TableName = reportInfo.TableName,
                            ColumnList = reportInfo.ColumnList
                        }));
            
            viewModel.Name = reportInfo.DisplayName;
            viewModel.Unit = HtmlUtil.GetUnitOptionByKey(unit);
            viewModel.Initialization();
            ViewData["StartDate_" + reportId] = startDate;
            ViewData["EndDate_" + reportId] = endDate;
            ViewData["Unit_" + reportId] = unit;

            if (reportType == "basic")
            {
                return View("_ReportTable", viewModel.StandardReport);
            }
            var themeName = ThemeHelper.GetTheme(Request);
            var staticalViewModel = (StatisticalReportViewModel) viewModel;
            staticalViewModel.TopChartModel.Theme = themeName;
            staticalViewModel.BottomChartModel.Theme = themeName;
            return View("_StatisticalReportTable", staticalViewModel);
        }

        [Localization]
        public ActionResult GetStructuredReport(int reportId, string startDate, string endDate, string unit)
        {
            var reportInfo = ReportService.GetReportInfoById(reportId);
            var reportData = ReportService.GetReportById(reportId,
                new ReportParameter
                {
                    StartDate =
                        string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate),
                    EndDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate),
                    Unit = unit,
                    TableName = reportInfo.TableName,
                    ColumnList = string.IsNullOrEmpty(endDate) ? null : reportInfo.ColumnList
                });
            var jsonTable = buildJsonTable(reportData);
            return Json(jsonTable, JsonRequestBehavior.AllowGet);
        }

        public JsonTable buildJsonTable(BaseReport report)
        {
            var table = new JsonTable();
            switch (report.GetType().Name)
            {
                case "StandardReport":
                    var data = (StandardReport) report;
                    if (data.ResultDataTable.Rows.Count > 0) {
                        table.ReportDate = ((DateTime)data.ResultDataTable.Rows[0]["REDATE"]).ToString("yyyy-MM");
                    }

                    foreach (var column in data.Columns)
                    {
                        table.ColumTemplate.Add(new JsonColumn()
                        {
                            ColumnName = column.ColumnName.ToLower(),
                            ColumnStyle = column.ColumnStyle,
                            ColumnType = column.ColumnType,
                            Name = column.Culture == "zh-CN" ? column.ColumnHeaderCN : column.ColumnHeaderEN
                        });
                    }

                    table.ExtraHeaders = new List<JsonExtraColumn>();
                    foreach (var extraHeader in data.ExtraHeaderCollection)
                    {
                        table.ExtraHeaders.Add(new JsonExtraColumn()
                        {
                            Name = extraHeader.Culture == "zh-CN" ? extraHeader.HeaderTextCN : extraHeader.HeaderTextEN,
                            ColumnStyle = extraHeader.HeaderStyle,
                            ColSpan = extraHeader.HeaderColumnSpan,
                            HeaderLevel = extraHeader.HeaderLevel
                        });
                    }

                    table.RowData = new List<Dictionary<string, string>>();

                    foreach (DataRow row in data.ResultDataTable.Rows)
                    {
                        var currentRow = new Dictionary<string, string>();

                        foreach (var column in data.Columns)
                        {
                            if (!currentRow.Keys.Contains(column.ColumnName))
                                currentRow.Add(column.ColumnName.ToLower(), UIGenerator.FormatCellValue(row, column));
                        }
                        if (row.Table.Columns.Contains("id"))
                        {
                            currentRow.Add("id",row["id"].ToString());
                        }
                        if (row.Table.Columns.Contains("last_update"))
                        {
                            currentRow.Add("last_update", row["last_update"].ToString());
                        }
                        if (row.Table.Columns.Contains("row_index"))
                        {
                            currentRow.Add("row_index", row["row_index"].ToString());
                        }
                        if (row.Table.Columns.Contains("chart_source"))
                        {
                            currentRow.Add("chart_source", row["chart_source"].ToString());
                        }
                        if (row.Table.Columns.Contains("row_level"))
                        {
                            currentRow.Add("row_level", row["row_level"].ToString());
                        }

                        table.RowData.Add(currentRow);
                    }

                    break;
            }
            return table;
        }



        [HttpPost]
        [Localization]
        public ActionResult GetDetailedReportByRowName(int reportId, string startDate, string endDate, string unit,
            string rowName)
        {
            if (string.IsNullOrEmpty(rowName))
            {
                return new EmptyResult();
            }
            var rowId = 0;
            if (!int.TryParse(rowName, out rowId))
            {
                return new EmptyResult();
            }
            var reportInfo = ReportService.GetReportInfoById(reportId);
            var reportData = ReportService.GetReportById(reportId,
                new ReportParameter
                {
                    StartDate =
                        string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate),
                    EndDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate),
                    Unit = unit,
                    RowName = rowName.Trim(),
                    TableName = reportInfo.TableName,
                    ColumnList = string.IsNullOrEmpty(endDate) ? null : reportInfo.ColumnList
                });
            var viewModel = new StatisticalReportViewModel(reportId, (StandardReport) reportData);
            
            viewModel.Name = reportInfo.DisplayName;
            viewModel.Unit = GetUnit(unit);
            viewModel.Initialization();
            var themeName = ThemeHelper.GetTheme(Request);
            viewModel.TopChartModel.Theme = themeName;
            viewModel.BottomChartModel.Theme = themeName;

            ViewData["StartDate_" + reportId] = startDate;
            ViewData["EndDate_" + reportId] = endDate;
            ViewData["Unit_" + reportId] = unit;
            ViewData["RowName_" + reportId] = rowName.Trim();

            return View("_StatisticalReportDetailTable", viewModel);
        }

        private static string GetUnit(string unit)
        {
            string result;
            switch (unit)
            {
                case "100M":
                    result = Global.Unit_Option_100M;
                    break;
                case "10K":
                    result = Global.Unit_Option_10K;
                    break;
                case "K":
                    result = Global.Unit_Option_K;
                    break;
                case "M":
                    result = Global.Unit_Option_M;
                    break;
                default:
                    result = Global.Unit_Option_100M;
                    break;
            }
            return result;
        }

        [Localization]
        public ActionResult RefreshChart(string model, string selected, string chartType, bool isLarge,
            bool isResizeable)
        {
            var serializer = new JavaScriptSerializer();

            if (model.Contains("&quot"))
                model = HttpUtility.HtmlDecode(model);
            var cols = serializer.Deserialize(model, typeof (ChartViewModel)) as ChartViewModel;

            if (string.IsNullOrEmpty(selected))
                selected = cols.StatisticalAspects.FirstOrDefault().Key;

            var values =
                cols.ColumnValues[cols.StatisticalAspects[selected]].ToArray().Select(v => v ?? 0);

            var categories = cols.DataCategories;
            var pairValues = new List<object>();
            //var serie = new List<Series>();
            for (var i = 0; i < values.Count(); i++)
            {
                //skip i which no need to render in chart 
                pairValues.Add(new[] {categories[i], values.ToList()[i]});
                //serie.Add(new Series { Data = new Data(values.ToArray()), Name = selected });
            }

            var series = GetChartType(chartType) == ChartTypes.Pie
                ? new Series {Data = new Data(pairValues.ToArray()), Name = selected, Type = ChartTypes.Pie}
                : new Series {Data = new Data(pairValues.ToArray()), Name = selected};

            var yText = selected == Global.Number || selected.Contains("数") || selected == "Issues" ||
                        selected == "Maturity Bonds"
                ? selected
                : cols.Unit != null
                    ? Global.Unit + "(" + cols.Unit + ")"
                    : Global.Unit + "(" + Global.Unit_Option_100M + ")";

            var themeName = ThemeHelper.GetTheme(Request);

            var chart = new ChartModel(Global.Source)
            {
                ChartName = "chart" + cols.ReportId + (cols.IsTop ? "top" : "bottom"),
                Series = new[] {series},
                Title = cols.Title,
                SubTitle = cols.ChartName,
                XAxisCategory = categories.Cast<string>(),
                ChartType = GetChartType(chartType),
                IsXAxisDate = !cols.IsTop,
                YAxisText = yText,
                Theme = themeName,
                IsResizeable = isResizeable,
                ReportID = cols.ReportId,
                NoUnit = yText.Substring(0, 2) != Global.Unit
            };
            return new ChartContentResult(isLarge, chart);
        }

        [Localization]
        public ActionResult ExportReport(bool isIncludeParam, int reportId, string startDate, string endDate,
            string unit, string rowName)
        {
            var reportInfo = ReportService.GetReportInfoById(reportId);
            //System.Diagnostics.Trace.Write(string.Format("isIncludeParamString: {0}", isIncludeParamString));
            var isInEnglish = CultureHelper.IsEnglishCulture();
            var reportParam = new ReportParameter
            {
                StartDate = string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate),
                EndDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate),
                Unit = string.IsNullOrEmpty(unit) ? "100M" : unit,
                RowName = string.IsNullOrEmpty(rowName) ? string.Empty : rowName.Trim(),
                TableName = reportInfo.TableName,
                ColumnList = string.IsNullOrEmpty(endDate) ? null : reportInfo.ColumnList
            };
            var standardReport = (StandardReport) ReportService.GetReportById(reportId, reportParam);

            var isDetailedReport = !string.IsNullOrEmpty(rowName);
            if (isDetailedReport)
            {
                standardReport.Columns.Insert(0, new Column
                {
                    ColumnFormat = "{0:yyyy-MM}",
                    ColumnName = "REDATE",
                    ColumnHeaderCN = "月份",
                    ColumnHeaderEN = "Month",
                    IsDetailedColumn = true
                });
            }
            var columnsToExport = isDetailedReport
                ? standardReport.Columns.Where(c => c.IsDetailedColumn).ToList()
                : standardReport.Columns;

            var headers = isInEnglish
                ? columnsToExport.Select(c => c.ColumnHeaderEN).ToArray()
                : columnsToExport.Select(c => c.ColumnHeaderCN).ToArray();
            var rowKeys = columnsToExport.Select(c => c.ColumnName).ToArray();

            var rows = standardReport.ResultDataTable.AsEnumerable().AsQueryable();
            var extraHeaderCollection = standardReport.ExtraHeaderCollection;

            var tempReportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;
            var reportName = string.Empty;

            if (isDetailedReport)
            {
                if (isInEnglish)
                    reportName = "Detailed " + tempReportName;
                else
                    reportName = tempReportName + "明细";
            }
            else
            {
                reportName = tempReportName;
            }

            var dateFormat = "yyyy-MM";
            try
            {
                return new ExcelResult(Global.SourceCCDC, rows, headers, rowKeys, reportName, reportName, false,
                    extraHeaderCollection, reportParam, !isDetailedReport, dateFormat);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Localization]
        public ActionResult ExportBondIssueRateReport(int reportId, string columnNames, string fieldNames, string grid,
            string selectedGrid)
        {
            var serializer = new JavaScriptSerializer();

            if (columnNames.Contains("&quot"))
                columnNames = HttpUtility.HtmlDecode(columnNames);
            var colNames = serializer.Deserialize(columnNames, typeof (List<string>)) as List<string>;

            if (fieldNames.Contains("&quot"))
                fieldNames = HttpUtility.HtmlDecode(fieldNames);
            var fldNames = serializer.Deserialize(fieldNames, typeof (List<string>)) as List<string>;

            if (grid.Contains("&quot"))
                grid = HttpUtility.HtmlDecode(grid);
            var gd = serializer.Deserialize(grid, typeof (List<BondIssueRate>)) as List<BondIssueRate>;

            var selected = (from g in gd
                where g.ItemName == selectedGrid
                select g).ToList();

            var reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;

            try
            {
                return new ExcelResult(selected.AsQueryable(), colNames.ToArray(), fldNames.ToArray(), reportName,
                    reportName);
            }
            catch (Exception)
            {
            }

            return new EmptyResult();
        }

        #region Private Properties & Methods

        private ChartTypes GetChartType(string type)
        {
            var _chartTypes =
                new Dictionary<string, ChartTypes>
                {
                    {Global.ChatType_Line, ChartTypes.Line},
                    {Global.ChatType_Column, ChartTypes.Column},
                    {Global.ChatType_Bar, ChartTypes.Bar},
                    {Global.ChatType_Pie, ChartTypes.Pie}
                };

            if (_chartTypes.ContainsKey(type))
            {
                return _chartTypes[type];
            }
            return ChartTypes.Column;
        }

        #endregion
    }
}