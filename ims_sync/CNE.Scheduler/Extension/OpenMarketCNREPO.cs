using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension
{
    public class OpenMarketCNREPO : OpenMarketBase
    {
        protected override bool IgnoreSql(OpenMarketOperation data, ref string ignoreMessage)
        {
            var issueRate = float.Parse(data._values[data.getColIdx("issueRate")]);
            var issueVolume = float.Parse(data._values[data.getColIdx("issueVolume")]);
            if (issueVolume == 0 && issueRate == 0)
            {
                ignoreMessage = " <span style=\"background:yellow;\">issueVolume = 0 && issueRate = 0</span> ";
                return true;
            }
            return false;
        }

    }
}
