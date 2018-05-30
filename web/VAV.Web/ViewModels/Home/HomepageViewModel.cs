using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.IPP;
using VAV.DAL.Report;
using VAV.Entities;
using VAV.Model.IPP;
using VAV.Web.Localization;
using VAV.Web.ViewModels.Report;

namespace VAV.Web.ViewModels.Home
{
    public class HomepageViewModel:BaseReportViewModel
    {

        PartnersReportRepository cmaRepository { get; set; }
        private IPPRepository ippRepository;

        public HomepageViewModel(int id, string user = "Unknown")
            : base(id, user)
        {
            cmaRepository = (PartnersReportRepository)DependencyResolver.Current.GetService(typeof(PartnersReportRepository));
            ippRepository = (IPPRepository)DependencyResolver.Current.GetService(typeof(IPPRepository));
        }

        public IEnumerable<HOMEMODULE> Modules;
        public List<HomeHotItem> HotItems;
        public List<HOMEANNOUNCEMENT> Announcements; 


        public override void Initialization()
        {
            Modules = cmaRepository.GetHomeModules();
            HotItems = ippRepository.GetTopFile("1m", CultureHelper.IsEnglishCulture(), 12);
            Announcements = cmaRepository.GetHomeAnnouncements();
        }
    }
}