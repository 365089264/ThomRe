using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Entities;
using VAV.Model.Data;
using VAV.Web.Common;
using VAV.Web.Localization;
using VAV.DAL.Report;

namespace VAV.Web.Controllers
{
    public class PartnersController : BaseController
    {
        [Dependency]
        public PartnersReportRepository partnersReportRepository { get; set; }

        [Localization]
        public JsonResult GetChinaSecuritiesData(string category, string title, string code, DateTime startDate, DateTime endDate, int startPage, int pageSize)
        {
            var data = GetChinaSecuritiesTable(category, title, code, startDate, endDate, startPage, pageSize);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        private JsonTable GetChinaSecuritiesTable(string category, string title, string code, DateTime startDate, DateTime endDate, int startPage, int pageSize)
        {
            int total;
            var dataTable = partnersReportRepository.GetChinaSecurities(category, title, code, startDate, endDate,out total, startPage, pageSize);
            return BuidJsonTable(dataTable, GetColTemplateByCategory(category), dataTable.Rows.Count == 0 ? 0 : total, startPage, pageSize);
        }
        private new JsonTable BuidJsonTable(DataTable table, IEnumerable<REPORTCOLUMNDEFINITION> colTemplate, int total, int currentPage, int pageSize)
        {
            var jTable = new JsonTable();
            jTable.CurrentPage = currentPage;
            jTable.PageSize = pageSize;
            jTable.Total = total;
            foreach (var column in colTemplate)
            {
                jTable.ColumTemplate.Add(new JsonColumn { ColumnName = column.COLUMN_NAME, ColumnType = column.COLUMN_TYPE, Name = column.DisplayName });
            }
            var columns = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columns[i] = table.Columns[i].ColumnName;
            }
            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                foreach (var column in columns)
                {
                    if (column == "PUBLISHED") currentRow.Add(column, Convert.ToDateTime(row[column]).ToString("yyyy-MM-dd HH:mm:ss"));
                    else currentRow.Add(column, row[column].ToString());
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }

        private IEnumerable<REPORTCOLUMNDEFINITION> GetColTemplateByCategory(string category)
        {
            var columns = new List<REPORTCOLUMNDEFINITION>();
            columns.Add(new REPORTCOLUMNDEFINITION { COLUMN_NAME = "TITLE", COLUMN_TYPE = "text" });
            if (category == "notice") columns.Add(new REPORTCOLUMNDEFINITION { COLUMN_NAME = "ORGCODE", COLUMN_TYPE = "text" });
            columns.Add(new REPORTCOLUMNDEFINITION { COLUMN_NAME = "PUBLISHED", COLUMN_TYPE = "datetime" });
            return columns;
        }
    }
}
