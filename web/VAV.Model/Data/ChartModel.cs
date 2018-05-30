using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Options;

namespace VAV.Model.Data
{
    public class ChartModel
    {
        public ChartModel(string source)
        {
            XAxisCategory = new List<string>();
            Series = new List<Series>();
            Source = source;
        }
        public string ChartName { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public ChartTypes ChartType { get; set; }

        public string XAxisText { get; set; }

        public string YAxisText { get; set; }

        public IEnumerable<string> XAxisCategory { get; set; }

        public bool IsXAxisDate { get; set; }

        public IEnumerable<Series> Series { get; set; }

        public int? Height { get; set; }

        public int? Width { get; set; }

        public DateTime StartDate { get; set; }

        public string Theme { get; set; }

        public bool IsResizeable { get; set; }

        public int ReportID { get; set; }

        public bool NoUnit { get; set; }

        public string Source { get; set; }
    }
}
