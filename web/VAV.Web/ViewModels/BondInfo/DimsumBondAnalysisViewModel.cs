using System.Collections.Generic;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.BondInfo
{
    public class DimsumBondAnalysisViewModel : BaseReportViewModel
    {
        public IEnumerable<string> TopGridColumns { get; set; }

        public DimsumBondAnalysisViewModel(int id, string userId)
            : base(id, userId)
        {
        }

        public override void Initialization()
        {
        }
    }
}