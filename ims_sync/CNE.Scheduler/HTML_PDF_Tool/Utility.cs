using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Security.Cryptography;
using System.IO;

namespace CNE.Scheduler.HTML_PDF_Tool
{
    public class Utility
    {
        static object stone = new object();
        public static string[] GetRegxMatchs(string regStr, string source)
        {
            Regex reg = new Regex(regStr);
            var ms = reg.Matches(source);
            string[] matchedStr = new string[ms.Count];

            for (int i = 0; i < ms.Count; i++)
            {
                matchedStr[i] = ms[i].Value;
            }

            return matchedStr;
        }
        static object md5Locker = new object();
        public static string GetHashName(string str)
        {
            lock (md5Locker)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] data = System.Text.Encoding.Default.GetBytes(str);
                byte[] md5data = md5.ComputeHash(data);
                md5.Clear();
                string resule = System.BitConverter.ToString(md5data);
                return resule;
            }
        }

        public static void ReplaceByReg(string regStr, ref string source, string newValue)
        {
            Regex reg = new Regex(regStr);
            source = reg.Replace(source, newValue);
        }

        public static string[] GetStringsByXpath(string xpath, string html)
        {
            lock (stone)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes != null && nodes.Count > 0)
                {
                    string[] nodeHtml = new string[nodes.Count];

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        nodeHtml[i] = nodes[i].InnerHtml;
                    }
                    return nodeHtml;
                }
                else
                    return null;
            }

        }

        public static bool IsAbsoluteLocalUri(string uri)
        {
            Regex reg = new Regex(StaticStrings.RegAbsoluteLocalUri);
            return reg.IsMatch(uri);


        }

        public static string DealFileName(string oriHeader)
        {
            Regex reg = new Regex(@"[^\w\.]+");
            oriHeader = reg.Replace(oriHeader, " ");

            //oriHeader =  oriHeader.Replace("<i>", " ")
            //    .Replace("<I>", " ")
            //    .Replace("</i>", " ")
            //    .Replace("</I>", " ")
            //    .Replace("/","_")
            //    .Replace("\\","_")
            // .Replace(":"," ")
            // .Replace("&nbsp;"," ")
            // .Replace("?","")
            // .Replace("<em>","")
            // .Replace("</em>","")
            //   .Replace("<EM>", " ")
            // .Replace("</EM>", " ")
            // .Replace
            // .Trim();
            if (oriHeader.Length > 145)
                oriHeader = oriHeader.Substring(0, 145);
            return oriHeader;
        }

        public class LocalIOHelper
        {
            static Dictionary<string, bool> dicDirectory = new Dictionary<string, bool>();



            /// <summary>
            /// check if the directory of the path is exist ,otherwise ,create it and return the directory path string
            /// </summary>
            /// <param name="path">file path</param>
            /// <returns></returns>
            public static string CheckDirectoryExist(string path,bool isDir = false)
            {               
                var dir = Path.GetDirectoryName(path);
                if (isDir)
                    dir = path;

                if (dicDirectory.ContainsKey(path))
                {
                    return dir;
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                dicDirectory.Add(path, true);
                return dir;
            }


            public static bool DeleteFilesFromFolder(string folderPath, string[] extention = null)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(folderPath);
                    if (extention == null)
                    {
                        if (dir.Exists)
                        {
                            dir.Delete(true);
                        }
                    }
                    else
                    {
                        if (dir.Exists)
                        {
                            for (int i = 0; i < extention.Length; i++)
                            {
                                var files = dir.GetFiles(extention[i], SearchOption.TopDirectoryOnly);
                                foreach (var fPath in files)
                                {
                                    File.Delete(fPath.FullName);
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
            public static bool DeleteCachedCssAndPicFromFolder()
            {
                string htmlCache = Path.Combine(StaticStrings.ApplicationMenu, "htmltemp", "ALL");
                string htmlCache2 = Path.Combine(Path.GetTempPath(), "ips_temp", "ALL");
                var extentions = new string[]{
                    "*.jpg","*.css","*.js","*.png","*.gif"
                };
                return DeleteFilesFromFolder(htmlCache, extentions) &&
                DeleteFilesFromFolder(htmlCache2, extentions);
                 
                
            }
        }

    }
}
