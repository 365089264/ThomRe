using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace VAV.Model.Data.CnE.GDT
{
   public  class GDTQueryCondition
    {
      
       private   List <SelectListItem> listItem=new List<SelectListItem> ();
       public int ID { get; set; }
       public int ItemID { get; set; }
       public int Direction { get; set; }
       public string DisplayName_CN { get; set; }
       public string DisplayName_EN { get; set; }
       //public string ColumnName { get; set; }
       public string QueryValue { get; set; }
       public string DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? DisplayName_CN : DisplayName_EN; } }
       public List<SelectListItem> ListItem { get { return listItem; } set { listItem = value; } }
       public string RelationColumn { get; set; }
       
    }
}
