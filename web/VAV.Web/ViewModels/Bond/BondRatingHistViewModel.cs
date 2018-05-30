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
using VAV.DAL.Report;
using VAV.DAL.ResearchReport;
using System.Data;

namespace VAV.Web.ViewModels.Bond
{
    public class BondRatingHistViewModel
    {
        public string BondCode { get; private set; }
        public List<BondRatingHist> RatingHistData { get; private set; }

        public BondRatingHistViewModel(string bondCode, BondReportRepository repository, ResearchReportRepository CMARepository)
        {
            RatingHistData = repository.GetBondRatingByCode(bondCode);
            var idlist = RatingHistData.Select(r => r.RATE_ID.ToString()).ToList();

            if (idlist != null && idlist.Count() != 0)
            {
                var ids = idlist.Aggregate((a, b) => a + "," + b).ToString();
                var dataTable = CMARepository.CheckCommonFileExsit(ids, "RATE_REP_DATA", "RATE_ID");

                if (dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                        RatingHistData.Where(r => r.RATE_ID == Convert.ToInt64(row["RATE_ID"])).ToList().ForEach(r => r.ContainFile = true);
                }
            }

            BondCode = bondCode;
        }
    }
}