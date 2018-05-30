using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.Practices.Unity;
using Resources;
using VAV.DAL.IPP;
using VAV.Web.Extensions;
using VAV.Web.Localization;
using VAV.Model.Data.IPP;
using VAV.Web.UserSetting;
using VAV.Web.ViewModels.IPP;
using VAV.Web.Common;
using System.Collections.Generic;
using System;
using VAV.Entities;
using System.Linq;
using System.Data;
using System.Collections;
using VAV.DAL.Common;


namespace VAV.Web.Controllers
{
    [Localization]
    public class IPPController : Controller
    {
        [Dependency]
        public IPPRepository IPPRepository { get; set; }

        public ActionResult IPPHome()
        {
            var isEnglish = CultureHelper.IsEnglishCulture();
            ViewBag.StaticTopics = IPPRepository.GetStaticTopics(isEnglish);
            ViewBag.Announcement = IPPRepository.GetAnnouncement(isEnglish);
            ViewBag.LatestFile = IPPRepository.GetLatestFile(isEnglish);
            ViewBag.InternalUser = UserSettingHelper.IsInternalUser(Request);

            var userAttibuteMap = UserSettingHelper.GetUserAttributeMap(Request);
            string submitterId;
            if (String.IsNullOrWhiteSpace(Request.Headers["reutersuuid"]))
                submitterId = Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? Request.ServerVariables["REMOTE_ADDR"];
            else
                submitterId = userAttibuteMap == null ? "" : userAttibuteMap.First(x => x.name.ToLower() == "UserId".ToLower()).value;

            var email = userAttibuteMap == null ? "" : userAttibuteMap.First(x => x.name.ToLower() == "UserId".ToLower()).value;
            var name = userAttibuteMap == null ? "" : userAttibuteMap.First(x => x.name.ToLower() == "FullName".ToLower()).value;
            var RM = userAttibuteMap == null ? "" : userAttibuteMap.First(x => x.name.ToLower() == "ReutersMessagingId".ToLower()).value;

            IPPRepository.InsertSubmitter(submitterId, email, name, RM);

            return View();
        }

