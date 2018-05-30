using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace VAV.Model.Data.Lfex
{
    public class LufaxDetail : BaseModel
    {
        public long ChannelID { get; set; }
        public string Name { get; set; }
        public string VALUE { get; set; }
        public DateTime CreateDate { get; set; }
        public string ProductName { get; set; }

    }
}
