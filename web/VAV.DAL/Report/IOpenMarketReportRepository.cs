using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAV.Model.Data.Bond;
using VAV.Model.Data.OpenMarket;
using System.Data;

namespace VAV.DAL.Report
{
    public interface IOpenMarketReportRepository
    {
        /// <summary>
        /// Get Open market repo data.
        /// </summary>
        /// <param name="reportPara"></param>
        /// <returns></returns>
        IEnumerable<OpenMarketRepo> GetOpenMarketRepo(DetailDataReportParams reportPara);

        DataTable GetImmaturityAmount(DateTime queryDate);
    } 
}
