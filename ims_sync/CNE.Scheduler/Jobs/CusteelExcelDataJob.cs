using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using Quartz;
using CNEToolsEntities;
using CNE.Scheduler.Extension;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace CNE.Scheduler.Jobs
{

    public class CusteelExcelDataJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {

            string attFetcherSavingPath = ConfigurationManager.AppSettings["CusteelExcelSavingPath"];

            string senderFilter = ConfigurationManager.AppSettings["CusteelExcelSenderFilter"];

            var titleFilter = ConfigurationManager.AppSettings["CusteelExcelTitleFilter"];

            CommMailBiz.ExcuteMailSync(JobType, DateTime.Now, attFetcherSavingPath, senderFilter, titleFilter, ConfigurationManager.AppSettings["CnEUserName"], ConfigurationManager.AppSettings["CnEPassWord"], o => WriteLogEntity(o), (a, b) =>
            {
                for (int i=a.Count-1;i>=0;i-- )
                {
                    var tempattachname = a[i];
                    if (tempattachname.Contains("重点企业营销分品种"))
                    {
                        b.AppendFormat(" [read excel : zhongdianqiyeyingxiaofenpinzhong] ");
                        CusteelMarketingExcelManager manager = new CusteelMarketingExcelManager();
                        manager.GetCellsBy(tempattachname, b);
                    }

                    else if (tempattachname.Contains("重点企业流向")||tempattachname .Contains ("重点钢企流向") )
                    {
                        CompanyFlowManager manager = new CompanyFlowManager();
                        manager.GetCellsByFirstSheet(tempattachname, b);
                    }

                    else if (tempattachname.Contains("路透数据"))
                    {
                        var processor = new CusteelReutersUnnormalizedData();
                        processor.ProcessData(tempattachname, b);

                    }
                }
            }
            , (a, b) =>
            {
                //merge data,from temp to permanent
                MergeData merge = new MergeData();
                merge.ExecuteCusteelExcel(a, b);
            });
        }
        #region Overrides of LunaJobObject

        /// <summary>
        /// 
        /// </summary>
        public override string JobType
        {
            get { return "CusteelExcel"; }
        }

        #endregion
    }
}
