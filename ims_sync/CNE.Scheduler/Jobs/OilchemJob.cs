using System;
using System.Linq;
using System.Text;
using Quartz;
using CNEToolsEntities;
using System.IO;
using CNE.Scheduler.Extension;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CNE.Scheduler.Jobs
{
    public class OilchemJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime };

            var strInfo = new StringBuilder();

            strInfo.AppendFormat("Source [Type: {0} Address: {1} Title: {2}]\n", "Email",
                                 ConfigurationManager.AppSettings["OilChemSenderFilter"],
                                 ConfigurationManager.AppSettings["OilChemTitleFilter"]);
            
            strInfo.AppendFormat("Destination [Type: {0} Address: {1}]\n", "ORACLE",
                                 ConfigurationManager.AppSettings["CnECon"]);

            var lastSyncTime = DateTime.Now;
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
                /*Attachment fetcher begins.*/
                string attFetcherSever = ConfigurationManager.AppSettings["CnEServer"];
                int attFetcherPort = int.Parse(ConfigurationManager.AppSettings["CnEPort"]);
                bool attFetcherUsingSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["CnEUsingSsl"]);
                string attFetcherUserName = ConfigurationManager.AppSettings["CnEUserName"];
                string attFetcherPassWord = ConfigurationManager.AppSettings["CnEPassWord"];
                string attFetcherSavingPath = ConfigurationManager.AppSettings["OilChemSavingPath"];
                var attFetcher = new IMAP4AttFetcher(attFetcherSever,
                                                                 attFetcherPort,
                                                                 attFetcherUsingSsl,
                                                                 attFetcherUserName,
                                                                 attFetcherPassWord,
                                                                 attFetcherSavingPath);
                var attFetcherSenderFilter = new SenderFilter();
                /*Filter example. If there are more filters , please write like below.*/
                string[] senders = ConfigurationManager.AppSettings["OilChemSenderFilter"].Split(';');
                foreach (var sender in senders)
                {
                    var senderRegex = new Regex(sender, RegexOptions.Compiled);
                    attFetcherSenderFilter.SetRule(senderRegex);
                }
                attFetcher.AppendFilter(attFetcherSenderFilter);
                var attFetcherTitleFilter = new TitleFilter();
                var titleRegex = new Regex(ConfigurationManager.AppSettings["OilChemTitleFilter"], RegexOptions.Compiled);
                attFetcherTitleFilter.SetRule(titleRegex);
                attFetcher.AppendFilter(attFetcherTitleFilter);
                attFetcher.Execute(lastSyncTime);
                /*Get the saved attachment names.*/
                var attachmentFileNames = attFetcher.GetAttachmentFileNames();


                foreach (var tempattachname in attachmentFileNames)
                {
                    var eos = new EnergyOilShandong(tempattachname);
                    eos.ImportTheWholeExcel(strInfo);
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

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "Oilchem_SYNC"; }
        }
    }
}
