using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace VAV.Web.ViewModels.IPP
{
    public class IPPFile
    {
        private string reportDate;

        public long? Id { get; set; }

        public int Status { get; set; }

        public byte[] Doc { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "IPP_Title_Cn_Val")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TitleCnLengthMessage")]
        public string TitleCn { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "IPP_Title_En_Val")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TitleEnLengthMessage")]
        public string TitleEn { get; set; }

        public string Module { get; set; }

        public string Topic { get; set; }

        public string DescriptionCn { get; set; }

        public string DescriptionEn { get; set; }

        public string UploadType { get; set; }

        public string FileType { get; set; }

        public string WebsiteRic { get; set; }

        public string EikonRic { get; set; }

        public string ReportDate { 
            get
            {
                return reportDate;
            }
            set
            {
                reportDate = value;
            }
        }

        [StringLength(50, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "AuthorLengthMessage")]
        public string Author { get; set; }

        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "AuthorRMLengthMessage")]
        public string AuthorRM { get; set; }

        [RegularExpression(".+@.+\\..+", ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "EmailAddressValidation")]
        public string AuthorEmail { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "SubmiterLengthMessage")]
        public string Submiter { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TagLengthMessage")]
        public string Tag { get; set; }

        public string SubmitterID { get; set; }

        public string SubmiterRM { get; set; }

        public string SubmiterName { get; set; }

        public List<SelectListItem> ModuleItems { get; set; }

        public List<SelectListItem> TopicItems { get; set; }

        public List<SelectListItem> UploadTypeItems { get; set; }

        public List<SelectListItem> RicTypeItems { get; set; }

        public string Source { get; set; }

        public string PreviousRequest { get; set; }

        public string FileName { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsSetTop
        {
            get { return DisplayOrder == 1; }
            set { DisplayOrder = value ? 1 : 10000; }
        }
    }
}
