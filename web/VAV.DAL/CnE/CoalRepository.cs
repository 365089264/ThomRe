using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VAV.DAL.Report;
using VAV.Model.Data.CnE;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using VAV.Entities;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;
using VAV.DAL.IPP;
using System.Threading;

namespace VAV.DAL.CnE
{
    public class CoalRepository : BaseReportRepository
    {

        public static DataTable GetCoalPortSchedularPagedData(string tableName, string strGetFields, string strOrder, string strWhere, int pageIndex, int doCount, int pageSize, out int recordCount)
        {
            var paramArray = new[]
                            {                                  
                                new OracleParameter("tblName", OracleDbType.NVarchar2) { Value = tableName },
                                new OracleParameter("strGetFields", OracleDbType.NVarchar2) { Value = strGetFields },
                                new OracleParameter("strOrder", OracleDbType.NVarchar2) { Value = strOrder },
                                new OracleParameter("strWhere", OracleDbType.NVarchar2) { Value = strWhere },
                                new OracleParameter("pageIndex", OracleDbType.Int32) { Value = pageIndex },
                                new OracleParameter("pageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("recordCount", OracleDbType.Int32,ParameterDirection.Output) { Value = pageSize },
                                new OracleParameter("doCount", OracleDbType.Int32) { Value = doCount },
                                new OracleParameter("isExcelReport", OracleDbType.Int32) { Value = 0 },
                                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                
                            };

            using (var cnEdb = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnEdb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(cnEdb.Database.Connection);
                    spCmd.CommandText = "GetDataPagedWithExportExcel";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);
                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    recordCount = Convert.ToInt32(spCmd.Parameters["recordCount"].Value.ToString());
                    return ds.Tables[0];
                }
            }
        }

        public CoalFilterObj GetDataFiltersWhichHasDataByReportId(int rid, string tableName)
        {
            var obj = GetDataFiltersByReportId(rid);
            if (obj != null && obj.PrimaryDropdown != null)
            {
                var categoryList = GetCategoryIdWhichHasData(tableName, obj.PrimaryDropdown.FieldName);
                for (int i = obj.PrimaryDropdown.Items.Count - 1; i >= 0; i--)
                {
                    string categoryId = obj.PrimaryDropdown.Items[i].Value;
                    if (!categoryList.Contains(categoryId))
                    {
                        obj.PrimaryDropdown.Items.RemoveAt(i);
                    }
                }
            }
            //use first item as default if have no default
            if (obj != null && (obj.PrimaryDropdown != null && obj.PrimaryDropdown.Items.Count > 0 && obj.PrimaryDropdown.Items.Count(o => o.Selected) == 0))
            {
                obj.PrimaryDropdown.Items[0].Selected = true;
            }
            return obj;
        }

