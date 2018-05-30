using System.Collections.Generic;
using VAV.DAL.Common;
using System.Web.Mvc;
using VAV.DAL.WMP;
using VAV.DAL.ResearchReport;
using VAV.DAL.Report;

namespace VAV.Web.Common
{
    /// <summary>
    /// used to distinguish different dropdown list items on the view
    /// </summary>
    public class HtmlOption
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// add values to the objects on the view
    /// </summary>
    public static class HtmlUtil
    {
        private static readonly WMPRepository _wmpRepository = (WMPRepository)DependencyResolver.Current.GetService(typeof(WMPRepository));
        private static readonly BondInfoRepository _bondInfoRepository = (BondInfoRepository)DependencyResolver.Current.GetService(typeof(BondInfoRepository));
        private static readonly ResearchReportRepository _rsRepository = (ResearchReportRepository)DependencyResolver.Current.GetService(typeof(ResearchReportRepository));
        
        public static List<SelectListItem> SelectOptions()
        {
            var options = new List<SelectListItem>();
            options.Add(new SelectListItem { Selected = true, Value = ConstValues.Type_CBankBill, Text = Resources.Global.Type_CBankBill });
            options.Add(new SelectListItem { Selected = true, Value = ConstValues.Type_Repo, Text = Resources.Global.Type_Repo });
            options.Add(new SelectListItem { Selected = true, Value = ConstValues.Type_ReverseRepo, Text = Resources.Global.Type_ReverseRepo });
            options.Add(new SelectListItem { Selected = true, Value = ConstValues.Type_MLF, Text = Resources.Global.OpenMarketMLF });
            options.Add(new SelectListItem { Selected = false, Value = ConstValues.Type_FMD, Text = Resources.Global.Type_Fmd });
            return options;
        }

