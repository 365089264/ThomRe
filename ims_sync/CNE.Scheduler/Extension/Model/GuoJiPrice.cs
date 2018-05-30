using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension.Model
{
  public   class GuoJiPrice:PriceBase 
    {
      //public float lowPrice { get; set; }
      //public float highPrice { get; set; }
      //public float priceType { get; set; }
        public string  lowPrice { get; set; }
        public string  highPrice { get; set; }
        public string  priceType { get; set; }
    }
}
