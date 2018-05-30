using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class DimBondInfo : BaseModel
    {
        public string AssetId { get; set; }
        public string IssueCountryCd { get; set; }
        public string IssueCountry { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<System.DateTime> MaturityDate { get; set; }
        public Nullable<double> IssueAmount { get; set; }
        public string DebtTypeCd { get; set; }
        public string DebtTypeDescr { get; set; }
        public string AssetTypeCd { get; set; }
        public string AssetTypeDescr { get; set; }
        public string PartyCntryIncorpCd { get; set; }
        public string PartyCntryIncorpDescr { get; set; }
        public string RatingInfoCd { get; set; }
        public string RatingInfo { get; set; }
        public string BondTermCd { get; set; }
        public string BondTerm { get; set; }
        public string IssuerInduSectorCd { get; set; }
        public string IssuerInduSectorDescr { get; set; }
    }
}
