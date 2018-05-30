using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace VAV.Web.Extensions
{
    public static class DebugHelper
    {
        public static string GetServerInfo()
        {
            var sb = new StringBuilder();
            var debug = ConfigurationManager.AppSettings["debugIP"];
            if (debug == "true")
            {
                sb.AppendFormat("Server Name:{0} \n", Environment.MachineName);
                sb.AppendLine("Server IP:");
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                for (var i = 1; i <= ipHostInfo.AddressList.Count(); i++)
                {
                    sb.AppendFormat("ip{0}:{1}\n", i, ipHostInfo.AddressList[i - 1]);
                }
            }
            return sb.ToString();
        }
    }
}