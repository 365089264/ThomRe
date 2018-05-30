using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    /// <summary>
    /// Columns
    /// </summary>
    public class Column : BaseModel
    {
        /// <summary>
        /// The name of this column.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is sortable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is sortable; otherwise, <c>false</c>.
        /// </value>
        public bool IsSortable { get; set; }

        public bool IsDetailedColumn { get; set; }

        /// <summary>
        /// Gets or sets the index/order.
        /// </summary>
        /// <value>The index.</value>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// The type of column data.
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        /// The format of column data.
        /// </summary>
        //public Func<dynamic, object> ColumnFormat { get; set; }
        public string ColumnFormat { get; set; }

        /// <summary>
        /// The style of this column.
        /// </summary>
        public string ColumnStyle { get; set; }

        /// <summary>
        /// The Chinese name of the header.
        /// </summary>
        public string ColumnHeaderCN { get; set; }

        /// <summary>
        /// The English name of the header.
        /// </summary>
        public string ColumnHeaderEN { get; set; }

        public string DisplayHeader { get; set; }
    }
}