        public JsonResult GetHotTopic(string period,bool isHtml = false)
        {
            var data = IPPRepository.GetTopTopic(period, CultureHelper.IsEnglishCulture());
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHotFile(string period,bool isHtml = false)
        {
            var data = IPPRepository.GetTopFile(period, CultureHelper.IsEnglishCulture());
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        #region File Editor
        public ActionResult FileEditor(long? id, int? topicid, string previousRequest)
        {
            FileCreate fileCreate = null; 
            
            if (id == null)
            {
                fileCreate = new FileCreate();
                var eikonUserId = UserSettingHelper.GetEikonUserID(Request);
                var submitter = IPPRepository.GeSubmitterById(eikonUserId);

                fileCreate.SubmitterID = submitter.ID;
                fileCreate.SubmiterName = submitter.Name;
                fileCreate.Submiter = submitter.Email;
            }
            else 
                fileCreate =  IPPRepository.GetFileByFileID((long)id);

            IPPFile ippFile;

            fileCreate = fileCreate ?? new FileCreate();
            var moduleItems = new List<SelectListItem>();
            var topicItems = new List<SelectListItem>();
            var ricTypeItems = new List<SelectListItem>();
            var uploadTypeItems = HtmlUtil.CookSelectOptions("Ipp_UploadType");

            if (id != null)
            {
                topicid = IPPRepository.GetTopicIdByFileId(id);
            }

            IEnumerable<MODULEINFO>  modules = IPPRepository.GetModuleList();
            var moduleId = (topicid == null || topicid == 0)? modules.FirstOrDefault().ID : (int)IPPRepository.GetModuleIdByTopicId((int)topicid);
            var topics = IPPRepository.GetTopicListByModuleId(moduleId);

            foreach (var m in modules)
            {
                moduleItems.Add(new SelectListItem { Selected = m.ID == moduleId ? true : false, Value = m.ID.ToString(), Text = CultureHelper.IsEnglishCulture() ? m.NAMEEN : m.NAMECN  });
            }

            var selectedTopicId = topicid == null ? topics.FirstOrDefault().ID : topicid;
            foreach (var m in topics)
            {
                topicItems.Add(new SelectListItem { Selected = m.ID == selectedTopicId ? true : false, Value = m.ID.ToString(), Text = CultureHelper.IsEnglishCulture() ? m.NAMEEN : m.NAMECN });
            }

            ricTypeItems.Add(new SelectListItem { Value = "Graph", Text = "Chart" });
            ricTypeItems.Add(new SelectListItem { Value = "Quote Object", Text = "Quote" });
            ricTypeItems.Add(new SelectListItem { Value = "News", Text = "News" });

            var userAttibuteMap = UserSettingHelper.GetUserAttributeMap(Request);
            var author = userAttibuteMap == null ? "" : userAttibuteMap.First(x => x.name.ToLower() == "FullName".ToLower()).value;
            var source = userAttibuteMap == null ? "" : userAttibuteMap.First(x => x.name.ToLower() == "AccountName".ToLower()).value;

            if (UserSettingHelper.IsInternalUser(Request))
                source = "Thomson Reuters";

            ippFile = new IPPFile
            {
                Id = fileCreate.ID,
                Author = string.IsNullOrEmpty(fileCreate.Author) ? author : fileCreate.Author,
                AuthorRM = fileCreate.AuthorRM,
                AuthorEmail = fileCreate.AuthorEmail,
                DescriptionCn = fileCreate.DescrCn,
                DescriptionEn = fileCreate.DescrEn,
                UploadType = fileCreate.UploadType,
                FileType = fileCreate.FileType,
                WebsiteRic = fileCreate.UploadType == "Upload_Website" ? fileCreate.RIC : "",
                EikonRic = fileCreate.UploadType == "Upload_Ric" ? fileCreate.RIC : "",
                ReportDate = fileCreate.ReportDate.ToString("yyyy-MM-dd"),
                SubmitterID = fileCreate.SubmitterID,
                SubmiterName = fileCreate.SubmiterName,
                Submiter = fileCreate.Submiter,
                Tag = fileCreate.Tag != null ? fileCreate.Tag.Replace('|', ';') : fileCreate.Tag,
                TitleCn = fileCreate.TitleCn,
                TitleEn = fileCreate.TitleEn,
                ModuleItems = moduleItems,
                Topic = selectedTopicId.ToString(),
                TopicItems = topicItems,
                UploadTypeItems = uploadTypeItems,
                RicTypeItems = ricTypeItems,
                Source = string.IsNullOrEmpty(fileCreate.Source) ? source : fileCreate.Source,
                PreviousRequest = previousRequest,
                Status = fileCreate.Status,
                FileName = fileCreate.FileName,
                DisplayOrder = fileCreate.DisplayOrder
            };

            return View(ippFile);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(IPPFile ippFile, HttpPostedFileBase file, string submit)
        {
            string fileType = "";
            string ric = "";

            if (ippFile.UploadType == "Upload_File")
            {
                if (file != null)
                {
                    var index = file.FileName.LastIndexOf(".");
                    fileType = file.FileName.Substring(index + 1);
                }
                else {
                    var index = ippFile.FileName.LastIndexOf(".");
                    fileType = ippFile.FileName.Substring(index + 1);
                }
            }
            else
            {
                fileType = ippFile.FileType;
            }

            if (ippFile.UploadType == "Upload_Website")
            {
                if (!ippFile.WebsiteRic.Contains("cpurl://"))
                {
                    if (!ippFile.WebsiteRic.Contains("http"))
                        ippFile.WebsiteRic = "http://" + ippFile.WebsiteRic;
                }

                ric = ippFile.WebsiteRic;
            }
            else if (ippFile.UploadType == "Upload_Ric")
            {
                ric = ippFile.EikonRic;
            }

            var fileCreate = new FileCreate
            {
                ID = ippFile.Id ?? 0,  /*0 to indicated 'insert'*/
                Author = ippFile.Author,
                AuthorRM = ippFile.AuthorRM,
                AuthorEmail = ippFile.AuthorEmail,
                DescrCn = ippFile.DescriptionCn,
                DescrEn = ippFile.DescriptionEn,
                UploadType = ippFile.UploadType,
                FileType = fileType,
                RIC = ric,
                SubmitDate = DateTime.Now,
                ReportDate = DateTime.Parse(ippFile.ReportDate),
                SubmitterID = ippFile.SubmitterID,
                Tag = ippFile.Tag == null ? "" : ippFile.Tag.Replace(';', '|').Replace('；', '|'),
                TitleCn = ippFile.TitleCn,
                TitleEn = ippFile.TitleEn,
                TopicID = Convert.ToInt32(ippFile.Topic),
                Source = ippFile.Source,
                FileName = ippFile.FileName,
                DisplayOrder = ippFile.DisplayOrder
            };

            if (file != null && file.ContentLength > 0)
            {
                fileCreate.Doc = new byte[file.ContentLength];
                file.InputStream.Read(fileCreate.Doc, 0, file.ContentLength);
            }

            fileCreate.Status = submit == Resources.IPP.IPP_Save ? 0 : -1; //-1: publish
            IPPRepository.UploadFile(fileCreate);

            switch (ippFile.PreviousRequest)
            {
                case "fileBrowser":
                    return RedirectToAction("FileBrowser", new { id = fileCreate.TopicID });
                case "prePublish":
                    return Redirect("~/ipp/MyDocument/prePublish");
                case "published":
                    return Redirect("~/ipp/MyDocument/published");
                case "approved":
                    return Redirect("~/ipp/MyDocument/approved");
                default:
                    return RedirectToAction("FileBrowser", new { id = fileCreate.TopicID });
            }
        }

        public ActionResult GetModule()
        {
            var modules = IPPRepository.GetModuleList();
            var options = (from m in modules
                           select new { Id = m.ID, Name = CultureHelper.IsEnglishCulture() ? m.NAMEEN : m.NAMECN }).ToList();
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetModuleWithAll()
        {
            var modules = IPPRepository.GetModuleList();
            var options = (from m in modules
                select new {Id = m.ID, Name = CultureHelper.IsEnglishCulture() ? m.NAMEEN : m.NAMECN}).ToList();
            options.Insert(0,new { Id = 0, Name = Global.Type_All });
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTopicByModule(int id)
        {
            var topics = IPPRepository.GetTopicListByModuleId(id);
            var options = (from t in topics
                           select new { Id = t.ID, Name = t.DisplayName }).ToList();
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTopicByModuleWithAll(int id)
        {
            var topics = IPPRepository.GetTopicListByModuleId(id);
            var options = (from t in topics
                           select new { Id = t.ID, Name = t.DisplayName }).ToList();
            options.Insert(0, new { Id = 0, Name = Global.Type_All });
            return Json(options, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Topic Editor
        public ActionResult TopicEditor(int? id)
        {
            var topicCreate = id == null ? new TopicCreate() : IPPRepository.GetTopicById((int)id);
            IPPTopic ippTopic;

            topicCreate = topicCreate ?? new TopicCreate();
            var moduleItems = new List<SelectListItem>();
            IEnumerable<MODULEINFO> modules = IPPRepository.GetModuleList();
            foreach (var m in modules)
                moduleItems.Add(new SelectListItem { Value = m.ID.ToString(), Selected = m.ID == topicCreate.ModuleID ? true : false, Text = CultureHelper.IsEnglishCulture() ? m.NAMEEN : m.NAMECN });

            ippTopic = new IPPTopic
            {
                ID = topicCreate.ID,
                DescriptionCn = topicCreate.DescriptionCn,
                DescriptionEn = topicCreate.DescriptionEn,
                Tag = topicCreate.Tag,
                NameCn = topicCreate.NameCn,
                NameEn = topicCreate.NameEn,
                ModuleItems = moduleItems,
                Approver = string.IsNullOrEmpty(topicCreate.Approver) ? UserSettingHelper.GetEikonUserID(Request) : topicCreate.Approver,
                IsApprove = topicCreate.IsApprove,
                IsDirectDelete = topicCreate.IsDirectDelete,
                IsInternalApprove = topicCreate.IsInternalApprove,
                ModuleID = topicCreate.ModuleID.ToString(),
                RMLink = topicCreate.RMLink,
                Thumbnail = topicCreate.Thumbnail,
                ImageName = topicCreate.ImageName
            };

            return View(ippTopic);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UploadTopic(IPPTopic ippTopic, HttpPostedFileBase image)
        {
            var tmp = ippTopic.Approver.Replace('；', ';').Trim();
            if (tmp[tmp.Length - 1] != ';')
                ippTopic.Approver += ";";

            var topicCreate = new TopicCreate
            {
                ID = ippTopic.ID,
                DescriptionCn = ippTopic.DescriptionCn,
                DescriptionEn = ippTopic.DescriptionEn,
                IsApprove = ippTopic.IsApprove,
                IsInternalApprove = ippTopic.IsInternalApprove,
                IsDirectDelete = ippTopic.IsDirectDelete,
                Approver = ippTopic.Approver,
                RMLink = ippTopic.RMLink,
                Tag = ippTopic.Tag,
                NameCn = ippTopic.NameCn,
                NameEn = ippTopic.NameEn,
                ModuleID = Convert.ToInt32(ippTopic.ModuleID),
                Creater = UserSettingHelper.GetEikonUserID(Request),
                ImageName = ippTopic.ImageName
            };

            if (image != null && image.ContentLength > 0)
            {
                topicCreate.Thumbnail = new byte[image.ContentLength];
                image.InputStream.Read(topicCreate.Thumbnail, 0, image.ContentLength);
            }

            IPPRepository.UploadTopic(topicCreate);
            return Redirect("~/ipp/MyDocument/topicManagement");
        }

        #endregion

        #region File Browser
        public ActionResult SingleFileBrowser(int? topicId, long? fileId)
        {
            if (!topicId.HasValue || !fileId.HasValue)
            {
                ViewBag.Order = "Hits";
                ViewBag.Asc = false;
                ViewBag.FileID = 0;
                ViewBag.PageTitle = Resources.IPP.IPP_TopHits;
                ViewBag.TruncatedPageTitle = HtmlUtil.Truncate(ViewBag.PageTitle, 30);
            }
            else
            {
                ViewBag.Order = "";
                ViewBag.Asc = false;
                ViewBag.FileID = fileId;
                var file = IPPRepository.GetFileByFileID(fileId.Value);
                ViewBag.PageTitle = CultureHelper.IsEnglishCulture() ? file.TitleEn : file.TitleCn;
                ViewBag.TruncatedPageTitle = HtmlUtil.Truncate(ViewBag.PageTitle, 30);
            }
            ViewBag.TopicID =  0;
            ViewBag.queryStr =  "";
            ViewBag.ChatroomUrl = "";
            ViewBag.Following = 0;
            return View("FileBrowser");
        }

        public ActionResult FileBrowser(int? id, string q)
        {
            if (id.HasValue || !string.IsNullOrEmpty(q))
            {
                ViewBag.Order = "";
                ViewBag.Asc = false;
                ViewBag.FileID = 0;
                ViewBag.TopicID = id ?? 0;
                ViewBag.queryStr = q ?? "";
                if (id.HasValue)
                {
                    var topic = IPPRepository.GetTopicByID(id.Value,UserSettingHelper.GetEikonUserID(Request));
                    ViewBag.PageTitle = CultureHelper.IsEnglishCulture() ? topic["NAMEEN"] : topic["NAMECN"];
                    ViewBag.TruncatedPageTitle = HtmlUtil.Truncate(ViewBag.PageTitle, 30);
                    ViewBag.ChatroomUrl = topic["RMLINK"];
                    ViewBag.Following = topic["FOLLOWING"];
                }
                else
                {
                    ViewBag.PageTitle = q;
                    ViewBag.TruncatedPageTitle = HtmlUtil.Truncate(ViewBag.PageTitle, 30);
                    ViewBag.ChatroomUrl = "";
                    ViewBag.Following = 0;
                }
            }
            else
            {
                return RedirectToAction("IPPHome");
            }
            return View();
        }

        public ActionResult GetTopicName(int id)
        {
            var topic = IPPRepository.GetTopicByID(id, UserSettingHelper.GetEikonUserID(Request));
            var data = new
            {
                Name = CultureHelper.IsEnglishCulture() ? topic["NAMEEN"] : topic["NAMECN"],
                TruncatedName = HtmlUtil.Truncate(CultureHelper.IsEnglishCulture() ? topic["NAMEEN"].ToString() : topic["NAMECN"].ToString(), 30),
                ChatroomUrl = topic["RMLINK"],
                Following = topic["FOLLOWING"]
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FollowFile(int id)
        {
            IPPRepository.FollowFile(UserSettingHelper.GetEikonUserID(Request), id);
            return Content("");
        }

        public ActionResult FollowTopic(int id)
        {
            IPPRepository.FollowTopic(UserSettingHelper.GetEikonUserID(Request), id);
            return Content("");
        }

        public ActionResult QueryFiles(int id, string q, string title, string author, DateTime? startDate, DateTime? endDate, bool enableDate, string description, string source, int pageNo, int pageSize, string order, bool isHtml = false, long fileID = 0)
        {
            var isEn = CultureHelper.IsEnglishCulture();
            int total;
            var table = IPPRepository.QueryFiles(id, q, title, author, startDate, endDate, enableDate, description, source, pageNo, pageSize, order, isEn, UserSettingHelper.GetEikonUserID(Request),out total, fileID);

            var data = new { Data = DatatableToJson(table), CurrentPage = pageNo, PageSize = pageSize, Total = total };
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion

        public ActionResult GetTopicByKeyWord(string key)
        {
            if(string.IsNullOrWhiteSpace(key))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            var data = IPPRepository.QueryTopic(key, CultureHelper.IsEnglishCulture());

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFileInfo(long id)
        {
            var file = IPPRepository.DownloadFile(id, UserSettingHelper.GetEikonUserID(Request));

            switch (file.UPLOADTYPE)
            {
                case ConstValues.IPP_Upload_File:
                    return Json( new {type = "file", url = Url.Action("DownloadFile",new {id=id})}, JsonRequestBehavior.AllowGet);
                case ConstValues.IPP_Upload_Ric:
                    return Json(new {type = "ric", ricType = file.FILETYPE,ricVal = file.RIC}, JsonRequestBehavior.AllowGet);
                case ConstValues.IPP_Upload_WebSite:
                    return Json(new { type = "url", url = file.RIC }, JsonRequestBehavior.AllowGet);
                default:
                    return HttpNotFound();
            }
        }

        public ActionResult DownloadFile(long id)
        {
            var file = IPPRepository.DownloadFile(id, string.Empty);
            switch (file.UPLOADTYPE)
            {
                case ConstValues.IPP_Upload_File:
                    var fileData = IPPRepository.GetFileDataByID(id);
                    if (fileData==null) return HttpNotFound();
                    if (fileData.LongLength > 0)
                    {
                        var contentType = MimeHelper.GetMimeTypeByExt(file.FILETYPE);
                        return File(fileData, contentType, Resources.Global.DownloadFileName + "." + file.FILETYPE);
                    }
                    else
                    {
                        var script = @"<script language='javascript' type='text/javascript'>
                                        alert('wrong');
                            </script>";
                        return Content(script);
                    }
                case ConstValues.IPP_Upload_Ric:
                    var script2 = @"<script language='javascript' type='text/javascript'>
                                    var data = {
                                        target: 'popup',
                                        // open a popup window
                                        location: {
                                            x: 200,
                                            y: 100,
                                            width: 600,
                                            height: 400
                                        },
                                        name: 'Quote Object',
                                        // open a Quote Object
                                        entities: [
                                            {
                                                type: '" + file.FILETYPE + @"',
                                                'RIC': '" + file.RIC + @"'
                                            }
                                        ]
                                    };
                                    JET.navigate(data);
                                    </script>";
                    return Content(script2);       
                case ConstValues.IPP_Upload_WebSite:
                    return Redirect(file.RIC);
                    
                default:
                    return JavaScript("alert('wrong');");
                    
            }
        }

        #region Topic management

        public ActionResult GetTopicsByCreater(int moduleID,int pageNo,int pageSize,bool isHtml = false)
        {
            int total;
            var table = IPPRepository.GetTopicsByCreater(moduleID, UserSettingHelper.GetEikonUserID(Request), pageNo,
                pageSize, out total);
            var data = new { Data = DatatableToJson(table), CurrentPage = pageNo, PageSize = pageSize, Total = total };
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTopic(int id)
        {
            var result = IPPRepository.DeleteTopic(id,UserSettingHelper.GetEikonUserID(Request));
            var message = "";
            switch (result)
            {
                case 1:
                    message = Resources.IPP.IPP_FileExist;
                    break;
                case 2:
                    message = "unknown error, try again later";
                    break;
                default:
                    message = "";
                    break;
            }
            var data = new {Result = result, Message = message};
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region favorite file

        public ActionResult GetFavoriteFiles(int moduleID,int topicID, string title, string order, int pageNo, int pageSize, bool isHtml = false)
        {
            var isEn = CultureHelper.IsEnglishCulture();
            int total;
            var table = IPPRepository.GetFavoriteFiles(moduleID, topicID, UserSettingHelper.GetEikonUserID(Request), title, order,
                pageNo, pageSize, isEn, out total);
            var data = new { Data = DatatableToJson(table), CurrentPage = pageNo, PageSize = pageSize, Total = total };
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Favorite topics

        public ActionResult GetFavoriteTopics(int moduleID, string userID, string topic, string order, int pageNo, int pageSize, bool isHtml = false)
        {
            var isEn = CultureHelper.IsEnglishCulture();
            int total;
            var table = IPPRepository.GetFavoriteTopics(moduleID, UserSettingHelper.GetEikonUserID(Request), topic, order, pageNo, pageSize, isEn, out total);
            var data = new { Data = DatatableToJson(table), CurrentPage = pageNo, PageSize = pageSize, Total = total };
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region publish and approve

        public ActionResult GetFilesByStatus(int moduleID, int topicID, string key, int pageNo, int pageSize, int status, bool isHtml = false)
        {
            var isEn = CultureHelper.IsEnglishCulture();
            int total;
            var table = IPPRepository.GetFilesByStatus(moduleID, topicID, UserSettingHelper.GetEikonUserID(Request), key, pageNo, pageSize, isEn, status, out total);
            var data = new { Data = DatatableToJson(table), CurrentPage = pageNo, PageSize = pageSize, Total = total };
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFilesForApproval(int moduleID, int topicID, string key, int pageNo, int pageSize, int status, bool isHtml = false)
        {
            var isEn = CultureHelper.IsEnglishCulture();
            int total;
            var table = IPPRepository.GetFilesForApproval(moduleID, topicID, UserSettingHelper.GetEikonUserID(Request), key, pageNo, pageSize, isEn, status, out total);
            var data = new { Data = DatatableToJson(table), CurrentPage = pageNo, PageSize = pageSize, Total = total };
            return isHtml ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateFileStatus(int id, string operation)
        {
            int status = 1;
            switch(operation)
            {
                case "publish":
                    status = 1;
                    break;
                case "approve":
                    status = 2;
                    break;
                case "reject":
                    status = 3;
                    break;
                case "delete":
                    return DeleteFileByID(id);
                default:
                    break;
            }

            int ret = IPPRepository.UpdateFileStatus(id, status, UserSettingHelper.GetEikonUserID(Request));
            string message = "";

            if (ret == 1)
                message = Resources.IPP.IPP_Already_Approve;    

            var data = new { Result = ret, Message = message };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFileByID(int id)
        {
            var result = IPPRepository.DeleteFile(id,UserSettingHelper.GetEikonUserID(Request));
            string message = "";
            if (result == 1)
                message = "Error.";
            var data = new { Result = result, Message = message };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region my document
        /// <summary>
        /// Mies the document.
        /// </summary>
        /// <returns></returns>
        public ActionResult MyDocument()
        {
            ViewBag.InternalUser = UserSettingHelper.IsInternalUser(Request);
            return View();
        }

        /// <summary>
        /// Mies the document partial.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public ActionResult MyDocumentPartial(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) return new EmptyResult();
            var pageId = id.Trim().ToLower();
            if (pageId == "preapprove" || pageId == "approved" || pageId == "topicmanagement")
            {
                if (! UserSettingHelper.IsInternalUser(Request))
                {
                    return new EmptyResult();
                }
            }
            return PartialView(id);
        }

        #endregion
        public object DatatableToJson(DataTable dt)
        {
            ArrayList arrayList = new ArrayList();
            foreach (DataRow dataRow in dt.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();  
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    dictionary.Add(dataColumn.ColumnName, UIGenerator.FormatJsonCellValue(dataRow[dataColumn.ColumnName], dataRow[dataColumn.ColumnName].GetType().ToString()));
                }
                arrayList.Add(dictionary); 
            }

            return arrayList; 
        }

        

        #region Rating
        public PartialViewResult RatingDailog(int id)
        {
            ViewBag.ID = id;
            ViewBag.Rate = IPPRepository.GetUserFileRate(id, UserSettingHelper.GetUserId(Request));
            var xAxisvalue = string.Join(",", GetRatingStatistics(id));
            ViewBag.X = string.Format("[{0}]", xAxisvalue);
            return PartialView();
        }

        private int[] GetRatingStatistics(int id)
        {
            var ratingData = IPPRepository.GetFileRatingStatistics(id);
            var dict = new Dictionary<int, int>();
            for (var i = 1; i < 6; i++)
            {
                dict.Add(i, 0);
            }
            foreach (DataRow row in ratingData.Rows)
            {
                var rate = Convert.ToInt32(row["RANK"]);
                var rank = Convert.ToInt32(row["COUNT"]);
                if (dict.ContainsKey(rate))
                {
                    dict[rate] = rank;
                }
            }
            return dict.Values.ToArray();
        }

        public JsonResult RateFile(int fileID, int rate)
        {
            IPPRepository.RateFile(fileID, UserSettingHelper.GetUserId(Request), rate);
            return Json(GetRatingStatistics(fileID));
        }
        #endregion
    }
}
