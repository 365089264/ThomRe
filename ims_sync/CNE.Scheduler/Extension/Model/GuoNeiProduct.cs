using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CNE.Scheduler.Extension.Model
{
    
    public class GuoNeiProduct : ProductBase
    {
        public string marketName { get; set; }
        public string manufactureName { get; set; }
    }
}
