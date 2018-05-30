using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension.Model
{
    public class Monitor
    {
        public string JobType { get; set; }
        public string TriggerInterval { get; set; }
        public List<MonitorMap> Mappings { get; set; }
    }
    public class MonitorMap
    {
        public string DataProvider { get; set; }
        public string Source { get; set; }
        public string DestinationType { get; set; }
        public string DestinationAddress { get; set; }
        public string DestinationCon { get; set; }
        public string DestinationFilter { get; set; }
        public string Description { get; set; }
    }
}
