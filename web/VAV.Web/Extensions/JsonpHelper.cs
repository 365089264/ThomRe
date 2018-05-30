using System.Web.Mvc;
using System.Threading;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Web;
using System;

namespace VAV.Web.Extensions
{
    public class JsonpResult : JsonResult
    {
        /// <summary>
        /// Gets or sets the javascript callback function that is
        /// to be invoked in the resulting script output.
        /// </summary>
        /// <value>The callback function name.</value>
        public string Callback { get; set; }

        /// <summary>
        /// Enables processing of the result of an action method by a
        /// custom type that inherits from
        /// <see cref="T:System.Web.Mvc.ActionResult"/>.
        /// </summary>
        /// <param name="context">The context within which the
        /// result is executed.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;
            if (!string.IsNullOrEmpty(ContentType))
                response.ContentType = ContentType;
            else
                response.ContentType = "application/javascript";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Callback == null || Callback.Length == 0)
            {
                Callback = context.HttpContext.Request.QueryString["jsoncallback"];
            }

            if (Data != null)
            {
                // The JavaScriptSerializer type was marked as obsolete
                // prior to .NET Framework 3.5 SP1 
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string ser = serializer.Serialize(Data);
                response.Write(Callback + "(" + ser + ");");
            }
        }
    }

    public class JsonpFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(
                ActionExecutedContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            // see if this request included a "callback" querystring
            // parameter
            string callback = filterContext.HttpContext.Request.QueryString["jsoncallback"];
            if (callback != null && callback.Length > 0)
            {
                // ensure that the result is a "JsonResult"
                JsonResult result = filterContext.Result as JsonResult;
                if (result == null)
                {
                    throw new InvalidOperationException(
                        "JsonpFilterAttribute must be applied only " +
                        "on controllers and actions that return a " +
                        "JsonResult object.");
                }

                filterContext.Result = new JsonpResult
                {
                    ContentEncoding = result.ContentEncoding,
                    ContentType = result.ContentType,
                    Data = result.Data,
                    Callback = callback
                };
            }
        }
    }

}
