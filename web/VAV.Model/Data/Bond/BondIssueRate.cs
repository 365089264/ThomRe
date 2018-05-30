using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VAV.Model.Data.Bond
{
    public class BondIssueRate
    {
        public string ItemName { get; set; }
        private string _culture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }
        public string bond_name
        {
            get { return _culture == "zh-CN" ? bond_name_cn : bond_name_en; }
        }
        public string bond_name_cn { get; set; }
        public string bond_name_en { get; set; }

        public string code { get; set; }
        public Nullable<System.DateTime> orig_issue_dt { get; set; }
        public Nullable<System.DateTime> maturity_dt { get; set; }

        public string bond_term
        {
            get
            {
                if (term != null)
                {
                    if (term > 60)
                        return term + "D";
                    return term + "Y";
                }
                return String.Empty;
            }
        }
        public Nullable<decimal> term { get; set; }

        public Nullable<decimal> yield { get; set; }
        public Nullable<decimal> orig_iss_amt { get; set; }

        public string latest_rating_cd { get; set; }
        public Nullable<decimal> cdc_asset_class_number { get; set; }

        public string cdc_asset_class
        {
            get { return _culture == "zh-CN" ? cdc_asset_class_cn : cdc_asset_class_en; }
        }
        public string cdc_asset_class_cn { get; set; }
        public string cdc_asset_class_en { get; set; }

        public string coupon_type
        {
            get
            {
                return _culture == "zh-CN" ? coupon_type_cn : coupon_type_en;
            }
        }
        public string coupon_type_cn { get; set; }
        public string coupon_type_en { get; set; }

        public string isfloat { get; set; }

        public string rating_number { get; set; }
        //public string callorput { get; set; }
        //public string isin_nm { get; set; }
        //public string BondTerm { get; set; }
        //public Nullable<double> bondterm_number { get; set; }
        //public Nullable<double> float_offset { get; set; }
        //public Nullable<double> orig_iss_px { get; set; }
        //public string rating_src { get; set; }
        //public string party_rating_number { get; set; }
        //public string party_rating_cd { get; set; }
        //public string party_rating_src { get; set; }
        //public string offer_registrant_name { get; set; }
        //public string float_index { get; set; }
        //public string day_count { get; set; }
        //public string orig_iss_curr_cd { get; set; }
        public string assetId { get; set; }
        //public string seniority { get; set; }
        public string re_issue { get; set; }
        public string issueComment { get { return re_issue == "1" ? (_culture == "zh-CN" ? "续发" : "Additional Issuing") : ""; } }
    }
}
