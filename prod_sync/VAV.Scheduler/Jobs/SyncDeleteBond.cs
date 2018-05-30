using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Quartz;
using VAVToolsEntities;
using System.Data.OracleClient;

namespace VAV.Scheduler.Jobs
{
    public class SyncDeleteBond : VavJobObject
    {
        /// <summary>
        /// Sync data from GeniusDB and hosted in .144 which is the transfer DB
        /// </summary>
        /// <seealso cref="M:Spring.Scheduling.Quartz.QuartzJobObject.Execute(Quartz.JobExecutionContext)"/>
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            if (DateTime.Now.Hour<10||DateTime.Now.Hour>17)
            {
                return;
            }

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            var lastSyncTime = new DateTime(2017, 06, 1);
            using (var vavEntities = new VAVEntities())
            {
                var date =
                    vavEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == this.JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                    lastSyncTime = date.Value.AddHours(8);
            }
            try
            {
                string localBondCon = ConfigurationManager.AppSettings["143FileDB"];
                var ds=new DataSet();
                strInfo.Append("Query 143DB assertID\n");
                using (OracleConnection conn = new OracleConnection(localBondCon))
                {
                    conn.Open();
                    OracleDataAdapter command = new OracleDataAdapter("SELECT ASSETID FROM BONDDELETE where ISVALID=1 and UPDATE_TIME>'" + lastSyncTime.ToString("dd-MMM-yyyy hh:mm:ss tt") + "'", conn);
                    command.Fill(ds, "ds");
                    conn.Close();
                    if (ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
                    {
                        return;
                    }
                    strInfo.Append("Need to delete assertID:" + ds.Tables[0].Rows.Count + " \n");
                }
               
                string proBondCon = ConfigurationManager.AppSettings["ProBondDB"];
                using (OracleConnection conn = new OracleConnection(proBondCon))
                {
                    strInfo.Append("Delete 143DB assertID:\n");
                    conn.Open();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        strInfo.Append("ASSETID=" + ds.Tables[0].Rows[i][0] + ": \n");
                        int result ;
                        string sql = "delete GOVCORP_ASSET where ASSETID='" + ds.Tables[0].Rows[i][0] + "'";
                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            result=cmd.ExecuteNonQuery();
                        }
                        strInfo.Append("GOVCORP_ASSET " + result + " rows deleted. ");
                        sql = "delete EJVBOND where ASSET_ID='" + ds.Tables[0].Rows[i][0] + "'";
                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            result=cmd.ExecuteNonQuery();
                        }
                        strInfo.Append("EJVBOND " + result + " rows deleted.\n\n");
                    }
                    conn.Close();
                    strInfo.Append("Completed!\n");
                }

                var endTime = DateTime.UtcNow;
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
            get { return "DeleteBondByAssetID"; }
        }

        #endregion
    }
}
