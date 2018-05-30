using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Luna.DataSync.Setting;

namespace Luna.DataSync.Core
{
    public class CheckIndexForFloatingRateNoteTask: ITask
    {
        private ISettingManager _settingManager;

        public void Init(ISettingManager settingManager)
        {
            this._settingManager = settingManager;
        }

        public void Run()
        {
            //TODO: this is not implemented.

            Debug.WriteLine("Check underling index for floating rate note.");
        }
    }
}
