using System;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Web.Controllers;

namespace VAV.Web.Extensions
{
    public class ReportControllerFactory : DefaultControllerFactory
    {
        private readonly IUnityContainer _container;
        private readonly ReportService _reportService;

        public ReportControllerFactory(IUnityContainer container)
        {
            _container = container;
            _reportService = (ReportService)_container.Resolve(typeof(ReportService));
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            Type requiredType = null;
            if (controllerName == "PageManager")
            {
                int id;
                if (int.TryParse(requestContext.RouteData.Values["id"].ToString(), out id))
                {
                    var reportInfo = _reportService.GetReportInfoById(id);
                    switch (reportInfo.ReportType)
                    {
                        case "CDC":
                            requiredType = typeof(CDCController);
                            requestContext.RouteData.Values["controller"] = "CDC";
                            break;
                        case "BondInfo":
                            requiredType = typeof(BondInfoController);
                            requestContext.RouteData.Values["controller"] = "BondInfo";
                            break;
                        case "OpenMarket":
                            requiredType = typeof(OpenMarketController);
                            requestContext.RouteData.Values["controller"] = "OpenMarket";
                            switch (reportInfo.ViewName)
                            {
                                case "DetailDataReport":
                                    requestContext.RouteData.Values["action"] = "GetOpenMarketSearch";
                                    break;
                                case "MonetaryAndReturnAnalysisReport":
                                    requestContext.RouteData.Values["action"] = "GetMonetaryAndReturnReport";
                                    break;
                                case "RatesAnalysis":
                                    requestContext.RouteData.Values["action"] = "GetRatesAnalysisReport";
                                    break;
                            }
                            break;
                        case "BondMarket":
                            requiredType = typeof(BondReportController);
                            requestContext.RouteData.Values["controller"] = "BondReport";
                            switch (reportInfo.ViewName)
                            {
                                case "RateOfIssues":
                                    requestContext.RouteData.Values["action"] = "GetRateOfIssuesReport";
                                    break;
                                case "IssueAmountReport":
                                    requestContext.RouteData.Values["action"] = "GetIssueAmountReport";
                                    break;
                            }
                            break;
                        case "MacroEconomy":
                            requiredType = typeof(MacroeconomyController);
                            requestContext.RouteData.Values["controller"] = "Macroeconomy";
                            break;
                        case "WMP":
                            requiredType = typeof(WMPController);
                            requestContext.RouteData.Values["controller"] = "WMP";
                            break;
                        case "Fundamental":
                            requiredType = typeof(FundamentalController);
                            requestContext.RouteData.Values["controller"] = reportInfo.ReportType;
                            break;
                        case "ResearchReport":
                            requiredType = typeof(ResearchReportController);
                            requestContext.RouteData.Values["controller"] = reportInfo.ReportType;
                            break;
                        case "Ad":
                            requiredType = typeof (AdController);
                            requestContext.RouteData.Values["controller"] = "Ad";
                            requestContext.RouteData.Values["action"] = "Yaozhi";
                            break;
                        case "GDT":
                            requiredType = typeof(GDTController);
                            requestContext.RouteData.Values["controller"] = "GDT";
                            requestContext.RouteData.Values["action"] = "GDTHome";
                            requestContext.RouteData.Values["ReportId"] = id;
                            break;
                        case "pdfviewer":
                            //PdfView
                            requiredType = typeof(PdfViewController);
                            requestContext.RouteData.Values["controller"] = "PdfView";
                            requestContext.RouteData.Values["action"] = "PdfView";
                            break;
                        case "CNE":
                            requiredType = typeof(CNEController);
                            requestContext.RouteData.Values["controller"] = reportInfo.ReportType;;
                            break;
                        case "Home":
                            requiredType = typeof (HomeController);
                            requestContext.RouteData.Values["controller"] = reportInfo.ReportType;
                            requestContext.RouteData.Values["action"] = reportInfo.ViewName;
                            break;
                        case "Partners":
                            requiredType = typeof(PartnersController);
                            requestContext.RouteData.Values["controller"] = reportInfo.ReportType;
                            break;
                        case "StructureReport":
                            requiredType = typeof(StructureReportController);
                            requestContext.RouteData.Values["controller"] = "StructureReport";
                            break;
                    }
                }
            }
            try
            {
                requiredType = requiredType ?? GetControllerType(requestContext, controllerName);
                return (IController)_container.Resolve(requiredType);
            }
            catch
            {
                return base.CreateController(requestContext, controllerName);
            }
        }

        public override void ReleaseController(IController controller)
        {
            _container.Teardown(controller);

            base.ReleaseController(controller);
        }
    }
}