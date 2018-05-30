using System.Net;
using System.Web.Mvc;
using VAV.DAL.Services;
using VAV.Web.Common;
using VAV.Web.Localization;

namespace VAV.Web.Controllers
{
    public class UserSettingController : Controller
    {
        private readonly UserColumnService _userColumnSvc;

        public UserSettingController(UserColumnService svc)
        {
            _userColumnSvc = svc;
        }
        
        [Localization]
        public ActionResult GetAvailableColumns(int reportId)
        {
            var columns = _userColumnSvc.GetAllColumnsWithUserSetting(UserSettingHelper.GetUserId(Request), reportId, CultureHelper.IsEnglishCulture());
            return Json(columns, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateOrCreateUserColumn(int reportId,string setting)
        {
            _userColumnSvc.SaveUserColumns(UserSettingHelper.GetUserId(Request), reportId, setting);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
