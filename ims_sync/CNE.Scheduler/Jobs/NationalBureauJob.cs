using System;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CNE.Scheduler.Jobs
{
    public class NationalBureauJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            var lastSyncTime = Convert.ToDateTime("2014-01-21");
            using (var cneEntities = new CnEEntities())
            {
                var date =
                    cneEntities.SCHEDULERLOGs.Where(x => x.STATUS == 0 && x.JOBTYPE == JobType).Select(
                        x => (DateTime?)x.STARTTIME).Max();
                if (date != null)
                {
                    lastSyncTime = date.Value;
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
                string attFetcherSavingPath = ConfigurationManager.AppSettings["NationaSavingPath"];
                var attFetcher = new IMAP4AttFetcher(attFetcherSever,
                                                                 attFetcherPort,
                                                                 attFetcherUsingSsl,
                                                                 attFetcherUserName,
                                                                 attFetcherPassWord,
                                                                 attFetcherSavingPath);
                var attFetcherSenderFilter = new SenderFilter();
                /*Filter example. If there are more filters , please write like below.*/
                string[] senders = ConfigurationManager.AppSettings["NationaSenderFilter"].Split(';');
                foreach (var sender in senders)
                {
                    var senderRegex = new Regex(sender, RegexOptions.Compiled);
                    attFetcherSenderFilter.SetRule(senderRegex);
                }
                attFetcher.AppendFilter(attFetcherSenderFilter);

             
                //var attFetcherTitleFilter = new TitleFilter();
                //var titleRegex = new Regex(ConfigurationManager.AppSettings["NationaTitleFilter"], RegexOptions.Compiled);
                //attFetcherTitleFilter.SetRule(titleRegex);
                //attFetcher.AppendFilter(attFetcherTitleFilter);
                attFetcher.Execute(lastSyncTime);
                var attachmentFileNames = attFetcher.GetAttachmentFileNames();
                strInfo.AppendFormat("Source [Type: Email , Sender: {0}  ]\n",
                            ConfigurationManager.AppSettings["NationaSenderFilter"]);
                strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", "Oracle",
                    ConfigurationManager.AppSettings["CnECon"]);
                foreach (var tempattachname in attachmentFileNames)
                {
                    var nb = new NationalBureau();
                    nb.ImportTheWholeExcel(tempattachname, strInfo);
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
                if (attachmentFileNames.Count == 0)
                {
                    strInfo.Append("No find files .\r\n");
                }

                #endregion

                var endTime = DateTime.UtcNow;
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
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "NationalBureau"; }
        }

        #endregion
    }
}
