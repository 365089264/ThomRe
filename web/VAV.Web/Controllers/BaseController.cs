using System;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Web.Common;
using VAV.Web.Localization;
using VAV.Web.ViewModels.Report;
using System.Collections.Generic;
using VAV.Model.Data;
using System.Data;
using VAV.Entities;
using System.Linq;

namespace VAV.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        [Dependency]
        public ReportService ReportService { get; set; }

        [Localization]
        public virtual ActionResult GetReport(int id)
        {
            var reportInfo = ReportService.GetReportInfoById(id);
            BaseReportViewModel viewModel = null;
            var viewModelType = System.Type.GetType(reportInfo.ViewModelName);
            if (viewModelType != null)
            {
                viewModel = (BaseReportViewModel)Activator.CreateInstance(viewModelType, new object[] { reportInfo.ReportId,UserSettingHelper.GetUserId(Request) });
                viewModel.ID = reportInfo.ReportId;
                viewModel.Name = reportInfo.DisplayName;
                viewModel.Initialization(Request);
            }
            else
            {
                ViewBag.ID = reportInfo.ReportId;
                ViewBag.Name = reportInfo.DisplayName;
            }
            return PartialView(reportInfo.ViewName, viewModel);
        }
        public JsonTable BuidJsonTable(DataTable table, IEnumerable<REPORTCOLUMNDEFINITION> columns, int total = 0, int currentPage = 0, int pageSize = 0)
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
                if (!columns.Any(x => x.COLUMN_NAME == "Code"))
                {
                    currentRow.Add("Code", row["Code"].ToString());
                }
                if (!columns.Any(x => x.COLUMN_NAME == "AssetId"))
                {
                    currentRow.Add("AssetId", row["AssetId"].ToString());
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }
        public List<DateTime> PopulateRateTimes(DateTime start, DateTime end, string rate)
        {
            var result = new List<DateTime>();
            if (rate.Equals("m"))
            {
                result.Add(start);
                start = start.AddDays(1 - start.Day);
                while (start.AddMonths(1) <= end)
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
                start = start.AddDays(1 - start.Day).AddMonths(0 - (start.Month - 1) % 3);
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
    }
}
