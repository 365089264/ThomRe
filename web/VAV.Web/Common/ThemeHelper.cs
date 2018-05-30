using System;
using System.Configuration;
using System.Text;
using System.Web;

namespace VAV.Web.Common
{
    /// <summary>
    /// Theme Helper
    /// Theme format:
    /// EIKON_USER_AGENT="NET40,EIKON8.0.189,SR0,UA12.1.32,DT13.51.3002,ADF6.20124.12.22,Charcoal"
    /// EIKON_USER_AGENT="NET40,EIKON8.0.189,SR0,UA12.1.32,DT13.51.3002,ADF6.20124.12.22,Pearl"; 
    /// </summary>
    public static class ThemeHelper
    {
        private const string EikonCookieName = "EIKON_USER_AGENT";
        private const string CharcoalThemeName = "Charcoal";
        private const string PearlThemeName = "Pearl";
        private const string LocalCookieName = "Local_CMA_Theme";

        /// <summary>
        /// Gets the theme.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static string GetTheme(HttpRequestBase request)
        {
            //return CharcoalThemeName;
            if (request == null) return CharcoalThemeName;
            var cookie = request.Cookies[EikonCookieName];
            if(cookie != null)
            {
                return GetTheme(cookie.Value);
            }
            var routeTheme = request.RequestContext.RouteData.Values["ThemeName"];
            if (routeTheme != null)
            {
                return GetTheme(routeTheme.ToString());
            }
            var localCookie = request.Cookies[LocalCookieName];
            return localCookie != null ? GetTheme(localCookie.Value) : ConfigurationManager.AppSettings["defaultTheme"];
        }

        public static void UpdateLocalThemeCookie(HttpRequestBase request,HttpResponseBase response)
        {
            var savedCookie = new HttpCookie(LocalCookieName, GetTheme(request)) { Expires = DateTime.Now.AddYears(1) };
            response.SetCookie(savedCookie);
        }

        /// <summary>
        /// Gets the theme.
        /// </summary>
        /// <param name="targetTheme">The target theme.</param>
        /// <returns></returns>
        private static string GetTheme(string targetTheme)
        {
            if (string.IsNullOrWhiteSpace(targetTheme)) return ConfigurationManager.AppSettings["defaultTheme"];
            if (targetTheme.IndexOf(CharcoalThemeName, StringComparison.InvariantCultureIgnoreCase) >= 0)
                return CharcoalThemeName;
            if (targetTheme.IndexOf(PearlThemeName, StringComparison.InvariantCultureIgnoreCase) >= 0)
                return PearlThemeName;
            return ConfigurationManager.AppSettings["defaultTheme"];
        }

        /// <summary>
        /// Traces the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="currentTheme">The current theme.</param>
        /// <returns></returns>
        public static string Trace(HttpRequestBase request,string currentTheme)
        {
            var sb = new StringBuilder();
            var istrace = false;
            if (!bool.TryParse(ConfigurationManager.AppSettings["ThemeTrace"],out istrace))
            {
                sb.AppendFormat("Theme Trace:{0}", "Not Setting!");
                return sb.ToString();
            }
            if(!istrace)
            {
                return "Disabled";
            }
            sb.AppendFormat("Current Theme:{0};", currentTheme);
            sb.Append("Output Cookie:");
            foreach (var key in request.Cookies.AllKeys)
            {
                sb.AppendFormat("Key:{0},Value:{1};", key, request.Cookies[key].Value);
            }
            sb.Append("Output Cookie End.");
            sb.AppendFormat("EIKON_USER_AGENT:{0};",
                            request.Cookies[EikonCookieName] == null ? "null" : request.Cookies[EikonCookieName].Value);
            sb.AppendFormat("RouteTheme:{0};",
                            request.RequestContext.RouteData.Values["ThemeName"] == null
                                ? "null"
                                : request.RequestContext.RouteData.Values["ThemeName"].ToString());
            sb.AppendFormat("Web.Config:{0};", ConfigurationManager.AppSettings["defaultTheme"]);
            return sb.ToString();
        }
    }
}