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
    
    public partial class COLUMNDEFINITION
    {
        public decimal ID { get; set; }
        public Nullable<decimal> ITEMID { get; set; }
        public Nullable<decimal> DIRECTION { get; set; }
        public string COLUMNNAME_CN { get; set; }
        public string COLUMNNAME_EN { get; set; }
        public string HEADER_CN { get; set; }
        public string HEADER_EN { get; set; }
        public string DISPLAY_FORMAT { get; set; }
        public string COLUMN_TYPE { get; set; }
        public Nullable<short> IS_SORT { get; set; }
        public Nullable<decimal> COLUMN_ORDER { get; set; }
    }
}