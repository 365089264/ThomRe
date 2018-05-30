using System.Threading;

namespace VAV.Entities
{
    public partial class REPORTCOLUMNDEFINITION
    {
        public string DisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? this.HEADER_TEXT_CN : this.HEADER_TEXT_EN; }
        }
    }
}
