using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VAV.DAL.Report;
using VAV.Web.Localization;
using VAV.DAL.WMP;
using VAV.Entities;
using VAV.DAL.Fundamental;

namespace VAV.Web.Common
{
    public class UIStaticDataCache
    {
        private const string SELECT_ALL_EN = "All";
        private const string SELECT_ALL_CN = "全部";
        Dictionary<string, string> companyTypes = new Dictionary<string, string>();
        public static UIStaticDataCache Instance { get; private set; }

        static UIStaticDataCache()
        {
            if (Instance == null)
                Instance = new UIStaticDataCache();
        }

        private readonly BondInfoRepository _repository = (BondInfoRepository)DependencyResolver.Current.GetService(typeof(BondInfoRepository));
        private readonly WMPRepository _wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
        private readonly ZCXRepository _zcxRepository = (ZCXRepository)DependencyResolver.Current.GetService(typeof(ZCXRepository));
        private readonly PartnersReportRepository _cmaRepository = (PartnersReportRepository)DependencyResolver.Current.GetService(typeof(PartnersReportRepository));

        private readonly List<SelectListItem> _couponClassCn = new List<SelectListItem>{new SelectListItem{Selected = true,Text = SELECT_ALL_CN,Value = "all"}};
        private readonly List<SelectListItem> _couponClassEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _assetClassCn = new List<SelectListItem>{};
        private readonly List<SelectListItem> _assetClassEn = new List<SelectListItem> {};
        private readonly List<SelectListItem> _optionClassCn = new List<SelectListItem>{new SelectListItem{Selected = true,Text = SELECT_ALL_CN,Value = "all"}};
        private readonly List<SelectListItem> _optionClassEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _bondRatingCN = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_CN, Value = "all" } };
        private readonly List<SelectListItem> _bondRatingEN = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _wmpBankTypeCn = new List<SelectListItem>{ };
        private readonly List<SelectListItem> _wmpBankTypeEn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _wmpReportTypeCn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_CN, Value = "all" } };
        private readonly List<SelectListItem> _wmpReportTypeEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _wmpCurrency = new List<SelectListItem>();
        private readonly List<SelectListItem> _wmpYieldCn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_CN, Value = "all" } };
        private readonly List<SelectListItem> _wmpYieldEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _wmpInvestCn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_CN, Value = "all" } };
        private readonly List<SelectListItem> _wmpInvestEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _zcxCompanyTypeCn = new List<SelectListItem>() { new SelectListItem {  Text = SELECT_ALL_CN, Value = "0", Selected = true } };
        private readonly List<SelectListItem> _zcxCompanyTypeEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "0", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerOrgTypeCn = new List<SelectListItem>();
        private readonly List<SelectListItem> _wmpBrokerOrgTypeEn = new List<SelectListItem>();
        private readonly List<SelectListItem> _wmpBrokerProdTypeCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerProdTypeEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerInvestTypeCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerInvestTypeEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerLowestTypeCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerLowestTypeEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerQdiiTypeCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerQdiiTypeEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerStateTypeCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerStateTypeEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _wmpBrokerBankTypeCn = new List<SelectListItem>();
        private readonly List<SelectListItem> _wmpBrokerBankTypeEn = new List<SelectListItem>();
        private readonly List<SelectListItem> _wmpRegionCn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _wmpRegionEn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _bondMarketsCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _bondMarketsEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _bondTrusteesCn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_CN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _bondTrusteesEn = new List<SelectListItem>() { new SelectListItem { Text = SELECT_ALL_EN, Value = "all", Selected = true } };
        private readonly List<SelectListItem> _bondClassCn = new List<SelectListItem> {};
        private readonly List<SelectListItem> _bondClassEn = new List<SelectListItem> {};
        private readonly List<SelectListItem> _rateTypeCn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_CN, Value = "all" } };
        private readonly List<SelectListItem> _rateTypeEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _rateHisCn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_CN, Value = "all" } };
        private readonly List<SelectListItem> _rateHisEn = new List<SelectListItem> { new SelectListItem { Selected = true, Text = SELECT_ALL_EN, Value = "all" } };
        private readonly List<SelectListItem> _issuerInduSectorCn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _issuerInduSectorEn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _partyCntryIncorpCn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _partyCntryIncorpEn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _HomeItemCn = new List<SelectListItem> { };
        private readonly List<SelectListItem> _HomeItemEn = new List<SelectListItem> { };
        public UIStaticDataCache()
        {
            foreach (var gcodesCouponClassCdse in _repository.GetCouponItems())
            {
                var enCoupon = new SelectListItem
                                   {
                                       Text = gcodesCouponClassCdse.COUPON_CLASS_DESCR,
                                       Value = gcodesCouponClassCdse.COUPON_CLASS_CD
                                   };
                var cnCoupon = new SelectListItem { Value = enCoupon.Value, Text = _repository.GetChineseName("gcodes.coupon_class_cds", gcodesCouponClassCdse.COUPON_CLASS_CD) };
                _couponClassCn.Add(cnCoupon);
                _couponClassEn.Add(enCoupon);
            }
            foreach (var bondClass in _repository.GetAssetClass())
            {
                var enAsset = new SelectListItem
                {
                    Text = bondClass.cdc_asset_class_en,
                    Value = bondClass.cdc_asset_class_cd,
                    Selected = true
                };
                _assetClassEn.Add(enAsset);
                var cnAsset = new SelectListItem
                {
                    Text = bondClass.cdc_asset_class_cn,
                    Value = bondClass.cdc_asset_class_cd,
                    Selected = true
                };
                _assetClassCn.Add(cnAsset);
            }
            foreach (var bondClass in _repository.GetAbsBondClass())
            {
                var bondC= new SelectListItem
                {
                    Selected = true,
                    Text =bondClass.CHINESE_NAME,
                    Value = bondClass.TABLE_CD 
                };
                var bondE= new SelectListItem
                {
                    Selected = true,
                    Text =bondClass.ENGLISH_NAME,
                    Value = bondClass.TABLE_CD 
                };
                _bondClassCn.Add(bondC);
                _bondClassEn.Add(bondE);
            }
            foreach (var rateType in _repository.GetAbsRateType())
            {
                var rateCn = new SelectListItem
                {
                    Text = rateType.CHINESE_NAME,
                    Value = rateType.TABLE_CD
                };
                var rateEn = new SelectListItem
                {
                    Text = rateType.ENGLISH_NAME,
                    Value = rateType.TABLE_CD
                };
                _rateTypeCn.Add(rateCn);
                _rateTypeEn.Add(rateEn);
            }

            foreach (var rateHis in _repository.GetAbsRateHis())
            {
                var ratePar = new SelectListItem
                {
                    Text = rateHis,
                    Value = rateHis
                };
                _rateHisCn.Add(ratePar);
                _rateHisEn.Add(ratePar);
            }
            foreach (var localization in _repository.GetOptionItems())
            {
                var enOption = new SelectListItem
                {
                    Text = localization.ENGLISH_NAME,
                    Value = localization.TABLE_CD
                };
                _optionClassEn.Add(enOption);
                var cnOption = new SelectListItem
                {
                    Text = localization.CHINESE_NAME,
                    Value = localization.TABLE_CD
                };
                _optionClassCn.Add(cnOption);
            }
            foreach (var rating in HtmlUtil.CookOptions("bond_rating"))
            {
                var option = new SelectListItem
                {
                    Text = rating.Name,
                    Value = rating.ID
                };
                _bondRatingCN.Add(option);
                _bondRatingEN.Add(option);
            }
            foreach (var bankTypeOption in _wmpRepository.GetWmpBankTypeOption())
            {
                var option = new SelectListItem
                {
                    Text = bankTypeOption.TypeName,
                    Value = bankTypeOption.Code,
                    Selected = true
                };
                _wmpBankTypeEn.Add(option);
                _wmpBankTypeCn.Add(option);
            }
            foreach (var reportOption in _wmpRepository.GetWmpReportTypeOption())
            {
                var option = new SelectListItem
                {
                    Text = reportOption.Name,
                    Value = reportOption.Type.ToString()
                };
                _wmpReportTypeEn.Add(option);
                _wmpReportTypeCn.Add(option);
            }
            foreach (var currencyOption in _wmpRepository.GetWmpCurrencyOption())
            {
                var option = new SelectListItem
                {
                    Text = currencyOption.Name,
                    Value = currencyOption.Type.ToString()
                };
                _wmpCurrency.Add(option);
            }
            foreach (var yieldOption in _wmpRepository.GetWmpYieldOption())
            {
                var option = new SelectListItem
                {
                    Text = yieldOption.Name,
                    Value = yieldOption.Type.ToString()
                };
                _wmpYieldEn.Add(option);
                _wmpYieldCn.Add(option);
            }
            foreach (var investOption in _wmpRepository.GetWmpInvestOption())
            {
                var option = new SelectListItem
                {
                    Text = investOption.Name,
                    Value = investOption.Type.ToString()
                };
                _wmpInvestCn.Add(option);
                _wmpInvestEn.Add(option);
            }
            foreach (var regionOption in _wmpRepository.GetWmpProvinceOption())
            {
                var option = new SelectListItem
                {
                    Text = regionOption.Name,
                    Value = regionOption.Code,
                    Selected = true
                };
                _wmpRegionEn.Add(option);
                _wmpRegionCn.Add(option);
            }
            foreach (var regionOption in _repository.GetIssuerInduSector())
            {
                var option = new SelectListItem
                {
                    Text = regionOption.CHINESE_NAME,
                    Value = regionOption.TABLE_CD,
                    Selected = true
                };
                var optionEn = new SelectListItem
                {
                    Text = regionOption.ENGLISH_NAME,
                    Value = regionOption.TABLE_CD,
                    Selected = true
                };
                _issuerInduSectorCn.Add(option);
                _issuerInduSectorEn.Add(optionEn);
            }
            foreach (var OptionInfo in _repository.GetPartyCntryIncorpCn())
            {
                var option = new SelectListItem
                {
                    Text = OptionInfo.Name,
                    Value = OptionInfo.Type,
                    Selected = true
                };
                _partyCntryIncorpCn.Add(option);
            }
            foreach (var OptionInfo in _repository.GetPartyCntryIncorpEn())
            {
                var option = new SelectListItem
                {
                    Text = OptionInfo.Name,
                    Value = OptionInfo.Type,
                    Selected = true
                };
                _partyCntryIncorpEn.Add(option);
            }
            foreach (var OptionInfo in _cmaRepository.GetHomeModules())
            {
                var option = new SelectListItem
                {
                    Text = OptionInfo.NAMECN,
                    Value = OptionInfo.ID.ToString(),
                    Selected = true
                };
                _HomeItemCn.Add(option);
            }
            foreach (var OptionInfo in _cmaRepository.GetHomeModules())
            {
                var option = new SelectListItem
                {
                    Text = OptionInfo.NAMEEN,
                    Value = OptionInfo.ID.ToString(),
                    Selected = true
                };
                _HomeItemEn.Add(option);
            }
            
            #region zcx company
            companyTypes.Add("政府机构、事业单位", "Government agencies, institutions");
            companyTypes.Add("银行", "Bank");
            companyTypes.Add("保险公司", "Insurance company");
            companyTypes.Add("信托投资公司", "Trust and Investment Company");
            companyTypes.Add("证券公司", "Securities");
            companyTypes.Add("资产管理公司", "Asset management companies");
            companyTypes.Add("租赁公司", "Leasing companies");
            companyTypes.Add("会计师事务所", "CPA");
            companyTypes.Add("资信评级机构", "Credit rating agencies");
            companyTypes.Add("财务公司", "Finance Company");
            companyTypes.Add("投资、咨询机构", "Investment, advisory bodies");
            companyTypes.Add("其他金融(服务)机构", "Other financial (services) agencies");
            companyTypes.Add("一般企业", "General corporate");
            companyTypes.Add("其它", "Other");
            companyTypes.Add("投资管理机构", "Investment management institutions");
            foreach (var companyOption in _zcxRepository.GetCompanyType())
            {
                var companyType = companyOption.PAR_NAME.Trim();
                if (companyType.Equals("(非政府机构、事业单位、金融机构)一般企业"))
                    companyType = "一般企业";
                if (companyType.Equals("非机构之其他类型"))
                    companyType = "其它";

                var option = new SelectListItem
                {
                    Text = companyType,
                    Value = companyOption.PAR_CODE.ToString()
                };
                _zcxCompanyTypeCn.Add(option);

                var optionEn = new SelectListItem
                {
                    Text = GetCompanyType(companyType),
                    Value = companyOption.PAR_CODE.ToString()
                };
                _zcxCompanyTypeEn.Add(optionEn);
            }
            #endregion


            _wmpBrokerOrgTypeCn = _wmpBrokerOrgTypeCn.Concat(_wmpRepository.GetWmpBrokerOrgType()).ToList();
            _wmpBrokerOrgTypeEn = _wmpBrokerOrgTypeEn.Concat(_wmpRepository.GetWmpBrokerOrgType()).ToList();

            _wmpBrokerProdTypeCn = _wmpBrokerProdTypeCn.Concat(_wmpRepository.GetWmpBrokerProductType()).ToList();
            _wmpBrokerProdTypeEn = _wmpBrokerProdTypeEn.Concat(_wmpRepository.GetWmpBrokerProductType()).ToList();

            _wmpBrokerInvestTypeCn = _wmpBrokerInvestTypeCn.Concat(_wmpRepository.GetWmpBrokerInvestType()).ToList();
            _wmpBrokerInvestTypeEn = _wmpBrokerInvestTypeEn.Concat(_wmpRepository.GetWmpBrokerInvestType()).ToList();

            _wmpBrokerLowestTypeCn = _wmpBrokerLowestTypeCn.Concat(_wmpRepository.GetWmpBrokerLowestType()).ToList();
            _wmpBrokerLowestTypeEn = _wmpBrokerLowestTypeEn.Concat(_wmpRepository.GetWmpBrokerLowestType()).ToList();

            _wmpBrokerQdiiTypeCn = _wmpBrokerQdiiTypeCn.Concat(_wmpRepository.GetWmpBrokerQdiiType()).ToList();
            _wmpBrokerQdiiTypeEn = _wmpBrokerQdiiTypeEn.Concat(_wmpRepository.GetWmpBrokerQdiiType()).ToList();

            _wmpBrokerStateTypeCn = _wmpBrokerStateTypeCn.Concat(_wmpRepository.GetWmpBrokerProdStateType()).ToList();
            _wmpBrokerStateTypeEn = _wmpBrokerStateTypeEn.Concat(_wmpRepository.GetWmpBrokerProdStateType()).ToList();

            _wmpBrokerBankTypeCn = _wmpBrokerBankTypeCn.Concat(_wmpRepository.GetWmpBrokerBankType()).ToList();
            _wmpBrokerBankTypeEn = _wmpBrokerBankTypeEn.Concat(_wmpRepository.GetWmpBrokerBankType()).ToList();


            #region bond Market & Trustee

            _bondMarketsCn.Add(new SelectListItem() { Text = "上海交易所", Value = "SHH" });
            _bondMarketsCn.Add(new SelectListItem() { Text = "深圳交易所", Value = "SHZ" });
            _bondMarketsCn.Add(new SelectListItem() { Text = "银行间", Value = "CFS" });
            _bondMarketsCn.Add(new SelectListItem() { Text = "其他", Value = "OTH" });

            _bondMarketsEn.Add(new SelectListItem() { Text = "SHANGHAI STOCK EXCHANGE", Value = "SHH" });
            _bondMarketsEn.Add(new SelectListItem() { Text = "SHENZHEN STOCK EXCHANGE", Value = "SHZ" });
            _bondMarketsEn.Add(new SelectListItem() { Text = "CHINA FOREIGN EXCHANGE TRADE SYSTEM", Value = "CFS" });
            _bondMarketsEn.Add(new SelectListItem() { Text = "Other", Value = "OTH" });
 

            _bondTrusteesCn.Add(new SelectListItem() { Text = "中债登", Value = "00038600074336f7" });
            _bondTrusteesCn.Add(new SelectListItem() { Text = "中证登-上海", Value = "0003860028b56a72" });
            _bondTrusteesCn.Add(new SelectListItem() { Text = "中证登-深圳", Value = "0003860074798e06" });
            _bondTrusteesCn.Add(new SelectListItem() { Text = "上清所", Value = "000405048503098a" });
            _bondTrusteesCn.Add(new SelectListItem() { Text = "其他", Value = "OTH" });

            _bondTrusteesEn.Add(new SelectListItem() { Text = "CHINA GOVERNMENT SECURITIES DEPOSITORY TRUST & CLEARING LTD", Value = "00038600074336f7" });
            _bondTrusteesEn.Add(new SelectListItem() { Text = "CHINA SECURITIES DEPOSITORY & CLEARING CORPORATION LTD (SHANGHAI BRANCH)", Value = "0003860028b56a72" });
            _bondTrusteesEn.Add(new SelectListItem() { Text = "CHINA SECURITIES DEPOSITORY & CLEARING CORPORATION LTD (SHENZHEN BRANCH)", Value = "0003860074798e06" });
            _bondTrusteesEn.Add(new SelectListItem() { Text = "SHANGHAI CLEARINGHOUSE", Value = "000405048503098a" });
            _bondTrusteesEn.Add(new SelectListItem() { Text = "Other", Value = "OTH" });


            #endregion

        }


        public List<SelectListItem> WMPBrokerBankType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerBankTypeEn : _wmpBrokerBankTypeCn;
            }
        }

        public List<SelectListItem> WMPRegion
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpRegionEn : _wmpRegionCn;
            }
        } 

        public List<SelectListItem> WMPBrokerStateType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerStateTypeEn : _wmpBrokerStateTypeCn;
            }
        } 


        public List<SelectListItem> WMPBrokerQdiiType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerQdiiTypeEn : _wmpBrokerQdiiTypeCn;
            }
        } 


        public List<SelectListItem> WMPBrokerLowestType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerLowestTypeEn : _wmpBrokerLowestTypeCn;
            }
        } 

        public List<SelectListItem> WMPBrokerInvestType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerInvestTypeEn : _wmpBrokerInvestTypeCn;
            }
        } 

        public List<SelectListItem> WMPBrokerProdType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerProdTypeEn : _wmpBrokerProdTypeCn;
            }
        }

        public List<SelectListItem> WmpBrokerOrgType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBrokerOrgTypeEn : _wmpBrokerOrgTypeCn;
            }
        }

        private string GetCompanyType(string key)
        {
            return companyTypes.ContainsKey(key) ? companyTypes[key] : "";
        }

        public List<SelectListItem> ZCXCompanyType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _zcxCompanyTypeEn : _zcxCompanyTypeCn;
            }
        }

        /// <summary>
        /// Gets or sets the coupon class.
        /// </summary>
        /// <value>The coupon class.</value>
        public List<SelectListItem> CouponClass
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ?_couponClassEn: _couponClassCn;
            }
        }

        /// <summary>
        /// Gets or sets the asset class.
        /// </summary>
        /// <value>The asset class.</value>
        public List<SelectListItem> AssetClass
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _assetClassEn : _assetClassCn;
            }
        }

        /// <summary>
        /// Gets or sets the abs bond class.
        /// </summary>
        /// <value>The asset class.</value>
        public List<SelectListItem> AbsBondClass
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _bondClassEn : _bondClassCn;
            }
        }

        /// <summary>
        /// Gets or sets the rate type.
        /// </summary>
        /// <value>The asset class.</value>
        public List<SelectListItem> AbsRateType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _rateTypeEn : _rateTypeCn;
            }
        }

        /// <summary>
        /// Gets or sets the rate type.
        /// </summary>
        /// <value>The asset class.</value>
        public List<SelectListItem> AbsRateHis
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _rateHisEn : _rateHisCn;
            }
        }
        /// <summary>
        /// Gets or sets the asset class.
        /// </summary>
        /// <value>The asset class.</value>
        public List<SelectListItem> OptionClass
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _optionClassEn : _optionClassCn;
            }
        }

        public List<SelectListItem> BondRatingClass
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _bondRatingEN : _bondRatingCN;
            }
        }

        public List<SelectListItem> WMPBankType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpBankTypeEn : _wmpBankTypeCn;
            }
        }

        public List<SelectListItem> WMPReportType
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpReportTypeEn : _wmpReportTypeCn;
            }
        }
        
        public List<SelectListItem> WMPCurrency
        {
            get
            {
                return _wmpCurrency;
            }
        }
        
        public List<SelectListItem> WMPYield
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpYieldEn : _wmpYieldCn;
            }
        }
        public List<SelectListItem> WMPInvest
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _wmpInvestEn : _wmpInvestCn;
            }
        }


        public List<SelectListItem> BondMarkets
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _bondMarketsEn : _bondMarketsCn;
            }
        }
        public List<SelectListItem> BondTrustees
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _bondTrusteesEn : _bondTrusteesCn;
            }
        }
        public List<SelectListItem> IssuerInduSector
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _issuerInduSectorEn : _issuerInduSectorCn;
            }
        }
        public List<SelectListItem> PartyCntryIncorp
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _partyCntryIncorpEn : _partyCntryIncorpCn;
            }
        }
        public List<SelectListItem> HomeItemTopic
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? _HomeItemEn : _HomeItemCn;
            }
        }
    }
}