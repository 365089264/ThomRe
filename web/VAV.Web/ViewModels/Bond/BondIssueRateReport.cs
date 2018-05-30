using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using VAV.Model.Data;
using VAV.Model.Data.Bond;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.Bond
{
    public class BondIssueRateReport : BaseReportViewModel
    {
        public List<BondIssueRate> Grid { get; set; }

        public ChartModel Chart { get; set; }

        public bool IsAllRating { get; set; }

        public string Result { get; set; }

        public List<string> ColumnTitles
        {
            get
            {
                return new List<string>
                           {
                               Resources.Global.Bond_Code,
                               Resources.Global.Bond_Name,
                               Resources.Global.Issue_Date,
                               Resources.Global.Maturity_Date,
                               Resources.Global.Bond_Term,
                               Resources.Global.Bond_Issue_Rate,
                               Resources.Global.Bond_Issue_Amt,
                               Resources.Global.Bond_Coupon_class,
                               Resources.Global.Bond_Rating,
                               Resources.Global.Bond_IssueComment
                           };
            }
        }

        public List<string> FieldNames
        {
            get
            {
                return new List<string>
                           {
                               "code",
                               "bond_name",
                               "orig_issue_dt",
                               "maturity_dt",
                               "term",
                               "yield",
                               "orig_iss_amt",
                               "coupon_type",
                               "latest_rating_cd",
                               "issueComment"
                           };
            }
        }

        public List<BondIssueRate> CurrentGrid { get; set; }

        public BondIssueRateReport(int id)
            : base(id)
        {

        }

        public bool InitOrUpdate(IEnumerable<BondIssueRate> bondIssueRates)
        {
            if (Grid == null)
            {
                Grid = new List<BondIssueRate>();
            }

            if (bondIssueRates.Count() == 0)
            {
                Result = Resources.Global.No_Result;
            }

            if (Grid.Count() != 0 || bondIssueRates.Count() != 0)
            {
                var b = bondIssueRates.FirstOrDefault();
                if (b != null)
                {
                    var key = b.cdc_asset_class + "_" + b.bond_term + "_" + b.coupon_type;
                    if (!IsAllRating)
                        key += "_" + b.latest_rating_cd;
                    var items = bondIssueRates.OrderBy(r => r.orig_issue_dt).ToList();
                    items.ForEach(i => i.ItemName = key);
                    var tmp = from f in Grid
                              where f.ItemName == key
                              select f;
                    if (0 == tmp.Count())
                        Grid.AddRange(items);
                }

                var series = new List<Series>();
                foreach (var g in Grid.GroupBy(i => i.ItemName))
                {
                    var data = from d in g
                               where d.orig_issue_dt != null
                               select new object[] { GetDate((DateTime)d.orig_issue_dt.Value).AddDays(1), d.yield };

                    if (data.Count() != 0)
                    {
                        series.Add(new Series
                                       {
                                           Name = g.Key,
                                           Data = new Data(data.ToArray()),
                                           //pointStart: Date.UTC(2006, 0, 01),
                                       });
                    }
                }

                //categories = (from d in Grid select ((DateTime)d.orig_issue_dt)).Distinct().OrderBy(d=>d).Select(d=>d.ToString("yyyy-MM")).ToList();

                //var startDate =
                //    (from d in Grid select ((DateTime)d.orig_issue_dt)).Distinct().OrderBy(d => d).FirstOrDefault();

                Chart = new ChartModel(Resources.Global.Source)
                            {
                                ChartName = "chart" + ID,
                                Series = series.ToArray(),
                                Title = Resources.Global.Bond_Issue_Rate_Chart,
                                ChartType = ChartTypes.Line,
                                IsXAxisDate = true,
                                YAxisText = Resources.Global.Bond_Issue_Rate,
                                IsResizeable=true,
                                ReportID =ID
                            };
                return true;
            }

            Chart = new ChartModel(Resources.Global.Source)
                            {
                                ChartName = "chart" + ID,
                                Title = Resources.Global.Bond_Issue_Rate_Chart,
                                ChartType = ChartTypes.Line,
                                IsXAxisDate = true,
                                YAxisText = Resources.Global.Bond_Issue_Rate,
                                IsResizeable = true,
                                ReportID = ID
                            };
            return false;
        }

        public bool UpdateChartOnly(IEnumerable<BondIssueRate> bondIssueRates)
        {
            if (Grid == null)
            {
                Grid = new List<BondIssueRate>();
            }

            if (Grid.Count() != 0 || bondIssueRates.Count() != 0)
            {
                var series = new List<Series>();
                foreach (var g in Grid.GroupBy(i => i.ItemName))
                {
                    var data = from d in g
                               where d.orig_issue_dt != null
                               select new object[] { GetDate((DateTime)d.orig_issue_dt.Value).AddDays(1), d.yield };

                    if (data.Count() != 0)
                    {
                        series.Add(new Series
                        {
                            Name = g.Key,
                            Data = new Data(data.ToArray()),
                        });
                    }
                }

                Chart = new ChartModel(Resources.Global.Source)
                {
                    ChartName = "chart" + ID,
                    Series = series.ToArray(),
                    Title = Resources.Global.Bond_Issue_Rate_Chart,
                    ChartType = ChartTypes.Line,
                    IsXAxisDate = true,
                    YAxisText = Resources.Global.Bond_Issue_Rate,
                    IsResizeable = true,
                    ReportID = ID
                };
                return true;
            }

            Chart = new ChartModel(Resources.Global.Source)
            {
                ChartName = "chart" + ID,
                Title = Resources.Global.Bond_Issue_Rate_Chart,
                ChartType = ChartTypes.Line,
                IsXAxisDate = true,
                YAxisText = Resources.Global.Bond_Issue_Rate,
                IsResizeable = true,
                ReportID = ID
            };
            return false;
        }

        public void GetCurrentGrid(string itemName)
        {
            CurrentGrid = (from g in Grid
                     where g.ItemName == itemName
                     select g).ToList();
        }

        private DateTime GetDate(DateTime dateTime)
        {
            var y = dateTime.Year;
            var m = dateTime.Month;
            var d = dateTime.Day;
            return new DateTime(y, m, d);
        }
        public override void Initialization()
        {
            throw new NotImplementedException();
        }
    }
}