using System.ServiceModel.Configuration;
using System.Configuration;

namespace CNE.Scheduler.Extension
{
    public static class ConfigHelper
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
