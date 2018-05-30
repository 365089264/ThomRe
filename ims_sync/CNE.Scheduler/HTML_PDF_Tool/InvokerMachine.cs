using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.HTML_PDF_Tool
{
    public class InvokerMachine
    {


        public delegate bool DeleDownloadAsset(string url,string path,string cookie);

        public delegate Tuple<string, string> DeleDownloadHtml(PostPageTemplate post, bool reDownload = false, bool saveHeader = true);

        public bool Invoke(int invokeTimes, DeleDownloadAsset dele, string url, string path, string cookie)
        {
            for (int i = 0; i < invokeTimes; i++)
            {
                if (dele(url,path,cookie))
                {
                    return true;
                }
            }
            return false;


        }

        
        public Tuple<string, string> Invoke(int invokeTimes, DeleDownloadHtml dele, PostPageTemplate post, bool reDownload = false, bool saveHeader = true)
        {
            for (int i = 0; i < invokeTimes; i++)
            {
                var rv = dele(post, reDownload, saveHeader);
                if (rv!=null)
                {
                    return rv;
                }
            }
            return null;


        }

    }
}
