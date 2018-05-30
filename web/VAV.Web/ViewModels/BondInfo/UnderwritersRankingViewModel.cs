using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VAV.DAL.IPP;
using VAV.DAL.Report;
using VAV.Entities;
using VAV.Model.Data.Bond;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.BondInfo
{
    public class UnderwritersRankingViewModel : BaseReportViewModel
    {
        public UnderwritersRankingViewModel(int id, string userId)
            : base(id, userId)
        {
        }
        public override void Initialization()
        {
            //Default
        }

    }
}