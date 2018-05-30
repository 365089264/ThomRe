using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using VAV.DAL.IPP;
using VAV.Entities;
using VAV.Model.Data.CnE.GDT;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace VAV.DAL.CnE
{
    public class NewGdtRespository : NewBaseRepository
    {
        public Dictionary<int, List<TabNode>> GetGDTTabNodes()
        {
            var dicTabs = new Dictionary<int, List<TabNode>>();
            var paramArray = new[]
                            {                                  
                                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                
                            };
            DataSet dataSet = GetDataSetBySpFromCma("GetGDTTabNodes", paramArray);
            List<TabNode> tabNodes = DataTableSerializer.ToList<TabNode>(dataSet.Tables[0]);
            foreach (var item in tabNodes)
            {

                if (dicTabs.ContainsKey(item.ReportId))
                {
                    dicTabs[item.ReportId].Add(item);
                }
                else
                {
                    var itemList = new List<TabNode> { item };
                    dicTabs.Add(item.ReportId, itemList);
                }
            }
            return dicTabs;
        }

        public new DataSet GetDataSetBySp(string inName, params OracleParameter[] inParams)
        {
            using (var cnE = new CneNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnE.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(cnE.Database.Connection);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    if (inParams != null)
                    {
                        spCmd.Parameters.AddRange(inParams);
                    }
                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }

        public DataSet GetDataSetBySpFromCma(string inName, params OracleParameter[] inParams)
        {
            using (var vav = new CMAEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    vav.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(vav.Database.Connection);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    if (inParams != null)
                    {
                        spCmd.Parameters.AddRange(inParams);
                    }
                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }

        public Dictionary<int, GDTQueryWithDirection> GetQueryConditions()
        {
            var dicQueryCondition = new Dictionary<int, GDTQueryWithDirection>();
            var paramArray = new[]
                            {                                  
                                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                
                            };
            var dataSet = GetDataSetBySpFromCma("GetQueryCondition", paramArray);
            List<GDTQueryCondition> queryConditions = DataTableSerializer.ToList<GDTQueryCondition>(dataSet.Tables[0]);
            foreach (var query in queryConditions)
            {
                if (dicQueryCondition.ContainsKey(query.ItemID))
                {
                    if (query.Direction == 0)
                    {
                        dicQueryCondition[query.ItemID].Left.Add(query);
                    }
                    else if (query.Direction == 1)
                    {
                        dicQueryCondition[query.ItemID].Right.Add(query);
                    }
                }
                else
                {
                    var qwd = new GDTQueryWithDirection();
                    if (query.Direction == 0)
                    {
                        qwd.Left.Add(query);
                    }
                    else if (query.Direction == 1)
                    {
                        qwd.Right.Add(query);
                    }
                    dicQueryCondition.Add(query.ItemID, qwd);


                }
            }
            return dicQueryCondition;
        }

        public List<SelectListItem> GetQueryConditionContent(string columnName, string tableName, string queryFilter,
            string relationColumn = null, string relationValue = null)
        {
            var condition = new List<SelectListItem>();
            var paras = new List<OracleParameter>
            {
                new OracleParameter("tableName", tableName),
                new OracleParameter("queryFilter", queryFilter),
                new OracleParameter("relationcolumn", columnName),
                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            DataSet set;
            set = GetDataSetBySp("GetQueryConditionContent", paras.ToArray());
            var strList = DataTableSerializer.ToList<QueryItem>(set.Tables[0]);

            foreach (QueryItem str in strList)
            {
                var exists = condition.Any(it => it.Text == str.DisplayName);
                if (!exists)
                {
                    var item = new SelectListItem { Text = str.DisplayName, Value = str.ItemText_EN };
                    condition.Add(item);
                }


            }


            return condition;
        }

        public DataTable GetPriceChartTable(int itemId, string key, string term, string reDate)
        {
            return GetPriceChartTable(itemId, key, term, reDate, 0);
        }

        public DataTable GetPriceChartTable(int itemId, string key, string term, string reDate, int isExport)
        {
            DateTime start;
            DateTime end = Convert.ToDateTime(reDate);
            switch (term)
            {
                case "1D":
                    start = end.Date;
                    end = end.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "1M":
                    start = end.AddMonths(-1);
                    break;
                case "3M":
                    start = end.AddMonths(-3);
                    break;
                case "6M":
                    start = end.AddMonths(-6);
                    break;
                case "1Y":
                    start = end.AddYears(-1);
                    break;
                case "5Y":
                    start = end.AddYears(-5);
                    break;
                default:
                    start = end.AddYears(-50);
                    break;
            }


            string chartSp;
            using (var cma = new CMAEntities())
            {
                chartSp = cma.ITEMDEFINITIONs.First(re => re.ITEMID == itemId).CHARTSP1;
            }

            var paramArray = new[]
            {
                new OracleParameter("P_chartSp", OracleDbType.NVarchar2, 50) {Value = chartSp},
                new OracleParameter("P_key", OracleDbType.NVarchar2, 50) {Value = key},
                new OracleParameter("P_start", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)start},
                new OracleParameter("P_end", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)end},
                new OracleParameter("P_isExport", OracleDbType.Int32) {Value = isExport},
                new OracleParameter("P_avg", OracleDbType.Double) {Value = 0, Direction = ParameterDirection.Output},
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            var table = GetDataSetBySp("GDTGetPriceData", paramArray).Tables[0];
            return table;

        }

        public DataTable GetPriceChart(int itemId, string key, string reDate, string term = "All")
        {
            return GetPriceChartTable(itemId, key, term, reDate);
        }

        #region output

        public DataTable GetOutputChartTable(int itemId, string key)
        {
            if (string.IsNullOrEmpty(key)) return new DataTable();
            string chartSp;
            using (var cma = new CMAEntities())
            {
                chartSp = cma.ITEMDEFINITIONs.First(re => re.ITEMID == itemId).CHARTSP1;
            }

            var paramArray = new[]
            {
                new OracleParameter("P_chartSp", OracleDbType.NVarchar2, 50) {Value = chartSp},
                new OracleParameter("P_key", OracleDbType.NVarchar2, 50) {Value = key},
                new OracleParameter("P_CUR", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };
            var table = GetDataSetBySp("GDTGetOutputData", paramArray).Tables[0];
            return table;

        }

        public DataTable GetOutputChart(int itemId, string key)
        {
            return GetOutputChartTable(itemId, key);
        }

        #endregion

        #region Ballance
        public List<string> GetBallanceColumnNames(int reportId, string areacode, string productCode)
        {
            var ds = new DataTable();
            using (var zcxDb = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    string str = "select distinct MARKET_YEAR from GetBalanceTableData where length(MARKET_YEAR)=9 ";
                    if (!string.IsNullOrEmpty(areacode)) str += " and areacode=" + areacode;
                    if (!string.IsNullOrEmpty(productCode)) str += " and productCode=" + productCode;
                    str += " and reportid=" + reportId + " order by MARKET_YEAR desc";
                    zcxDb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)zcxDb.Database.Connection;
                    spCmd.CommandText = str;

                    spCmd.CommandTimeout = 0;
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(ds);

                }
            }
            var columnNames = new List<string>();
            var rowItor = ds.Rows.GetEnumerator();
            while (rowItor.MoveNext())
            {
                string years = ((DataRow) rowItor.Current)[0].ToString();
                int startYear = Convert.ToInt32(years.Substring(0, 4));
                int endYear = Convert.ToInt32(years.Substring(5, 4));
                if (endYear == startYear + 1)
                {
                    columnNames.Add(years);
                }
            }
            if (columnNames.Count == 0)
            {
                columnNames.Add((DateTime.Now.Year - 1) + "/" + DateTime.Now.Year);
                columnNames.Add((DateTime.Now.Year - 2) + "/" + (DateTime.Now.Year - 1));
                columnNames.Add((DateTime.Now.Year - 3) + "/" + (DateTime.Now.Year - 2));
            }
            return columnNames;
        }


        public List<BallanceFilter> GetProductFilters(int reportId)
        {
            var ds = new DataTable();
            var paras = new[] { new OracleParameter("P_reportID", reportId),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output} };
            using (var zcxDb = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    zcxDb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)zcxDb.Database.Connection;
                    spCmd.CommandText = "GetProductFiltes";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    spCmd.Parameters.AddRange(paras);
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(ds);

                }
            }
            return DataTableSerializer.ToList<BallanceFilter>(ds);
        }

        public List<PartitionBallanceTableData> GetPartitionBallenceData(string areaCode, string productCode, List<string> years)
        {
            var ds = new DataTable();
            var paras = new[] { new OracleParameter("P_AreaCode", areaCode),
                                             new OracleParameter("P_productCode", productCode),
                                             new OracleParameter("P_myear1", years[2]), 
                                             new OracleParameter("P_myear2", years[1]),
                                             new OracleParameter("P_myear3", years[0]),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                              };
            using (var zcxDb = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    zcxDb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)zcxDb.Database.Connection;
                    spCmd.CommandText = "GetPartitionBallenceData";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    spCmd.Parameters.AddRange(paras);
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(ds);

                }
            }
            return DataTableSerializer.ToList<PartitionBallanceTableData>(ds);
        }

        public List<BallanceFilter> GetAreaFilters(int reportId, string productCode)
        {
            var ds = new DataTable();
            var paras = new[] { new OracleParameter("P_reportID", reportId), 
                new OracleParameter("P_ProductCode", productCode) ,
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}};
            using (var zcxDb = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    zcxDb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)zcxDb.Database.Connection;
                    spCmd.CommandText = "GetAreaFiltes";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    spCmd.Parameters.AddRange(paras);
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(ds);

                }
            }
            return DataTableSerializer.ToList<BallanceFilter>(ds);
        }
        #endregion Balance

        #region inventory

        public DataTable GetInventoryChartTable(int itemId, string key)
        {
            if (string.IsNullOrEmpty(key)) return new DataTable();
            string chartSp;
            using (var cma = new CMAEntities())
            {
                chartSp = cma.ITEMDEFINITIONs.First(re => re.ITEMID == itemId).CHARTSP1;
            }

            var paramArray = new[]
                            {
                                new OracleParameter("P_chartSp", chartSp),
                                new OracleParameter("P_key", key) ,
                                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            var table = GetDataSetBySp("GDTGetInventoryData", paramArray).Tables[0];
            return table;

        }

        public DataTable GetInventoryChart(int itemId, string key)
        {
            return GetInventoryChartTable(itemId, key);
        }

        #endregion


        #region EnergyInventory

        public DataTable GetEnergyInvntoryData(string tableName, string columns, string order, string filter = "1=1")
        {
            using (var cnE = new CneNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    var ds = new DataSet();
                    cnE.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)cnE.Database.Connection;
                    spCmd.CommandText = "SELECT " + columns + " FROM " + tableName + " where " + filter + " order by " + order;
                    spCmd.CommandTimeout = 0;
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(ds);
                    return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
                }
            }
        }

        public DataTable GetOilInventoryChart(int itemId, string key)
        {
            if (string.IsNullOrEmpty(key)) return new DataTable();
            string chartSp;
            using (var cma = new CMAEntities())
            {
                chartSp = cma.ITEMDEFINITIONs.First(re => re.ITEMID == itemId).CHARTSP1;
            }

            var paramArray = new[]
                            {
                                new OracleParameter("P_chartView", chartSp),
                                new OracleParameter("P_where", key),
                                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            var table = GetDataSetBySp("GDTGetOilInventoryData", paramArray).Tables[0];
            return table;

        }

        #endregion

        
        #region sales

        public DataTable GetSalesChartTable(int itemId, string key)
        {
            if (string.IsNullOrEmpty(key)) return new DataTable();
            string chartSp;
            using (var cma = new CMAEntities())
            {
                chartSp = cma.ITEMDEFINITIONs.First(re => re.ITEMID == itemId).CHARTSP1;
            }

            var paramArray = new[]
                            {
                                new OracleParameter("P_key", key) ,
                                new OracleParameter("P_chartSp", chartSp),
                                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            var table = GetDataSetBySp("GDTGetSalesData", paramArray).Tables[0];
            return table;

        }

        public DataTable GetSalesChart(int itemId, string key)
        {
            return GetSalesChartTable(itemId, key);
        }

        #endregion

       
    }
}