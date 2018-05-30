using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.Model.Data.Bond;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.Bond
{
    public class BondIssueAmountReport
    {

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the report name.
        /// </summary>
        public string ReportName { get; set; }

        /// <summary>
        /// Used to populate the top grid
        /// </summary>
        public IEnumerable<BondIssueAmount> TopGrid { get; set; }

        /// <summary>
        /// Used to populate the bottom grid
        /// </summary>
        public IEnumerable<BondDetail> BottomGrid { get; set; }

        /// <summary>
        /// Chart source
        /// </summary>
        public ChartViewModel Chart { get; set; }

        /// <summary>
        /// Top Grid Name
        /// </summary>
        public string TopGridName { get { return ReportName.Replace(Resources.Global.Skip_Statistics, "") + Resources.Global.Title_Statistics; } set { } }

        /// <summary>
        /// Bottom Grid Name
        /// </summary>
        public string BottomGridName { get { return ReportName.Replace(Resources.Global.Skip_Statistics, "") + Resources.Global.Title_Detail; } set { } }

        /// <summary>
        /// Chart Name
        /// </summary>
        public string ChartName { get { return ReportName.Replace(Resources.Global.Skip_Statistics, "") + Resources.Global.Title_StatisticsChart; } set { } }

        /// <summary>
        /// Bond Issue Amount Report constructor.
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="reportName"></param>
        /// <param name="topGrid"></param>
        /// <param name="bottomGrid"></param>
        public BondIssueAmountReport(int reportId, string reportName, string unit, IEnumerable<BondIssueAmount> topGrid, IEnumerable<BondDetail> bottomGrid)
        {
            this.ID = reportId;
            this.ReportName = reportName;
            this.TopGrid = topGrid;
            this.BottomGrid = bottomGrid;
            this.Chart = InitialChart(reportName, unit);
        }

        /// <summary>
        /// Initial chart
        /// </summary>
        /// <returns></returns>
        public ChartViewModel InitialChart(string reportName, string unit)
        {
            Chart = new ChartViewModel();
            Chart.StatisticalAspects = GetStatisticalAspects();
            Chart.ColumnValues = GetColumnValues();
            Chart.ColumnName = Resources.Global.BondIssue_Issue_Amt; // need to modify
            Chart.ReportId = ID;
            Chart.DataCategories = TopGrid.Select(t => t.TypeName == null ? Resources.Global.Tip_Other : t.TypeName).Cast<object>().ToList();

            Chart.IsTop = true;
            Chart.ChartTypeOptions = new List<string>
                                            {
                                                Resources.Global.ChatType_Column,
                                                Resources.Global.ChatType_Bar,
                                                Resources.Global.ChatType_Pie
                                            };
            Chart.Unit = unit;
            Chart.Title = reportName;
            return Chart;
        }

        private Dictionary<string, string> GetStatisticalAspects()
        {
            IList<KeyValuePair<string, string>> aspects = new List<KeyValuePair<string, string>>();

            aspects.Add(new KeyValuePair<string, string>(Resources.Global.BondIssue_Issue_Amt, "IssuesAmount"));
            aspects.Add(new KeyValuePair<string, string>(Resources.Global.BondIssue_Issues, "Issues"));

            return aspects.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
        }

        private IDictionary<string, List<object>> GetColumnValues()
        {
            Dictionary<string, List<object>> dict = new Dictionary<string, List<object>>();
            dict.Add("Issues", TopGrid.Where(t => t.IsParent==1).Select(t => t.Issues).Cast<object>().ToList());
            dict.Add("IssuesAmount", TopGrid.Where(t => t.IsParent==1).Select(t => t.IssuesAmount).Cast<object>().ToList());
            return dict;
        }
    }
}