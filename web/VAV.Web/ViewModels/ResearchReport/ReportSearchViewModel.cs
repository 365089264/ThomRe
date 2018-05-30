using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using VAV.Web.ViewModels.Report;
using Microsoft.Practices.Unity;
using VAV.DAL.ResearchReport;
using VAV.Entities;
using System.IO;


namespace VAV.Web.ViewModels.ResearchReport
{
    public class ReportSearchViewModel : BaseReportViewModel
    {

        //[Dependency]
        private ResearchReportRepository _repository { get; set; }

        public ReportSearchViewModel(int id, string user)
            : base(id, user)
        {
            _repository = (ResearchReportRepository)DependencyResolver.Current.GetService(typeof(ResearchReportRepository));
        }

        public override void Initialization()
        {
           
        }
    }
}