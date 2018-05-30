using System;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.IO;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Jobs
{
    public class MySteelDownLoad : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var lastSyncTime = Convert.ToDateTime("2014-04-03");
            using (var cneEntities = new CnEEntities())
            {
                DateTime lastTime = DateTime.UtcNow.AddDays(-1);
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType && x.STARTTIME < lastTime).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value.AddHours(8);
                }
            }

            var strInfo = new StringBuilder();

            try
            {
                #region 执行数据同步程序
                    var settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                       @"config\Ftp-MySteel-data-sync.xml");
                    var settingManager = new FtpSyncXmlManager(File.ReadAllText(settingFilePath), lastSyncTime.AddHours(8), startTime.AddHours(8));
                    settingManager.Init();
                    strInfo.AppendFormat("FTP DownLoad  begin at {0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    strInfo.AppendFormat("Source [Type: {0} Address: {1}]\r\n", settingManager.SourceDb.Type,
                                                     settingManager.SourceDb.Conn);
                    var ftpSync = new FtpSyncLoad(settingManager, strInfo);
                    ftpSync.Excute();
                #endregion

                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("DownLoad completed at {0}.\r\n", endTime.ToGmt8String());
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
            get { return "MySteelDownLoad_SYNC"; }
        }

        #endregion
    }
}
