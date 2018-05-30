using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondBasicInfo : BaseModel
    {
        public string AssetId { get; set; }
        public string Issuer { get; set; }
        public string BondClassDescr { get; set; }
        public string CouponClassDescr { get; set; }
        public string PayFrequency { get; set; }
        public string IssuerInduSectorDescr { get; set; }
        public string codlist { get; set; }
    }
}
