using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public class ColumnMetaData
    {
        public string ColumnName { get; set; }
        public DbColumnType ColumnType { get; set; }
        public bool IsKey { get; set; }
    }
}
