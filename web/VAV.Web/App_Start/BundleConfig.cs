using System.Web.Optimization;

namespace VAV.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery.ui.menubar.js",
                        "~/Scripts/jquery.layout-latest.js",
                        "~/Scripts/jquery.blockUI.js",
                        "~/Scripts/jquery.mtz.monthpicker.js",
                        "~/Scripts/DatePicker.js",
                        "~/Scripts/jquery.scrollabletab.js",
                        "~/Scripts/jquery.jstree.js",
                        "~/Scripts/Datepicker-zh.js",
                        "~/Scripts/jquery.ba-outside-events.js",
                        "~/Scripts/jquery.tmpl.js",
                        "~/Scripts/jquery.mask.js",
                        "~/Scripts/jquery.paginate.js",
                        "~/Scripts/jquery.multiselect.js",
                        "~/Scripts/Slider/jssor.core.js",
                        "~/Scripts/Slider/jssor.slider.js",
                        "~/Scripts/Slider/jssor.utils.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/amplify").Include("~/Scripts/amplify.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));


            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/themes/Charcoal/bundle").Include(
                        "~/Content/themes/Charcoal/jquery.ui.core.css",
                        "~/Content/themes/Charcoal/jquery.ui.resizable.css",
                        "~/Content/themes/Charcoal/jquery.ui.selectable.css",
                        "~/Content/themes/Charcoal/jquery.ui.accordion.css",
                        "~/Content/themes/Charcoal/jquery.ui.autocomplete.css",
                        "~/Content/themes/Charcoal/jquery.ui.button.css",
                        "~/Content/themes/Charcoal/jquery.ui.dialog.css",
                        "~/Content/themes/Charcoal/jquery.ui.slider.css",
                        "~/Content/themes/Charcoal/jquery.ui.tabs.css",
                        "~/Content/themes/Charcoal/jquery.ui.datepicker.css",
                        "~/Content/themes/Charcoal/jquery.ui.progressbar.css",
                        "~/Content/themes/Charcoal/jquery.ui.theme.css",
                        "~/Content/themes/Charcoal/jquery.ui.all.css",
                        "~/Content/themes/Charcoal/jquery.ui.menu.css",
                        "~/Content/themes/Charcoal/site.css",
                        "~/Content/themes/Charcoal/jsTreeStyle.css",
                        "~/Content/themes/Charcoal/jquery.ui.menubar.css",
                        "~/Content/themes/Charcoal/layout-default-latest.css",
                        "~/Content/themes/Charcoal/paginate.css",
                        "~/Content/themes/base/jquery.multiselect.css"));

            bundles.Add(new StyleBundle("~/Content/themes/Pearl/bundle").Include(
                        "~/Content/themes/Pearl/jquery.ui.core.css",
                        "~/Content/themes/Pearl/jquery.ui.resizable.css",
                        "~/Content/themes/Pearl/jquery.ui.selectable.css",
                        "~/Content/themes/Pearl/jquery.ui.accordion.css",
                        "~/Content/themes/Pearl/jquery.ui.autocomplete.css",
                        "~/Content/themes/Pearl/jquery.ui.button.css",
                        "~/Content/themes/Pearl/jquery.ui.dialog.css",
                        "~/Content/themes/Pearl/jquery.ui.slider.css",
                        "~/Content/themes/Pearl/jquery.ui.tabs.css",
                        "~/Content/themes/Pearl/jquery.ui.datepicker.css",
                        "~/Content/themes/Pearl/jquery.ui.progressbar.css",
                        "~/Content/themes/Pearl/jquery.ui.theme.css",
                        "~/Content/themes/Pearl/jquery.ui.all.css",
                        "~/Content/themes/Pearl/jquery.ui.menu.css",
                        "~/Content/themes/Pearl/site.css",
                        "~/Content/themes/Pearl/jsTreeStyle.css",
                        "~/Content/themes/Pearl/jquery.ui.menubar.css",
                        "~/Content/themes/Pearl/layout-default-latest.css",
                        "~/Content/themes/Pearl/paginate.css",
                        "~/Content/themes/base/jquery.multiselect.css"));


            bundles.Add(new ScriptBundle("~/bundles/common").Include(
                        "~/Scripts/Common.js",
                        "~/Scripts/ReportCore.js",
                        "~/Scripts/VAV_1_0.js",
                        "~/Scripts/BondInfo/BondInfoCommon.js",
                        "~/Scripts/WMP/WMPCommon.js",
                        "~/Scripts/WMP/WMPCompare.js",
                        "~/Scripts/Fundamental/FundamentalCommon.js",
                        "~/Scripts/Common/wmp.compare.js",
                        "~/Scripts/Common/wmp.compare.detail.js",
                        "~/Scripts/CnE/GDTCommon.js",
                        "~/Scripts/CnE/GDTprice.js",
                        "~/Scripts/CnE/GDTOutput.js",
                        "~/Scripts/CnE/GDTSales.js",
                        "~/Scripts/CnE/CommodityNews.js",
                        "~/Scripts/CnE/GDTInventory.js",
                        "~/Scripts/CnE/GDTEnergyInventory.js",
                        "~/Scripts/CnE/GDTBalanceTable.js",
                        "~/Scripts/CnE/Coal.js",
                        "~/Scripts/Common/hierarchy.js",
                        "~/Scripts/StructuredReport/StructuredReport.js"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
            "~/Scripts/Highstock/highstock.js",
            "~/Scripts/Highstock/modules/exporting.src.js",
            "~/Scripts/Highstock/modules/technical-indicators.src.js"));

            bundles.Add(new ScriptBundle("~/bundles/pearltheme").Include(
            "~/Scripts/Highstock/themes/pearl.js"));

            bundles.Add(new ScriptBundle("~/bundles/novatheme").Include(
            "~/Scripts/Highstock/themes/gray.js"));

            bundles.Add(new ScriptBundle("~/bundles/JET").Include(
                "~/Scripts/JET/JET.js",
                "~/Scripts/JET/plugins/Quotes.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jslangen").Include(
                "~/Scripts/i18n/wmp.compare.en_US.js",
                "~/Scripts/i18n/CnE.en_US.js"));

            bundles.Add(new ScriptBundle("~/bundles/jslangzh").Include(
                "~/Scripts/i18n/wmp.compare.zh_CN.js",
                "~/Scripts/i18n/CnE.zh_CN.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/YaozhiJs").Include(
                "~/Scripts/jquery.tools.js"));
            bundles.Add(new ScriptBundle("~/bundles/d3").Include(
                "~/Scripts/D3/d3.js",
                "~/Scripts/D3/topojson.js",
                "~/Scripts/D3/datamaps.all.js"));
            bundles.Add(new StyleBundle("~/bundles/YaozhiPearl").Include(
                "~/Content/themes/Pearl/Yaozhi/global-pearl.css",
                "~/Content/themes/Pearl/Yaozhi/tabs-ui-charcoal2.css"
                ));
            bundles.Add(new StyleBundle("~/bundles/YaozhiCharcoal").Include(
                "~/Content/themes/Charcoal/Yaozhi/global-s.css",
                "~/Content/themes/Charcoal/Yaozhi/tabs-ui-charcoal2.css"
                ));

            bundles.Add(new StyleBundle("~/Content/themes/Charcoal/IPPbundle").Include(
                        "~/Content/themes/Charcoal/IPPSite.css",
                        "~/Content/themes/Charcoal/jquery.ui.core.css",
                        "~/Content/themes/Charcoal/jquery.ui.selectable.css",
                        "~/Content/themes/Charcoal/jquery.ui.accordion.css",
                        "~/Content/themes/Charcoal/jquery.ui.autocomplete.css",
                        "~/Content/themes/Charcoal/jquery.ui.button.css",
                        "~/Content/themes/Charcoal/jquery.ui.dialog.css",
                        "~/Content/themes/Charcoal/jquery.ui.tabs.css",
                        "~/Content/themes/Charcoal/jquery.ui.datepicker.css",
                        "~/Content/themes/Charcoal/jquery.ui.theme.css",
                        "~/Content/themes/Charcoal/paginate.css",
                        "~/Content/themes/base/jquery.multiselect.css",
                        "~/Content/themes/Charcoal/jquery.ui.menu.css",
                        "~/Content/themes/Charcoal/jquery.ui.menubar.css",
                        "~/Content/themes/Charcoal/rating.css"));
            bundles.Add(new StyleBundle("~/Content/themes/Pearl/IPPbundle").Include(
                        "~/Content/themes/Pearl/IPPSite.css",
                        "~/Content/themes/Pearl/jquery.ui.core.css",
                        "~/Content/themes/Pearl/jquery.ui.selectable.css",
                        "~/Content/themes/Pearl/jquery.ui.accordion.css",
                        "~/Content/themes/Pearl/jquery.ui.autocomplete.css",
                        "~/Content/themes/Pearl/jquery.ui.button.css",
                        "~/Content/themes/Pearl/jquery.ui.dialog.css",
                        "~/Content/themes/Pearl/jquery.ui.tabs.css",
                        "~/Content/themes/Pearl/jquery.ui.datepicker.css",
                        "~/Content/themes/Pearl/jquery.ui.theme.css",
                        "~/Content/themes/Pearl/paginate.css",
                        "~/Content/themes/base/jquery.multiselect.css",
                        "~/Content/themes/Pearl/jquery.ui.menu.css",
                        "~/Content/themes/Pearl/jquery.ui.menubar.css",
                        "~/Content/themes/Pearl/rating.css"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui/IPP").Include(
            "~/Scripts/jquery-ui-{version}.js",
            "~/Scripts/jquery.tmpl.js",
            "~/Scripts/jquery.mask.js",
            "~/Scripts/jquery.paginate.js",
            "~/Scripts/jquery.blockUI.js",
            "~/Scripts/jquery.multiselect.js",
            "~/Scripts/jquery.ui.menubar.js"));

            bundles.Add(new ScriptBundle("~/js/angular").Include(
                "~/scripts/angular/angular.js",
                "~/scripts/angular/angular-route.js"
                ));

            bundles.Add(new ScriptBundle("~/js/ippCommon").Include(
                "~/Scripts/Common.js",
                "~/scripts/IPP/Common.js"
                ));
            bundles.Add(new ScriptBundle("~/js/ippMyDocument").Include("~/scripts/IPP/MyDocument.js"));

            bundles.Add(new ScriptBundle("~/bundles/ippjslangen").Include(
                "~/Scripts/IPP/IPPGlobal.en_US.js"));

            bundles.Add(new ScriptBundle("~/bundles/ippjslangzh").Include(
                "~/Scripts/IPP/IPPGlobal.zh_CN.js"));
        }
    }
}