using System;
using System.Collections.Generic;
using VAV.Model.Data;

namespace VAV.Web.Common
{
    /// <summary>
    /// Json Excel Parameter
    /// </summary>
    public class JsonExcelParameter
    {
        public JsonExcelParameter()
        {
            SubTitle = new List<Tuple<int, string>>();
        }
        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>
        /// The table.
        /// </value>
        public JsonTable Table { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }


        /// <summary>
        /// Gets or sets the sub Title.
        /// </summary>
        /// <value>
        /// The sub header.
        /// int column id, start from 0, string value of the cell
        /// </value>
        public List<Tuple<int, string>> SubTitle { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }

        public List<string> totalColumns { get; set; }
        public string sumGroupColumnName { get; set; }
        public bool isTotal { get; set; }
    }
}