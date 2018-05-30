using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using VAV.Model.Data;

namespace VAV.Web
{
    public static class IntConverter
    {
        public static string ConvertInt2String(int? i)
        {
            switch (i)
            {
                case 1:
                    return Resources.Global.Common_Yes;
                case 0:
                    return Resources.Global.Common_No;
                default:
                    return Resources.Global.Common_No;
            }
        }
    }
}
