using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    /// <summary>
    /// Report Info
    /// </summary>
    public class ReportInfo : BaseModel
    {
        /// <summary>
        /// Gets or sets the name of the view.
        /// </summary>
        /// <value>The name of the view.</value>
        public string ViewName { get; set; }

        /// <summary>
        /// Gets or sets the name of the view model.
        /// </summary>
        /// <value>The name of the view model.</value>
        public string ViewModelName { get; set; }

        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        public int ReportId { get; set; }

        /// <summary>
        /// Gets or sets the report type
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Decide whether english name or chinese name will display.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Culture == "zh-CN" ? _chineseName : TraceName;
            }
        }

        /// <summary>
        /// Gets the name of the trace.
        /// </summary>
        /// <value>
        /// The name of the trace.
        /// </value>
        public string TraceName { get; private set; }

        /// <summary>
        /// Gets the table name ,add by yy 20140827
        /// </summary>
        /// <value>
        /// tablename
        /// </value>
        public string TableName { get; private set; }

        /// <summary>
        /// Private _chineseName
        /// </summary>
        private string _chineseName;

        public ReportInfo(string viewNmae, string viewModelName, int reportId, string reportType, string englishName, string chineseName,string tableName,string columnlist="")
        {
            ViewName = viewNmae;
            ViewModelName = viewModelName;
            ReportId = reportId;
            ReportType = reportType;
            TraceName = englishName;
            _chineseName = chineseName;
            TableName = tableName;
            ColumnList = columnlist;
        }

        /// <summary>
        /// Gets or sets the name of the ColumnList.
        /// </summary>
        /// <value>The name of the view model.</value>
        public string ColumnList { get; set; }
    }
}
