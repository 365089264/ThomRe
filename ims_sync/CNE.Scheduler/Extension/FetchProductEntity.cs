using System.Text;
using System.Configuration;

namespace CNE.Scheduler.Extension
{
    public class FetchProductEntity : FetchDataBase
    {
        private readonly string _urlname;
        public FetchProductEntity(string urlname)
        {
            _urlname = urlname;
        }
        public string DataUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ProductUrl"].Replace("{type}", _urlname) + "?username=lutou&password=lutou2014data";
            }
        }


        public override void FetchAndFill(StringBuilder sb)
        {
            
        }
    }
}
