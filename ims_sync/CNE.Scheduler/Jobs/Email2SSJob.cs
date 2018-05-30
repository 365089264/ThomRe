using System;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using System.Configuration;
using CNE.Scheduler.Extension;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using CNE.Scheduler.Extension.Model;
using System.Collections;
using Luna.DataSync.Core;

namespace CNE.Scheduler.Jobs
{
    public class EmailInfo
    {
        public string Etime { get; set; }
        public string Ename { get; set; }
        public string CurrentAttrName { get; set; }

    }

    public class Email2SSJob : CmaJobBase
    {
        public static bool IsRunningReportJob = false;
        private static readonly object LockObj = new object();

        private static StringBuilder strInfo;
        private string attFetcherSavingPath = ConfigurationManager.AppSettings["ReportSavingPath"];
        private IMAP4AttFetcher attFetcher { get; set; }
        public Email2SSJob()
        {

            var attFetcherSever = ConfigurationManager.AppSettings["CnEServer"];
            int attFetcherPort = int.Parse(ConfigurationManager.AppSettings["CnEPort"]);
            bool attFetcherUsingSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["CnEUsingSsl"]);
            string attFetcherUserName = ConfigurationManager.AppSettings["CnEUserName"];
            string attFetcherPassWord = ConfigurationManager.AppSettings["CnEPassWord"];

            var attfetcher = new IMAP4AttFetcher(attFetcherSever,
                                                             attFetcherPort,
                                                             attFetcherUsingSsl,
                                                             attFetcherUserName,
                                                             attFetcherPassWord,
                                                             attFetcherSavingPath);
            this.attFetcher = attfetcher;
        }

        private void ConfigSenderFilter(string emails, SenderFilter attFetcherSenderFilter)
        {
            string[] emailList = emails.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < emailList.Length; i++)
            {
                var senderRegex = new Regex(emailList[i], RegexOptions.Compiled);
                attFetcherSenderFilter.SetRule(senderRegex);
            }
        }
        private SenderFilter CreateAndConfigSenderFilter()
        {
            var attFetcherSenderFilter = new SenderFilter();
            string sql = "select distinct(email) from EMAILKEYWORDS";
            DataTable tb = DBHelper.GetDataTableBySql(sql);

            if (tb != null && tb.Rows.Count > 0)
            {
                IEnumerator itor = tb.Rows.GetEnumerator();
                while (itor.MoveNext())
                {
                    DataRow dr = itor.Current as DataRow;
                    string email = dr["email"].ToString();
                    ConfigSenderFilter(email, attFetcherSenderFilter);
                }

            }


            return attFetcherSenderFilter;
        }
        private List<EmailDetail> GetEmailList(DateTime lastSyncTime, SenderFilter attFetcherSenderFilter)
        {


            attFetcher.AppendFilter(attFetcherSenderFilter);
            attFetcher.Execute(lastSyncTime);
            List<EmailDetail> emails = attFetcher.GetEmailDetailList();
            return emails;

        }
        private void DeleteAllFilesInDirectory(string attFetcherSavingPath)
        {
            var fold = new DirectoryInfo(attFetcherSavingPath);
            if (fold.Exists)
            {
                FileInfo[] files = fold.GetFiles();
                foreach (FileInfo f in files)//删除目录下所有文件
                {
                    f.Delete();
                }
            }
        }


        private int ManagerEmails(EmailDetail detail)
        {
            int successCount = 0;;
            var info = new EmailInfo { Ename = detail.Sender, Etime = detail.ReceiveTime };
            var reportManager = new ReportManager();
            foreach (var t in detail.AttachNames)
            {
                info.CurrentAttrName = t;
                if (reportManager.InsertReportToService(info, strInfo))
                {
                    //attFetcher.DeleteEmailByID(detail.ID, detail.BelongMailBox);
                    successCount++;
                }
            }
            return successCount;
        }

        public void WriteMailLogEntity(GETMAILLOG logEntity)
        {
            var paras = new OracleParameter[4];
            paras[0] = new OracleParameter("I_GETMAILDATE", OracleType.DateTime);
            paras[0].Value = logEntity.GETMAILDATE;
            paras[1] = new OracleParameter("I_STATUS", OracleType.Number);
            paras[1].Value = logEntity.STATUS;
            paras[2] = new OracleParameter("I_JOBTYPE", OracleType.VarChar);
            paras[2].Value = JobType;
            paras[3] = new OracleParameter("I_DESCRIPTION", OracleType.Clob);
            paras[3].Value = logEntity.DESCRIPTION;

            DBHelper.ExecuteLogStorage("GETMAILLOG_INSERT", paras);
        }

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            lock (LockObj)
            {
                if (IsRunningReportJob)
                    return;
                IsRunningReportJob = true;
            }

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };
            try
            {
                var descption =  SyncReportData();

                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = descption;
            }
            catch (Exception e)
            {
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = "update failed!" + "\n" + e;
            }
            finally
            {
                logEntity.ENDTIME = DateTime.UtcNow;
                WriteLogEntity(logEntity);

                lock (LockObj)
                {
                    IsRunningReportJob = false;
                }

            }
        }

        private string SyncReportData()
        {
            strInfo = new StringBuilder();
            strInfo.AppendFormat("ReportData  Sync begin at {0}\n", DateTime.UtcNow);
            strInfo.AppendFormat("Source [Type: Email, CnEUserName: " + ConfigurationManager.AppSettings["CnEUserName"] + ", CnEPassWord: " + ConfigurationManager.AppSettings["CnEPassWord"] + "]\n");
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: " + ConfigurationManager.AppSettings["reportConnstr"] + "]\n");
            strInfo.AppendFormat("Destination [Type: WebService, Address: " + ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort") + "]\n");


            var lastSyncTime = GetLastSyncTime();
            //string sql = "SELECT MAX(MTIME) FROM FILEDETAIL";
            //var lastSyncTime = Convert.ToDateTime(DBHelper.ExecuteScaler(sql).ToString());

            strInfo.AppendFormat("Last successfully sync time : {0}\n", lastSyncTime);
            var attFetcherSenderFilter = CreateAndConfigSenderFilter();

            #region 执行数据同步程序


            List<EmailDetail> emails = GetEmailList(lastSyncTime, attFetcherSenderFilter);
            attFetcher.OpenConnection();
            int successAttachCount = 0;
            foreach (EmailDetail detail in emails)
            {
                successAttachCount += ManagerEmails(detail);
            }
            attFetcher.CloseConnection();
            DeleteAllFilesInDirectory(attFetcherSavingPath);
            #endregion

            strInfo.AppendFormat("Synchronization completed at {0}.\n", DateTime.UtcNow);

            return "update success!updatefilecount:" + successAttachCount + "\n[" + strInfo + "]";
        }
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Email2SS"; }
        }

        #endregion
    }
}
