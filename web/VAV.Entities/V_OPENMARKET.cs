//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VAV.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class V_OPENMARKET
    {
        public string RIC { get; set; }
        public System.DateTime ISSUEDATE { get; set; }
        public Nullable<decimal> VOLUME { get; set; }
        public Nullable<decimal> ISSUERATE { get; set; }
        public Nullable<decimal> YIELD { get; set; }
        public Nullable<System.DateTime> MATURITY_DT { get; set; }
        public string TRADETYPE { get; set; }
        public Nullable<decimal> TERM { get; set; }
        public string ASSET_ID { get; set; }
        public string TERM_EN { get; set; }
        public string TERM_CN { get; set; }
        public Nullable<decimal> TERM_ORDER { get; set; }
    }
}
