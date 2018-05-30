using System;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Linq;

namespace CNE.Scheduler.Jobs
{
    public class FxNewsJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            try
            {
                var lastSyncTime = new DateTime(2016, 9, 28);

                using (var cneEntities = new CnEEntities())
                {
                    var date =
                        cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                            x => (DateTime?)x.STARTTIME).Max();
                    if (date != null)
                    {
                        //ToGMT8
                        lastSyncTime = date.Value.AddHours(8);
                    }
                }

                #region 执行数据同步程序
                var fn=new FxNews();
                fn.GetWebData(strInfo, lastSyncTime);
                #endregion

                var endTime = DateTime.UtcNow;
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
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "FxNews"; }
        }

        #endregion
    }
}
