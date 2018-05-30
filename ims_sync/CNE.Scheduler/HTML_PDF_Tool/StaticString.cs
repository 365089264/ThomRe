using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace CNE.Scheduler.HTML_PDF_Tool
{
    public class StaticStrings
    {
        static StaticStrings()
        {
            //OutPutDirectoryFullPath = Path.Combine(Application.StartupPath, OutPutDirectory);
        }
        
        public const string DateTimeNowDateString = "";
        public const string CofigsPath = "Configs";
        public const string NewCofigsPath = "Configuration";
        public const string Stepstxt = "Steps.txt";
        public const string HeaderSplitor = "////header";
        public const string BodySplitor = "////body";
        public const string FilterPath = "Filter.txt";
        public const string PostBackSuffix = "_PostBack.txt";
        public const string PostSuffix = ".txt";
        public const string ParameterSpaceRegex = @"\{\w+?\}";
        public const string HtmlPdfLibDirectory = @"\Lib";
        public const string OutPutDirectory = "output";
        public static string OutPutDirectoryFullPath = "";
        public const string LoggerName = "logerror";
        public const string RegExtractJsCssJpg = "(?<=(src\\=|href\\=)\")[^\"]+?\\.(jpg|gif|css|js|png)(?=\"|#)";
        //public const string RegExtractReleaventLink = "((?<=href\\s*\\=\\s*\").+?(?=\"))|((?<=href\\s*\\=\\s*\').+?(?=\'))";
        public const string RegExtractReleaventLink = "((?<=href\\s*\\=\\s*)\"/.*?\")|((?<=href\\s*\\=\\s*)'/.*?')";
        public const string RegImportedCss = "(?<=import\\s*url\\(('|\")).+\\.css(?=('|\")\\))";


        public const string RegAbsoluteLocalUri = @"^[a-zA-Z]\:.+$";
        public const string HistoryLogName = "pdf.log";
        public const string ResultFailed = "ERROR";
        public const string UIConfigPath = "custom.xml";
        public struct TreeImageKey
        {
            public static string Normal = "Normal";
            public static string Running = "Running";
            public static string Pause = "Pause";
        }

        


        public static string DateTimeNow
        {
            get { return DateTime.Now.ToString("yyyMMddhhmmss"); }
        }
        public static string DirectoryAll
        {
            //get { return DateTime.Now.ToString("yyyMMdd"); }
            get { return "All"; }
        }
 
        private static string _path;
        public static string ApplicationMenu
        {
            get 
            {
                if (string.IsNullOrEmpty(_path))
                {
                    _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                }
                return _path;
            }
        }

        public const string OutPutFileNameParam = "{TargetName}";
        public const string OutPutFileNameByURL = "{TargetNameUrl}";
        public const string OutPutSubFolder = "{TargetSubFolder}";
        public const string OutPutFileBody = "{TargetBody}";


      
        
    }
}
