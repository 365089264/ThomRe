using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.Fundamental
{
    public class OTCMarketCompaniesViewModel : BaseReportViewModel
    {
        public OTCMarketCompaniesViewModel(int id, string user)
            : base(id, user)
        {
        }

        public override void Initialization()
        {
            
        }
    }
}