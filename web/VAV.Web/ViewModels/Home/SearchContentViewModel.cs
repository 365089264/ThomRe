using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.Model.Data;
using SolrNet.Attributes;
using VAV.Web.Localization;

namespace VAV.Web.ViewModels.Home
{
    public class SearchContentViewModel
    {
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        [SolrField("code")]
        public string Code { get; set; }

        //[SolrField("code_list")]
        //public string CodeList { get; set; }

        [SolrField("pyc")]
        public string Pyc { get; set; }

        [SolrField("name_cn")]
        public string NameCn { get; set; }

        [SolrField("name_en")]
        public string NameEn { get; set; }

        public string Name
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? NameEn : NameCn;
            }
        }

        [SolrField("short_name_cn")]
        public string ShortNameCn { get; set; }

        [SolrField("short_name_en")]
        public string ShortNameEn { get; set; }

        [SolrField("ric")]
        public string Ric { get; set; }

        [SolrField("exchange_code")]
        public string ExchangeCode { get; set; }

        [SolrField("display_type")]
        public string DisplayType { get; set; }

        [SolrField("open_type")]
        public string OpenType { get; set; }

        [SolrField("action_type")]
        public string ActionType { get; set; }

        [SolrField("report_id")]
        public string ReportId { get; set; } 
    }
}