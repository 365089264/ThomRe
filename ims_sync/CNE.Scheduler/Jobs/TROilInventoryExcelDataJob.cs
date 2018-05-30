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
using Aspose.Cells;
using System.Data;

namespace CNE.Scheduler.Jobs
{

    public class TROilInventoryExcelDataJob : CmaJobBase
    {
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            string attFetcherSavingPath = ConfigurationManager.AppSettings["TROilInventoryExcelSavingPath"];
            string senderFilter = ConfigurationManager.AppSettings["TROilInventoryExcelSenderFilter"];
            var titleFilter = ConfigurationManager.AppSettings["TROilInventoryExcelTitleFilter"];

            CommMailBiz.ExcuteMailSync(JobType, DateTime.Now, attFetcherSavingPath, senderFilter, titleFilter, ConfigurationManager.AppSettings["CnEUserName"], ConfigurationManager.AppSettings["CnEPassWord"], o => WriteLogEntity(o), (a, b) =>
            {
                for (int i=a.Count-1;i>=0;i-- )
                {
                    var tempattachname = a[i];
                    TROilInventoryManager processor = new TROilInventoryManager();
                    processor.ProcessData(tempattachname, b);
                }
            }
            , 
             (a, b) =>
            {
                //merge data,from temp to permanent
                MergeData merge = new MergeData();
                merge.ExecuteOilInventoryMax(a, b);
            });
        }

        #region Overrides of LunaJobObject
        public override string JobType
        {
            get { return "TROilInventoryExcel"; }
        }
        #endregion
    }
}
