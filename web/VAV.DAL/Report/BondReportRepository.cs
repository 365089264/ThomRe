using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using VAV.DAL.Common;
using VAV.Entities;
using VAV.Model.Data;
using VAV.Model.Data.Bond;
using VAV.DAL.IPP;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;



namespace VAV.DAL.Report
{
    public class BondReportRepository : BaseReportRepository, IBondReportRepository
    {
        /// <summary>
        /// Get report infomation by report id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReportInfo GetReportInfoById(int id)
        {
            using (var cmadb = new CMAEntities())
            {
                return ((from r in cmadb.REPORTDEFINITIONs
                         where r.ID == id
                         select r)
                        .AsEnumerable()
                        .Select(n => new ReportInfo(
                            n.VIEW_NAME,
                            n.VIEWMODEL_NAME,
                            (int) n.ID,
                            n.REPORT_TYPE,
                            n.ENGLISH_NAME,
                            n.CHINESE_NAME,
                            n.TABLE_NAME
                            ))).ToList().FirstOrDefault();
            }
        }

        /// <summary>
        /// Get extra header information by report id.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public IEnumerable<ExtraHeader> GetExtraHeaderById(int reportId)
        {
            using (var cmadb = new CMAEntities())
            {
                return from h in cmadb.REPORTEXTRAHEADERDEFINITIONs
                    where h.REPORT_ID == reportId
                    select new ExtraHeader
                    {
                        HeaderLevel = (int) h.EXTRA_HEADER_LEVEL,
                        HeaderTextCN = h.EXTRA_HEADER_TEXT_CN,
                        HeaderTextEN = h.EXTRA_HEADER_TEXT_EN,
                        HeaderColumnSpan = (int) h.EXTRA_HEADER_COLUMN_SPAN
                    };
            }
        }

        /// <summary>
        /// Get bond issue rate data
        /// </summary>
        /// <param name="bondIssueParams"></param>
        /// <returns></returns>
        public IEnumerable<BondIssueRate> GetBondIssueRatesRepo(BondIssueParams bondIssueParams)
        {
            var term = bondIssueParams.Term;
            Func<BondIssueRate, bool> termCondition;
            switch (term)
            {
                case "6M": termCondition = t => t.term > 170 && t.term < 190; break;
                case "9M": termCondition = t => t.term > 260 && t.term < 280; break;
                case "1Y": termCondition = t => t.term == 1; break;
                case "2Y": termCondition = t => t.term == 2; break;
                case "3Y": termCondition = t => t.term == 3; break;
                case "5Y": termCondition = t => t.term == 5; break;
                case "7Y": termCondition = t => t.term == 7; break;
                case "10Y": termCondition = t => t.term == 10; break;
                case "15Y": termCondition = t => t.term == 15; break;
                case "20Y": termCondition = t => t.term == 20; break;
                case "30Y": termCondition = t => t.term == 30; break;
                default: termCondition = t => t.term > 80 && t.term < 100; break; //3M
            }

            Func<BondIssueRate, bool> ratingCondition = r => r.rating_number == bondIssueParams.Rating;

            IEnumerable<BondIssueRate> repo;
            using (var vavdb = new BondDBEntities())
            {
                repo = (from r in vavdb.BOND
                        where (r.orig_issue_dt >= bondIssueParams.StartDate)
                         && (r.cdc_asset_class_cd == bondIssueParams.BondType)
                         && (r.isfloat == bondIssueParams.IsFloat)
                         && (r.re_issue != "1")
                        select new BondIssueRate
                        {
                            bond_name_cn = r.bond_name_cn,
                            bond_name_en = r.bond_name_en,
                            code = r.code,
                            orig_issue_dt = r.orig_issue_dt,
                            maturity_dt = r.maturity_dt,
                            term = r.term,
                            yield = r.yield,
                            orig_iss_amt = r.orig_iss_amt,
                            coupon_type_cn = r.coupon_class_cn,
                            coupon_type_en = r.coupon_class_en,
                            latest_rating_cd = r.latest_rating_cd,
                            cdc_asset_class_number = r.cdc_asset_class_number,
                            cdc_asset_class_cn = r.cdc_asset_class_cn,
                            cdc_asset_class_en = r.cdc_asset_class_en,
                            isfloat = r.isfloat,
                            rating_number = r.rating_number,
                            assetId = r.assetId,
                            re_issue = r.re_issue
                        }).ToList();
            }

            IEnumerable<BondIssueRate> result = repo;

            if (bondIssueParams.Rating != null && bondIssueParams.Rating != "All")
                result = repo.Where(ratingCondition);

            return result.Where(termCondition);
        }


