using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    public class JsonColumn
    {
        public string Name { get; set; }
        public string ColumnType { get; set; }
        public string ColumnName { get; set; }
        public string ColumnStyle { get; set; }
        public string Sort { get; set; }
    }

    public class JsonExtraColumn
    {
        public string Name { get; set; }
        public int ColSpan { get; set; }
        public string ColumnStyle { get; set; }
        public int HeaderLevel { get; set; }
    }
}
