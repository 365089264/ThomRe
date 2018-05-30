using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.Bond
{
    public class BondInfo : BaseModel
    {
        public string AssetId { get; set; }
        public string Code { get; set; }
        public string Ric { get; set; }
        public string ShortName { get; set; }
        public string IssueCountryCd { get; set; }
        public string IssueCountry { get; set; }
        public Nullable<System.DateTime> IssueDate { get; set; }
        public Nullable<double> IssueAmount { get; set; }
        public Nullable<double> InterestRate { get; set; }
        public Nullable<double> IssuePrice { get; set; }
        public string Term { get; set; }
        public string BondClassCd { get; set; }
        public string BondClassDescr { get; set; }
        public string Option { get; set; }
        public string OptionDescr { get; set; }
        public string CouponClassCd { get; set; }
        public string CouponClassDescr { get; set; }
        public Nullable<int> PayFrequencyCd { get; set; }
        public string PayFrequency { get; set; }
        public Nullable<System.DateTime> AnnoucementDate { get; set; }
        public Nullable<System.DateTime> OrigDatedDate { get; set; }
        public Nullable<System.DateTime> MaturityDate { get; set; }
        public Nullable<System.DateTime> ListingDate { get; set; }
        public string FloatIndex { get; set; }
        public string FloatIndexDescr { get; set; }
        public Nullable<double> FloatOffset { get; set; }
        public string RatingCd { get; set; }
        public string RatingSrcCd { get; set; }
        public string RatingSrcDescr { get; set; }
        public Nullable<double> FirstDayClosingPrice { get; set; }
        public Nullable<double> FirstDayChange { get; set; }
        public Nullable<double> FirstDayVolume { get; set; }
        public Nullable<double> FirstDayTurnoverRate { get; set; }
        public string PartyRatingCd { get; set; }
        public string PartyRatingSrcCd { get; set; }
        public string PartyRatingSrcDescr { get; set; }
        public string Issuer { get; set; }
        public string BookManager { get; set; }
        public string LeadUnderWriter { get; set; }
        public string CoUnderWriter { get; set; }
        public string UnderWriterMember { get; set; }
        public Nullable<System.DateTime> PayDate { get; set; }
        public string BondName { get; set; }
        public string StatusOnLiqCd { get; set; }
        public string StatusOnLiqDescr { get; set; }
        public string CodeList { get; set; }
        public string IssuerInduSectorCd { get; set; }
        public string IssuerInduSectorDescr { get; set; }
        public string IssuerInduSubSectorCd { get; set; }
        public string IssuerInduSubSectorDescr { get; set; }
        public string GuarantorName { get; set; }
        public string GuarantorInduSectorCd { get; set; }
        public string GuarantorInduSectorDescr { get; set; }
        public Nullable<System.DateTime> EndDateOfIssue { get; set; }
        public Nullable<short> IssueTypeCd { get; set; }
        public string IssueTypeDescr { get; set; }
        public string AuctionType { get; set; }
        public Nullable<System.DateTime> AuctionDate { get; set; }
        public string SeniorityCd { get; set; }
        public string SeniorityDescr { get; set; }
        public string CIBondFlag { get; set; }
        public string TapIssueFlag { get; set; }
        public string TermEx { get; set; }
        public string ItermEx { get; set; }
        public string ProvinceOfIssuer { get; set; }
        public string GuarantorRating { get; set; }
        public string AuctionOject { get; set; }
        public string GuarantorItem { get; set; }
        public string UnderWriterType { get; set; }
        public Nullable<double> BidPrice { get; set; }
        public string BidRegion { get; set; }
        public Nullable<double> MultiPurchase { get; set; }
        public Nullable<double> FeeRate { get; set; }
        public Nullable<double> OnlinePosition { get; set; }
        public string OnlineCode { get; set; }
        public string Domicile { get; set; }
        public Nullable<System.DateTime> LastChangeDate { get; set; }
        public string DebtTypeCd { get; set; }
        public string DebtTypeDescr { get; set; }
        public string AssetTypeCd { get; set; }
        public string AssetTypeDescr { get; set; }
        public string PartyCntryIncorpCd { get; set; }
        public string PartyCntryIncorpDescr { get; set; }
        public string RatingInfoCd { get; set; }
        public string RatingInfo { get; set; }
        public string SecClassCd { get; set; }
        public string BondTermCd { get; set; }
        public string BondTerm { get; set; }
        public string ExchangeName { get; set; }
        public string TrusteeName { get; set; }
    }
}