        /// <summary>
        /// get bond issue amount statistical data
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<BondIssueAmount> GetBondIssueAmountNew(BondIssueAmountParams param)
        {
            var table = GetBondIssueAmountDtNew(param.Type, param.StartDate, param.EndDate, param.Unit, string.Join(",", param.TypeList.ToArray()), param.UseSubType ? 1 : 0, param.SubType);
            var pResult = DataTableSerializer.ToList<BondIssueAmount>(table);

            return pResult;
        }

        public DataTable GetBondIssueAmountDtNew(string category, DateTime startDate, DateTime endDate, string unit, string itemList, int isUseSubCategory, string subCategory)
        {
            var paramArray = new[]
            {
                new OracleParameter("CategoryVal", OracleDbType.Varchar2) {Value = category},
                new OracleParameter("StartDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)startDate},
                new OracleParameter("EndDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)endDate},
                new OracleParameter("Unit", OracleDbType.Varchar2) {Value = unit},
                new OracleParameter("ItemList", OracleDbType.Varchar2) {Value = itemList},
                new OracleParameter("IsUseSubCategory", OracleDbType.Int32) {Value = isUseSubCategory},
                new OracleParameter("SubCategory", OracleDbType.Varchar2) {Value = subCategory},
                new OracleParameter("Cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };

            var table = GetDataSetBySpFromBondDB("GetBondIssueAmount", paramArray).Tables[0];
            return table;
        }

        

        public DataTable GetBondDepositoryBalanceNew(string category, DateTime startDate, DateTime endDate, string unit, string itemList, int isUseSubCategory, string subCategory)
        {
            var paramArray = new[]
            {
                new OracleParameter("CategoryVal", OracleDbType.Varchar2) {Value = category},
                new OracleParameter("StartDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)startDate},
                new OracleParameter("EndDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)endDate},
                new OracleParameter("Unit", OracleDbType.Varchar2) {Value = unit},
                new OracleParameter("BondClass", OracleDbType.Varchar2) {Value = itemList},
                new OracleParameter("IsUseSubCategory", OracleDbType.Int32) {Value = isUseSubCategory},
                new OracleParameter("SubCategory", OracleDbType.Varchar2) {Value = subCategory},
                new OracleParameter("Cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };

            var table = GetDataSetBySpFromBondDB("GetBondDepositoryBalance", paramArray).Tables[0];
            return table;
        }

        public DataTable GetBondDepositoryBalanceChart(string columnType, DateTime start, DateTime end, string category, string itemList, string unit, int isUseSubCategory = 0, string subCategory = "Bond_Class", string subCategoryValue = "")
        {
            var paramArray = new[]
            {
                new OracleParameter("CategoryVal", OracleDbType.Varchar2) {Value = category},
                new OracleParameter("StartDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)start},
                new OracleParameter("EndDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp)end},
                new OracleParameter("Unit", OracleDbType.Varchar2) {Value = unit},
                new OracleParameter("BondClass", OracleDbType.Varchar2) {Value = itemList},
                new OracleParameter("ChartColumn", OracleDbType.Varchar2) {Value = columnType},
                new OracleParameter("IsUseSubCategory", OracleDbType.Int32) {Value = isUseSubCategory},
                new OracleParameter("SubCategory", OracleDbType.Varchar2) {Value = subCategory},
                new OracleParameter("SubCategoryValue", OracleDbType.Varchar2) {Value = subCategoryValue},
                new OracleParameter("Cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };

            var table = GetDataSetBySpFromBondDB("GetBondDepositoryBalanceChart", paramArray).Tables[0];
            return table;
        }
        public IEnumerable<BondDetail> GetBondDetailByType(BondDetailParams param)
        {
            Func<BOND, bool> whereFunc = GetFunc(param.Type, param.TypeValue);
            Func<BOND, bool> dateFilter = b => (b.orig_issue_dt >= param.StartDate && b.orig_issue_dt <= param.EndDate) || (b.maturity_dt >= param.StartDate && b.maturity_dt <= param.EndDate);

            using (var bonddb = new BondDBEntities())
            {
                return bonddb.BOND
                    .Where(whereFunc)
                    .Where(dateFilter)
                    .Select(b => new BondDetail
                    {
                        AssetId = b.assetId,
                        Code = b.code,
                        BondNameCn = b.bond_name_cn,
                        BondNameEn = b.bond_name_en,
                        BondTermEn = b.BondTerm_en,
                        BondTermCn = b.BondTerm_cn,
                        BondRating = b.latest_rating_cd,
                        BondRatingAgencyEn = b.rating_src_en,
                        BondRatingAgencyCn = b.rating_src_cn,
                        PartyRating = b.party_rating_cd,
                        PartyRatingAgencyCn = b.party_rating_src_cn,
                        PartyRatingAgencyEn = b.party_rating_src_en,
                        IssueDate = b.orig_issue_dt,
                        ValueDate = b.orig_dated_dt,
                        MaturityDate = b.maturity_dt,
                        ListingDate = b.listing_dt,
                        IssueAmount = b.orig_iss_amt,
                        IssuePrice = b.orig_iss_px,
                        RefYield = b.yield,
                        CouponClassCn = b.coupon_class_cn,
                        CouponClassEn = b.coupon_class_en,
                        CouponFreqEn = b.freq_en,
                        CouponFreqCn = b.freq_cn,
                        CouponRate = b.orig_iss_cpn,
                        CDCType = b.cdc_asset_class_cd,
                        CDCTypeCn = b.cdc_asset_class_cn,
                        CDCTypeEn = b.cdc_asset_class_en,
                        Currency = b.orig_iss_curr_cd,
                        FloatIndex = b.float_index,
                        Spread = b.float_offset,
                        DayCountEn = b.day_count_en,
                        DayCountCn = b.day_count_cn,
                        OptionEn = b.callorput_en,
                        OptionCn = b.callorput_cn,
                        Issuer = b.offer_registrant_name,
                        ISBN = b.isin_nm,
                        Seniority = b.seniority,
                        IsIssued = b.orig_issue_dt >= param.StartDate && b.orig_issue_dt <= param.EndDate,
                        IsMatured = b.maturity_dt >= param.StartDate && b.maturity_dt <= param.EndDate,
                        OrigAvgLife = b.orig_avg_life,
                        Term = Convert.ToString(b.term),
                        re_issue = b.re_issue,
                        ExchangeNameEn = b.exchange_name_en,
                        ExchangeNameCn = b.exchange_name_cn,
                        TrusteeNameCn = b.trustee_name_cn,
                        TrusteeNameEn = b.trustee_name_en
                    }).ToList().OrderByDescending(b => b.ValueDate);
            }
        }

        public DataTable GetBondDetailByTypeNew(BondDetailParams param, out int total)
        {
            if (param.StartDate != null)
            {
                if (param.EndDate != null)
                {
                    var paramArray = new[]
                    {
                        new OracleParameter("AssetType", OracleDbType.Varchar2) {Value = param.Type},
                        new OracleParameter("AssetTypeValue", OracleDbType.Varchar2) {Value = param.TypeValue},
                        new OracleParameter("IsUseSubAssetType", OracleDbType.Int32) {Value = param.UseSubType ? 1 : 0},
                        new OracleParameter("SubAssetType", OracleDbType.Varchar2) {Value = param.SubType},
                        new OracleParameter("SubAssetTypeValue", OracleDbType.Varchar2) {Value = param.SubTypeValue},
                        new OracleParameter("StartDate", OracleDbType.TimeStamp)
                        {
                            Value =(OracleTimeStamp) param.StartDate.Value
                        },
                        new OracleParameter("EndDate", OracleDbType.TimeStamp)
                        {
                            Value = (OracleTimeStamp)param.EndDate.Value
                        },
                        new OracleParameter("Culture", OracleDbType.Varchar2)
                        {
                            Value = Thread.CurrentThread.CurrentUICulture.Name
                        },
                        new OracleParameter("BondTermType", OracleDbType.Varchar2) { Value = param.Term },
                        new OracleParameter("OrderBy", OracleDbType.Varchar2,50) { Value = param.OrderBy,Direction = ParameterDirection.InputOutput}, 
                        new OracleParameter("ItemList", OracleDbType.Varchar2) {Value = param.ItemList},
                        new OracleParameter("StartPage", OracleDbType.Int32) {Value = param.StartPage},
                        new OracleParameter("PageSize", OracleDbType.Int32) {Value = param.PageSize},
                        new OracleParameter("Total", OracleDbType.Int32, ParameterDirection.Output),
                        new OracleParameter("Cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
                    };
                    object value;
                    var table = GetDataSetBySpFromBondDB("GetIssuanceMaturesBondDetail", paramArray, "Total", out value).Tables[0];
                    total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;
                    return table;
                }
            }
            total = 0;
            return new DataTable();
        }

        /// <summary>
        /// Get type order dic from db
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetTypeOrder()
        {
            Dictionary<string, int> typeOrderDic = new Dictionary<string, int>();

            using (var cmadb = new BondDBEntities())
            {
                var typeOrder = from t in cmadb.TYPELIST select new { key = t.ENGLISH_NAME, value = t.TABLE_CD };

                foreach (var i in typeOrder)
                    typeOrderDic.Add(i.key, (int)i.value);
            }

            return typeOrderDic;
        }

        /// <summary>
        /// Get bond detail data
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<BondDetail> GetBondDetailByTypeAndSubType(BondDetailParams param)
        {
            var table = GetIssueAmountBondDetailByTypeAndSubType(param);
            var pResult = DataTableSerializer.ToList<BondDetail>(table);
            return pResult;
        }

        public DataTable GetIssueAmountBondDetailByTypeAndSubType(BondDetailParams param)
        {
            if (param.StartDate != null)
            {
                if (param.EndDate != null)
                {
                    var paramArray = new[]
                    {
                        new OracleParameter("AssetType", OracleDbType.Varchar2) {Value = param.Type},
                        new OracleParameter("AssetTypeValue", OracleDbType.Varchar2) {Value = param.TypeValue},
                        new OracleParameter("IsUseSubAssetType", OracleDbType.Int32) {Value = param.UseSubType ? 1 : 0},
                        new OracleParameter("SubAssetType", OracleDbType.Varchar2) {Value = param.SubType},
                        new OracleParameter("SubAssetTypeValue", OracleDbType.Varchar2) {Value = param.SubTypeValue},
                        new OracleParameter("StartDate", OracleDbType.TimeStamp)
                        {
                            Value = (OracleTimeStamp)param.StartDate.Value
                        },
                        new OracleParameter("EndDate", OracleDbType.TimeStamp)
                        {
                            Value = (OracleTimeStamp)param.EndDate.Value
                        },
                        new OracleParameter("Culture", OracleDbType.Varchar2)
                        {
                            Value = Thread.CurrentThread.CurrentUICulture.Name
                        },
                        new OracleParameter("ItemList", OracleDbType.Varchar2) {Value = param.ItemList},
                        new OracleParameter("StartPage", OracleDbType.Int32) {Value = param.StartPage},
                        new OracleParameter("PageSize", OracleDbType.Int32) {Value = param.PageSize},
                        new OracleParameter("Total", OracleDbType.Int32, ParameterDirection.Output),
                        new OracleParameter("Cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
                    };
                    object value;
                    var table = GetDataSetBySpFromBondDB("GetBondIssueAmountDetail", paramArray, "Total", out value).Tables[0];
                    //var total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;
                    return table;
                }
            }
            return new DataTable();
        }

        public List<BondRatingHist> GetBondRatingByCode(string bondCode)
        {

            OracleParameter[] paramArray =
            {
                new OracleParameter("P_BondCode", bondCode),
                new OracleParameter("P_CUR", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };
            var table = GetZcxDataSetBySp("GetBondRatingHist", paramArray).Tables[0];
            return DataTableSerializer.ToList<BondRatingHist>(table);
        }
        protected DataSet GetZcxDataSetBySp(string inName, OracleParameter[] inParms)
        {
            using (var cnEdb = new ZCXEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    cnEdb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cnEdb.Database.Connection.ConnectionString);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    if (inParms != null)
                        spCmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }

        private Func<BOND, bool> GetFunc(string type, string typeValue)
        {
            Func<BOND, bool> func = t => t.cdc_asset_class_cd == typeValue;
            switch (type)
            {
                case ConstValues.Type_Bond_Rating:
                    if (string.IsNullOrEmpty(typeValue))
                        func = t => string.IsNullOrEmpty(t.latest_rating_cd);
                    else
                        func = t => t.latest_rating_cd == typeValue;
                    break;
                case ConstValues.Type_Issuer_Rating:
                    if (string.IsNullOrEmpty(typeValue))
                        func = t => string.IsNullOrEmpty(t.party_rating_cd);
                    else
                        func = t => t.party_rating_cd == typeValue;
                    break;
                case ConstValues.Type_Maturity_Term:
                    if (string.IsNullOrEmpty(typeValue))
                        func = t => string.IsNullOrEmpty(t.BondTerm_cn) || string.IsNullOrEmpty(t.BondTerm_en);
                    else
                        func = t => t.BondTerm_en == typeValue || t.BondTerm_cn == typeValue; // for ch & en
                    break;
                case ConstValues.Type_Coupon_Type:
                    if (string.IsNullOrEmpty(typeValue))
                        func = t => string.IsNullOrEmpty(t.coupon_class_en) || string.IsNullOrEmpty(t.coupon_class_cn);
                    else
                        func = t => t.coupon_class_en == typeValue || t.coupon_class_cn == typeValue;
                    break;
                case ConstValues.Type_Option:
                    if (string.IsNullOrEmpty(typeValue))
                        func = t => string.IsNullOrEmpty(t.callorput_cn) || string.IsNullOrEmpty(t.callorput_en);
                    else
                        func = t => t.callorput_en == typeValue || t.callorput_cn == typeValue;
                    break;
            }
            return func;
        }
    }
}
