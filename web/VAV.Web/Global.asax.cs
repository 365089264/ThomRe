using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Aspose.Cells;
using Common.Logging;
using log4net;
using SolrNet.Impl;
using SolrNet;
using VAV.Web.ViewModels.Home;
using System.Configuration;
using VAV.Web.Common;

namespace VAV.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bootstrapper.Initialise();
            var license = new License();
            var commonAssembly = System.Reflection.Assembly.Load("VAV.DAL");
            var s = commonAssembly.GetManifestResourceStream("VAV.DAL.Aspose.Cells.lic");
            license.SetLicense(s);

            new UIStaticDataCache();

            var solrServer = ConfigurationManager.AppSettings["SolrServer"];
            var connection0 = new SolrConnection("http://" + solrServer + ":8983/solr/core0");
            var connection1 = new SolrConnection("http://" + solrServer + ":8983/solr/core1");
            Startup.Init<SearchContentViewModel>(connection0);
            Startup.Init<AutoSuggestViewModel>(connection1);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            _log.Error("Unhandled Exception Happened!" + Environment.NewLine + Environment.NewLine + "Error Message:", exception);
        }

    }
}