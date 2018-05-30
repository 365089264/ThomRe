using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace VAV.Model.Data.IPP
{
    public class Submitter : BaseModel
    {
        public string ID { get; set; }
        public string Email { get; set; }
        public string RM { get; set; }
        public string Name { get; set; }
    }
}
