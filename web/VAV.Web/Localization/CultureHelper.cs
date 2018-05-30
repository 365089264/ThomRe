using System;
using System.Linq;
using System.Threading;
using System.Web;
using VAV.Web.UserSetting;

namespace VAV.Web.Localization
{
    public static class CultureHelper
    {
        public const string DefaultCulture = "zh-CN";
        public const string CultureKey = "Vav.Localization.CurrentUICulture";
        public static string GetCulture(HttpRequestBase request)
        {
            var routeLan = request.RequestContext.RouteData.Values["Lan"];
            if (routeLan != null)
            {
                return string.Equals(routeLan.ToString(), "cn", StringComparison.OrdinalIgnoreCase) ? DefaultCulture : "en-US";
            }
            var cookie = request.Cookies[CultureKey];
            return cookie == null ? GetServerSideCultureSetting(request) : cookie.Value;
        }

        private static string GetServerSideCultureSetting(HttpRequestBase request)
        {
            var culture = DefaultCulture;
            var uuid = request.Headers["reutersuuid"];
            if (!String.IsNullOrEmpty(uuid))
            {
                try
                {
                    var userManagement = new AAAASUserManagementClient();
                    var resp = userManagement.GetAllUserDetails_1(new GetAllUserDetails { uuid = uuid });
                    culture = resp.user.attributeMap.First(x => x.name.ToLower() == "PreferredLanguage".ToLower()).value;
                }
                catch (Exception )
                {
                    culture = DefaultCulture;
                }
            }
            return culture;
        }

        public static void UpdateCultureCookie(HttpResponseBase response)
        {
            var cookie = new HttpCookie(CultureKey, Thread.CurrentThread.CurrentUICulture.Name) { Expires = DateTime.Now.AddYears(1) };
            response.SetCookie(cookie);
        }


        public static bool IsEnglishCulture()
        {
            return Thread.CurrentThread.CurrentUICulture.Name != "zh-CN";
        }

    }
}