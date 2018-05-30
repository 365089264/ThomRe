using System;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Luna.DataSync.Setting;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Jobs
{
    public class CoffedDataJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var connStr = ConfigurationManager.AppSettings["mergeData"];
            var conn = new OracleConnection(connStr);
            conn.Open();
            OracleTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();
            strInfo.AppendFormat("Source [Email:{0}]\n", ConfigurationManager.AppSettings["CoffedSenderFilter"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: {0}]\n", connStr);
            strInfo.AppendFormat("Destination Table:Cofeed\n" + "\r\n");

            var lastSyncTime = Convert.ToDateTime("2016-09-03");
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
                StringBuilder sb = new StringBuilder();
                #region 执行数据同步程序
                var attFetcherSever = ConfigurationManager.AppSettings["CnEServer"];
                int attFetcherPort = int.Parse(ConfigurationManager.AppSettings["CnEPort"]);
                bool attFetcherUsingSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["CnEUsingSsl"]);
                string attFetcherUserName = ConfigurationManager.AppSettings["CnEUserName"];
                string attFetcherPassWord = ConfigurationManager.AppSettings["CnEPassWord"];
                string attFetcherSavingPath = ConfigurationManager.AppSettings["CoffedSavingPath"];
                var attFetcher = new IMAP4AttFetcher(attFetcherSever,
                                                                 attFetcherPort,
                                                                 attFetcherUsingSsl,
                                                                 attFetcherUserName,
                                                                 attFetcherPassWord,
                                                                 attFetcherSavingPath);

                var attFetcherSenderFilter = new SenderFilter();
                string[] senders = ConfigurationManager.AppSettings["CoffedSenderFilter"].Split(';');
                foreach (var sender in senders)
                {
                    var senderRegex = new Regex(sender, RegexOptions.Compiled);
                    attFetcherSenderFilter.SetRule(senderRegex);
                }
                attFetcher.AppendFilter(attFetcherSenderFilter);
                var attFetcherTitleFilter = new TitleFilter();
                var titleRegex = new Regex(ConfigurationManager.AppSettings["CoffedTitleFilter"], RegexOptions.Compiled);
                attFetcherTitleFilter.SetRule(titleRegex);
                attFetcher.AppendFilter(attFetcherTitleFilter);
                attFetcher.Execute(lastSyncTime);
                var attachmentFileNames = attFetcher.GetAttachmentFileNames();

                foreach (var tempattachname in attachmentFileNames)
                {
                    CofeedManager manager = new CofeedManager();
                    manager.GetCellsByFirstSheet(tempattachname, sb);
                }
                strInfo.Append(sb);
                var fold = new DirectoryInfo(attFetcherSavingPath);
                if (fold.Exists)
                {
                    FileInfo[] files = fold.GetFiles();
                    foreach (FileInfo f in files)//删除目录下所有文件
                    {
                        f.Delete();
                    }
                }
                #endregion
                if (attachmentFileNames.Count > 0)
                {
                    MergeData merge = new MergeData();
                    merge.Execute(conn, tran);
                    tran.Commit();

                    strInfo.Append("Execute Procedure : createMaxAgricultrueTable \r\n");
                    strInfo.Append("Insert Into Table : GDT_AgricultureMax \r\n");
                   
                }
                else
                {
                    strInfo.Append("No files found\r\n");
                }
                var settingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    @"config\Ftp-Cofeed-data-sync.xml");
                var settingManager = new FtpSyncXmlManager(File.ReadAllText(settingFilePath), lastSyncTime, startTime.AddHours(8));
                settingManager.Init();
                var ftpSync = new FtpSyncLoad(settingManager, strInfo);
                ftpSync.Excute();
                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();
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
            finally
            {
                conn.Close();
            }

        }
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "CofeedPriceData"; }
        }

        #endregion
    }
}
