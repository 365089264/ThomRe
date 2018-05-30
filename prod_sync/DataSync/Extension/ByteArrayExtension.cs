using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public static class ByteArrayExtension
    {
        public static string ToStringBySybaseStandard(this byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < byteArray.Length; index++)
                sb.Append(byteArray[index].ToString("x2"));

            return sb.ToString();
        }
    }
}
