using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using VAVToolsEntities;

namespace VAV.Scheduler.Util
{
    public class ConfigHelper
    {
        /// <summary>
        /// 读取EndpointAddress
        /// </summary>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static string GetEndpointClientAddress(string endpointName)
        {
            var clientSection = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
            foreach (ChannelEndpointElement item in clientSection.Endpoints)
            {
                if (item.Name == endpointName)
                    return item.Address.ToString();
            }
            return string.Empty;
        }
    }
}
