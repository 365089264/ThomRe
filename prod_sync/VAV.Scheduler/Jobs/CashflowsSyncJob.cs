using System;
using System.Linq;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Quartz;
using System.IO;
using Luna.DataSync.Setting;
using System.Configuration;
using System.Data.EntityClient;
using Luna.DataSync.Core;
using VAVToolsEntities;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using VAV.Scheduler.Util;
using System.Xml;
using System.Collections.Generic;
using VAV.Scheduler.TRACS;
using System.Globalization;

namespace VAV.Scheduler.Jobs
{
    public class CashflowsSyncJob : VavJobObject
    {
        /// <summary>
        /// Sync data to CashflowsInfo table
        /// </summary>
        /// <seealso cref="M:Spring.Scheduling.Quartz.QuartzJobObject.Execute(Quartz.JobExecutionContext)"/>
        int syncCount = 0;
        StringBuilder syncBondId = new StringBuilder();
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var strInfo = new StringBuilder();
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType};
            string settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                      @"config\Cashflows-data-sync.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(settingFilePath);
            string dbType = doc.SelectSingleNode("settings/destination-db/type").FirstChild.Value;
            string dbConn = doc.SelectSingleNode("settings/destination-db/conn").FirstChild.Value;
            var bondIDs = GetBondIDs(dbConn);
            DateTime initialDate = new DateTime(1949, 10, 1);
            string cmdText;
            int countPer=int.Parse(doc.SelectSingleNode("settings/countPer").FirstChild.Value);
            string userId = doc.SelectSingleNode("settings/lunaRequestUserId").FirstChild.Value;
            syncCount = 0;
            syncBondId.Clear();
            List<Tuple<string, DateTime>> batchList = new List<Tuple<string, DateTime>>();
            var tracsClient = new TRACS.FixedIncomeAnalyticsClient();

            strInfo.AppendFormat("Source, Destination [Type: {0} Address: {1}]\n", dbType, dbConn);
            strInfo.AppendFormat("Cashflows_SYNC expected to Sync {0} BondIDs.\n", bondIDs.Count());

            try
            {
                for (int i = 0; i < bondIDs.Count(); i++)
                {
                    batchList.Add(bondIDs[i]);
                    if ((batchList.Count() >= countPer) || (i == bondIDs.Count() - 1))//countPer
                    {
                        
                        var lunaRequest = new LunaRequest();
                        var requestItems = new List<LUNAFixedRateBondRequest>();
                        foreach (var bondID in batchList)
                        {
                            requestItems.Add(new LUNAFixedRateBondRequest()
                            {
                                BondId = bondID.Item1,
                                AsOfDate = bondID.Item2.Date,
                                Price = 1,
                                CalculateFloatingRateNoteAsFixedRateBond = true,
                                RequestTimestamp = DateTime.Now.ToString("o")
                            });
                        }
                        lunaRequest.Items = requestItems.ToArray();
                        lunaRequest.UserId = userId.Trim();
                        lunaRequest.ApplicationId = "LUNA";
                        lunaRequest.Position = "127.0.0.1/net";

                        var response = tracsClient.LunaPricer(lunaRequest);
                        cmdText = BuildSqlCmdTextForInsertData(response);
                        if (cmdText != string.Empty)
                        {
                            InsertDatas(dbConn, cmdText);
                        }
                        batchList.Clear();
                    }
                }
                logEntity.JobStatus = JobStatus.Success;
            }
            catch (Exception exception)
            {
                logEntity.JobStatus = JobStatus.Fail;
                strInfo.Append( "\n" + exception);
            }
            finally
            {
                strInfo.AppendFormat("It processed {0} BondIDs:{1}\n", syncCount.ToString(), string.IsNullOrEmpty(syncBondId.ToString())?"none":syncBondId.ToString());
                logEntity.RUNDETAIL = strInfo.ToString();
                logEntity.ENDTIME = DateTime.UtcNow;
                WriteLogEntity(logEntity);
            }
        }

        private string BuildSqlCmdTextForInsertData(TRACS.LunaResponse response)
        {
            StringBuilder sb = new StringBuilder("begin\n");
            bool isAllNull = true;
            if (response.Items == null)
                return string.Empty;
            for (int i = 0; i < response.Items.Count(); i++)
            {
                if (response.Items[i].Cashflows != null && response.Items[i].Cashflows.Count() != 0)
                {
                    syncCount++;
                    syncBondId.Append(response.Items[i].BondId+" ");
                    isAllNull = false;
                    for (int j = 0; j < response.Items[i].Cashflows.Count(); j++)
                    {
                        var currentItem = response.Items[i];
                        int isCoupon = j == currentItem.Cashflows.Count() - 1 ? 0 : 1;

                        double? couponValue = double.IsInfinity(currentItem.Cashflows[j].CouponValue.Value) ? null : currentItem.Cashflows[j].CouponValue;
                        double? annualRate = currentItem.Cashflows[j].AnnualRate.Value < 0 ? null : currentItem.Cashflows[j].AnnualRate;
                        double? couponPercentage = currentItem.Cashflows[j].CouponPercentage.Value < 0 ? null : currentItem.Cashflows[j].CouponPercentage;
                        sb.AppendFormat("INSERT INTO CashflowsInfo(BONDID,CASHFLOWDATE,ANNUALRATE,CAPITALPERCENT,CAPITALVALUE,COUPONPERCENTAGE,COUPONVALUE,ISCOUPON) VALUES ( '{0}',to_date('{1}', 'mm/dd/yyyy hh24:mi:ss'),'{2}','{3}','{4}','{5}','{6}','{7}');", currentItem.BondId, currentItem.Cashflows[j].Date.ToString("MM/dd/yyyy HH:mm:ss"), annualRate, currentItem.Cashflows[j].CapitalPercent, currentItem.Cashflows[j].CapitalValue, couponPercentage, couponValue, isCoupon);
                    }
                }
            }
            if (isAllNull) return string.Empty;
            else sb.Append("end;");
            //else
            //{
            //    sb.Remove(sb.Length - 2, 2);
            //    sb.Append(");");

            //    sb.Replace("''", "null");
            //    return sb.ToString();
            //}
            return sb.ToString();
        }

        private List<Tuple<string, DateTime>> GetBondIDs(string dbConn)
        {
            using (var conn = new OracleConnection(dbConn))
            {
                List<Tuple<string, DateTime>> BondIDs = new List<Tuple<string, DateTime>>();
              
                conn.Open();
                string cmdText = "select AssetId,IssueDate from V_BOND_EN where (select count(1) as num from CashflowsInfo where CashflowsInfo.BondID = V_BOND_EN.AssetId) = 0 AND COUPONCLASSCD='FIX' AND BONDCLASSCD='BTB' and MaturityDate > sysdate and issueDate is not null";
                var cmd = new OracleCommand(cmdText, conn);
                var thisSqlDataReader = cmd.ExecuteReader();
                while (thisSqlDataReader.Read())
                {

                    var tuple = Tuple.Create(thisSqlDataReader[0].ToString(), (DateTime)thisSqlDataReader[1]);
                    BondIDs.Add(tuple);
                }
                return BondIDs;
            }
        }

        private void InsertDatas(string dbConn,string cmdText)
        {
            using (var conn = new OracleConnection(dbConn))
            {
                conn.Open();
                var cmd = new OracleCommand(cmdText, conn);
                int rowsCount = cmd.ExecuteNonQuery();
            }
        }

        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Cashflows_SYNC"; }
        }

        #endregion
    }
}