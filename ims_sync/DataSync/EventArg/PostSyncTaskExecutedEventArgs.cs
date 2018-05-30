using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public class PostSyncTaskExecutedEventArgs : EventArgs
    {
        public string TaskName { get; private set; }

        public PostSyncTaskExecutedEventArgs(string taskName)
        {
            TaskName = taskName;
        }
    }
}
