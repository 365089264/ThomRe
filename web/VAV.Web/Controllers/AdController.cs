using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VAV.Web.Localization;

namespace VAV.Web.Controllers
{
    public class AdController : Controller
    {
        [Localization]
        public ActionResult Yaozhi()
        {
            if (CultureHelper.IsEnglishCulture())
            {
                return View("YaozhiEn");
            }
            else
            {
                return View("YaozhiCn");
            }
        }
    }
}
