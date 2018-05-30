using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CNE.Scheduler.Extension.Model
{

    public class PriceBase
    {

        public int id { get; set; }

        public string priceDate { get; set; }

        public string memo { get; set; }

        //public float price { get; set; }
        public string  price { get; set; }

        public string  updateDate { get; set; }
    }
}
