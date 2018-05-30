using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAV.Model.Data.Bond;

namespace VAV.DAL.Report
{
    public interface IBondReportRepository
    {
        /// <summary>
        /// Repository for getting issue rate
        /// </summary>
        /// <param name="bondIssueParams"></param>
        /// <returns></returns>
        IEnumerable<BondIssueRate> GetBondIssueRatesRepo(BondIssueParams bondIssueParams);

        /// <summary>
        /// Repository for geeting issue amount
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<BondIssueAmount> GetBondIssueAmountNew(BondIssueAmountParams param);
        /// <summary>
        /// Get bond detail by type and date
        /// </summary>
        /// <param name="reportParams"></param>
        /// <returns></returns>
        IEnumerable<BondDetail> GetBondDetailByTypeAndSubType(BondDetailParams reportParams);

        /// <summary>
        /// Get bond depository data for isssue bond and mutarity bond
        /// </summary>
        /// <param name="param"></param>
        /// <param name="isIssued"></param>
        /// <returns></returns>
        IEnumerable<BondDetail> GetBondDetailByType(BondDetailParams param);

        /// <summary>
        /// Get type order dictionary
        /// </summary>
        /// <returns></returns>
        Dictionary<string, int> GetTypeOrder();
    } 
}
