using System.Collections.Generic;
using VAV.Web.ViewModels.Report;
using VAV.Entities;
using System.Data;

namespace VAV.Web.ViewModels.BondInfo
{
    public class BondFutureSampleViewModel : BaseReportViewModel
    {
        public DataTable Data { get; private set; }
        public IEnumerable<REPORTCOLUMNDEFINITION> Columns { get; private set; }

        public BondFutureSampleViewModel(int id, string user) : base(id, user)
        {
        }

        public override void Initialization()
        {
        }


    }
}