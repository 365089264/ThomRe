using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    public class JsonTable
    {
        public JsonTable()
        {
            ColumTemplate = new List<JsonColumn>();
            RowData = new List<Dictionary<string, string>>();
        }
        public List<JsonColumn> ColumTemplate { get; private set; }
        public List<JsonExtraColumn> ExtraHeaders { get; set; }
        public List<Dictionary<string, string>> RowData { get; set; }
        public int Total { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string ReportDate { get; set; }
    }
}
