using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    public class ExtraHeader : BaseModel
    {
        /// <summary>
        /// Display header
        /// </summary>
        public string HeaderText { get { return Culture == "zh-CN" ? HeaderTextCN : HeaderTextEN; } }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string HeaderTextCN { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string HeaderTextEN { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        public int HeaderColumnSpan { get; set; }

        /// <summary>
        /// The level of header.
        /// </summary>
        public int HeaderLevel { get; set; }

        /// <summary>
        /// The style of header.
        /// </summary>
        public string HeaderStyle { get; set; }
    }
}
