using System;
using System.Collections;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.IO;
using Luna.DataSync.Setting;
using System.Xml;

namespace CNE.Scheduler.Jobs
{
    public class FtpTimingDownLoad : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var lastSyncTime = Convert.ToDateTime("2016-12-18");
            using (var cneEntities = new CnEEntities())
            {
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value;
                }
            }
            
            var strInfo = new StringBuilder();
        
            try
            {
                #region 执行数据同步程序
                var mapListFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                      @"config\Ftp-Map-data-sync.xml");
                var doc = new XmlDocument();
                doc.Load(mapListFilePath);
                var maplist = doc.SelectNodes("settings/map");
                var arr = new ArrayList();
                if (maplist != null)
                {
                    foreach (XmlNode map in maplist)
                    {
                        arr.Add(map.InnerText);
                    }
                }
                strInfo.AppendFormat("Source [Type: ORACLE Address: Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.35.63.143)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME = CMADB)));User Id=cne;Password=cne;]");
                foreach (var map in arr)
                {
                    strInfo.Append("\r\n");
                    var settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                       @"config\" + map);
                    var settingManager = new FtpSyncXmlManager(File.ReadAllText(settingFilePath), lastSyncTime.AddHours(8), startTime.AddHours(8));
                    settingManager.Init();
                  
                    var ftpSync = new FtpSyncLoad(settingManager, strInfo);
                    ftpSync.Excute(); 
                }
                #endregion

                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);

            }
            catch (Exception exception)
            {
                logEntity.ENDTIME = DateTime.UtcNow;
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                WriteLogEntity(logEntity);
            }
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "FtpTimingDownLoad_SYNC"; }
        }

        #endregion
    }
}
