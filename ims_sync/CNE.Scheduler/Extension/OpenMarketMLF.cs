using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension
{
    public class OpenMarketMLF : OpenMarketBase
    {

        protected override bool IgnoreSql(OpenMarketOperation data, ref string ignoreMessage)
        {
            if (!data._ric.Contains("MLF")) return true;
            var issueVolume = float.Parse(data._values[data.getColIdx("IssueVolume")]);
            var yield = float.Parse(data._values[data.getColIdx("Yield")]);
            var residualVolume = float.Parse(data._values[data.getColIdx("ResidualVolume")]);
            if (issueVolume == 0 && yield == 0 && residualVolume == 0)
            {
                ignoreMessage = " <span style=\"color:yellow;\"> issueVolume = 0 && yield = 0 && residualVolume = 0</span>";
                return true;
            }
            return false;
        }

    }
}
