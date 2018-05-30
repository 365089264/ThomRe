using System.IO;
using System.Web.Mvc;
using log4net;
using VAV.DAL.ResearchReport;
using Microsoft.Practices.Unity;
using VAV.Web.Common;
using VAV.Web.Localization;
using System;


namespace VAV.Web.Controllers
{
    public class ResearchReportController : BaseController
    {
        [Dependency]
        public ResearchReportRepository _repository { get; set; }

        private readonly ILog _log = LogManager.GetLogger(typeof(ResearchReportController));
        [Localization]
        public ActionResult DownloadFile(int id)
        {
            //_log.Error("Start:" + id);
            var file = _repository.GetFileDataById(id);
            //_log.Error("Get FileData:" + id);
            var fileDetail = _repository.GetFileDetailById(id);
            //_log.Error("Get FileDetail Success:" + id);
            if (file == null)
                return HttpNotFound();
            //_log.Error("Start Get Mime：" + id);
            //var contentType = MimeHelper.GetMimeTypeByExt(fileDetail.EXTENSION);
            var contentType = MimeHelper.GetMimeTypeByExt(fileDetail.EXTENSION.ToLower());
            //_log.Error("Success Get Mime：" + contentType);
            //_log.Error("Start DownLoad:" + id);
            var ms = new MemoryStream(file.Content);
            return File(ms, contentType, Resources.Global.DownloadFileName + "." + fileDetail.EXTENSION);
        }

        [Localization]
        public string UpdateRsReportTypeOption(string orgCodes)
        {
            var options = HtmlUtil.GetRsReportTypeOptionHtml(orgCodes);
            return options;
        }

        public string GeResUrl(int id)
        {
            _log.Error("Start GeResUrl：" + id);
            _repository.AddFileVisitLog(id, UserSettingHelper.GetUserId(Request));
            var fileDetail = _repository.GetFileDetailById(id);
            string instiu = fileDetail.INSTITUTIONINFOCODE;
            string result = string.Empty;
            _log.Error("Get fileDetail.EXTENSION：" + fileDetail.EXTENSION);
            if (fileDetail.EXTENSION == "pdf" && "NationalGrainAndOilsInformationCenter,BeijingZhongdianJingwei".Contains(instiu))
                result = "/PdfView/PdfView?did="+id ;
            else
                result = "/ResearchReport/DownloadFile?id=" + id ;
            return result;

        }
        [Localization]
        public ActionResult GeRSReport(DateTime startDate, DateTime endDate, string orgCodes, string reportTypes, string reportName, int pageNo, int pageSize, bool isHTML = false)
        {
            int total;
            var reports = _repository.GetRSReport(startDate, endDate, orgCodes, reportTypes, reportName, pageNo, pageSize, out total);
            var result = new
            {
                Data = reports,
                Total = total,
                CurrentPage = pageNo,
                PageSize = pageSize
            };
            return isHTML ? Json(result, "text/html", JsonRequestBehavior.AllowGet) : Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
