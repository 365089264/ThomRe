using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public class TableSynchedEventArgs: EventArgs
    {
        public string Source { get; private set; }
        public string Dest { get; private set; }
        public long NumOfRowsSynched { get; private set; }
        public string SyncInfo { get; private set; }

        public TableSynchedEventArgs(string source, string dest, long numOfRowsSynched, string syncInfo)
        {
            Source = source;
            Dest = dest;
            NumOfRowsSynched = numOfRowsSynched;
            SyncInfo = syncInfo;
        }
    }
}
