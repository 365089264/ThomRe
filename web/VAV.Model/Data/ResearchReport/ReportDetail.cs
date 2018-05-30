using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.RSReport
{
    public class ReportDetail
    {
        public int FileId { get; set; }
        public string FileTypeCode { get; set; }
        public string FileType { get; set; }
        public string DisplayName { get; set; }
        public string FileNameCn { get; set; }
        public string FileNameEn { get; set; }
        public string Author { get; set; }
        public Nullable<System.DateTime> ReportDate { get; set; }
        public string DisplayDate { get; set; }
        public bool IsValid { get; set; }
        public string InstitutionInfoCode { get; set; }
        public string InstitutionName { get; set; }
        public string Ext { get; set; }
    }
}
