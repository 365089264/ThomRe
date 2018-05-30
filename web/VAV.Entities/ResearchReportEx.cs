using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VAV.Entities
{
    public partial class FILEDETAIL
    {
        //public bool HasFile { get; set; }

        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.FILENAMECN : this.FILENAMEEN; }
        }
    }

    public partial class FILETYPEINFO
    {
        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.FILENAMECN : this.FILENAMEEN; }
        }
    }

    public partial class INSTITUTIONINFO
    {
        public string LogoPath { get; set; }
        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.INSTITUTIONNAMECN : this.INSTITUTIONNAMEEN; }
        }
    }

    public partial class FileData
    {
        public int FileId { get; set; }
        public DateTime? CTIME { get; set; }
        public DateTime? MTIME { get; set; }
        public byte[] Content { get; set; }
    }

}
