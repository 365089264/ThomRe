using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using VAV.Web.Extensions;
using Microsoft.Practices.Unity;
using VAV.DAL.ResearchReport;
using VAV.Web.Common;

namespace VAV.Web.Controllers
{
    public class PdfViewController : Controller
    {
        //
        // GET: /PdfView/
        [Dependency]
        public ResearchReportRepository _repository { get; set; }
        private static PdfConverter pdf = new PdfConverter();
        private void CreatePicDir(int dID, string path)
        {

            if (!Directory.Exists(path))
            {

                //string pdfPath = Server.MapPath("/Cache/PDF/" + dID + "/");
                Directory.CreateDirectory(path);
                string pdfFilePath = Path.Combine(path, "test.pdf");
                if (!System.IO.File.Exists(pdfFilePath))
                {
                    var file = _repository.GetFileDataById(dID);
                    System.IO.File.WriteAllBytes(pdfFilePath, file == null ? null : file.Content);
                }

                lock (pdf)
                {
                    pdf.pdf2jpg(pdfFilePath, dID, path);
                }

            }
        }
        public ActionResult PdfView(int dID)
        {
            var file = _repository.GetFileDataById(dID);
            //var fileDetail = _repository.GetFileDetailById(dID);
            if (file == null)
                return HttpNotFound();
           // var contentType = MimeHelper.GetMimeFromBytes(file.Content);
            string path = Path.Combine(Server.MapPath("/Cache/image/"), dID.ToString());
            CreatePicDir(dID, path);
            string[] files = Directory.GetFiles(path); 
            List<string > picFiles=new List<string> ();
            for (int i=0;i <files .Length ;i ++) 
            {
                 if (Path.GetExtension(files[i]) == ".jpg")
                 {
                     picFiles.Add (files[i]);
                 }
            }
            List<string> list = new List<string>();
            for (int i = 0; i < picFiles.Count ; i++)
            {
                //C:\Users\UC171530\Desktop\SourceCode\trunk\Src\VAV\VAV.Web\Report\image\594\test001.jpg 
               
                    string str = "/PdfView/GetPic?fileID=" + dID + "&picNo=" + (i + 1);
                    list.Add(str);
                
            }
            return PartialView("~/Views/ResearchReport/PdfViewer.cshtml", list);
        }

        public ActionResult GetPic(int fileID, int picNo)
        {
            string path = Path.Combine(Server.MapPath("/Cache/image/"), fileID.ToString());
            if (!Directory.Exists(path))
            {
                CreatePicDir(fileID, path);
            }
            string picPath = Path.Combine(path, "test" + picNo + ".jpg");

            byte[] picbytes = null;
            using (FileStream fs = new FileStream(picPath, FileMode.Open, FileAccess.Read))
            {
                picbytes = new byte[fs.Length];
                //System.IO.File.WriteAllBytes(picPath, picbytes);
                fs.Read(picbytes, 0, picbytes.Length);
            }
            return File(picbytes, "image/jpeg");
        }
    }
}