        public static List<HtmlOption> CookOptions(string key)
        {
            var list = new List<HtmlOption>();
            switch (key)
            {
                case "unit":
                    list.Add(new HtmlOption { ID = ConstValues.Unit_100M, Name = Resources.Global.Unit_Option_100M });
                    list.Add(new HtmlOption { ID = ConstValues.Unit_M, Name = Resources.Global.Unit_Option_M });
                    list.Add(new HtmlOption { ID = ConstValues.Unit_10K, Name = Resources.Global.Unit_Option_10K });
                    list.Add(new HtmlOption { ID = ConstValues.Unit_K, Name = Resources.Global.Unit_Option_K });
                    break;
                case "type":
                    list.Add(new HtmlOption { ID = ConstValues.Type_All, Name = Resources.Global.Type_All });
                    list.Add(new HtmlOption { ID = ConstValues.Type_CBankBill, Name = Resources.Global.Type_CBankBill });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Repo, Name = Resources.Global.Type_Repo });
                    list.Add(new HtmlOption { ID = ConstValues.Type_ReverseRepo, Name = Resources.Global.Type_ReverseRepo });
                    list.Add(new HtmlOption { ID = ConstValues.Type_FMD, Name = Resources.Global.Type_Fmd });
                    break;
                case "category":
                    list.Add(new HtmlOption { ID = "OperationTerm", Name = Resources.Global.Category_Term });
                    list.Add(new HtmlOption { ID = "OperationType", Name = Resources.Global.Category_Variety });
                    break;
                case "bond_market_classify":
                    list.Add(new HtmlOption { ID = ConstValues.Type_Bond_Class, Name = Resources.Global.Type_Bond_Class });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Bond_Rating, Name = Resources.Global.Type_Bond_Rating });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Maturity_Term, Name = Resources.Global.Type_Maturity_Term });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Issuer_Rating, Name = Resources.Global.Type_Issuer_Rating });
                    break;
                case "bond_market_classify2":
                    list.Add(new HtmlOption { ID = ConstValues.Type_Bond_Class, Name = Resources.Global.Type_Bond_Class });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Maturity_Term, Name = Resources.Global.Type_Maturity_Term });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Coupon_Type, Name = Resources.Global.Type_Coupon_Type });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Option, Name = Resources.Global.Type_Option });
                    break;
                case "bond_market_classify3":
                    list.Add(new HtmlOption { ID = ConstValues.Type_Bond_Class, Name = Resources.Global.Type_Bond_Class });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Bond_Rating, Name = Resources.Global.Type_Bond_Rating });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Maturity_Term, Name = Resources.Global.Type_Maturity_Term });
                    list.Add(new HtmlOption { ID = ConstValues.Type_Issuer_Rating, Name = Resources.Global.Type_Issuer_Rating });
                    list.Add(new HtmlOption { ID = ConstValues.Option_DomicileOfIssuer, Name = Resources.Global.Option_DomicileOfIssuer });
                    list.Add(new HtmlOption { ID = ConstValues.Option_Indu, Name = Resources.Global.Option_Indu });
                    list.Add(new HtmlOption { ID = ConstValues.ExchangeName, Name = Resources.Global.Bond_Markets });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Trustee, Name = Resources.Global.Bond_Trustee });
                    break;
                case "bond_class":
                    foreach (var y in UIStaticDataCache.Instance.AssetClass)
                    {
                        list.Add(new HtmlOption { ID = y.Value, Name = y.Text });
                    }
                    break;
                case "bond_term":
                    list.Add(new HtmlOption { ID = ConstValues.Term_3M, Name = Resources.Global.Term_3M });
                    list.Add(new HtmlOption { ID = ConstValues.Term_6M, Name = Resources.Global.Term_6M });
                    list.Add(new HtmlOption { ID = ConstValues.Term_9M, Name = Resources.Global.Term_9M });
                    list.Add(new HtmlOption { ID = ConstValues.Term_1Y, Name = Resources.Global.Term_1Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_2Y, Name = Resources.Global.Term_2Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_3Y, Name = Resources.Global.Term_3Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_5Y, Name = Resources.Global.Term_5Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_7Y, Name = Resources.Global.Term_7Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_10Y, Name = Resources.Global.Term_10Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_15Y, Name = Resources.Global.Term_15Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_20Y, Name = Resources.Global.Term_20Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_30Y, Name = Resources.Global.Term_30Y });
                    break;
                case "maturity_term":
                    list.Add(new HtmlOption { ID = ConstValues.Term_LT1Y, Name = Resources.Global.Term_LT1Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT1YAndLT2Y, Name = Resources.Global.Term_GT1YAndLT2Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT2YAndLT3Y, Name = Resources.Global.Term_GT2YAndLT3Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT3YAndLT4Y, Name = Resources.Global.Term_GT3YAndLT4Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT4YAndLT5Y, Name = Resources.Global.Term_GT4YAndLT5Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT5YAndLT6Y, Name = Resources.Global.Term_GT5YAndLT6Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT6YAndLT7Y, Name = Resources.Global.Term_GT6YAndLT7Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT7YAndLT8Y, Name = Resources.Global.Term_GT7YAndLT8Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT8YAndLT9Y, Name = Resources.Global.Term_GT8YAndLT9Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT9YAndLT10Y, Name = Resources.Global.Term_GT9YAndLT10Y });
                    list.Add(new HtmlOption { ID = ConstValues.Term_GT10Y, Name = Resources.Global.Term_GT10Y });
                    break;
                case "bond_rating":
                case "issuer_rating":
                    list.Add(new HtmlOption { ID = ConstValues.Rating_NR, Name = "NR" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleA, Name = "AAA" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleA_Minus, Name = "AAA-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleA_Plus, Name = "AA+" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleA, Name = "AA" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleA_Minus, Name = "AA-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_A_Plus, Name = "A+" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_A, Name = "A" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_A_Minus, Name = "A-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_A1, Name = "A-1" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_A2, Name = "A-2" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleB_Plus, Name = "BBB+" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleB, Name = "BBB" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleB_Minus, Name = "BBB-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleB_Plus, Name = "BB+" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleB, Name = "BB" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleB_Minus, Name = "BB-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_B_Plus, Name = "B+" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_B, Name = "B" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_B_Minus, Name = "B-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleC_Plus, Name = "CCC+" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleC, Name = "CCC" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_TripleC_Minus, Name = "CCC-" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_DoubleC, Name = "CC" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_C, Name = "C" });
                    list.Add(new HtmlOption { ID = ConstValues.Rating_D, Name = "D" });
                    break;
                case "summarizing_frequency":
                    list.Add(new HtmlOption { ID = Constants.SummarizingFrequency.Week.ToString(), Name = Resources.Global.SummarizingFrequency_Week });
                    list.Add(new HtmlOption { ID = Constants.SummarizingFrequency.Day.ToString(), Name = Resources.Global.SummarizingFrequency_Day });
                    list.Add(new HtmlOption { ID = Constants.SummarizingFrequency.Month.ToString(), Name = Resources.Global.SummarizingFrequency_Month });
                    list.Add(new HtmlOption { ID = Constants.SummarizingFrequency.Quarter.ToString(), Name = Resources.Global.SummarizingFrequency_Quarter });
                    list.Add(new HtmlOption { ID = Constants.SummarizingFrequency.Year.ToString(), Name = Resources.Global.SummarizingFrequency_Year });
                    break;
                case "dimSumBond_option":
                    list.Add(new HtmlOption { ID = ConstValues.Option_DomicileOfIssuer, Name = Resources.Global.Option_DomicileOfIssuer });
                    list.Add(new HtmlOption { ID = ConstValues.Option_Issue_Country, Name = Resources.Global.Option_Issue_Country });
                    list.Add(new HtmlOption { ID = ConstValues.Option_Asset_Type, Name = Resources.Global.Option_Asset_Type });
                    list.Add(new HtmlOption { ID = ConstValues.Option_Term, Name = Resources.Global.Option_Term });
                    list.Add(new HtmlOption { ID = ConstValues.Option_Debt_Type, Name = Resources.Global.Option_Debt_Type });
                    list.Add(new HtmlOption { ID = ConstValues.Option_RatingInfo, Name = Resources.Global.Option_RatingInfo });
                    list.Add(new HtmlOption { ID = ConstValues.Option_Indu, Name = Resources.Global.Option_Indu });
                    break;
                case "dimSumBondSummary_Header":
                    list.Add(new HtmlOption { ID = ConstValues.Header_Type, Name = Resources.Global.DimSum_Column_Type });
                    list.Add(new HtmlOption { ID = ConstValues.Header_IBalance, Name = Resources.Global.DimSum_Column_IBalance });
                    list.Add(new HtmlOption { ID = ConstValues.Header_Issues, Name = Resources.Global.DimSum_Column_Issues });
                    list.Add(new HtmlOption { ID = ConstValues.Header_IssuesPnt, Name = Resources.Global.DimSum_Column_IssuesPtn });
                    list.Add(new HtmlOption { ID = ConstValues.Header_IssueAmount, Name = Resources.Global.DimSum_Column_IssueAmount });
                    list.Add(new HtmlOption { ID = ConstValues.Header_IssueAmountPnt, Name = Resources.Global.DimSum_Column_IssueAmountPtn });
                    list.Add(new HtmlOption { ID = ConstValues.Header_Maturity, Name = Resources.Global.DimSum_Column_Maturities });
                    list.Add(new HtmlOption { ID = ConstValues.Header_MaturityPnt, Name = Resources.Global.DimSum_Column_MaturitiesPtn });
                    list.Add(new HtmlOption { ID = ConstValues.Header_MaturityAmount, Name = Resources.Global.DimSum_Column_MaturityAmount });
                    list.Add(new HtmlOption { ID = ConstValues.Header_MaturityAmountPnt, Name = Resources.Global.DimSum_Column_MaturityAmountPtn });
                    list.Add(new HtmlOption { ID = ConstValues.Header_EndBalance, Name = Resources.Global.DimSum_Column_EndBalance });
                    break;
                case "wmpProductSate":
                    list.Add(new HtmlOption { ID = ConstValues.WMP_PState_All, Name = Resources.Global.WMP_Pstate_All });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_PState_PreSale, Name = Resources.Global.WMP_Pstate_PreSale });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_PState_InSale, Name = Resources.Global.WMP_Pstate_InSale });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_PState_StopSale, Name = Resources.Global.WMP_Pstate_StopSale });
                    break;
                case "wmpTerm":
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_All, Name = Resources.Global.WMP_Term_All });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_LT_1M, Name = Resources.Global.WMP_Term_LT_1M });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_1M_3M, Name = Resources.Global.WMP_Term_1M_3M });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_3M_6M, Name = Resources.Global.WMP_Term_3M_6M });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_6M_12M, Name = Resources.Global.WMP_Term_6M_12M });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_GT_12M, Name = Resources.Global.WMP_Term_GT_12M });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Term_Unpublished, Name = Resources.Global.WMP_Term_Unpublished });
                    break;
                case "wmpInitAmount":
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_All, Name = Resources.Global.WMP_InitAmount_ALL });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_LT_50TH, Name = Resources.Global.WMP_InitAmount_LT_50TH });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_50TH_100TH, Name = Resources.Global.WMP_InitAmount_50TH_100TH });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_100TH_200TH, Name = Resources.Global.WMP_InitAmount_100TH_200TH });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_200TH_500TH, Name = Resources.Global.WMP_InitAmount_200TH_500TH });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_500TH_1000TH, Name = Resources.Global.WMP_InitAmount_500TH_1000TH });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_InitAmount_GT_1000TH, Name = Resources.Global.WMP_InitAmount_GT_1000TH });
                    break;
                case "wmpYield":
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Yield_All, Name = Resources.Global.WMP_Yield_All });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Yield_LT_2hpt, Name = Resources.Global.WMP_Yield_LT_2hpt });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Yield_2h_5pt, Name = Resources.Global.WMP_Yield_2h_5pt });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Yield_5pt_10pt, Name = Resources.Global.WMP_Yield_5pt_10pt });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Yield_GT_10pt, Name = Resources.Global.WMP_Yield_GT_10pt });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_Yield_Unpublished, Name = Resources.Global.WMP_Yield_Unpublished });
                    break;
                case "wmpQDII":
                    list.Add(new HtmlOption { ID = ConstValues.WMP_QDII_All, Name = Resources.Global.WMP_QDII_All });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_QDII_Yes, Name = Resources.Global.WMP_QDII_Yes });
                    list.Add(new HtmlOption { ID = ConstValues.WMP_QDII_No, Name = Resources.Global.WMP_QDII_No });
                    break;
                case "wmpCurrency":
                    foreach (var c in _wmpRepository.GetWmpCurrencyOption())
                    {
                        list.Add(new HtmlOption { ID = c.Type.ToString(), Name = Resources.Global.ResourceManager.GetString(string.Format("WMP_Trend_{0}_{1}", "C", c.Type.ToString())) });
                    }
                    break;
                case "wmpInvestOption":
                    list.Add(new HtmlOption { ID = "all", Name = Resources.Global.Type_All });
                    foreach (var i in _wmpRepository.GetWmpInvestOption())
                    {
                        list.Add(new HtmlOption { ID = i.Type, Name = Resources.Global.ResourceManager.GetString(string.Format("WMP_Trend_{0}_{1}", "IBT", i.Type)) });
                    }
                    break;
                case "wmpYieldType":
                    list.Add(new HtmlOption { ID = "all", Name = Resources.Global.Type_All });
                    foreach (var y in _wmpRepository.GetWmpYieldOption())
                    {
                        list.Add(new HtmlOption { ID = y.Type.ToString(), Name = Resources.Global.ResourceManager.GetString(string.Format("WMP_Trend_{0}_{1}", "YT", y.Type)) });
                    }
                    break;
                case "Ipp_UploadType":
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_File, Name = Resources.IPP.IPP_File });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_WebSite, Name = Resources.IPP.IPP_Website });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_Ric, Name = Resources.IPP.IPP_Ric });
                    break;
                case "Home_UploadType":
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_File, Name = Resources.IPP.IPP_File });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_WebSite, Name = Resources.IPP.IPP_Website });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_Ric_Chart, Name = Resources.IPP.IPP_Ric_Chart });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_Ric_Quote, Name = Resources.IPP.IPP_Ric_Quote });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_Ric_QuoteList, Name = Resources.IPP.IPP_Ric_QuoteList });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_Ric_News, Name = Resources.IPP.IPP_Ric_News });
                    list.Add(new HtmlOption { ID = ConstValues.IPP_Upload_RMLink, Name = Resources.IPP.IPP_RMLink });
                    break;
                case "WmpBrokerDateType":
                    list.Add(new HtmlOption { ID = ConstValues.WmpBroker_DECLAREDATE, Name = Resources.WMP.Broker_DECLAREDATE });
                    list.Add(new HtmlOption { ID = ConstValues.WmpBroker_ESTAB_DATE, Name = Resources.WMP.Broker_ESTAB_DATE });
                    list.Add(new HtmlOption { ID = ConstValues.WmpBroker_STARTDATE, Name = Resources.WMP.Broker_STARTDATE });
                    list.Add(new HtmlOption { ID = ConstValues.WmpBroker_ENDDATE, Name = Resources.WMP.Broker_ENDDATE });
                    list.Add(new HtmlOption { ID = ConstValues.WmpBroker_EXPE_ENDDATE, Name = Resources.WMP.Broker_EXPE_ENDDATE });
                    break;
                case "MDBondOption":
                    list.Add(new HtmlOption { ID = "all", Name = Resources.Global.Type_All });
                    list.Add(new HtmlOption { ID = "y", Name = Resources.Global.Common_Yes });
                    list.Add(new HtmlOption { ID = "n", Name = Resources.Global.Common_No });
                    break;
                case "OthBondClass":
                    list.Add(new HtmlOption { ID = "all", Name = Resources.Global.Type_All });
                    list.Add(new HtmlOption { ID = "CNCORP", Name = Resources.Global.Bond_Type_Corporate_Bonds });
                    list.Add(new HtmlOption { ID = "CNENTERP", Name = Resources.Global.Bond_Type_Enterprise_Bonds });
                    list.Add(new HtmlOption { ID = "CDBCRP", Name = Resources.Global.Bond_Type_Certificate_Deposit });
                    list.Add(new HtmlOption { ID = "CNSMEPPB", Name = Resources.Global.Bond_Type_SMEPPBonds });
                    list.Add(new HtmlOption { ID = "Oth", Name = Resources.Global.Tip_Other });
                    break;
                case "CnESdDailyOutputColumn":
                    list.Add(new HtmlOption { Name = Resources.CnE.CNE_SdR_DailyOutputDate });
                    list.Add(new HtmlOption { Name = Resources.CnE.CNE_SdR_DailyOutputCapacity });
                    list.Add(new HtmlOption { Name = Resources.CnE.CNE_SdR_DailyOutputGasoline });
                    list.Add(new HtmlOption { Name = Resources.CnE.CNE_SdR_DailyOutputDiesel });
                    break;
                case "CnESdDeviceInfoColumn":
                    list.Add(new HtmlOption { Name = Resources.CnE.CNE_SdR_Device });
                    list.Add(new HtmlOption { Name = Resources.CnE.CNE_SdR_DeviceYieldByTon });
                    list.Add(new HtmlOption { Name = Resources.CnE .CNE_SdR_YieldByBarrel});
                    break;
                case "bondUnderWrites":
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Medium_Term_Notes, Name = Resources.Global.Bond_Type_Medium_Term_Notes });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Commercial_Paper, Name = Resources.Global.Bond_Type_Commercial_Paper });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Super_Short_Term_Commercial_Paper, Name = Resources.Global.Bond_Type_Super_Short_Term_Commercial_Paper });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Enterprise_Collecting_Notes, Name = Resources.Global.Bond_Type_Enterprise_Collecting_Notes });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Municipal, Name = Resources.Global.Bond_Type_Municipal });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Securities_Firm_Commercial_Paper, Name = Resources.Global.Bond_Type_Securities_Firm_Commercial_Paper });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Government_Agency, Name = Resources.Global.Bond_Type_Government_Agency });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Private_Placement_Notes, Name = Resources.Global.Bond_Type_Private_Placement_Notes });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Commercial_Bank_Bonds, Name = Resources.Global.Bond_Type_Commercial_Bank_Bonds });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_NonBank_Financial_Institution_Bonds, Name = Resources.Global.Bond_Type_NonBank_Financial_Institution_Bonds });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Policy_Bank_Bonds, Name = Resources.Global.Bond_Type_Policy_Bank_Bonds });
                    list.Add(new HtmlOption { ID = ConstValues.Bond_Type_Local_Corporate_Bonds, Name = Resources.Global.Bond_Type_Local_Corporate_Bonds });
                    break;
                case "domicile_of_issuer":
                    foreach (var y in UIStaticDataCache.Instance.PartyCntryIncorp)
                    {
                        list.Add(new HtmlOption { ID = y.Value, Name = y.Text });
                    }
                    list.Add(new HtmlOption { ID ="OTH" , Name =Resources.Global.Tip_Other  });
                    break;
                case "indu_of_issuer":
                    foreach (var y in UIStaticDataCache.Instance.IssuerInduSector)
                    {
                        list.Add(new HtmlOption { ID = y.Value, Name = y.Text });
                    }
                    break;
                case "exchangename":
                    foreach (var y in UIStaticDataCache.Instance.BondMarkets)
                    {
                        if (y.Value == "all") continue;
                        list.Add(new HtmlOption { ID = y.Value, Name = y.Text });
                    }
                    break;
                case "trustee":
                    foreach (var y in UIStaticDataCache.Instance.BondTrustees)
                    {
                        if (y.Value == "all") continue;
                        list.Add(new HtmlOption { ID = y.Value, Name = y.Text });
                    }
                    break;
                case "topic":
                    list.Add(new HtmlOption { ID = "0", Name = Resources.Global.Type_All });
                    foreach (var y in UIStaticDataCache.Instance.HomeItemTopic)
                    {
                        if (y.Value == "all") continue;
                        list.Add(new HtmlOption { ID = y.Value, Name = y.Text });
                    }
                    break;
                default:
                    break;
            }

            return list;
        }

        public static List<SelectListItem> CookSelectOptions(string item)
        {
            var list = new List<SelectListItem>();

            foreach (var o in HtmlUtil.CookOptions(item))
            {
                var option = new SelectListItem
                {
                    Text = o.Name,
                    Value = o.ID
                };
                list.Add(option);
            }

            return list;
        }

        public static List<SelectListItem> CookMultiSelectOptions(string item)
        {
            var list = new List<SelectListItem>();

            foreach (var o in HtmlUtil.CookOptions(item))
            {
                var option = new SelectListItem
                {
                    Text = o.Name,
                    Value = o.ID,
                    Selected = true
                };
                list.Add(option);
            }

            return list;
        }

        public static string GetSubTypeOptions2(string type)
        {
            var optionValues = CookOptions(type.ToLower());
            var options = "";

            foreach (var o in optionValues)
            {
                options += "<option value='" + o.ID + "'>" + o.Name + "</option>";
            }

            return options;
        }

        public static string GetSubTypeOptions3(string type)
        {
            var optionValues = CookOptions(type.ToLower());
            var options = "";

            foreach (var o in optionValues)
            {
                options += "<option value='" + o.ID + "' selected='true'>" + o.Name + "</option>";
            }

            return options;
        }

        public static string GetUnitOptionByKey(string unit)
        {
            string option = "";

            switch (unit)
            {
                case ConstValues.Unit_100M:
                    option = Resources.Global.Unit_Option_100M;
                    break;
                case ConstValues.Unit_M:
                    option = Resources.Global.Unit_Option_M;
                    break;
                case ConstValues.Unit_10K:
                    option = Resources.Global.Unit_Option_10K;
                    break;
                case ConstValues.Unit_K:
                    option = Resources.Global.Unit_Option_K;
                    break;
                default:
                    break;
            }

            return option;
        }

        public static List<SelectListItem> GetWMPGenRefByCode(int code)
        {
            var wmpType = new List<SelectListItem> { new SelectListItem { Selected = true, Text = Resources.Global.Type_All, Value = "all" } };
            wmpType.AddRange(_wmpRepository.GetWmpGenRefByCode(code));
            return wmpType;
        }

        public static List<SelectListItem> GetWmpTrustCompany(string type)
        {
            return _wmpRepository.GetTrustCompany(type);
        }

        public static string GetWmpTrustCompanyHtml(string type)
        {
            var optionValues = GetWmpTrustCompany(type);
            var options = "";

            foreach (var o in optionValues)
            {
                options += "<option value='" + o.Value + "'>" + o.Text + "</option>";
            }

            return options;
        }

        public static List<SelectListItem> GetWmpTrustOrgType()
        {
            var orgType = new List<SelectListItem> { new SelectListItem { Selected = true, Text = Resources.Global.Type_All, Value = "all" } };
            orgType.AddRange(_wmpRepository.GetWmpOrgType());
            return orgType;
        }

        #region Report Search

        public static List<SelectListItem> GetOrgOptions(string orgCode)
        {
            return _rsRepository.GetOrgOptions(orgCode);
        }

        public static List<SelectListItem> GetReportTypeOptions(string orgCode, string reportType)
        {
            return _rsRepository.GetReportTypeOptions(orgCode, reportType);
        }

        public static string GetRsReportTypeOptionHtml(string orgCodes)
        {
            var optionValues = GetReportTypeOptions(orgCodes, "all");
            var options = "";

            foreach (var o in optionValues)
            {
                var selected = o.Selected ? "selected" : "";
                options += "<option value='" + o.Value + "' " + selected + " >" + o.Text + "</option>";
            }

            return options;
        }

        #endregion


        public static string Truncate(string inputString, int length)
        {
            string tempString = string.Empty;
            for (int i = 0, tempIndex = 0; i < inputString.Length; ++i, ++tempIndex)
            {
                if (System.Text.Encoding.UTF8.GetBytes(new char[] { inputString[i] }).Length > 1)
                {
                    ++tempIndex;
                }
                if (tempIndex >= length)
                {
                    tempString += "...";
                    break;
                }
                tempString += inputString[i];
            }
            return tempString;
        }
    }
}