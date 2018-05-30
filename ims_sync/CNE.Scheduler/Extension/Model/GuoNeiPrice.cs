using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CNE.Scheduler.Extension.Model
{

    public class GuoNeiPrice : PriceBase
    {
        
        //public float lowPrice { get; set; }
      
        //public float highPrice { get; set; }
        public string  lowPrice { get; set; }

        public string  highPrice { get; set; }
    }
}
