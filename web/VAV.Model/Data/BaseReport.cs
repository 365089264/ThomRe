using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    /// <summary>
    /// Base Report
    /// </summary>
    public class BaseReport
    {
        /// <summary>
        /// Gets or sets the report ID.
        /// </summary>
        /// <value>The report ID.</value>
        public int ReportID { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}
