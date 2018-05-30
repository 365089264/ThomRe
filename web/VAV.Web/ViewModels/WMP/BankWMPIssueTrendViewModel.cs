using System.Collections.Generic;
using System.Web.Mvc;
using VAV.DAL.WMP;
using VAV.Model.Data.WMP;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.WMP
{
    public class BankWMPIssueTrendViewModel : BaseReportViewModel
    {
        public IEnumerable<WMPBankOption> Banks { get; private set; }

        public BankWMPIssueTrendViewModel(int id, string user) : base(id, user)
        {
        }
        /// <summary>
        /// Initializations this instance.
        /// </summary>
        public override void Initialization()
        {
            var wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
            Banks = wmpRepository.GetWmpBankOptionByType("all");
        }
    }
}