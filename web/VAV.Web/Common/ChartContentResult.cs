using System;
using System.Web;
using System.Web.Mvc;
using VAV.DAL.Services;
using VAV.Model.Data;

namespace VAV.Web.Common
{
    public class ChartContentResult : ContentResult
    {
        private bool _isLarge;
        private ChartModel _model;
        public ChartContentResult( bool isLarge,ChartModel model)
        {
            _isLarge = isLarge;
            _model = model;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            if (Content == null)
            {
                var chartService = (ChartService)DependencyResolver.Current.GetService(typeof(ChartService));
                Content = _isLarge ? chartService.GetLargeChart(null, _model).ToHtmlString() : chartService.GetChart(null, _model).ToHtmlString();
            }
            response.Write(Content);
        }
    }
}