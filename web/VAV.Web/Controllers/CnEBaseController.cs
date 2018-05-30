using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Web.Common;
using VAV.Web.Localization;
using VAV.Model.Data;
using System.Data;
using VAV.Entities;
using VAV.DAL.CnE;

namespace VAV.Web.Controllers
{
    public abstract class CnEBaseController : BaseController
    {
        [Dependency]
        public CNERespository Respository { get; set; }
        [Dependency]
        public NewCNERespository NewRespository { get; set; }
        [Dependency]
        public MenuService MenuService { get; set; }
        [Localization]
        public override ActionResult GetReport(int id)
        {
            var reportInfo = ReportService.GetReportInfoById(id);
            ViewBag.ID = reportInfo.ReportId;
            ViewBag.Name = reportInfo.DisplayName;
            return PartialView(reportInfo.ViewName);
        }

        public JsonTable BuildJsonTable(DataTable table, IEnumerable<REPORTCOLUMNDEFINITION> columns, int total, int currentPage, int pageSize)
        {
            var jTable = new JsonTable();
            jTable.CurrentPage = currentPage;
            jTable.PageSize = pageSize;
            jTable.Total = total;
            foreach (var column in columns)
            {
                if (column.COLUMN_NAME == "KeyWord") continue;
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = column.COLUMN_NAME, ColumnType = column.COLUMN_TYPE, Name = column.DisplayName });
            }
            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                foreach (var column in columns)
                {

                    if (!currentRow.Keys.Contains(column.COLUMN_NAME))
                        currentRow.Add(column.COLUMN_NAME, UIGenerator.FormatCellValue(row, column));
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        public JsonTable BuidJsonTable(DataTable table, IEnumerable<COLUMNDEFINITION> columns, int total, int currentPage, int pageSize)
        {
            var jTable = new JsonTable();
            jTable.CurrentPage = currentPage;
            jTable.PageSize = pageSize;
            jTable.Total = total;
            foreach (var column in columns)
            {
                if (column.ColumnDisplayName == "KeyWord") continue;
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = column.ColumnDisplayName, ColumnType = column.COLUMN_TYPE, Name = column.HearderDisplayName });
            }
            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                foreach (var column in columns)
                {

                    if (!currentRow.Keys.Contains(column.ColumnDisplayName))
                        currentRow.Add(column.ColumnDisplayName, GDTFormatCellValue(row, column));
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        public string GDTFormatCellValue(DataRow row, COLUMNDEFINITION column)
        {
            var retValue = string.Empty;
            if (row.Table.Columns.Contains(column.ColumnDisplayName))
            {
                var dataValue = row[column.ColumnDisplayName];

                retValue = UIGenerator.FormatCellValue(dataValue, column.COLUMN_TYPE, column.DISPLAY_FORMAT);
            }
            return retValue;
        }
    }
}
