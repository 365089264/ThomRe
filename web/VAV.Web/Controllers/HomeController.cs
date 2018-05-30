using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using SolrNet.Commands.Parameters;
using VAV.DAL.Services;
using VAV.Model.Data;
using VAV.Web.Common;
using VAV.Web.ViewModels.Home;
using VAV.Web.ViewModels.Upload;
using VAV.Web.Localization;
using SolrNet;
using Microsoft.Practices.ServiceLocation;
using System.Web;
using VAV.DAL.Report;
using System;
using System.Collections.Generic;
using System.Data;
using VAV.Web.Extensions;
using System.Threading;


namespace VAV.Web.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// _menuService
        /// </summary>
        [Dependency]
        public MenuService MenuService { get; set; }

        [Dependency]
        public ReportService ReportService { get; set; }

        [Dependency]
        public PartnersReportRepository CmaRepository { get; set; }

        [Localization]
        public ActionResult Index()
        {
            CultureHelper.UpdateCultureCookie(Response);
            ThemeHelper.UpdateLocalThemeCookie(Request, Response);
            var menu = MenuService.GetMenu();
            var defaultReport = MenuService.GetDefaultReport();
            return View("Index", new IndexViewModel(menu, defaultReport));
        }

        [Localization]
        public ActionResult Commodity()
        {
            CultureHelper.UpdateCultureCookie(Response);
            ThemeHelper.UpdateLocalThemeCookie(Request, Response);
            var menu = MenuService.GetMenu();
            var cneMenu = menu.Children.Find(x => x.Id == 11000);
            var firstPage = cneMenu.Children.Find(x => x.Id == 14006);
            return View(new IndexViewModel(cneMenu, firstPage));
        }

        [Localization]
        public ActionResult RefreshTreeView(int id)
        {
            var menuNode = MenuService.GetMenuNodeByNodeId(id);
            return PartialView("_Tree", new TreeViewModel(menuNode));
        }

        public ActionResult ColumnChooser()
        {
            return View();
        }

        [Localization]
        public ActionResult External(int id)
        {
            CultureHelper.UpdateCultureCookie(Response);
            ThemeHelper.UpdateLocalThemeCookie(Request, Response);
            var reportInfo = ReportService.GetReportInfoById(id);
            ViewBag.ID = id;
            ViewBag.DisplayName = reportInfo.DisplayName;
            ViewBag.Trace = reportInfo.TraceName;
            if (string.IsNullOrWhiteSpace(ViewBag.Trace))
            {
                ViewBag.ID = -1;
                ViewBag.DisplayName = "Sorry";
            }
            return View();
        }

        [Localization]
        public ActionResult AutoSuggest(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<SearchContentViewModel>>();
            var results = solr.Query(new SolrQuery(key), new QueryOptions() { Rows = 15 });
            var t = results.Select(r => r).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [JsonpFilter]
        public ActionResult AutoSuggestForBond(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<AutoSuggestViewModel>>();
            var results = solr.Query(new SolrQuery(key), new QueryOptions() { Rows = 15 });

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult Homepage(int id = 0)
        {
            var model = new HomepageViewModel(id);
            model.Initialization();
            return PartialView(model);
        }

        [Localization]
        public ActionResult DownloadHomeItem(int id)
        {
            var file = CmaRepository.GetHomeItemFileData(id);
            if (file == null)
                return HttpNotFound();
            if (file.Content == null)
                return HttpNotFound();
            var contentType = MimeHelper.GetMimeTypeByExt(file.FileType);
            return File(file.Content, contentType,
                Resources.Global.DownloadFileName + "." + file.FileType);
        }

        [Localization]
        public ActionResult HomeItemEditor(string ID)
        {
            CultureHelper.UpdateCultureCookie(Response);
            ThemeHelper.UpdateLocalThemeCookie(Request, Response);

            var uploadItem = new UploadItemViewModel();

            var modules = CmaRepository.GetHomeModules();
            var selectedModuleId = modules.FirstOrDefault().ID;
            var moduleItems = new List<SelectListItem>();


            uploadItem.UploadType = Resources.IPP.IPP_File;
            uploadItem.UploadTypeItems = HtmlUtil.CookSelectOptions("Home_UploadType");
            uploadItem.SubmitDate = DateTime.Now.ToString("yyyy-MM-dd");
            uploadItem.IsVisible = true;
            if (!string.IsNullOrEmpty(ID))
            {
                DataTable dt = CmaRepository.GetItemFile(ID);
                uploadItem.Id = Convert.ToInt32(dt.Rows[0]["ID"]);
                switch (dt.Rows[0]["TYPE"].ToString())
                {
                    case "File":
                        uploadItem.UploadType = "Upload_File";
                        uploadItem.FileName = Resources.Global.DownloadFileName + "." + dt.Rows[0]["FILETYPE"];
                        break;
                    case "Url":
                        uploadItem.UploadType = "Upload_Website";
                        uploadItem.Url = dt.Rows[0]["TYPEVALUE"].ToString();
                        break;
                    case "Chart":
                        uploadItem.UploadType = "Upload_RIC_Chart";
                        uploadItem.Ric = dt.Rows[0]["TYPEVALUE"].ToString();
                        break;
                    case "Quote":
                        uploadItem.UploadType = "Upload_RIC_Quote";
                        uploadItem.Ric = dt.Rows[0]["TYPEVALUE"].ToString();
                        break;
                    case "QuoteList":
                        uploadItem.UploadType = "Upload_RIC_QuoteList";
                        uploadItem.Ric = dt.Rows[0]["TYPEVALUE"].ToString();
                        break;
                    case "News":
                        uploadItem.UploadType = "Upload_RIC_News";
                        uploadItem.Ric = dt.Rows[0]["TYPEVALUE"].ToString();
                        break;
                    case "Rmlink":
                        uploadItem.UploadType = "Upload_RMLink";
                        uploadItem.RMLink = dt.Rows[0]["TYPEVALUE"].ToString();
                        break;

                }
                uploadItem.IsVisible = (dt.Rows[0]["ISVALID"].ToString() == "1");
                uploadItem.Module = dt.Rows[0]["MODULEID"].ToString();
                selectedModuleId = Convert.ToDecimal(uploadItem.Module);
                uploadItem.TitleCn = dt.Rows[0]["NAMECN"].ToString();
                uploadItem.TitleEn = dt.Rows[0]["NAMEEN"].ToString();
                if (dt.Rows[0]["TYPEPARAM"] != null)
                    uploadItem.TypeParam = dt.Rows[0]["TYPEPARAM"].ToString();

            }
            foreach (var m in modules)
            {
                moduleItems.Add(new SelectListItem { Selected = m.ID == selectedModuleId ? true : false, Value = m.ID.ToString(), Text = CultureHelper.IsEnglishCulture() ? m.NAMEEN : m.NAMECN });
            }
            uploadItem.ModuleItems = moduleItems;

            return View(uploadItem);
        }

        [Localization]
        public ActionResult HomeItemList()
        {
            return View();
        }
       
        public ActionResult GetHomeItemList(int moduleId, string itemName, int startPage=1, int pageSize=50)
        {
            int total;
            var dataTable = CmaRepository.GetHomeItems(moduleId, itemName, startPage, pageSize, out total);
            return Json(BuildHomeItemTable(dataTable, total, startPage, pageSize), JsonRequestBehavior.AllowGet);
        }
        private JsonTable BuildHomeItemTable(DataTable table, int total, int startPage, int pageSize)
        {
            var jTable = new JsonTable();
            jTable.CurrentPage = startPage;
            jTable.PageSize = pageSize;
            jTable.Total = total;
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "MODULENAME", Name = "MODULENAME",ColumnType = "text"});
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "NAMECN", Name = "NAMECN", ColumnStyle = "width: 200px; overflow: hidden; text-overflow: ellipsis; display: inline-block;" ,ColumnType = "text"});
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "NAMEEN", Name = "NAMEEN", ColumnStyle = "width: 200px; overflow: hidden; text-overflow: ellipsis; display: inline-block;", ColumnType = "text" });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "TYPE", Name = "TYPE", ColumnType = "text" });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "TYPEVALUE", Name = "TYPEVALUE", ColumnStyle = "width: 200px; overflow: hidden; text-overflow: ellipsis; display: inline-block;", ColumnType = "text" });
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "MTIME", Name = "MTIME", ColumnType = "text"});
            jTable.ColumTemplate.Add(new JsonColumn { ColumnName = "ID", Name = "ID" });
            for (int j=0;j<table.Rows.Count;j++)
            {
                var currentRow = new Dictionary<string, string>();
                for (int i = 0; i < jTable.ColumTemplate.Count; i++)
                {
                    currentRow.Add(jTable.ColumTemplate[i].ColumnName, table.Rows[j][jTable.ColumTemplate[i].ColumnName].ToString());
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }
        public int DeleteHomeItem(int id)
        {
            var result = CmaRepository.DeleteHomeItem(id);
            return result;
        }


        [HttpPost]
        [Localization]
        public ActionResult UploadHomeItem(UploadItemViewModel uploadItem, HttpPostedFileBase file)
        {
            string uploadType = "";
            string fileType = "";
            byte[] doc = null;

            switch (uploadItem.UploadType)
            {
                case "Upload_File":
                    uploadType = "File";
                    break;
                case "Upload_Website":
                    uploadType = "Url";
                    break;
                case "Upload_RIC_Chart":
                    uploadType = "Chart";
                    break;
                case "Upload_RIC_Quote":
                    uploadType = "Quote";
                    break;
                case "Upload_RIC_QuoteList":
                    uploadType = "QuoteList";
                    break;
                case "Upload_RIC_News":
                    uploadType = "News";
                    break;
                case "Upload_RMLink":
                    uploadType = "Rmlink";
                    break;

            }

            if (file != null && file.ContentLength > 0)
            {
                var index = file.FileName.LastIndexOf(".");
                fileType = file.FileName.Substring(index + 1);
                doc = new byte[file.ContentLength];
                file.InputStream.Read(doc, 0, file.ContentLength);
            }
            int id = uploadItem.Id == null ? 0 : Convert.ToInt32(uploadItem.Id);
            CmaRepository.UploadFile(id, uploadItem.Module, uploadItem.TitleCn, uploadItem.TitleEn,
                uploadItem.DescriptionCn, uploadItem.DescriptionEn, uploadType, uploadItem.UploadTypeValue, uploadItem.TypeParam, uploadItem.IsVisible, uploadItem.Submiter, DateTime.Parse(uploadItem.SubmitDate), fileType, doc);

            return RedirectToAction("HomeItemList");
        }
    }
}
