using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    /// <summary>
    /// Report Parameter
    /// </summary>
    public class ReportParameter : BaseModel
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime? StartDate { get; set; }


        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>The end date.</value>
        public DateTime? EndDate { get; set; }


        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>The unit.</value>
        public string Unit { get; set; }


        /// <summary>
        /// Gets or sets row name.
        /// </summary>
        public string RowName { get; set; }

        public string ColumnList { get; set; }
        public string TableName { get; set; }

    }
}
