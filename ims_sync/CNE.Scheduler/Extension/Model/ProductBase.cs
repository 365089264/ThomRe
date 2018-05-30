using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CNE.Scheduler.Extension.Model
{
   
    public class ProductBase
    {

        public int id { get; set; }

        public string productName { get; set; }

        public string modelName { get; set; }

        public string areaName { get; set; }

        public string unit { get; set; }

        public string memo { get; set; }

    }
}
