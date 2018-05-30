using System;
using System.Collections.Generic;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Jobs
{
    public class LongZhongExcelJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            string connStr = ConfigurationManager.AppSettings["mergeData"];
            OracleConnection conn = new OracleConnection(connStr);
            conn.Open();
            OracleTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();
            strInfo.AppendFormat("Source [Email:{0},Title:{1}]\n", ConfigurationManager.AppSettings["LongZhongSenderFilter"], ConfigurationManager.AppSettings["LongZhongTitleFilter"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", connStr);
            var lastSyncTime = Convert.ToDateTime("2016-11-16");
            using (var cneEntities = new CnEEntities())
            {
                 var date =
                   cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                       x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    //ToGMT8
                    lastSyncTime = date.Value.AddHours(8);
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
                string attFetcherSavingPath = ConfigurationManager.AppSettings["LongZhongSavingPath"];
                var attFetcher = new IMAP4AttFetcher(attFetcherSever,
                                                                 attFetcherPort,
                                                                 attFetcherUsingSsl,
                                                                 attFetcherUserName,
                                                                 attFetcherPassWord,
                                                                 attFetcherSavingPath);
                var attFetcherSenderFilter = new SenderFilter();
                /*Filter example. If there are more filters , please write like below.*/
                string[] senders = ConfigurationManager.AppSettings["LongZhongSenderFilter"].Split(';');
                foreach (var sender in senders)
                {
                    var senderRegex = new Regex(sender, RegexOptions.Compiled);
                    attFetcherSenderFilter.SetRule(senderRegex);
                }
                attFetcher.AppendFilter(attFetcherSenderFilter);
                var attFetcherTitleFilter = new TitleFilter();
                var titleRegex = new Regex(ConfigurationManager.AppSettings["LongZhongTitleFilter"], RegexOptions.Compiled);
                attFetcherTitleFilter.SetRule(titleRegex);
                attFetcher.AppendFilter(attFetcherTitleFilter);
                attFetcher.Execute(lastSyncTime);
                /*Get the saved attachment names.*/
                var attachmentFileNames = attFetcher.GetAttachmentFileNames();
                //List<string> attachmentFileNames = Directory.GetFiles(@"C:\DataFeedApp\Scheduler\CnE\LongZhong").ToList<string>();
                
                foreach (var tempattachname in attachmentFileNames)
                {

                    LongZhongExcelManager manager = new LongZhongExcelManager();

                    manager.GetCellsByFirstSheet(tempattachname, strInfo);
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
                    MergeData merge = new MergeData();
                    merge.ExecuteLongZhongExcel(conn, tran);
                    tran.Commit();

                    strInfo.Append("Execute Procedure : CREATEMAXENERGYYIELDTABLE, CREATEMAXCHEMISTRYOUTPUTTABLE \r\n");
                    strInfo.Append("Insert Into Table : GDT_CHEMISTRYOUTPUTMAX, GDT_ENERGYYIELDMAX \r\n");
                }
                else
                {
                    strInfo.Append("No files found\r\n");
                }

                #endregion

                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
                WriteLogEntity(logEntity);
              
                //merge data from temp to persistence
             
            }
            catch (Exception exception)
            {
                tran.Rollback();

                logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                WriteLogEntity(logEntity);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

        }
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "LongZhongOutput"; }
        }

        #endregion
    }
}
