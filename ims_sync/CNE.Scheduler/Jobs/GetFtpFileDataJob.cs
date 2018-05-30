using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;

namespace CNE.Scheduler.Jobs
{
   public  class GetFtpFileDataJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("FTP_file_Data  Sync begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            try
            {
                #region 执行数据同步程序
                var fn = new GetFtpFileData();
                var sb = new StringBuilder();
                fn.GetWebData(ref sb);
                strInfo.Append(sb);
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
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "FTPFileData_SYNC"; }
        }

        #endregion
    }
}
