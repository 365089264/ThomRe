using System;
using System.Linq;
using System.Text;
using CNE.Scheduler.Extension;
using Quartz;
using CNEToolsEntities;

namespace CNE.Scheduler.Jobs
{
    public class WmpAreaJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("WmpArea  Sync begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            var lastSyncTime = Convert.ToDateTime("1900-1-1");
            using (var cneEntities = new CnEEntities())
            {
                var date =
                   cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                       x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    //ToGMT8
                    lastSyncTime = date.Value.AddHours(8);
                    strInfo.AppendFormat("Last successfully time : {0}.\n", lastSyncTime);
                    //Add Buffer
                    lastSyncTime = lastSyncTime.AddHours(-1);
                }
            }

            try
            {
                #region 执行数据同步程序
                var wmpArea = new WmpArea();
                wmpArea.SyncData(strInfo, lastSyncTime);
                #endregion

                var endTime = DateTime.UtcNow;
                strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime.ToGmt8String());
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);
            }
            catch (Exception exception)
            {
                logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                WriteLogEntity(logEntity);
            }
        }

        #region Overrides of CNEJobObject

        public override string JobType
        {
            get { return "WmpArea_SYNC"; }
        }

        #endregion
    }
}
