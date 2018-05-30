using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VAV.Web.ViewModels.WMP
{
    /// <summary>
    /// 产品公告
    /// </summary>
    public class BrokerContentModel
    {
        public Int64 SEQ { get; set; }
        public DateTime MTIME { get; set; }
        public string DISC_ID { get; set; }
        public string TXT_CONTENT { get; set; }
        public string TITLE { get; set; }
        public DateTime DECLAREDATE { get; set; }
        public string IS_ACCE { get; set; }
    }
}