using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension.Model
{
    public class QueueMessageFromRFA
    {
        public string Ric { get; set; }

        public string RicType { get; set; }

        public string ReturnMessage { get; set; }

        public string OperationType { get; set; }

        public string ExecSql { get; set; }
    }
}
