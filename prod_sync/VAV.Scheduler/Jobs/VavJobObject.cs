using System;
using Common.Logging;
using Oracle.ManagedDataAccess.Client;
using Spring.Scheduling.Quartz;
using VAV.Scheduler.Util;
using VAVToolsEntities;

namespace VAV.Scheduler.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class VavJobObject : QuartzJobObject
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ILog log = LogManager.GetLogger("VAVScheduler");

        /// <summary>
        /// 
        /// </summary>
        public abstract string JobType { get; }

        /// <summary>
        /// 
        /// </summary>
        public void WriteLogEntity(SCHEDULERLOG logEntity)
        {
            var paras = new OracleParameter[5];
            paras[0] = new OracleParameter("I_STARTTIME", OracleDbType.Date);
            paras[0].Value = logEntity.STARTTIME;
            paras[1] = new OracleParameter("I_ENDTIME", OracleDbType.Date);
            paras[1].Value = logEntity.ENDTIME;
            paras[2] = new OracleParameter("I_STATUS", OracleDbType.Int32);
            paras[2].Value = logEntity.STATUS;
            paras[3] = new OracleParameter("I_JOBTYPE", OracleDbType.Varchar2);
            paras[3].Value = JobType;
            paras[4] = new OracleParameter("I_RUNDETAIL", OracleDbType.Clob);
            paras[4].Value = logEntity.RUNDETAIL;


            DBHelper.ExecNonQuerySp("SCHEDULERLOG_INSERT", paras);

            var detail = logEntity.RUNDETAIL;
            if (!String.IsNullOrEmpty(detail))
                detail = detail.Replace("\n", "<br/>");
            var str =
                String.Format("<Html><Body style='font-family:Verdana;font-size:12'><h2 style='color:{5}'>[{0}] Scheduler job \"<b>{1}</b>\" finished.</h2><hr/><p>Job Type:{1}</p><p>Start Time:{2}</p><p>End Time:{3}</p><p><b>Detail:</b></p><p>{4}</p></Body></Html>",
                    logEntity.JobStatus, JobType,
                    logEntity.STARTTIME.ToGMT8String(),
                    logEntity.ENDTIME.ToGMT8String(), detail,
                    logEntity.JobStatus == JobStatus.Success ? "green" : "red");
            if (logEntity.JobStatus == JobStatus.Success)
                log.Info(str);
            else
                log.Error(str);
        }

        public void Test()
        {
            ExecuteInternal(null);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToGMT8String(this DateTime dateTime) 
        {
            return dateTime.ToUniversalTime().AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") + "(GMT +8:00)";
        }
    }

}