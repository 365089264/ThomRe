using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNEToolsEntities;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Extension
{
    public class CommMailBiz
    {
        public static void ExcuteMailSync(string jobType, DateTime syncStartTime, string attFetcherSavingPath, string senderFilter, string titleFilter, string userName, string password
            , Action<SCHEDULERLOG> saveLog
            , Action<List<string>, StringBuilder> syncCallback
            , Action<OracleConnection, OracleTransaction> mergeCallback
            )
        {
            string connStr = ConfigurationManager.AppSettings["mergeData"];
            OracleConnection conn = new OracleConnection(connStr);
            conn.Open();
            OracleTransaction tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            var lastSyncTime = syncStartTime;
            using (var cneEntities = new CnEEntities())
            {
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == jobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value;
                }
            }

            try
            {
                #region 执行数据同步程序

                strInfo.AppendFormat("Source [Type: Email , Sender: {0} ,Title {1} ]\n",
                                senderFilter, titleFilter);
                strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", "Oracle",
                                     connStr);
                var attFetcherSever = ConfigurationManager.AppSettings["CnEServer"];
                int attFetcherPort = int.Parse(ConfigurationManager.AppSettings["CnEPort"]);
                bool attFetcherUsingSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["CnEUsingSsl"]);
                string attFetcherUserName = userName; 
                string attFetcherPassWord = password;

                var attFetcher = new IMAP4AttFetcher(attFetcherSever,
                                                                 attFetcherPort,
                                                                 attFetcherUsingSsl,
                                                                 attFetcherUserName,
                                                                 attFetcherPassWord,
                                                                 attFetcherSavingPath);
                var attFetcherSenderFilter = new SenderFilter();
                /*Filter example. If there are more filters , please write like below.*/
                string[] senders = senderFilter.Split(';');
                foreach (var sender in senders)
                {
                    var senderRegex = new Regex(sender, RegexOptions.Compiled);
                    attFetcherSenderFilter.SetRule(senderRegex);
                }
                attFetcher.AppendFilter(attFetcherSenderFilter);

                if (!string.IsNullOrEmpty(titleFilter))
                {
                    var attFetcherTitleFilter = new TitleFilter();
                    var titleRegex = new Regex(titleFilter, RegexOptions.Compiled);
                    attFetcherTitleFilter.SetRule(titleRegex);
                    attFetcher.AppendFilter(attFetcherTitleFilter);
                }
                attFetcher.Execute(lastSyncTime);

                /*Get the saved attachment names.*/
                var attachmentFileNames = attFetcher.GetAttachmentFileNames();
                //var attachmentFileNames=new List<string>();
                //attachmentFileNames.Add(@"C:\DataFeedApp\Scheduler\CnE\路透数据20161109.xls");
                StringBuilder sb = new StringBuilder();
                
                //var attachmentFileNames = Directory.GetFiles(@"C:\xx\troil").ToList<string>();
                syncCallback(attachmentFileNames, sb);

                var fold = new DirectoryInfo(attFetcherSavingPath);
                if (fold.Exists)
                {
                    FileInfo[] files = fold.GetFiles();
                    foreach (FileInfo f in files)//删除目录下所有文件
                    {
                        f.Delete();
                    }
                }
                if (attachmentFileNames.Count == 0)
                {
                    strInfo.Append("No find files .\r\n");
                }
                #endregion
                var endTime = DateTime.UtcNow;
                logEntity.ENDTIME = endTime;
                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo + sb.ToString();
                saveLog(logEntity);
                //merge data from temp to persistence

                if (mergeCallback != null)
                    mergeCallback(conn, tran);

                tran.Commit();
            }
            catch (Exception exception)
            {
                tran.Rollback();

                logEntity.ENDTIME = DateTime.UtcNow.AddDays(-1);
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + exception;
                saveLog(logEntity);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