        private List<string> GetCategoryIdWhichHasData(string tableName, string categoryName)
        {
            string proName = "select distinct " + categoryName + " from " + tableName;
            var dt = new DataTable();
            using (var cnE = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnE.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(cnE.Database.Connection);
                    spCmd.CommandText = proName;
                    spCmd.CommandType = CommandType.Text;
                    spCmd.CommandTimeout = 0;
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(dt);
                }
            }
            return (from DataRow dr in dt.Rows select dr[0] == null ? "" : dr[0].ToString()).ToList();
        }

        public CoalFilterObj GetDataFiltersByReportId(int rid)
        {
            List<COALFILTER> filters;
            using (var db = new CMAEntities())
            {
                filters = db.COALFILTERs.Where(o => o.REPORTID == rid).ToList();
            }
            var obj = new CoalFilterObj(rid);
            if (filters.Count == 0)
                return obj;

            var priItem = filters.FirstOrDefault(o => o.ISPRIMARY == 1);
            var secItem = filters.FirstOrDefault(o => o.ISPRIMARY == 0);
            if (priItem != null)
            {
                obj.PrimaryDropdown = GetSubDropdownByFilterId(priItem.ID, "");
                if (secItem != null)
                {
                    var firstOrDefault = obj.PrimaryDropdown.Items.Where(o => o.Selected).FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        var primDefault = firstOrDefault.Value;
                        obj.SecondDropdown = GetSubDropdownByFilterId(secItem.ID, primDefault);
                    }
                }
            }
            return obj;
        }

        public static CoalFilterItem GetSubDropdownByFilterId(decimal filterid, string selectedPrimaryItem)
        {
            COALFILTER filter;
            using (var db = new CMAEntities())
            {
                filter = db.COALFILTERs.FirstOrDefault(o => o.ID == filterid);
            }

            var res = new CoalFilterItem();
            res.FilterId = Convert.ToInt32(filterid);
            res.Name_CN = filter.NAME_CN;
            res.Name_EN = filter.NAME_EN;
            res.FieldName = filter.FIELDNAME;
            //20141027 yy Localization
            var filterSql = filter.FILERSQL;
            if (IsEnglishCulture())
            {
                filterSql = filter.FILERSQL_EN;
            }
            using (var zcxdb = new CNE_ZCXNewEntities())
            {
                try
                {
                    zcxdb.Database.Connection.Open();
                    var cmd = zcxdb.Database.Connection.CreateCommand();
                    cmd.CommandText = string.Format(filterSql, selectedPrimaryItem);
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var tempRes = new SelectListItem
                            {
                                Value = reader[0].ToString(),
                                Text = reader[1].ToString()
                            };
                            tempRes.Selected = (tempRes.Value == filter.DEFAULTARG);
                            res.Items.Add(tempRes);
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    zcxdb.Database.Connection.Close();
                }
                //use first item as default if have no default
                if (res.Items.Count > 0 && res.Items.Where(o => o.Selected).Count() == 0)
                {
                    res.Items[0].Selected = true;
                }
            }
            return res;
        }
        public DataTable GetPagedTableData(IEnumerable<REPORTCOLUMNDEFINITION> columns, string tableName, string strOrder, string strWhere, int pageIndex, out int recordCount)
        {
            var strGetFields =
                columns.Select(x => x.COLUMN_NAME).Aggregate((a, b) => a + "," + string.Format("{0}", b));
            if (string.IsNullOrEmpty(tableName))
            {
                recordCount = 0;
                return new DataTable();
            }
            else//20141027 yy Localization
            {
                if (IsEnglishCulture())
                {
                    tableName = tableName + "_EN";
                }
            }
            if (string.IsNullOrEmpty(strOrder))
            {
                strOrder = "parcode";
            }
            var paramArray = new[]
                            {                                  
                                 new OracleParameter("tblName", OracleDbType.NVarchar2) { Value = tableName },
                                new OracleParameter("strGetFields", OracleDbType.NVarchar2) { Value = strGetFields },
                                new OracleParameter("strOrder", OracleDbType.NVarchar2) { Value = strOrder },
                                new OracleParameter("strWhere", OracleDbType.NVarchar2) { Value = strWhere },
                                new OracleParameter("pageIndex", OracleDbType.Int32) { Value = pageIndex },
                                new OracleParameter("recordCount", OracleDbType.Int32,ParameterDirection.Output) { Value = 0 },
                                new OracleParameter("O_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                
                            };

            using (var cnEdb = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnEdb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(cnEdb.Database.Connection);
                    spCmd.CommandText = "GetDataPaged";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);
                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    recordCount = Convert.ToInt32(spCmd.Parameters["recordCount"].Value.ToString());
                    return ds.Tables[0];
                }
            }

        }

        public CoalChartLegendDisplay GetChartLegendByReportId(int reportId)
        {
            COALCHARTLEGEND legend;
            using (var db = new CMAEntities())
            {
                db.Database.Connection.Open();
                try
                {
                    legend = db.COALCHARTLEGENDs.FirstOrDefault(x => x.REPORTID == reportId);
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }

            var chartDisplay = new CoalChartLegendDisplay(Convert.ToInt32(legend.REPORTID), legend.CHARTSQL, legend.UNIT, legend.LEGEND, legend.CHARTYLABEL_CN, legend.CHARTYLABEL_EN, legend.CHARTTITLE_CN, legend.CHARTTITLE_EN);

            return chartDisplay;
        }

        public static List<CoalTrafficPortChartEntity> GetCoalTrafficPortData(string key)
        {
            const string proName = "CoalTrafficPort_ChartData";
            var paras = new[] {
                                       new OracleParameter ("P_key",key),
                                       new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                                   };
            var chartTable = new DataTable();
            using (var cnE = new CNE_ZCXNewEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnE.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(cnE.Database.Connection);
                    spCmd.CommandText = proName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    spCmd.Parameters.AddRange(paras);
                    var da = new OracleDataAdapter(spCmd);
                    da.Fill(chartTable);
                }
            }
            return DataTableSerializer.ToList<CoalTrafficPortChartEntity>(chartTable);
        }


        public static DataTable GetCoalChartDataTable(int reportId, string key)
        {
            //check params
            if (string.IsNullOrEmpty(key))
            {
                return new DataTable();
            }
            COALCHARTLEGEND chartL;
            using (var db = new CMAEntities())
            {
                chartL = db.COALCHARTLEGENDs.FirstOrDefault(o => o.REPORTID == reportId);
            }
            var sqlWhere = key.Replace("^|^", "=").Replace("||", " AND ");
            var sqlStr = string.Format(chartL.CHARTSQL, sqlWhere);

            var ds = new DataSet();
            using (var zcxdb = new CNE_ZCXNewEntities())
            {
                using (var adp = new OracleDataAdapter(sqlStr, zcxdb.Database.Connection.ConnectionString))
                {
                    adp.Fill(ds);
                }
            }
            return ds.Tables[0];
        }

        /// <summary>
        /// check the culture
        /// </summary>
        /// <returns></returns>
        public static bool IsEnglishCulture()
        {
            return Thread.CurrentThread.CurrentUICulture.Name != "zh-CN";
        }

    }
}
