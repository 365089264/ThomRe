using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Pechkin;
using Pechkin.Synchronized;

namespace CNE.Scheduler.HTML_PDF_Tool
{
    public class PdfSaverByPechkin
    {
       
        SynchronizedPechkin pechkinTool = null;
        //Pechkin.SimplePechkin pechkinTool = null;
        ObjectConfig config = null;

        public PdfSaverByPechkin()
        {
            Inital();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlPath">for saving by pechkin,this parameter can be set with local path or url</param>
        /// <param name="pdfPath"></param>
        /// <returns></returns>
        public bool Save(string htmlPath, string pdfPath)
        {

            lock (this)
            {
                try
                {
                    config.SetPageUri(htmlPath);
                    Utility.LocalIOHelper.CheckDirectoryExist(pdfPath);
                    byte[] buf = null;
                    buf = pechkinTool.Convert(config);
                    if (buf == null)
                    {
                        return false;
                    }

                    File.WriteAllBytes(pdfPath, buf);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        private void Inital()
        {
            pechkinTool = new SynchronizedPechkin(new GlobalConfig()
                //pechkinTool = new SimplePechkin(new GlobalConfig()
                          //.SetImageQuality(500)
                          .SetLosslessCompression(false)
                          .SetPaperSize(PaperKind.Tabloid));

            config = new ObjectConfig().SetPrintBackground(true)
                //.SetProxyString(String.Concat(StaticStrings.ProxyIP, ":", StaticStrings.ProxyPort))
            .SetAllowLocalContent(true)
            .SetCreateExternalLinks(false)
            .SetCreateForms(false)
            .SetCreateInternalLinks(false)
            .SetErrorHandlingType(ObjectConfig.ContentErrorHandlingType.Ignore).SetFallbackEncoding(Encoding.ASCII)
            .SetIntelligentShrinking(false)
            .SetRenderDelay(2000)
            .SetRunJavascript(true)
            .SetIncludeInOutline(true)
            .SetZoomFactor(1)
            .SetLoadImages(true);
        }

        public bool Save(string url, string pdfPath, string cookie)
        {
            string tempHtmlPath;

            lock (this)
            {
                var dir = Utility.LocalIOHelper.CheckDirectoryExist(pdfPath);
                var rv = SaveWebPage(url, null, cookie, out tempHtmlPath);
                return rv == StaticStrings.ResultFailed ? false : Save(tempHtmlPath, pdfPath);
            }
        }

        public bool SaveByHtml(string url, out string pdfPathOut, out string title, out string author)
        {
            string tempHtmlPath;

            lock (this)
            {

                var rv = SaveWebPage(url, string.Empty, string.Empty, out tempHtmlPath);
                //cut html 
                var newpath = HtmlManager.SliceHtml(tempHtmlPath, out title, out author);
                var pdfPath = Path.Combine(Path.GetDirectoryName(tempHtmlPath), title + ".pdf");
                pdfPathOut = pdfPath;
                return rv == StaticStrings.ResultFailed ? false : Save(newpath, pdfPath);
            }
        }

        public bool SaveByHtml(string html, string url, string pdfPath, string cookie)
        {
            string tempHtmlPath;

            lock (this)
            {
                var dir = Utility.LocalIOHelper.CheckDirectoryExist(pdfPath);
                var rv = SaveWebPage(url, html, cookie, out tempHtmlPath);
                return rv == StaticStrings.ResultFailed ? false : Save(tempHtmlPath, pdfPath);
            }
        }

        public static string SaveWebPage(string url, string html, string cookie, out string tempHtmlPath)
        {
            tempHtmlPath = Path.Combine(Path.GetTempPath(), "ips_temp", String.Format("{0}{1}", Utility.GetHashName(url), ".html"));
            if (File.Exists(tempHtmlPath)) File.Delete(tempHtmlPath);
            Utility.LocalIOHelper.CheckDirectoryExist(Path.Combine(Path.GetTempPath(), "ips_temp"), true);
            return DownLoadRelevantAssets(url, Path.GetDirectoryName(tempHtmlPath), cookie, html);
        }

        private static string DownLoadRelevantAssets(string pageUrl, string localDirectory, string cookie, string soucreHtml = "")
        {
            string html = soucreHtml;
            var htmlPath = Path.Combine(localDirectory, String.Format("{0}{1}", Utility.GetHashName(pageUrl), ".html"));
            if (string.IsNullOrEmpty(soucreHtml))
            {
                var rv = DownLoadAssetsFile(pageUrl, htmlPath, cookie);
                if (!rv)//main html was failed to download 
                    return StaticStrings.ResultFailed;
                html = File.ReadAllText(htmlPath);
            }

            string localPath = htmlPath;
            string url = pageUrl;
            localDirectory = Path.GetDirectoryName(htmlPath);
            Regex reg = new Regex(StaticStrings.RegExtractJsCssJpg, RegexOptions.IgnoreCase);
            var resources = reg.Matches(html);
            List<string> resourceRelativeUrl = new List<string>();
            List<string> assetsUrl = new List<string>();
            List<string> assetsLocalPath = new List<string>();

            for (int i = 0; i < resources.Count; i++)
            {
                resourceRelativeUrl.Add(resources[i].Value);
            }

            var baseUri = new Uri(url);
            for (int i = 0; i < resourceRelativeUrl.Count; i++)
            {
                if (resourceRelativeUrl[i].StartsWith("http"))
                {
                    assetsUrl.Add(resourceRelativeUrl[i]);
                }
                else
                    assetsUrl.Add(new Uri(baseUri, resourceRelativeUrl[i]).ToString());
            }

            assetsUrl.ForEach(absolateUrl =>
            {
                assetsLocalPath.Add(Path.Combine(localDirectory, Path.GetFileName(absolateUrl)));

            });

            #region Replace Html
            for (int i = 0; i < resourceRelativeUrl.Count; i++)
            {
                html = html.Replace(resourceRelativeUrl[i], assetsLocalPath[i]);
            }
            #endregion

            #region   download relavent resource such as pictures,javascript files and css files
            InvokerMachine invoker = new InvokerMachine();

            for (int i = 0; i < assetsUrl.Count; i++)
            {
                if (!File.Exists(assetsLocalPath[i]))
                    if (!invoker.Invoke(3, DownLoadAssetsFile, assetsUrl[i], assetsLocalPath[i], cookie))
                    {
                        System.Diagnostics.Debug.WriteLine("a relevant resource was fail to download");
                        System.Diagnostics.Debug.WriteLine("{0} was fail to download", assetsUrl[i]);
                    }
            }

            #endregion

            #region  convert the relative url to absolute url

            HashSet<string> linkSet = new HashSet<string>();

            Regex regRelatedUrl = new Regex(StaticStrings.RegExtractReleaventLink, RegexOptions.IgnoreCase);
            var relatedLinks = regRelatedUrl.Matches(html);
            for (int i = 0; i < relatedLinks.Count; i++)
            {
                linkSet.Add(relatedLinks[i].Value);
            }
            linkSet.ExceptWith(resourceRelativeUrl);
            linkSet.RemoveWhere(link => link.StartsWith("http", StringComparison.CurrentCultureIgnoreCase));
            linkSet.Remove("\"\\\"");
            linkSet.Remove("'\\'");
            var linksToConvert = linkSet.GetEnumerator();
            linksToConvert.MoveNext();
            while (linksToConvert.MoveNext())
            {
                var pureUrl = linksToConvert.Current.Replace("'", "").Replace("\"", "");
                string absolutePath = new Uri(baseUri, pureUrl).ToString();
                html = html.Replace(linksToConvert.Current, "'" + absolutePath + "'");
            }


            #endregion

            File.WriteAllText(htmlPath, html);

            return html;
        }

        private static bool DownLoadAssetsFile(string url, string tempFileName, string cookie = "")
        {

            if (File.Exists(tempFileName))
            {
                return true;
            }
            try
            {
                WebClient client = new WebClient();
                if (!string.IsNullOrEmpty(cookie))
                    client.Headers.Add("Cookie", cookie);
                if (url.ToLower().StartsWith("https"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                }

                client.Headers.Add("Accept-Encoding", "gzip");
                //client.Headers.Add("Accept-Encoding", "gzip,deflate");
                string sUrl = url;
                byte[] byteArray = client.DownloadData(sUrl);

                string contentEncoding = client.ResponseHeaders[HttpResponseHeader.ContentEncoding];
                if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.Contains("gzip"))
                {
                    MemoryStream ms = new MemoryStream(byteArray);
                    MemoryStream msTemp = new MemoryStream();
                    int count = 0;
                    GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                    byte[] buf = new byte[1000];
                    while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                    {
                        msTemp.Write(buf, 0, count);
                    }
                    byteArray = msTemp.ToArray();
                }

                if (url.EndsWith(".png")
                  || url.EndsWith(".gif")
                  || url.EndsWith(".GIF")
                  || url.EndsWith(".PNG"))
                {
                    MemoryStream ms = new MemoryStream(byteArray);

                    var img = System.Drawing.Image.FromStream(ms);
                    img.Save(tempFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    img.Dispose();
                    ms.Close();
                }
                else
                {
                    File.WriteAllBytes(tempFileName, byteArray);
                }
            }
            catch (Exception ex)
            {
                return false;
            }


            return true;
        }
    }
}
