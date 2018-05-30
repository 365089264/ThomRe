using System.Linq;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using VAV.Entities;
using VAV.Model.Data;

namespace VAV.DAL.Report
{
    public class StandardReportRepository : BaseReportRepository, IStandardReportRepository
    {
        /// <summary>
        /// Get the report data by report id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public DataTable GetReportById(int id, ReportParameter parameter)
        {
           
            bool isDetailedReport = !string.IsNullOrEmpty(parameter.RowName);
            var columnList = new OracleParameter("@columnList", parameter.ColumnList??"a.*");
            var tableName = new OracleParameter("@tableName", parameter.TableName);
            var startDatePara = new OracleParameter("@StartDate", parameter.StartDate != null ? ((OracleTimeStamp)parameter.StartDate) : OracleTimeStamp.Null);
            var endDatePara = new OracleParameter("@EndDate", parameter.EndDate != null ? ((OracleTimeStamp)parameter.EndDate) : OracleTimeStamp.Null);
            var culture = new OracleParameter("@Culture", parameter.Culture);
            var cur = new OracleParameter("@cur", OracleDbType.RefCursor, ParameterDirection.Output);


            string storedProcedureName = "GETCCDCREPORT";
            var paramArray = new[] { columnList, tableName, startDatePara, endDatePara, culture, cur };
            
            if (isDetailedReport)
            {
                storedProcedureName = "GETDETAILEDREPORT";
                var rowNamePara = new OracleParameter("@RowName", parameter.RowName);
                paramArray = new[] {  tableName, startDatePara, endDatePara, rowNamePara, culture, cur };
            }

            return GetOracleDataSetBySp(storedProcedureName, paramArray).Tables[0];
        }

        public DataTable GetStructureReportById(int id, ReportParameter parameter)
        {

            var columnList = new OracleParameter("@columnList", parameter.ColumnList ?? "a.*");
            var tableName = new OracleParameter("@tableName", parameter.TableName);
            var startDatePara = new OracleParameter("@StartDate", parameter.StartDate != null ? ((OracleTimeStamp)parameter.StartDate) : OracleTimeStamp.Null);
            var endDatePara = new OracleParameter("@EndDate", parameter.EndDate != null ? ((OracleTimeStamp)parameter.EndDate) : OracleTimeStamp.Null);
            var culture = new OracleParameter("@Culture", parameter.Culture);
            var cur = new OracleParameter("@cur", OracleDbType.RefCursor, ParameterDirection.Output);


            string storedProcedureName = "GETSTRUCTUREREPORT";
            var paramArray = new[] { columnList, tableName, startDatePara, endDatePara, culture, cur };


            return GetOracleDataSetBySp(storedProcedureName, paramArray).Tables[0];
        }

        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetOracleDataSetBySp(string inName, OracleParameter[] inParms)
        {
            using (var srdb = new SRDBEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    DataSet ds = null;

                    srdb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(srdb.Database.Connection.ConnectionString);
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

        /// <summary>
        /// Get the columns the report uses.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<Column> GetReportColumns(int reportId)
        {
            var columns = GetColumnDefinitionByReportId(reportId);
            return (from col in columns
                    where col.REPORT_ID == reportId
                    orderby col.COLUMN_INDEX ascending
                    select new Column
                    {
                        ColumnName = col.COLUMN_NAME,
                        ColumnHeaderCN = col.HEADER_TEXT_CN,
                        ColumnHeaderEN = col.HEADER_TEXT_EN,
                        ColumnFormat = col.DISPLAY_FORMAT,
                        IsSortable = col.IS_SORTABLE == 1,
                        IsDetailedColumn = col.IS_DETAIL_COLUMN==1,
                        ColumnStyle = col.COLUMN_STYLE,
                        ColumnType = col.COLUMN_TYPE
                    }).ToList();
        }

        /// <summary>
        /// Get report information by report id.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReportInfo> GetReportInfo()
        {
            using (var CMADB = new CMAEntities())
            {
                return ((from r in CMADB.REPORTDEFINITIONs
                         select r)
                        .AsEnumerable()
                        .Select(n => new ReportInfo(
                            n.VIEW_NAME,
                            n.VIEWMODEL_NAME,
                            (int) n.ID,
                            n.REPORT_TYPE,
                            n.ENGLISH_NAME,
                            n.CHINESE_NAME,
                            n.TABLE_NAME,
                            n.COLUMN_LIST
                            ))).ToList();
            }
        }

        /// <summary>
        /// Get extra header information by report id.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public IEnumerable<ExtraHeader> GetExtraHeaderById(int reportId)
        {
            using (var CMADB = new CMAEntities())
            {
                return (from h in CMADB.REPORTEXTRAHEADERDEFINITIONs
                        where h.REPORT_ID == reportId
                        select new ExtraHeader
                        {
                            HeaderLevel = (int) h.EXTRA_HEADER_LEVEL,
                            HeaderTextCN = h.EXTRA_HEADER_TEXT_CN,
                            HeaderTextEN = h.EXTRA_HEADER_TEXT_EN,
                            HeaderColumnSpan = (int) h.EXTRA_HEADER_COLUMN_SPAN
                        }).ToList();
            }
        }
    }
}
