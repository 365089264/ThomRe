using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondDetail : BaseModel
    {
        public string AssetId { get; set; }
        public string Code { get; set; }

        public string BondName { get { return Culture == "zh-CN" ? BondNameCn : BondNameEn; } }
        public string BondNameEn { get; set; }
        public string BondNameCn { get; set; }

        public string Term { get; set; }
        public string BondTerm { get { return Culture == "zh-CN" ? BondTermCn : BondTermEn; } }
        public string BondTermEn { get; set; }
        public string BondTermCn { get; set; }

        public string BondRating { get; set; }
        public string BondRatingAgency { get { return Culture == "zh-CN" ? BondRatingAgencyCn : BondRatingAgencyEn; } }
        public string BondRatingAgencyEn { get; set; }
        public string BondRatingAgencyCn { get; set; }

        public string PartyRating { get; set; }
        public string PartyRatingAgency { get { return Culture == "zh-CN" ? PartyRatingAgencyCn : PartyRatingAgencyEn; } }
        public string PartyRatingAgencyEn { get; set; }
        public string PartyRatingAgencyCn { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ValueDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public DateTime? ListingDate { get; set; }

        public decimal? IssueAmount { get; set; }
        public decimal? IssuePrice { get; set; }
        public decimal? RefYield { get; set; }

        public string CouponClass { get { return Culture == "zh-CN" ? CouponClassCn : CouponClassEn; } }
        public string CouponClassEn { get; set; }
        public string CouponClassCn { get; set; }

        public string CouponFreq { get { return Culture == "zh-CN" ? CouponFreqCn : CouponFreqEn; } }
        public string CouponFreqEn { get; set; }
        public string CouponFreqCn { get; set; }

        public decimal? CouponRate { get; set; }

        public string Currency { get; set; }

        public string CDCType { get; set; }
        public string CDCTypeName { get { return Culture == "zh-CN" ? CDCTypeCn : CDCTypeEn; } }
        public string CDCTypeEn { get; set; }
        public string CDCTypeCn { get; set; }

        public string FloatIndex { get; set; }
        public decimal? Spread { get; set; }
        public string DayCount { get { return Culture == "zh-CN" ? DayCountCn : DayCountEn; } }
        public string DayCountEn { get; set; }
        public string DayCountCn { get; set; }

        public string Option { get { return Culture == "zh-CN" ? OptionCn : OptionEn; } }
        public string OptionEn { get; set; }
        public string OptionCn { get; set; }

        public string Issuer { get; set; }
        public string ISBN { get; set; }
        public string Seniority { get; set; }

        public bool IsIssued { get; set; }  // issued in the date span specified
        public bool IsMatured { get; set; } // matured in the date span specified

        public decimal? OrigAvgLife { get; set; }
        public string re_issue { get; set; }
        public string issueComment { get { return re_issue == "1" ? (Culture == "zh-CN" ? "续发" : "Additional Issuing") : ""; } }

        public string ExchangeName { get { return Culture == "zh-CN" ? ExchangeNameCn : ExchangeNameEn; } }
        public string ExchangeNameEn { get; set; }
        public string ExchangeNameCn { get; set; }

        public string TrusteeName { get { return Culture == "zh-CN" ? TrusteeNameCn : TrusteeNameEn; } }
        public string TrusteeNameEn { get; set; }
        public string TrusteeNameCn { get; set; }
    }
}
