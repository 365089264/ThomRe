using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Luna.DataSync.Setting;

namespace Luna.DataSync.Core
{
    public interface IBulkDataWritter
    {
        void Init();
        long Write(DataTable data, TableMapping tableMapping, ref StringBuilder msg);
        long Write(DataTable data, TableMapping tableMapping, ref StringBuilder msgBuilder, ref int insertRowCount,
            ref int updateRowCount);

        DateTime GetMaxDateTime(string tableName, string columnName);
        void Close();
        void BeginTrans();
        void CommitTrans();
        void RollBackTrans();
    }
}
