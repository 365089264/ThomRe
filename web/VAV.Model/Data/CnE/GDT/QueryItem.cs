using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VAV.Model.Data.CnE.GDT
{
   public  class QueryItem
    {
       public string ItemText_CN { get; set; }
       public string ItemText_EN { get; set; }
       public string DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? ItemText_CN : ItemText_EN; } }
    }
}
