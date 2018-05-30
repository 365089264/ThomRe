using System.Collections.Generic;
using System.Data;
using VAV.Model.Data;

namespace VAV.DAL.Report
{
    public interface IStandardReportRepository
    {
        /// <summary>
        /// Get report data.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        DataTable GetReportById(int id, ReportParameter parameter);

        /// <summary>
        /// Get the columns used by report.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        List<Column> GetReportColumns(int reportId);

        /// <summary>
        /// Get report info by report id.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ReportInfo> GetReportInfo();

        /// <summary>
        /// Get report header.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        IEnumerable<ExtraHeader> GetExtraHeaderById(int reportId);

        DataTable GetStructureReportById(int id, ReportParameter parameter);
    }
}
