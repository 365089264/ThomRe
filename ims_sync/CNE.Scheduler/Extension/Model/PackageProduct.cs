using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CNE.Scheduler.Extension.Model
{
   
    public class PackageProduct<T>
    {
      
        public int code { get; set; }
        
        public string msg { get; set; }
       
        public List<T> data { get; set; }
    }
}
