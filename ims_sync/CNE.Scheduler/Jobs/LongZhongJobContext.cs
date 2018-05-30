using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CNEToolsEntities;
using CNE.Scheduler.Extension;

namespace CNE.Scheduler.Jobs
{
  public   class LongZhongJobContext: CmaJobBase
    {
        public override string JobType
        {
            get { return "LongZhongPrice"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            var connStr = ConfigurationManager.AppSettings["CnECon"];
            var strInfo = new StringBuilder();
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", connStr);

            try
            {
                #region 执行数据同步程序

                var ch = new LongZhongJob();
                
                ch.Execute();
                strInfo.AppendFormat(ch.LogMsg.ToString());
                #endregion

                ModifyLongZhongMax max = new ModifyLongZhongMax();
                max.ExecuteMax();

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
    }
}
