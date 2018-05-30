using System;
using System.Configuration;
using System.Diagnostics;
using System.Web.Mvc;

namespace VAV.Web.Controllers
{
    public class ImageExportController : Controller
    {
        //BATIK_PATH,$typeString,$outfile,$width,$tempName
        private const string BatikCmdFormat = @"-jar {0} {1} -d {2} {3} {4}";

        [HttpPost, ValidateInput(false)]
        public ActionResult TranslateSvg(string type, string svg, string filename, int width = 0)
        {
            var batikPath = ConfigurationManager.AppSettings["BatikPath"];
            var tempId = Guid.NewGuid();
            var svgFile = HttpContext.Server.MapPath(string.Format("{0}{1}.svg", "~/Cache/", tempId));
            var typeString = string.Empty;
            string ext;
            switch (type)
            {
                case @"image/png":
                    typeString = @"-m image/png";
                    ext = "png";
                    break;
                case @"image/jpeg":
                    typeString = "-m image/jpeg";
                    ext = "jpg";
                    break;
                case @"application/pdf":
                    typeString = "-m application/pdf";
                    ext = "pdf";
                    break;
                case @"image/svg+xml":
                    ext = "svg";
                    break;
                default:
                    ext = "txt";
                    break;
            }

            var outputFile = HttpContext.Server.MapPath(string.Format("{0}{1}.{2}", "~/Cache/", tempId, ext));
            var outputPath = HttpContext.Server.MapPath("~/Cache/");
            if (System.IO.File.Exists(svgFile))
            {
                System.IO.File.Delete(svgFile);
            }
            System.IO.File.WriteAllText(svgFile, svg);

            var widthString = string.Empty;
            if (width > 0)
            {
                widthString = "-w " + width;
            }
            var cmd = string.Format(BatikCmdFormat, batikPath, typeString, outputPath, widthString, svgFile);
            // Start the child process.
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "java",
                    Arguments = cmd,
                    CreateNoWindow = true
                }
            };
            // Redirect the output stream of the child process.
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            //var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var targetFile = string.Format("{0}.{1}", filename, ext);
            var b = System.IO.File.ReadAllBytes(outputFile);
            System.IO.File.Delete(svgFile);
            System.IO.File.Delete(outputFile);
            var result = new FileContentResult(b, type) { FileDownloadName = targetFile };
            return result;
        }

    }
}
