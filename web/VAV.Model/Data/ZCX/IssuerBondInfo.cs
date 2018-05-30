using System;

namespace VAV.Model.Data.ZCX
{
    public class IssuerBondInfo : BaseModel
    {
        /// <summary>
        /// [AssetId]
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// 债券名称
        /// </summary>
        public string BondName { get; set; }

        /// <summary>
        /// 债券代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 债券简称
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// 发行日期
        /// </summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>
        /// 发行规模(亿)
        /// </summary>
        public double? IssueAmount { get; set; }

        /// <summary>
        /// 债券品种
        /// </summary>
        public string BondClassDescr { get; set; }

        /// <summary>
        /// 期权方式
        /// </summary>
        public string OptionDescr { get; set; }

        /// <summary>
        /// 计息方式
        /// </summary>
        public string CouponClassDescr { get; set; }

        /// <summary>
        /// 付息频率
        /// </summary>
        public string PayFrequency { get; set; }

        /// <summary>
        /// 票面利率(%)
        /// </summary>
        public double? InterestRate { get; set; }

        /// <summary>
        /// 发行期限
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// 公告日期
        /// </summary>
        public DateTime? AnnoucementDate { get; set; }

        /// <summary>
        /// 起息日
        /// </summary>
        public DateTime? OrigDatedDate { get; set; }

        /// <summary>
        /// 到期日
        /// </summary>
        public DateTime? MaturityDate { get; set; }

        /// <summary>
        /// 上市流通日
        /// </summary>
        public DateTime? ListingDate { get; set; }

        /// <summary>
        /// 缴款日
        /// </summary>
        public DateTime? PayDate { get; set; }

        /// <summary>
        /// 浮动利率基准
        /// </summary>
        public string FloatIndexDescr { get; set; }

        /// <summary>
        /// 固定利差(%)
        /// </summary>
        public double? FloatOffset { get; set; }

        /// <summary>
        /// 债券评级
        /// </summary>
        public string RatingCd { get; set; }

        /// <summary>
        /// 债券评级机构
        /// </summary>
        public string RatingSrcDescr { get; set; }

        /// <summary>
        /// 主体评级
        /// </summary>
        public string PartyRatingCd { get; set; }

        /// <summary>
        /// 主体评级机构
        /// </summary>
        public string PartyRatingSrcDescr { get; set; }
    }
}
