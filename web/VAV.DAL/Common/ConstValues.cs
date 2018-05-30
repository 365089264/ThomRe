namespace VAV.DAL.Common
{
	public class ConstValues
	{
		#region Type option for Open Market
		public const string Type_All = "ALL";
		public const string Type_CBankBill = "CBB";
		public const string Type_CBankBillIE = "CBBIE";
		public const string Type_Repo = "RP";
		public const string Type_RepoIE = "RPIE";
		public const string Type_ReverseRepo = "RRP";
		public const string Type_ReverseRepoIE = "RRPIE";
        public const string Type_MLF = "MLF";
        public const string Type_MLFIE = "MLFIE";
		public const string Type_FMD = "FMD";
		public const string Type_FMDIE = "FMDIE";
		#endregion

		#region Unit Option
		public const string Unit_100M = "100M";
		public const string Unit_M = "M";
		public const string Unit_10K = "10K";
		public const string Unit_K = "K";
		#endregion

		#region Category
		public const string category_Term = "Term";
		public const string category_Variety = "Variety";
		#endregion

		#region Current context
		public const  string english = "English";
		public const  string chinese = "中文";
		#endregion

		#region Type option for Bond Market
		public const string Type_Bond_Class = "Bond_Class"; //Bond Class
		public const string Type_Bond_Rating = "Bond_Rating"; //Bond Rating
		public const string Type_Maturity_Term = "Maturity_Term"; //Maturity Term
		public const string Type_Issuer_Rating = "Issuer_Rating"; //Issuer Rating
		public const string Type_Coupon_Type = "Coupon Type"; //Coupon Type
		public const string Type_Option = "Option"; //Option
		#endregion

		#region Bond Type

		//资产支持证券 记帐式国债 央行票据 集合企业债 商业银行债券 短期融资券 凭证式国债 政府支持机构债 政府支持债券 
		//地方企业债 中期票据 地方政府债 非银行金融机构债 中央企业债 其它 政策性银行债券 定向工具 储蓄国债 集合票据 
		//证券公司短期融资券 证券公司债 特种金融债 超短期融资券 国际机构债券 

		public const string Bond_Type_ABS_MBS = "AMS";
		public const string Bond_Type_Book_Entry_Treasury_Bonds = "BTB";
		public const string Bond_Type_CentralBank_Bills = "CBB";
		public const string Bond_Type_Collecting_Bonds = "CLB";
		public const string Bond_Type_Commercial_Bank_Bonds = "CMB";
		public const string Bond_Type_Commercial_Paper = "CMP";
		public const string Bond_Type_Certificate_Treasury_Bonds = "CTB";
		public const string Bond_Type_Government_Agency = "GAG";
		public const string Bond_Type_Government_Backed_Bonds = "GBB";
		public const string Bond_Type_Local_Corporate_Bonds = "LCB";
		public const string Bond_Type_Medium_Term_Notes = "MTN";
		public const string Bond_Type_Municipal = "MUN";
		public const string Bond_Type_NonBank_Financial_Institution_Bonds = "NBI";
		public const string Bond_Type_National_Corporate_Bonds = "NCB";
		public const string Bond_Type_Other_Instruments = "OTH";
		public const string Bond_Type_Policy_Bank_Bonds = "PBB";
		public const string Bond_Type_Private_Placement_Notes = "PPN";
		public const string Bond_Type_Savings_Bonds = "SAB";
		public const string Bond_Type_Enterprise_Collecting_Notes = "SCN";
		public const string Bond_Type_Securities_Firm_Commercial_Paper = "SCP";
		public const string Bond_Type_Securities_Firm_Bonds = "SFB";
		public const string Bond_Type_Special_Financial_Bonds = "SPB";
		public const string Bond_Type_Super_Short_Term_Commercial_Paper = "STP";
		public const string Bond_Type_Supranational = "SUP";			

		#endregion

		#region Bond Term

		public const string Term_3M = "3M";
		public const string Term_6M = "6M";
		public const string Term_9M = "9M";
		public const string Term_1Y = "1Y";
		public const string Term_2Y = "2Y";
		public const string Term_3Y = "3Y";
		public const string Term_5Y = "5Y";
		public const string Term_7Y = "7Y";
		public const string Term_10Y = "10Y";
		public const string Term_15Y = "15Y";
		public const string Term_20Y = "20Y";
		public const string Term_30Y = "30Y";

		#endregion

		#region Bond Term Interval

		public const string Term_LT1Y = "Under 1Y";
		public const string Term_GT1YAndLT2Y = "1Y~2Y";
		public const string Term_GT2YAndLT3Y = "2Y~3Y";
		public const string Term_GT3YAndLT4Y = "3Y~4Y";
		public const string Term_GT4YAndLT5Y = "4Y~5Y";
		public const string Term_GT5YAndLT6Y = "5Y~6Y";
		public const string Term_GT6YAndLT7Y = "6Y~7Y";
		public const string Term_GT7YAndLT8Y = "7Y~8Y";
		public const string Term_GT8YAndLT9Y = "8Y~9Y";
		public const string Term_GT9YAndLT10Y = "9Y~10Y";
		public const string Term_GT10Y = "Over 10Y";
	
		#endregion

		#region Rating

		public const string Rating_NR = "NR";
		public const string Rating_TripleA = "AAA";
		public const string Rating_TripleA_Minus = "AAA-";
		public const string Rating_DoubleA_Plus = "AA+";
		public const string Rating_DoubleA = "AA";
		public const string Rating_DoubleA_Minus = "AA-";
		public const string Rating_A_Plus = "A+";
		public const string Rating_A = "A";
		public const string Rating_A_Minus = "A-";
		public const string Rating_A1 = "A-1";
		public const string Rating_A2 = "A-2";
		public const string Rating_TripleB_Plus = "BBB+";
		public const string Rating_TripleB = "BBB";
		public const string Rating_TripleB_Minus = "BBB-";
		public const string Rating_DoubleB_Plus = "BB+";
		public const string Rating_DoubleB = "BB";
		public const string Rating_DoubleB_Minus = "BB-";
		public const string Rating_B_Plus = "B+";
		public const string Rating_B = "B";
		public const string Rating_B_Minus = "B-";
		public const string Rating_TripleC_Plus = "CCC+";
		public const string Rating_TripleC = "CCC";
		public const string Rating_TripleC_Minus = "CCC-";
		public const string Rating_DoubleC = "CC";
		public const string Rating_C = "C";
		public const string Rating_D = "D";
						   
		#endregion

		#region DimSumBond

		public const string Option_Debt_Type = "Debt_Type";
		public const string Option_Asset_Type = "Asset_Type";
		public const string Option_DomicileOfIssuer = "Domicile_Of_Issuer";
		public const string Option_RatingInfo = "Rating_Info";
		public const string Option_Term = "Term";
		public const string Option_Indu = "Indu_Of_Issuer";
		public const string Option_Issue_Country = "Issue_Country";

		public const string Header_Type = "Type";
		public const string Header_IBalance = "IBalance";
		public const string Header_Issues = "Issues";
		public const string Header_IssuesPnt = "IssuesPnt";
		public const string Header_IssueAmount = "IssueAmount";
		public const string Header_IssueAmountPnt = "IssueAmountPnt";
		public const string Header_Maturity = "Maturity";
		public const string Header_MaturityPnt = "MaturityPnt";
		public const string Header_MaturityAmount = "MaturityAmount";
		public const string Header_MaturityAmountPnt = "MaturityAmountPnt";
		public const string Header_EndBalance = "EndBalance";

		#endregion

		#region WMP
		public const string WMP_PState_All = "all";
		public const string WMP_PState_PreSale = "PreSale";
		public const string WMP_PState_InSale = "InSale";
		public const string WMP_PState_StopSale = "StopSale";
        public const string WMP_Term_All = "all";
		public const string WMP_Term_LT_1M = "LT1M";
		public const string WMP_Term_1M_3M = "1MTo3M";
		public const string WMP_Term_3M_6M = "3MTo6M";
		public const string WMP_Term_6M_12M = "6MTo12M";
		public const string WMP_Term_GT_12M = "GT12M";
		public const string WMP_Term_Unpublished = "Unpublished";
        public const string WMP_InitAmount_All = "all";
		public const string WMP_InitAmount_LT_50TH = "LT50th";
		public const string WMP_InitAmount_50TH_100TH = "50th_100th";
		public const string WMP_InitAmount_100TH_200TH = "100th_200th";
		public const string WMP_InitAmount_200TH_500TH = "200th_500th";
		public const string WMP_InitAmount_500TH_1000TH = "500th_1000th";
		public const string WMP_InitAmount_GT_1000TH = "GT1000th";
        public const string WMP_Yield_All = "all";
		public const string WMP_Yield_LT_2hpt = "LT2hpt";
		public const string WMP_Yield_2h_5pt = "2hpt_5pt";
		public const string WMP_Yield_5pt_10pt = "5pt_10pt";
		public const string WMP_Yield_GT_10pt = "GT10pt";
		public const string WMP_Yield_Unpublished = "Unpublished";
        public const string WMP_QDII_All = "all";
		public const string WMP_QDII_Yes = "Yes";
		public const string WMP_QDII_No = "No";
		#endregion

        #region IPP
        public const string IPP_Upload_File = "Upload_File";
        public const string IPP_Upload_WebSite = "Upload_Website";
        public const string IPP_Upload_Ric = "Upload_Ric";
        public const string IPP_Upload_Ric_Chart = "Upload_RIC_Chart";
        public const string IPP_Upload_Ric_Quote = "Upload_RIC_Quote";
        public const string IPP_Upload_Ric_QuoteList = "Upload_RIC_QuoteList";
        public const string IPP_Upload_Ric_News = "Upload_RIC_News";
        public const string IPP_Upload_RMLink = "Upload_RMLink";

        #endregion

        #region wmp broker

	    public const string WmpBroker_DECLAREDATE = "DECLAREDATE";
	    public const string WmpBroker_ESTAB_DATE = "ESTAB_DATE";
	    public const string WmpBroker_STARTDATE = "STARTDATE";
	    public const string WmpBroker_ENDDATE = "ENDDATE";
	    public const string WmpBroker_EXPE_ENDDATE = "EXPE_ENDDATE";

	    #endregion

        public const string ExchangeName = "ExchangeName";
        public const string Bond_Trustee = "Trustee";
	}

	#region
	public class Constants
	{
		public enum SummarizingFrequency
		{
			Day = 1,
			Week = 2,
			Month = 3,
			Quarter = 4,
			Year = 5
		}

	}
	#endregion

}