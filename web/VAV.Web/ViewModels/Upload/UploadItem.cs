using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using VAV.Web.ViewModels.Report;
using VAV.DAL.Report;
using VAV.Web.Localization;
using VAV.Web.Common;


namespace VAV.Web.ViewModels.Upload
{
    public class UploadItemViewModel
    {
        public BondInfoRepository bondInfoRepository { get; set; }
        
        private string submitDate;

        public long? Id { get; set; }

        public byte[] Doc { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "IPP_Title_Cn_Val")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TitleCnLengthMessage")]
        public string TitleCn { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "IPP_Title_En_Val")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "TitleEnLengthMessage")]
        public string TitleEn { get; set; }

        public string Module { get; set; }

        public string DescriptionCn { get; set; }

        public string DescriptionEn { get; set; }

        public string UploadType { get; set; }

        public string Url { get; set; }

        public string Ric { get; set; }

        public string RMLink { get; set; }

        public string UploadTypeValue
        {
            get
            {
                if (UploadType == "Upload_Website")
                    return Url;
                else if (UploadType == "Upload_RMLink")
                    return RMLink;
                else if (UploadType != "Upload_File")
                    return Ric;
                else
                    return "";
            }
            set
            {
            }
        }

        public string SubmitDate { 
            get
            {
                return submitDate;
            }
            set
            {
                submitDate = value;
            }
        }

        [StringLength(50, ErrorMessageResourceType = typeof(Resources.IPP), ErrorMessageResourceName = "SubmiterLengthMessage")]
        public string Submiter { get; set; }

        public List<SelectListItem> ModuleItems { get; set; }

        public List<SelectListItem> UploadTypeItems { get; set; }

        public bool IsVisible { get; set; }

        public string TypeParam { get; set; }

        public string FileName { get; set; }
    }
}
