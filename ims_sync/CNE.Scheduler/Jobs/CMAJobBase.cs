using System;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using CNE.Scheduler.Extension;
using Common.Logging;
using Luna.DataSync.Core;
using Spring.Scheduling.Quartz;
using CNEToolsEntities;

namespace CNE.Scheduler.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CmaJobBase : QuartzJobObject
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger("CNEScheduler");

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
            paras[0] = new OracleParameter("I_STARTTIME", OracleType.DateTime);
            paras[0].Value = logEntity.STARTTIME;
            paras[1] = new OracleParameter("I_ENDTIME", OracleType.DateTime);
            paras[1].Value = logEntity.ENDTIME;
            paras[2] = new OracleParameter("I_STATUS", OracleType.Number);
            paras[2].Value = logEntity.STATUS;
            paras[3] = new OracleParameter("I_JOBTYPE", OracleType.VarChar);
            paras[3].Value = JobType;
            paras[4] = new OracleParameter("I_RUNDETAIL", OracleType.Clob);
            paras[4].Value = logEntity.RUNDETAIL;


            DBHelper.ExecuteLogStorage("SCHEDULERLOG_INSERT", paras);

            var detail = logEntity.RUNDETAIL;
            if (!String.IsNullOrEmpty(detail))
                detail = detail.Replace("\n", "<br/>");
            var str =
                String.Format("<Html><Body style='font-family:Verdana;font-size:12'><h2 style='color:{5}'>[{0}] Scheduler job \"<b>{1}</b>\" finished.</h2><hr/><p>Start Time:{2}</p><p>End Time:{3}</p><p><b>Detail:</b></p><p>{4}</p></Body></Html>",
                    logEntity.JobStatus, JobType,
                    logEntity.STARTTIME.ToGmt8String(),
                    logEntity.ENDTIME.ToGmt8String(), detail,
                    logEntity.JobStatus == JobStatus.Success ? "green" : "red");
            if (logEntity.JobStatus == JobStatus.Success)
                _log.Info(str);
            else
                _log.Error(str);
        }

        public DateTime GetLastSyncTime()
        {
            var lastSyncTime = new DateTime(2001, 1, 1);
            using (var cneEntities = new CnEEntities())
            {
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == this.JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value.AddHours(-0.5);
                }
            }
            return lastSyncTime;
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
        public static string ToGmt8String(this DateTime dateTime) 
        {
            return dateTime.ToUniversalTime().AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") + "(GMT +8:00)";
        }
    }

}