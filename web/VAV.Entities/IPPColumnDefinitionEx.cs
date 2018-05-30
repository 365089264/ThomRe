using System.Threading;

namespace VAV.Entities
{
    public partial class TOPIC
    {
        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.NAMECN : this.NAMEEN; }
        }
    }

    public partial class FILEINFO
    {
        public string Title
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.TITLECN : this.TITLEEN; }
        }

        public string Description
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.DESCRCN : this.DESCREN; }
        }
    }
}
