using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Web.Configuration;
using VAV.Entities;
using VAV.Model.Data;
using VAV.Model.Data.Bond;
using VAV.Model.Data.OpenMarket;
using OpenMarketRepo = VAV.Model.Data.OpenMarket.OpenMarketRepo;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace VAV.DAL.Report
{
    public class OpenMarketReportRepository : BaseRepository, IOpenMarketReportRepository
    {
        
        public DataTable GetImmaturityAmount(DateTime queryDate)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("@queryDate",OracleDbType.TimeStamp, (OracleTimeStamp)queryDate,ParameterDirection.InputOutput),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output) 
                            };
            return GetOracleDataSetBySp("GETIMMATURITYAMOUNT", paramArray).Tables[0];
        }

        public IEnumerable<OpenMarketRepo> GetOpenMarketRepo(DetailDataReportParams reportPara)
        {
            #region
            List<OpenMarketRepo> repoRet = new List<OpenMarketRepo>();
            List<OpenMarketRepo> excludeExpiredRepo = GetOpenMarketRepoExcludeExpired(reportPara).ToList();
            if (reportPara.IncludeExpired)
            {
                IEnumerable<OpenMarketRepo> includeExpiredRepo = GetOpenMarketRepoIncludeExpired(reportPara);
                foreach (OpenMarketRepo item in excludeExpiredRepo)
                {
                    repoRet.Add(item);
                }
                foreach (OpenMarketRepo item in includeExpiredRepo)
                {
                    repoRet.Add(item);
                }
                return SetReportOption(repoRet);
            }
            #endregion
            return SetReportOption(excludeExpiredRepo);
        }

        /// <summary>
        /// Add value to the OptionType propity of the object got from the DB
        /// </summary>
        /// <param name="temp">The entity got from the database</param>
        /// <returns></returns>
        private IEnumerable<OpenMarketRepo> SetReportOption(List<OpenMarketRepo> repoList)
        {
            List<OpenMarketRepo> repo = new List<OpenMarketRepo>();

            IEnumerable<string> assetIds;
            using (var bonddb = new BondDBEntities())
            {
                assetIds = (from a in bonddb.V_ASSET where a.CDC_ASSET_CLASS_CD == "CBB" select a.ID_NUMBER).Distinct().ToList();
            }

            foreach (OpenMarketRepo item in repoList)
            {
                if (item.OperationType == null) item.OperationType = "";
                if (item.OperationTerm == null) item.OperationTerm = "";
                if (item.Direction == null) item.Direction = "";
                string subStr = item.Code.Substring(0, 2);
                if (subStr == "CN")
                {
                    string subStr1 = item.Code.Substring(item.Code.TrimEnd().Length - 8, 3);
                    string subStr2 = item.Code.Substring(item.Code.TrimEnd().Length - 7, 2);
                    string subMlf = item.Code.Substring(2, 3);
                    if (subMlf == "MLF")
                    {
                        
                        item.Category = "MLF";
                        item.OperationType = "MLF" + item.OperationType.Trim();
                        repo.Add(item);
                    }
                    else if (subStr1 == "RRP")
                    {
                        item.Category = "RRP";
                        item.OperationType = "RRP" + item.OperationType.Trim();
                        repo.Add(item);
                    }
                    else if (subStr2 == "RP")
                    {
                        item.Category = "RP";
                        item.OperationType = "RP" + item.OperationType.Trim();
                        repo.Add(item);
                    }
                    else if (subStr1 == "FMD")
                    {
                        item.Category = "FMD";
                        item.OperationType = "FMD" + item.OperationType.Trim();
                        repo.Add(item);
                    }
                }
                else
                {
                    //if (temp.Any(re => assetIds.Contains(re.Code.Trim())))
                    if (assetIds.Contains(item.Code.Trim()))
                    {
                        item.Category = "CBB";
                        item.OperationType = "CBB" + item.OperationType.Trim();
                        repo.Add(item);
                    }
                }
            }
            return repo;
        }

        /// <summary>
        /// select items by trade_dt when the expried checkbox is not checked
        /// </summary>
        /// <param name="reportPara"></param>
        /// <returns></returns>
        private IEnumerable<OpenMarketRepo> GetOpenMarketRepoExcludeExpired(DetailDataReportParams reportPara)
        {
            
            using (var VAVDB = new OpenMarketEntities())
            {
                return (from r in VAVDB.V_OPENMARKET
                        where DateTime.Compare((DateTime)r.ISSUEDATE, (DateTime)reportPara.StartDate) >= 0 && DateTime.Compare((DateTime)r.ISSUEDATE, (DateTime)reportPara.EndDate) <= 0
                        select new OpenMarketRepo
                        {
                            IsSumItem = false,
                            Code = r.RIC,
                            Date = r.ISSUEDATE,
                            IssueDate = r.ISSUEDATE,
                            MaturityDate = r.MATURITY_DT,
                            Direction = "",
                            OperationType = "",
                            AssetId = r.ASSET_ID,
                            Volume = (double)r.VOLUME,
                            Amount = (double)r.VOLUME,
                            PirceRate = (double)r.ISSUERATE,
                            RefRate = (double)r.YIELD,
                            OperationTerm = "",
                            Term = (int)r.TERM,
                            TermEn = r.TERM_EN,
                            TermCn = r.TERM_CN,
                            Category = ""
                        }).ToList();
            }
        }

        /// <summary>
        /// select items by maturity_dt when the expried checkbox is checked
        /// </summary>
        /// <param name="reportPara"></param>
        /// <returns></returns>
        private IEnumerable<OpenMarketRepo> GetOpenMarketRepoIncludeExpired(DetailDataReportParams reportPara)
        {
            using (var VAVDB = new OpenMarketEntities())
            {
                return (from r in VAVDB.V_OPENMARKET
                        where DateTime.Compare((DateTime)r.MATURITY_DT, (DateTime)reportPara.StartDate) > 0 && DateTime.Compare((DateTime)r.MATURITY_DT, (DateTime)reportPara.EndDate) < 0
                        select new OpenMarketRepo
                        {
                            IsSumItem = false,
                            Code = r.RIC,
                            Date = r.MATURITY_DT,
                            IssueDate = r.ISSUEDATE,
                            MaturityDate = r.MATURITY_DT,
                            Direction = "",
                            OperationType = "IE",
                            AssetId = r.ASSET_ID,
                            Volume = (double)r.VOLUME,
                            Amount = (double)r.VOLUME,
                            PirceRate = (double)r.YIELD,
                            RefRate = (double)r.YIELD,
                            OperationTerm = "",
                            Term = (int)r.TERM,
                            TermEn = r.TERM_EN,
                            TermCn = r.TERM_CN,
                            Category = ""
                        }).ToList();
            }
        }

        #region Open Market Search
        public DataTable GetOpenMarketOperation(string type, DateTime startDate, DateTime endDate, string unit, int includeExpired)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("@StartDate", (OracleTimeStamp)startDate), 
                                new OracleParameter("@EndDate", (OracleTimeStamp)endDate),
                                new OracleParameter("@CategoryType", type),
                                new OracleParameter("@IncludeExpired", includeExpired),
                                new OracleParameter("@Unit", unit),
                                new OracleParameter("@Culture", Thread.CurrentThread.CurrentUICulture.Name),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetOracleDataSetBySp("GetOpenMarkerSearch", paramArray).Tables[0];
        }

        public DataTable GetOpenMarketSLO(DateTime startDate, DateTime endDate, string unit)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("@StartDate", (OracleTimeStamp)startDate),
                                new OracleParameter("@EndDate", (OracleTimeStamp)endDate),
                                new OracleParameter("@Unit", unit),
                                new OracleParameter("@Culture", Thread.CurrentThread.CurrentUICulture.Name),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetOracleDataSetBySp("GetOpenMarkerSLO", paramArray).Tables[0];
        }

        public DataTable GetOpenMarketMLF(DateTime startDate, DateTime endDate, string unit)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("@StartDate", (OracleTimeStamp)startDate),
                                new OracleParameter("@EndDate", (OracleTimeStamp)endDate),
                                new OracleParameter("@Unit", unit),
                                new OracleParameter("@Culture", Thread.CurrentThread.CurrentUICulture.Name),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetOracleDataSetBySp("GetOpenMarkerMLF", paramArray).Tables[0];
        }

        public DataTable GetOpenMarketSLF(DateTime startDate, DateTime endDate, string unit)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("@StartDate", (OracleTimeStamp)startDate),
                                new OracleParameter("@EndDate", (OracleTimeStamp)endDate),
                                new OracleParameter("@Unit", unit),
                                new OracleParameter("@Culture", Thread.CurrentThread.CurrentUICulture.Name),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetOracleDataSetBySp("GetOpenMarkerSLF", paramArray).Tables[0];
        }

        public IEnumerable<REPORTCOLUMNDEFINITION> GetOpenMarketColumnDefinitionByReportId(int reportID, string cloumnStyle)
        {
            using (var CMADB = new CMAEntities())
            {
                return CMADB.REPORTCOLUMNDEFINITIONs.Where(x => x.REPORT_ID == reportID && x.COLUMN_STYLE == cloumnStyle).OrderBy(x => x.COLUMN_INDEX).ToList();
            }
        }
        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetOracleDataSetBySp(string inName, OracleParameter[] inParms)
        {
            using (var openmarket = new OpenMarketEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    DataSet ds = null;

                    openmarket.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(openmarket.Database.Connection.ConnectionString);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    if (inParms != null)
                        spCmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(spCmd);
                    ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }
        #endregion
    }
}
