using System;
using System.Configuration;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using CNE.Scheduler.HTML_PDF_Tool;

namespace CNE.Scheduler.Jobs
{

    public class BOCCrawler2PDFJob : CmaJobBase
    {
        public static bool IsRunningBOCCrawler2PDF = false;
        private static readonly object LockObj = new object();

        protected override void ExecuteInternal(JobExecutionContext context)
        {
            lock (LockObj)
            {
                if (IsRunningBOCCrawler2PDF)
                    return;
                IsRunningBOCCrawler2PDF = true;
            }

            var startTime = DateTime.UtcNow;
            var logEntity = new SCHEDULERLOG { STARTTIME = startTime, JOBTYPE = JobType };
            var strInfo = new StringBuilder();
            try
            {
                strInfo = SyncBOCData(startTime);

                logEntity.JobStatus = JobStatus.Success;
                logEntity.RUNDETAIL = strInfo.ToString();

            }
            catch (Exception e)
            {
                logEntity.JobStatus = JobStatus.Fail;
                logEntity.RUNDETAIL = strInfo + "\n" + e;
            }
            finally
            {
                logEntity.ENDTIME = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(logEntity.RUNDETAIL))
                    WriteLogEntity(logEntity);
                lock (LockObj)
                {
                    IsRunningBOCCrawler2PDF = false;
                }

            }
        }

        protected StringBuilder SyncBOCData(DateTime startTime)
        {
            var strInfo = new StringBuilder();

            var title = "";
            var url = HtmlManager.GetUrl(out title);

            var ch = new ReportManager();
            if (ch.IsBocFileExists(title))
            {
                return new StringBuilder("");
            }

            strInfo.AppendFormat("Job begin at {0}\n", startTime);
            strInfo.AppendFormat("Source [Type: WebPage, Address: {0}]\n", ConfigurationManager.AppSettings["BOC_URL"]);
            strInfo.AppendFormat("Destination [Type: Oracle DB,  Address: " + ConfigurationManager.AppSettings["reportConnstr"] + "]\n");
            strInfo.AppendFormat("Destination [Type: WebService, Address: {0}]\n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"));

            #region 执行数据同步程序
            PdfSaverByPechkin saver = new PdfSaverByPechkin();
            var filePath = string.Empty;
            var titStr = string.Empty;
            var auStr = string.Empty;
            saver.SaveByHtml(url, out filePath, out titStr, out auStr);
            auStr = auStr.Replace("&nbsp;", "");

            //call storage service, insert to filedb
            ch.InsertReportFile(filePath, "BOC", "FX", "FXDaily", auStr, strInfo);
            HtmlManager.DelFiles(filePath);
            #endregion

            var endTime = DateTime.UtcNow;
            strInfo.AppendFormat("Synchronization completed at {0}.\n", endTime);

            return strInfo;
        }

        #region Overrides
        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "BOCCrawler2PDF"; }
        }

        #endregion

    }
}
