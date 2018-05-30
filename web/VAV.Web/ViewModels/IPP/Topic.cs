using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace VAV.Web.ViewModels.IPP
{
    public class IPPTopic
    {
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "IPP_Title_Cn_Val")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TitleCnLengthMessage")]
        public string NameCn { get; set;    }

        [Required(ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "IPP_Title_En_Val")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TitleEnLengthMessage")]
        public string NameEn { get; set; }

        public byte[] Thumbnail { get; set; }

        public string DescriptionCn { get; set; }

        public string DescriptionEn { get; set; }

        public bool IsApprove { get; set; }

        public bool IsInternalApprove { get; set; }

        public bool IsDirectDelete { get; set; }

        public string Creater { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TagLengthMessage")]
        public string Tag { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "ApproverLengthMessage")]
        public string Approver { get; set; }

        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "RMLinkLengthMessage")]
        public string RMLink { get; set; }

        public string ModuleID { get; set; }

        public string ImageName { get; set; }

        public List<SelectListItem> ModuleItems { get; set; }
    }
}
