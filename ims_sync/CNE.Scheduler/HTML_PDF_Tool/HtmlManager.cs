using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace CNE.Scheduler.HTML_PDF_Tool
{
    public class HtmlManager
    {
        /// <summary>
        /// reorgnize html
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SliceHtml(string path, out string title, out string author)
        {
            HtmlDocument document = new HtmlDocument();
            document.Load(path, Encoding.UTF8);

            var direct = Path.GetDirectoryName(path);
            var fname = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);

            //cut
            var cutList = new List<string>();
            cutList.Add("html/body/div[1]/div[@class='top clearfix']");
            cutList.Add("html/body/div[1]/div[@class='header']");
            cutList.Add("html/body/div[1]/div[@class='cramb']");
            cutList.Add("html/body/div[1]/div[@class='per_bank_login']");
            cutList.Add("html/body/div[1]/div[last()]");
            cutList.Add("//script");
            cutList.Add("//div[@class='container']/div[@class='slider']");
            cutList.Add("//div[@class='container']/div[@class='content con_area']/div[@class='function']");

            cutList.All(o =>
            {
                var nodes = document.DocumentNode.SelectNodes(o);
                nodes.All(p =>
                {
                    p.Remove();
                    return true;
                });
                return true;
            });

            //get title
            var titleNode = document.DocumentNode.SelectSingleNode("//h2[@class='title']");
            title = titleNode.InnerText.Trim();
            //get author
            var auNode = document.DocumentNode.SelectSingleNode("//div[@class='sub_con']/p[1]");
            author = auNode.InnerText.Trim();


            var newpath = Path.Combine(direct, fname + "-Sliced" + ext);
            document.Save(newpath);
            return newpath;
        }

        /// <summary>
        /// read web ,get url for download
        /// </summary>
        /// <returns></returns>
        public static string GetUrl(out string title)
        {
            var parentPage = ConfigurationManager.AppSettings["BOC_URL"];
            WebRequest request = WebRequest.Create(parentPage);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            HtmlDocument document = new HtmlDocument();
            document.Load(stream, Encoding.UTF8);

            var node = document.DocumentNode.SelectSingleNode("//div[@class='news']/ul/li[1]/a");
            title = node.InnerText.Trim();
            var urlR = node.Attributes[0].Value;
            var tagUrl = parentPage + urlR;
            return tagUrl;
        }


        public static bool DelFiles(string pdfpath)
        {
            try
            {
                var dictStr = Path.GetDirectoryName(pdfpath);

                var dict = new DirectoryInfo(dictStr);

                foreach (var item in dict.GetFiles())
                {
                    item.Delete();
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
