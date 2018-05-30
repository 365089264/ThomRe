using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace VAV.Web.Controllers
{
    public class ChartController : Controller
    {
        /*Get RMB Highlight 100 charts*/
        public ActionResult GetChartByName(string name)
        {
            var dir = Server.MapPath("~/Content/charts/");
            var path = Path.Combine(dir, name + ".chart");
            return File(path, "text/xml", name + ".chart");
        }
    }
}
