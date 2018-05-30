using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace CNE.Scheduler.Jobs
{
    public class Sail_Traval_SH_DataJob : CmaJobBase
    {
        public override string JobType
        {
            get { return "Sail_Traval_SH_DataJob"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("Smm data Sync begin at {0}\n", DateTime.UtcNow.ToGmt8String());
            StringBuilder sb = new StringBuilder();
            try
            {
                #region 执行数据同步程序

                Sail_Traval_Manager manager = new Sail_Traval_Manager();
                manager.Manager(sb);


                using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["CnECon"]))
                {
                    try
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "[dbo].[CreateShangHaiShippingMaxTable]";
                            cmd.Connection = conn;
                            cmd.ExecuteNonQuery();

                        }
                       
                    }
                    catch { }
                    finally
                    {
                        conn.Close();
                    }
                }
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
