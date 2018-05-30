using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNet.Highcharts.Enums;

namespace VAV.Web.ViewModels.Report
{
    public class ChartViewModel
    {
        public bool IsTop { get; set; }
        public int ReportId { get; set; }
        public string ChartName { get; set; }
        public string ColumnName { get; set; }
        public ChartTypes ChartType { get; set; }
        public Dictionary<string, string> StatisticalAspects { get; set; } // column names: k->v => cn->en
        public IDictionary<string, List<object>> ColumnValues { get; set; } // data table in a colum->row structure
        public IList<object> DataCategories { get; set; }
        public IList<string> ChartTypeOptions { get; set; }
        public string Title { get; set; }
        public string Unit { get; set; } // unit of measurement
        public string Theme { get; set; }
    }
}