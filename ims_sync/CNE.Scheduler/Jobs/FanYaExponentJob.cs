using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using CNE.Scheduler.Extension.Model;

namespace CNE.Scheduler.Jobs
{
    public class FanYaExponentJob : CmaJobBase
    {
        public override string JobType
        {
            get { return "FanYaExponentJob"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("FanYaExponentJob  begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            var sb = new StringBuilder();
            try
            {
                #region 执行数据同步程序
                ////FanYaStockData---FanYaExponentData
                FanYaExponent fanYaExponent = new FanYaExponent();
                fanYaExponent.ManagerStock<FanYaStockData>();
                fanYaExponent.ManagerStock<FanYaExponentData>();




                #endregion
                var endTime = DateTime.UtcNow;
                strInfo.Append(sb);
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
    }
}

