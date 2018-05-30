using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Setting
{
	public interface ISettingManager
	{
        void Init(string DestinationDbConn);
        DbSetting SourceDb { get; }
        DbSetting DestinationDb { get; }
        IEnumerable<TableMapping> TableMappings { get; }
        int SqlCommandTimeout { get; }
        List<string> CustomBonds { get; }
        List<Task> PostSyncTasks { get; }
	}
}
