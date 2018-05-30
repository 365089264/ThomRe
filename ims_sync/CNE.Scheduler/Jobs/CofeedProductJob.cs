using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Text.RegularExpressions;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Jobs
{
    public class CofeedProductJob : CmaJobBase
    {
        public override string JobType
        {
            get { return "CofeedOutput"; }
        }

        protected override void ExecuteInternal(Quartz.JobExecutionContext context)
        {
            string connStr = ConfigurationManager.AppSettings["mergeData"].ToString();
            OracleConnection conn = new OracleConnection(connStr);
            conn.Open();
            OracleTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();
            strInfo.AppendFormat("Source [Email:{0}]\n", ConfigurationManager.AppSettings["CoffedProductSenderFilter"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", connStr);
            var lastSyncTime = Convert.ToDateTime("2016-11-14");
            using (var cneEntities = new CnEEntities())
            {
                var date =
                   cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                       x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value.AddMinutes(-10);
                }
            }

            try
            {
                #region 执行数据同步程序
                var attFetcherSever = ConfigurationManager.AppSettings["CnEServer"];
                int attFetcherPort = int.Parse(ConfigurationManager.AppSettings["CnEPort"]);
                bool attFetcherUsingSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["CnEUsingSsl"]);
                string attFetcherUserName = ConfigurationManager.AppSettings["CnEUserName"];
                string attFetcherPassWord = ConfigurationManager.AppSettings["CnEPassWord"];
                string attFetcherSavingPath = ConfigurationManager.AppSettings["CoffedProductSavingPath"];
                var attFetcher = new IMAP4AttFetcher(attFetcherSever,
                                                                 attFetcherPort,
                                                                 attFetcherUsingSsl,
                                                                 attFetcherUserName,
                                                                 attFetcherPassWord,
                                                                 attFetcherSavingPath);
                var attFetcherSenderFilter = new SenderFilter();
                string[] senders = ConfigurationManager.AppSettings["CoffedProductSenderFilter"].Split(';');
                foreach (var sender in senders)
                {
                    var senderRegex = new Regex(sender, RegexOptions.Compiled);
                    attFetcherSenderFilter.SetRule(senderRegex);
                }
                attFetcher.AppendFilter(attFetcherSenderFilter);

                attFetcher.Execute(lastSyncTime);
                var attachmentFileNames = attFetcher.GetAttachmentFileNames();

               // List<string> attachmentFileNames = Directory.GetFiles(@"C:\DataFeedApp\Scheduler\CnE\CoffedProductData").ToList<string>();

                for (int i = attachmentFileNames.Count - 1; i >= 0; i--)
                {
                    var tempattachname = attachmentFileNames[i];
                    if (tempattachname.IndexOf("Productinventory") != -1)
                    {
                        CofeedProductManager c = new CofeedProductManager();
                        c.GetCellsByFirstSheet(tempattachname, strInfo);
                    }
                    else if (tempattachname.IndexOf("week") != -1)
                    {
                        CoffedWeekManager c = new CoffedWeekManager();
                        c.GetCellsByFirstSheet(tempattachname, strInfo);

                    }
                }
                var fold = new DirectoryInfo(attFetcherSavingPath);
                if (fold.Exists)
                {
                    FileInfo[] files = fold.GetFiles();
                    foreach (FileInfo f in files)//删除目录下所有文件
                    {
                        f.Delete();
                    }
                }
                if (attachmentFileNames.Count > 0)
                {
                    MergeData m = new MergeData();
                    m.ExecuteCoffedAndMetalMax(conn, tran);
                    strInfo.Append("Execute Procedure : CREATEMAXAGRICULTRUEOUTPUT\r\n");
                    strInfo.Append("Insert Into Table : GDT_AGRICULTRUEOUTPUTMAX, GDT_ENERGYYIELDMAX \r\n");
                    //生成MAX表；
                    //ExecuteCoffedAndMetalMax
                    tran.Commit();
                }
                else
                {
                    strInfo.Append("No files found\r\n");
                }
;
                #endregion

                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString() ;
                WriteLogEntity(logEntity);
            }
            catch (Exception exception)
            {
                tran.Rollback();

                logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                WriteLogEntity(logEntity);
            }

        }
    }
}
