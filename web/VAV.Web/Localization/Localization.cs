using System.Web.Mvc;
using System.Threading;
using System.Globalization;

namespace VAV.Web.Localization
{
    public class LocalizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            var culture = CultureHelper.GetCulture(request);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture);
            base.OnActionExecuting(filterContext);
        }
    }

}
