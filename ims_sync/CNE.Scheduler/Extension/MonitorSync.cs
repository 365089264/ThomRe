using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CNE.Scheduler.Extension.Model;
using CNEToolsEntities;

namespace CNE.Scheduler.Extension
{
    public class MonitorSync : BaseDataHandle
    {
        public StringBuilder Log;
        private string _path;
        private List<Monitor> _monitorMaps;
        private string _lastDate;

        public MonitorSync(StringBuilder sb, string path, string lastDate)
        {
            Log = sb;
            _path = path;
            _lastDate = lastDate;
            Init();
        }

        public void Init()
        {
            _monitorMaps = new List<Monitor>();
            var doc = new XmlDocument();
            doc.Load(_path);
            var nodeList = doc.SelectNodes("settings/Job");
            if (nodeList != null)
                foreach (XmlNode node in nodeList)
                {
                    _monitorMaps.Add(LoadMonitorInfo(node));
                }
        }

        public Monitor LoadMonitorInfo(XmlNode node)
        {
            var monitor = new Monitor();
            var jobType = node.SelectSingleNode("JobType");
            if (jobType == null || string.IsNullOrEmpty(jobType.InnerText))
                throw new Exception("Cannot find jobType info under the node: " + node);
            monitor.JobType = jobType.InnerText;

            var triggerInterval = node.SelectSingleNode("TriggerInterval");
            if (triggerInterval == null || string.IsNullOrEmpty(triggerInterval.InnerText))
                throw new Exception("Cannot find triggerInterval info under the node: " + node);
            monitor.TriggerInterval = triggerInterval.InnerText;

            var nodes = node.SelectNodes("mappings/table-mapping");
            if (nodes != null)
            {
                monitor.Mappings = new List<MonitorMap>();
                foreach (XmlNode node1 in nodes)
                {
                    var map = new MonitorMap();
                    var source = node1.SelectSingleNode("source");
                    if (source != null)
                        map.Source = source.InnerText;

                    var dataProvider = node1.SelectSingleNode("DataProvider");
                    if (dataProvider != null)
                        map.DataProvider = dataProvider.InnerText;

                    var description = node1.SelectSingleNode("description");
                    if (description != null)
                        map.Description = description.InnerText;

                    var selectSingleNode1 = node1.SelectSingleNode("destination/type");
                    if (selectSingleNode1 != null)
                        map.DestinationType = selectSingleNode1.InnerText;
                    var singleNode1 = node1.SelectSingleNode("destination/address");
                    if (singleNode1 != null)
                    {
                        map.DestinationAddress = singleNode1.InnerText;
                        if (singleNode1.Attributes != null)
                        {
                            if (singleNode1.Attributes["con"] != null)
                            {
                                map.DestinationCon = singleNode1.Attributes["con"].InnerText;
                            }
                            if (singleNode1.Attributes["filter"] != null)
                            {
                                map.DestinationFilter = singleNode1.Attributes["filter"].InnerText;
                            }
                        }
                    }
                    monitor.Mappings.Add(map);
                }
            }
            return monitor;
        }

        public void Excute()
        {
            Log.Append("<table  border=\"1\"  style=\"border: 1px solid black; border-collapse: collapse;font-size: 14px;\"><tr style=\"font-size: 16px;font-weight: bold;text-align: center;\"><td>Data provider</td><td>JOBtype</td><td>source</td><td>destination</td><td>description</td><td>Frequency</td><td>Last Updated Date</td><td>Updated Rows</td></tr>");
            foreach (var map in _monitorMaps)
            {

                using (var cneEntities = new CnEEntities())
                {
                    DateTime last = Convert.ToDateTime(_lastDate).AddHours(-8);

                    var successCount = cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == map.JobType && x.STARTTIME > last).Select(x => x.ID).Count();
                    var failCount = cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 1 && x.JOBTYPE == map.JobType && x.STARTTIME > last).Select(x => x.ID).Count();
                    var lastUpdateTime = cneEntities.SCHEDULERLOGs.Where(x => x.JOBTYPE == map.JobType).OrderByDescending(x => x.STARTTIME).Select(x => x.STARTTIME).First();
                    foreach (var source in map.Mappings)
                    {
                        Log.Append("<tr><td>" + source.DataProvider + "</td>");
                        Log.Append("<td>" + map.JobType + "</td>");
                        Log.Append("<td>" + source.Source + "</td>");
                        if (source.DestinationType == "FTP")
                        {
                            Log.Append("<td>" + source.DataProvider + " FTP</td>");
                        }
                        else
                        {
                            Log.Append("<td>" + source.DestinationAddress + "</td>");
                        }
                        Log.Append("<td>" + source.Description + "</td>");
                        //Log.Append("<td>" + successCount + "</td>");
                        //Log.Append("<td>" + failCount + "</td>");
                        Log.Append("<td>" + map.TriggerInterval + "</td>");
                        Log.Append("<td>" + lastUpdateTime + "</td>");
                        Log.Append("<td>");
                        switch (source.DestinationType)
                        {
                            case "SQLSERVER":
                                MonitorDb(source);
                                break;
                            case "FTP":
                                MonitorFtp(source);
                                break;
                        }
                        Log.Append("</td></tr>");
                    }
                }
            }
            Log.Append("</table>");
        }

        public void MonitorDb(MonitorMap source)
        {
            using (var con = new SqlConnection(source.DestinationCon))
            {
                con.Open();
                var sql = "select count(*) from " + source.DestinationAddress;
                if (!string.IsNullOrEmpty(source.DestinationFilter))
                {
                    sql = sql + "where " + source.DestinationFilter.Replace("{LastSyncTime}", _lastDate);
                }
                var cmd = new SqlCommand(sql, con);
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
                Log.Append("" + count + "");
            }

        }

        public void MonitorFtp(MonitorMap source)
        {
            var parentdi = new DirectoryInfo(source.DestinationAddress);
            if (!parentdi.Exists)
            {
                parentdi.Create();
            }
            var date = Convert.ToDateTime(_lastDate);
            Log.Append("downLoad files: ");
            foreach (FileInfo fi in parentdi.GetFiles())
            {
                if (fi.CreationTime > date)
                    Log.Append(fi.Name + " ");
            }
            Log.Append(";");
        }
    }
}
