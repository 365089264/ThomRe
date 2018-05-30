using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VAV.Web.ViewModels.Upload
{
    public class HomeItemModel
    {
        public string Id { get; set; }
        public string NameCn { get; set; }
        public string NameEn { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public string UploadType { get; set; }
        public string UploadValue { get; set; }
        public string Mtime { get; set; }
    }
}