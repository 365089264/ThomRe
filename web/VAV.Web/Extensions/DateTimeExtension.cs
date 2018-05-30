using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.DAL.Common;

namespace VAV.Web.Extensions
{
    public static class DateTimeExtension
    {
        public static double ToUtc(this DateTime date)
        {
            return date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }
    }
}