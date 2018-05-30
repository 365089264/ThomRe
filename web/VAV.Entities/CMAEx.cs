using System.Threading;

namespace VAV.Entities
{
    public partial class HOMEMODULE
    {
        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? NAMECN : NAMEEN; }
        }
    }

    public partial class HOMEITEM
    {
        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? NAMECN : NAMEEN; }
        }
    }

    public partial class COLUMNDEFINITION
    {
        public string ColumnDisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.COLUMNNAME_CN : this.COLUMNNAME_EN; }
        }

        public string HearderDisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.HEADER_CN : this.HEADER_EN; }
        }
    }
    public partial class HOMEANNOUNCEMENT
    {
        public string Content
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.CONTENTCN : this.CONTENTEN; }
        }
    }
}
