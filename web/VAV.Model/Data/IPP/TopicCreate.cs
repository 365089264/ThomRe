using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace VAV.Model.Data.IPP
{
    public class TopicCreate : BaseModel
    {
        public int ID { get; set; }

        public byte[] Thumbnail { get; set; }

        public string NameCn { get; set; }

        public string NameEn { get; set; }

        public string DescriptionCn { get; set; }

        public string DescriptionEn { get; set; }

        private bool? isApprove; 
        public bool IsApprove {
            get
            {
                return isApprove == null ? true : (bool)isApprove;
            }
            set
            {
                isApprove = value;
            }
        }

        private bool? isInternalApprove;
        public bool IsInternalApprove 
        {
            get 
            {
                return isInternalApprove == null ? false : (bool)isInternalApprove;
            }
            set
            {
                isInternalApprove = value;
            }
        }

        private bool? isDirectDelete;
        public bool IsDirectDelete 
        { 
            get
            {
                return isDirectDelete == null ? false : (bool)isDirectDelete;
            }
            set
            {
                isDirectDelete = value;
            }
        }

        public string Creater { get; set; }

        public string Tag { get; set; }

        public string Approver { get; set; }

        public string RMLink { get; set; }

        public int ModuleID { get; set; }

        public string ImageName { get; set; }
    }
}
