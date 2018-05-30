using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Setting
{
    public class Task
    {
        public string Name { get; private set; }

        public Task(string name)
        {
            Name = name;
        }
    }
}
