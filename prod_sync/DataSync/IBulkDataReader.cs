using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;
using System.Data;

namespace Luna.DataSync.Core
{
    public interface IBulkDataReader
    {
        void Init();
        DataTable ReadAsDataTable(TableMapping tableMapping);
        DataTable ReadAsDataTable(TableMapping tableMapping, DateTime from);
        DataTable ReadWithRowCount(TableMapping tableMapping, DateTime from, int count);
        void Close();
    }
}
