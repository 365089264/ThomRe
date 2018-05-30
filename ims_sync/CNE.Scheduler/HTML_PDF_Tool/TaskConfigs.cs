using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;


namespace CNE.Scheduler.HTML_PDF_Tool
{
    public abstract class Page
    {

        public Page(string cfgPath)
        {
            CfgPath = cfgPath;
            strAll = File.ReadAllText(CfgPath).Replace("\r","");
            CfgDirectory = Path.GetDirectoryName(cfgPath);
        }

        public Page(step cfg)
        {
            
        }

        public string CfgDirectory = "";
        public string CfgPath;
        public string strAll = "";
    }
    public class PostPageTemplate : Page, ICloneable
    {

        public PostPageTemplate(string cfgPath)
            : base(cfgPath)
        {
            LoadCfg();
        }
        public PostPageTemplate(step cfg)
            : base(cfg)
        {
            LoadCfgFromXml(cfg);
        }

        private void LoadCfg()
        {
            var indexHeader = strAll.IndexOf(StaticStrings.HeaderSplitor);
            var bp = strAll.IndexOf(StaticStrings.BodySplitor);
            var indexBody = bp == -1 ? strAll.Length : bp;

            header = strAll.Substring(indexHeader + StaticStrings.HeaderSplitor.Length, indexBody - indexHeader - StaticStrings.HeaderSplitor.Length);
            body = indexBody == strAll.Length ? "" : strAll.Substring(indexBody + StaticStrings.BodySplitor.Length);
            var parameterPlace = Utility.GetRegxMatchs(StaticStrings.ParameterSpaceRegex, strAll);
            parameters = parameterPlace.Distinct().ToArray();
        }

        private void LoadCfgFromXml(step cfg)
        {
            //var indexHeader = strAll.IndexOf(StaticStrings.HeaderSplitor);
            //var bp = strAll.IndexOf(StaticStrings.BodySplitor);
            //var indexBody = bp == -1 ? strAll.Length : bp;
            header = cfg.post_header;
            body = cfg.post_body.Trim().Replace("\n","").Replace("\r","");

            var parameterPlace = Utility.GetRegxMatchs(StaticStrings.ParameterSpaceRegex, header).Concat(Utility.GetRegxMatchs(StaticStrings.ParameterSpaceRegex, body));
            parameters = parameterPlace.Distinct().ToArray();
        }
        public string cookie
        {
            get 
            {
                if (Headers.ContainsKey("Cookie"))
                {
                    return Headers["Cookie"];
                }
                else
                    return "";

            }
        }
        private string _header;
        public string header
        {
            get { return _header; }
            set { _header = value.Replace("\r",""); GetHeaderFromString(); }
        }
        public string body;
        public string[] parameters;
        Dictionary<string, string> _Headers = null;
        public Dictionary<string, string> Headers
        {
            get { return _Headers; }
        }
        private string _url;
        public string Url
        {
            get{return _url;}
        }


        private string _method;
        public string Method
        {
            get { return _method; }
        }
        private void GetHeaderFromString()
        {
            _Headers = new Dictionary<string, string>();
            string[] temps = header.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);    
            for (int i = 0; i < temps.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(temps[i]))
                    continue;
                if (i == 0)
                {
                    var method = temps[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _Headers.Add(method[0].Trim(), method[1].Trim());
                }
                else
                {
                    int splitPos = temps[i].IndexOf(':');
                    _Headers.Add(temps[i].Substring(0, splitPos).Trim(), temps[i].Substring(splitPos + 1).Trim());
                }
            }

            if (Headers.Keys.Contains("POST"))
            {
                _url = Headers["POST"];
                _method = "POST";
            }
            else if (Headers.Keys.Contains("GET"))
            {
                _url = Headers["GET"];
                _method = "GET";

            }
            else if (Headers.Keys.Contains("CONNECT"))
            {
                _url = Headers["CONNECT"];
                _method = "CONNECT";
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public PostPageTemplate Clone2()
        {
            return (PostPageTemplate)Clone();
        }

        
    }

    public class PostBackPageTemplate : Page
    {
        public PostBackPageTemplate(string cfgPath)
            : base(cfgPath)
        {
            
            var indexHeader = strAll.IndexOf(StaticStrings.HeaderSplitor);
            var strParam = strAll.Substring(0, indexHeader);
            var parameters = strParam.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string str in parameters)
            {
                var firstequal = str.IndexOf('=');
       
                var paramName = str.Substring(0,firstequal-1).Trim();
                var paramValue = str.Substring(firstequal+1).Trim();
                InputParameter.Add(paramName, paramValue);
            }
        }

        public PostBackPageTemplate(step cfg)
            : base(cfg)    
        {
            InputParameter = cfg.pastback;
        }


        public Dictionary<string, string> InputParameter = new Dictionary<string, string>();


    }
    public class CoreConfiguration
    {
        public SaverEnum SaverType;
        public string ProjectName;
        public steps steps = new steps();
        public Dictionary<string, object> DefaultParamValues = new Dictionary<string, object>();
    }
    public enum SaverEnum
    {
        Word,
        Pechin
    }
    public class steps : List<step>
    {

    }

    public class step
    {
        public string name;
        public string post_header;
        public string post_body;
        public Dictionary<string, string> pastback = new Dictionary<string, string>();

    }
}
