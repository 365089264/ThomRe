using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;

namespace Luna.DataSync.Core
{
    public interface ITask
    {
        void Init(ISettingManager settingManager);
        void Run();
    }
}
