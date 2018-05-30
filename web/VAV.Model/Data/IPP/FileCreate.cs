using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace VAV.Model.Data.IPP
{
    public class FileCreate : BaseModel
    {
        private DateTime? reportDate = null;

        public long? ID { get; set; }

        public int Status { get; set; }

        public byte[] Doc { get; set; }

        public string TitleCn { get; set; }

        public string TitleEn { get; set; }

        public int TopicID { get; set; }

        public string DescrCn { get; set; }

        public string DescrEn { get; set; }

        public string UploadType { get; set; }

        public string FileType { get; set; }

        public string RIC { get; set; }

        public DateTime SubmitDate { get; set; }

        public DateTime ReportDate
        {
            get { return (reportDate == null ? DateTime.Now : (DateTime) reportDate); }
            set { reportDate = value; }
        }

        public string Author { get; set; }

        public string AuthorRM { get; set; }

        public string AuthorEmail { get; set; }

        public string SubmitterID { get; set; }

        public string Submiter { get; set; }

        public string SubmiterRM { get; set; }

        public string SubmiterName { get; set; }

        public string Tag { get; set; }

        public string Approver { get; set; }

        public string Source { get; set; }

        public string FileName { get; set; }

        public int DisplayOrder { get; set; }
    }
}
