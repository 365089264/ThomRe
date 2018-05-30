using System.Web.Mvc;
using System.Web.Routing;

namespace VAV.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.MapRoute("UploadPubInfo", "Upload/{Lan}/{ThemeName}/", new { controller = "Home", action = "HomeItemEditor", Lan = UrlParameter.Optional, ThemeName = UrlParameter.Optional });
            routes.MapRoute("Commodity", "Commodity/{Lan}/{ThemeName}/", new { controller = "Home", action = "Commodity", Lan = UrlParameter.Optional, ThemeName = UrlParameter.Optional });
            routes.MapRoute("LanguageTest", "{Lan}", new { controller = "Home", action = "Index" });
            routes.MapRoute("ThemeTest", "Theme/{ThemeName}", new { controller = "Home", action = "Index" });
            routes.MapRoute("ExternalLink", "ExLink/{id}/{Lan}/{ThemeName}/", new { controller = "Home", action = "External", Lan = UrlParameter.Optional, ThemeName = UrlParameter.Optional });
            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            
            //routes.MapRoute("LanguageTest", "{Lan}", new { controller = "IPP", action = "IPPHome" });
            //routes.MapRoute("ThemeTest", "Theme/{ThemeName}", new { controller = "IPP", action = "IPPHome" });
            //routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "IPP", action = "IPPHome", id = UrlParameter.Optional }
            );
        }
    }
}