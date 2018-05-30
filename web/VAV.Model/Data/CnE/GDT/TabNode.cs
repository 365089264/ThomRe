using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace VAV.Model.Data.CnE.GDT
{
   public  class TabNode
    {
       public int ItemID { get; set; }
       public string TabName_CN { get; set; }
       public string TabName_EN { get; set; }
       public int ReportId { get; set; }
       public string ViewModelName1 { get; set; }
       public string ViewModelName2 { get; set; }
       public string TableName1 { get; set; }
       public string TableFilter1 { get; set; }
       public string TableName2 { get; set; }
       public string TableFilter2 { get; set; }
       public string ViewName1 { get; set; }
       public string ViewName2 { get; set; }
       public string DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? TabName_CN : TabName_EN; } }
       public string Legend { get; set; }
    }
}
