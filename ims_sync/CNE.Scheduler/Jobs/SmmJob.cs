using System;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using System.IO;
using CNE.Scheduler.Extension;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CNE.Scheduler.Jobs
{
    public class SmmJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();


            try
            {
                #region 执行数据同步程序
                if (DateTime.Now.Hour < 15 || DateTime.Now.Hour >= 22)
                {
                    strInfo.Append("Only this time(15:00Am-8:00Pm) point synchronous;");
                    return;
                }
                #endregion

                #region 执行数据同步程序
                SmmShNew smm = new SmmShNew();
                StringBuilder sb = new StringBuilder();
                var connStr = ConfigurationManager.AppSettings["CnECon"];
                strInfo.Append("Source [Type: URL,  Address: http://www.smm.cn/reuters.php ]\n");
                strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", connStr);

                string[] strs = { "SMMPrices_路透", "SMMPrices_toThomsonReuters", "SMM行业数据", "SMM data" };
                smm.InsertDataBase(strs, sb);
                #endregion
                var endTime = DateTime.UtcNow;
                strInfo.Append(sb);
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

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "SMMPrice"; }
        }
    }
}
