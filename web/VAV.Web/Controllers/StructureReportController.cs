using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Resources;
using VAV.DAL.Services;
using VAV.Model.Data;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.Web.Localization;

namespace VAV.Web.Controllers
{
    /// <summary>
    /// Structure report for cfets
    /// </summary>
    public class StructureReportController : Controller
    {
        /// <summary>
        /// Report Generator
        /// </summary>
        [Dependency]
        public ReportService ReportService { get; set; }

        [Dependency]
        public MenuService MenuService { get; set; }


        [Localization]
        public ActionResult GetReport(int id)
        {
            var reportInfo = ReportService.GetReportInfoById(id);
            

            ViewData["Name"] = reportInfo.DisplayName;
            ViewData["ID"] = id;
            
            return View(reportInfo.ViewName);
        }

        [Localization]
        public ActionResult GetStructuredReport(int reportId, string startDate, string endDate, string unit)
        {
            var reportInfo = ReportService.GetReportInfoById(reportId);
            var reportData = ReportService.GetStructureReportById(reportId,
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

        [Localization]
        public ActionResult ExportReport(int reportId, string startDate, string endDate,
            string unit, string rowName)
        {
            var reportInfo = ReportService.GetReportInfoById(reportId);
            //System.Diagnostics.Trace.Write(string.Format("isIncludeParamString: {0}", isIncludeParamString));
            var isInEnglish = CultureHelper.IsEnglishCulture();
            var reportParam = new ReportParameter
            {
                StartDate = string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate),
                EndDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate),
                //Unit = string.IsNullOrEmpty(unit) ? "100M" : unit,
                RowName = string.IsNullOrEmpty(rowName) ? string.Empty : rowName.Trim(),
                TableName = reportInfo.TableName,
                ColumnList = string.IsNullOrEmpty(endDate) ? null : reportInfo.ColumnList
            };
            var standardReport = (StandardReport)ReportService.GetStructureReportById(reportId, reportParam);

            var isDetailedReport = !string.IsNullOrEmpty(rowName);
            
            var columnsToExport = standardReport.Columns;

            var headers = isInEnglish
                ? columnsToExport.Select(c => c.ColumnHeaderEN).ToArray()
                : columnsToExport.Select(c => c.ColumnHeaderCN).ToArray();
            //var rowKeys = columnsToExport.Select(c => c.ColumnName).ToArray();
            var rowKeys = new List<string>();
            foreach (var column in columnsToExport)
            {
                if (isInEnglish)
                {
                    if (standardReport.ResultDataTable.Columns.Contains(column.ColumnName + "EN"))
                    {
                        rowKeys.Add(column.ColumnName + "EN");
                    }
                    else
                    {
                        rowKeys.Add(column.ColumnName);
                    }
                }
                else
                {
                    rowKeys.Add(column.ColumnName);
                }
            }

            var rows = standardReport.ResultDataTable.AsEnumerable().AsQueryable();
            var extraHeaderCollection = standardReport.ExtraHeaderCollection;

            var tempReportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;
            var reportName = string.Empty;


                reportName = tempReportName;
            
            reportName = reportName.Replace('/', ' ');
            var dateFormat = "yyyy-MM";
            try
            {
                return new ExcelResult(Global.SourceCfets, rows, headers, rowKeys.ToArray(), reportName, reportName, false, extraHeaderCollection, reportParam, !isDetailedReport, dateFormat);
                //return new ExcelResult(rows, headers, rowKeys.ToArray(), reportName, reportName,false,extraHeaderCollection);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonTable buildJsonTable(BaseReport report)
        {
            var table = new JsonTable();
            switch (report.GetType().Name)
            {
                case "StandardReport":
                    var data = (StandardReport)report;
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
                            {
                                if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
                                {
                                    currentRow.Add(column.ColumnName.ToLower(), UIGenerator.FormatCellValue(row, column));
                                }
                                else
                                {
                                    currentRow.Add(column.ColumnName.ToLower(),
                                        row.Table.Columns.Contains(column.ColumnName + "EN")
                                            ? UIGenerator.FormatCellValue(row[column.ColumnName + "EN"],
                                                column.ColumnType, column.ColumnFormat)
                                            : UIGenerator.FormatCellValue(row, column));
                                }
                            }
                        }
                        if (row.Table.Columns.Contains("id"))
                        {
                            currentRow.Add("id", row["id"].ToString());
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

    }
}
