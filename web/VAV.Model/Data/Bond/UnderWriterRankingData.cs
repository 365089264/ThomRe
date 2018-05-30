using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class UnderWriterRankingData : BaseModel
    {
        public string Undwrt_Id { get; set; }
        public string Undwrt_Long_Name { get; set; }
        public Int64? Row_Num { get; set; }
        public Decimal? UnderWr_Amount { get; set; }
        public Decimal? UnderWr_pert { get; set; }
        public int Issues { get; set; }
    }
}
