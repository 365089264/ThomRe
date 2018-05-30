using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Dynamic;

namespace VAV.Model.Data
{
    /// <summary>
    /// Report
    /// </summary>
    public class StandardReport : BaseReport
    {
        public StandardReport(int id)
        {
            ReportID = id;
        }

        public bool IsStatisticalReport { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>The header.</value>
        public List<Column> Columns
        {
            get
            {
                if (columns == null)
                    columns = new List<Column>();
                return columns;
            }
            set
            { columns = value; }
        }
        private List<Column> columns;

        /// <summary>
        /// Gets or sets the rows.
        /// </summary>
        /// <value>The rows.</value>
        public List<ExtraHeader> ExtraHeaderCollection
        {
            get
            {
                if (extraHeaderCollection == null)
                    extraHeaderCollection = new List<ExtraHeader>();
                return extraHeaderCollection;
            }
            set { extraHeaderCollection = value; }
        }
        private List<ExtraHeader> extraHeaderCollection;

        public string GridStyleCss { get; set; }

        /// <summary>
        /// DataTable contains all the result.
        /// </summary>
        private DataTable resultDataTable;
        public DataTable ResultDataTable
        {
            get
            {
                if (resultDataTable == null)
                    resultDataTable = new DataTable();
                return resultDataTable;
            }
            set { resultDataTable = value; }
        }

        public string EmptyMessage
        {
            get
            {
                bool isInEnglish = System.Threading.Thread.CurrentThread.CurrentUICulture.Name != "zh-CN";
                return isInEnglish ? "No result matching search criteria" : "无符合条件数据";
            }
        }
    }
}
