using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.CnE.GDT
{
   public  class GDTQueryWithDirection
    {
       private  List<GDTQueryCondition> _left = new List<GDTQueryCondition>();
       private  List<GDTQueryCondition> _right = new List<GDTQueryCondition>();
       public List<GDTQueryCondition> Left { get { return _left; } }
       public List<GDTQueryCondition> Right { get { return _right; } }
    }
}
