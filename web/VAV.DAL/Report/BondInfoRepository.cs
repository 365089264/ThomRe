using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using VAV.DAL.Common;
using VAV.DAL.IPP;
using VAV.Entities;
using VAV.Model.Data.Bond;
using VAV.Model.Data;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Model.Data.ZCX;
using log4net;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;


namespace VAV.DAL.Report
{
    
    public class BondInfoRepository : BaseReportRepository
    {

        ILog _log = LogManager.GetLogger(typeof(BondInfoRepository));

        public IEnumerable<GCODES_COUPON_CLASS_CDS> GetCouponItems()
        {
            using (var bonddb = new BondDBEntities())
            {

                return bonddb.GCODES_COUPON_CLASS_CDS.ToList();
            }
        }

        public IEnumerable<LOCALIZATION> GetAbsBondClass()
        {
            return GetLocalization().Where(x => x.TABLE_NAME == "AbsBondClass").OrderBy(re=>re.MEMO).ToList();
        }

        public IEnumerable<LOCALIZATION> GetAbsRateType()
        {
            return GetLocalization().Where(x => x.TABLE_NAME == "AbsCoupon").ToList();
        }



        public List<v_bondClass> GetAssetClass()
        {
            var paramArray = new[]
                            {
                                new OracleParameter("cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            var table = GetDataSetBySpFromBondDB("GetBondClass", paramArray).Tables[0];
            return DataTableSerializer.ToList<v_bondClass>(table);
        }

        public IEnumerable<LOCALIZATION> GetOptionItems()
        {
            return GetLocalization().Where(x => x.TABLE_NAME == "option").ToList();
        }

        public IEnumerable<BondInfoOption> GetPartyCntryIncorpCn()
        {
            using (var bonddb = new BondDBEntities())
            {
                return (from area in bonddb.BOND
                        where area.PartyCntryIncorpCd!=null
                        orderby area.PartyCntryIncorpCd
                        select new BondInfoOption { Type = area.PartyCntryIncorpDescr_en, Name = area.PartyCntryIncorpDescr_cn.Trim() }).Distinct().ToList();
            }
        }
        public IEnumerable<BondInfoOption> GetPartyCntryIncorpEn()
        {
            using (var bonddb = new BondDBEntities())
            {
                return (from area in bonddb.BOND
                        where area.PartyCntryIncorpCd != null
                        orderby area.PartyCntryIncorpCd
                        select new BondInfoOption { Type = area.PartyCntryIncorpDescr_en, Name = area.PartyCntryIncorpDescr_en.Trim() }).Distinct().ToList();
            }
        }

        public IEnumerable<LOCALIZATION> GetIssuerInduSector()
        {
            return GetLocalization().Where(x => x.TABLE_NAME == "gcodes.indu_sector_cds" && x.CHINESE_NAME != null && x.ENGLISH_NAME != null).OrderBy(re => re.MEMO).ToList();
        }

        public DataTable GetScheduledIssueBondInfo(string bondClass, string couponClass, string option, string columnList, int startPage, int pageSize, out int total)
        {
            bondClass = bondClass.Replace("(", "").Replace(")", "").Replace("'", "");
            var paramArray = new[]
                            {
                                new OracleParameter("BondClass", OracleDbType.Varchar2) { Value = bondClass }, 
                                new OracleParameter("CouponClass", OracleDbType.Varchar2) { Value = couponClass} , 
                                new OracleParameter("OptionC", OracleDbType.Varchar2) { Value = option },
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("ColumnList", OracleDbType.Varchar2,1000) { Value = columnList,Direction = ParameterDirection.InputOutput },
                                new OracleParameter("StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySpFromBondDB("GetScheduledIssueBondInfo", paramArray, "Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public DataTable GetNewIssueDetails(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string option, string bondRating, string bondCode, string columnList, string isMD, string othBondClass, string bondMarket, string bondTrustee, int startPage, int pageSize, out int total)
        {
            bondClass = bondClass.Replace("(", "").Replace(")", "").Replace("'", "");
            var paramArray = new[]
                            {
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)startDate }, 
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)endDate }, 
                                new OracleParameter("BondClass", OracleDbType.Varchar2) { Value = bondClass }, 
                                new OracleParameter("CouponClass", OracleDbType.Varchar2) { Value = couponClass} , 
                                new OracleParameter("OptionC", OracleDbType.Varchar2) { Value = option },
                                new OracleParameter("BondRating", OracleDbType.Varchar2) { Value = bondRating },
                                new OracleParameter("BondeCode", OracleDbType.Varchar2) { Value = bondCode },
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("ColumnList", OracleDbType.Varchar2,1000) { Value = columnList,Direction = ParameterDirection.InputOutput },
                                new OracleParameter("IsMD", OracleDbType.Varchar2) { Value = isMD },
                                new OracleParameter("OthClass", OracleDbType.Varchar2) { Value = othBondClass },
                                new OracleParameter("BondMarket", OracleDbType.Varchar2) { Value = bondMarket },
                                new OracleParameter("BondTrustee", OracleDbType.Varchar2) { Value = bondTrustee },
                                new OracleParameter("StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySpFromBondDB("GetNewIssuedBondInfo", paramArray, "Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public DataTable GetNewListDetails(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string option, string bondCode, string columnList, string isMD, string othBondClass, int startPage, int pageSize, out int total)
        {
            bondClass = bondClass.Replace("(", "").Replace(")", "").Replace("'", "");
            var paramArray = new[]
                            {
                                new OracleParameter("@StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)startDate }, 
                                new OracleParameter("@EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)endDate}, 
                                new OracleParameter("@BondClass", OracleDbType.Varchar2) { Value = bondClass }, 
                                new OracleParameter("@CouponClass", OracleDbType.Varchar2) { Value = couponClass} , 
                                new OracleParameter("@OptionC", OracleDbType.Varchar2) { Value = option },
                                new OracleParameter("@BondeCode", OracleDbType.Varchar2) { Value = bondCode },
                                new OracleParameter("@Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("@ColumnList", OracleDbType.Varchar2,1000) { Value = columnList,Direction = ParameterDirection.InputOutput },
                                new OracleParameter("@IsMD", OracleDbType.Varchar2) { Value = isMD },
                                new OracleParameter("@OthClass", OracleDbType.Varchar2) { Value = othBondClass },
                                new OracleParameter("@StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySpFromBondDB("GetNewListedBondInfo", paramArray, "@Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public DataTable GetQToListBondInfo(string bondClass, string couponClass, string option, string columnList, int startPage, int pageSize, out int total)
        {
            bondClass = bondClass.Replace("(", "").Replace(")", "").Replace("'", "");
            var paramArray = new[]
                            {
                                new OracleParameter("@BondClass", OracleDbType.Varchar2) { Value = bondClass }, 
                                new OracleParameter("@CouponClass", OracleDbType.Varchar2) { Value = couponClass} , 
                                new OracleParameter("@OptionC", OracleDbType.Varchar2) { Value = option },
                                new OracleParameter("@Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("@ColumnList", OracleDbType.Varchar2,1000) { Value = columnList,Direction = ParameterDirection.InputOutput },
                                new OracleParameter("@StartPage", OracleDbType.Int32) { Value = startPage},
                                new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySpFromBondDB("GetQToListBondInfo", paramArray, "@Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public List<DimSumBondSummary> GetDimSumBondSummary(DateTime start, DateTime end, string category, string unit, string columnType,string TypeValue)
        {
            var table = GetDimSumBondSummaryTable(start, end, category, unit, columnType, TypeValue);
            var pResult = DataTableSerializer.ToList<DimSumBondSummary>(table);

            //For sovr item
            if (category == ConstValues.Option_Asset_Type && (TypeValue == "all" || TypeValue == "Sovr"))
            {
                var govItem = pResult.Where(b => b.Type == "Gov").Select(b => b).FirstOrDefault();
                var agenItem = pResult.Where(b => b.Type == "Agen").Select(b => b).FirstOrDefault();
                var sovrItem = AddTwoDimSumBondSummary(govItem, agenItem);

                var re = pResult.ToList();
                re.Add(sovrItem);

                return re.OrderBy(b => b.Order).ToList();
            }

            if (category == ConstValues.Option_Indu)
                return pResult.Where(b => b.Issues != 0 || b.MaturityBonds != 0).Select(b => b).OrderBy(b => b.Order).ToList();

            return pResult.OrderBy(b => b.Order).ToList();
        }

        public IEnumerable<DimBondInfo> GetDimSumBondList(DateTime start, DateTime end, string category, bool isEnglishCulture)
        {
            _log.Error("GetDimSumBondList Start: " +DateTime.Now);
            var table= GetDimSumBond(start, end, category);
            var bondList = DataTableSerializer.ToList<DimBondInfo>(table);
            _log.Error("GetDimSumBondList finish: " + DateTime.Now);
            return bondList ?? new List<DimBondInfo>();
        }
        
        public DataTable GetDimSumBondDetail(DateTime start, DateTime end, string category, string typeValue, string columnList, string unit, string term, string order, int startPage, int pageSize,out int total)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("I_Category", OracleDbType.Varchar2) { Value = category }, 
                                new OracleParameter("I_TypeValue", OracleDbType.Varchar2) { Value = typeValue} , 
                                new OracleParameter("I_StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)start },
                                new OracleParameter("I_EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)end },
                                new OracleParameter("I_Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("I_ColumnList", OracleDbType.Varchar2,500) { Value = columnList },
                                new OracleParameter("I_Unit", OracleDbType.Varchar2) { Value = unit },
                                new OracleParameter("I_BondTermType", OracleDbType.NVarchar2) { Value = term },
                                new OracleParameter("I_OrderBy", OracleDbType.Varchar2,50) { Value = order,Direction = ParameterDirection.Output}, 
                                new OracleParameter("I_StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("I_PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("O_Total", OracleDbType.Int32,ParameterDirection.Output) { Value = 0 },
                                new OracleParameter("O_Cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySpFromBondDB("GetDimSumBondDetail", paramArray, "O_Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;
            return table;
        }

        public DataTable GetDimSumBond(DateTime start, DateTime end, string category)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("BondCategory", OracleDbType.Varchar2) { Value = category }, 
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)start },
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value =(OracleTimeStamp) end },
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetDataSetBySpFromBondDB("GetDimSumBond", paramArray).Tables[0];
        }

        public DataTable GetDimSumBondSummaryTable(DateTime start, DateTime end, string category, string unit, string columnType, string typeValue)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("BondCategory", OracleDbType.Varchar2) { Value = category }, 
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)start },
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)end },
                                new OracleParameter("Unit", OracleDbType.Varchar2) { Value = unit },
                                new OracleParameter("ColumnType", OracleDbType.Varchar2) { Value = columnType }, 
                                new OracleParameter("TypeValue", OracleDbType.Varchar2) { Value = typeValue }, 
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetDataSetBySpFromBondDB("GetDimSumBondSummary", paramArray).Tables[0];
        }

        public BondBasicInfo GetBondBasicInfoById(string id, string culture)
        {
            using (var VAVDB = new BondDBEntities())
            {
                if (culture == "zh-CN")
                    return VAVDB.V_BOND_CN
                        .Where(b => b.ASSETID == id)
                        .Select(b => new BondBasicInfo
                        {
                            AssetId = id,
                            Issuer = b.ISSUER,
                            BondClassDescr = b.BONDCLASSDESCR,
                            CouponClassDescr = b.COUPONCLASSDESCR,
                            IssuerInduSectorDescr =b.ISSUERINDUSECTORDESCR,
                            PayFrequency = b.PAYFREQUENCY
                        }).FirstOrDefault();
                else
                    return VAVDB.V_BOND_EN
                         .Where(b => b.ASSETID == id)
                         .Select(b => new BondBasicInfo
                         {
                             AssetId = id,
                             Issuer = b.ISSUER,
                             BondClassDescr = b.BONDCLASSDESCR,
                             CouponClassDescr = b.COUPONCLASSDESCR,
                             IssuerInduSectorDescr = b.ISSUERINDUSECTORDESCR,
                             PayFrequency = b.PAYFREQUENCY
                         }).FirstOrDefault();
            }
        }

        public string GetBondCodeById(string id)
        {
            using (var VAVDB = new BondDBEntities())
            {
                return VAVDB.V_BOND_CN.Where(b => b.ASSETID == id).Select(b => b.CODE).FirstOrDefault();
            }
        }

        public BondExchangeCode GetBondExchangeCodeById(string id)
        {
            using (var bonddb = new BondDBEntities())
            {
                var exchangeCodes = bonddb.GOVCORP_ASSET_IDENT.Where(re => re.ASSETID == id).ToList();
                var bondInfo = bonddb.V_BOND_CN.Where(b => b.ASSETID == id).FirstOrDefault();
                if (bondInfo == null) bondInfo = new V_BOND_CN();
                return new BondExchangeCode
                         {
                             AssetId = id,
                             Ric = bondInfo.RIC,
                             Isin = bondInfo.ISINNUM,
                             ExchaneCodeShg = exchangeCodes.FirstOrDefault(re => re.ID_CD == "SHG") != null ? exchangeCodes.FirstOrDefault(re => re.ID_CD == "SHG").ID_NUMBER : "",
                             ExchaneCodeShz = exchangeCodes.FirstOrDefault(re => re.ID_CD == "SHZ") != null ? exchangeCodes.FirstOrDefault(re => re.ID_CD == "SHZ").ID_NUMBER : "",
                             ExchaneCodeShc = exchangeCodes.FirstOrDefault(re => re.ID_CD == "SHC") != null ? exchangeCodes.FirstOrDefault(re => re.ID_CD == "SHC").ID_NUMBER : "",
                             ExchaneCodeSed = exchangeCodes.FirstOrDefault(re => re.ID_CD == "SED") != null ? exchangeCodes.FirstOrDefault(re => re.ID_CD == "SED").ID_NUMBER : ""
                         };
            }
        }
        private DimSumBondSummary AddTwoDimSumBondSummary(DimSumBondSummary b1, DimSumBondSummary b2)
        {
            return new DimSumBondSummary
            {
                Type = "Sovr",
                TypeName = Thread.CurrentThread.CurrentUICulture.Name != "zh-CN" ? "Sovereign Bonds" : "主权债",

                EndBalance = b1.EndBalance + b2.EndBalance,
                InitialBalance = b1.InitialBalance + b2.InitialBalance,

                Issues = b1.Issues + b2.Issues,
                IssuesPercent = b1.IssuesPercent + b2.IssuesPercent,

                IssuesAmount = b1.IssuesAmount + b2.IssuesAmount,
                IssuesAmountPercent = b1.IssuesAmountPercent + b2.IssuesAmountPercent,

                MaturityBonds = b1.MaturityBonds + b2.MaturityBonds,
                MaturityBondsPercent = b1.MaturityBondsPercent + b2.MaturityBondsPercent,

                MaturityAmount = b1.MaturityAmount + b2.MaturityAmount,
                MaturityAmountPercent = b1.MaturityAmountPercent + b2.MaturityAmountPercent,

                IsParent = true,
                Order = GetOrderByKey("Sovr"),
                CurrentDate = b1.CurrentDate
            };
        }

        private void GetAllFunc(string category, out Func<DimBondInfo, string> groupFunc, out Func<DimBondInfo, string> selectFunc)
        {
            groupFunc = t => t.DebtTypeCd;
            selectFunc = t => t.DebtTypeDescr;

            if (!string.IsNullOrEmpty(category))
            {
                switch (category)
                {
                    case ConstValues.Option_Debt_Type:
                        groupFunc = t => t.DebtTypeCd;
                        selectFunc = t => t.DebtTypeDescr;
                        break;
                    case ConstValues.Option_Asset_Type:
                        groupFunc = t => t.AssetTypeCd;
                        selectFunc = t => t.AssetTypeDescr;
                        break;
                    case ConstValues.Option_DomicileOfIssuer:
                        groupFunc = t => t.PartyCntryIncorpCd;
                        selectFunc = t => t.PartyCntryIncorpDescr;
                        break;
                    case ConstValues.Option_RatingInfo:
                        groupFunc = t => t.RatingInfoCd;
                        selectFunc = t => t.RatingInfo;
                        break;
                    case ConstValues.Option_Term:
                        groupFunc = t => t.BondTermCd.ToString();
                        selectFunc = t => t.BondTerm;
                        break;
                    case ConstValues.Option_Indu:
                        groupFunc = t => t.IssuerInduSectorCd;
                        selectFunc = t => t.IssuerInduSectorDescr;
                        break;
                    case ConstValues.Option_Issue_Country:
                        groupFunc = t => t.IssueCountryCd;
                        selectFunc = t => t.IssueCountry;
                        break;
                    default:
                        break;
                }
            }
        }

        private int GetMultiPlier(string unit)
        {
            int multiplier = 1;
            switch (unit)
            {
                case ConstValues.Unit_100M:
                    multiplier = 1;
                    break;
                case ConstValues.Unit_M:
                    multiplier = 100;
                    break;
                case ConstValues.Unit_10K:
                    multiplier = 10000;
                    break;
                case ConstValues.Unit_K:
                    multiplier = 100000;
                    break;
                default:
                    break;
            }

            return multiplier;
        }

        #region CityInvestment


        #endregion


        #region CityBond

        public DataTable GetCityBondTopGrid(DateTime start, DateTime end, string unit, string ciBondFlag, string provOrIssValue)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("CiBondFlag", OracleDbType.Varchar2) { Value = ciBondFlag },
                                new OracleParameter("ProvOrIss", OracleDbType.Varchar2) { Value = provOrIssValue },
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)start }, 
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)end} , 
                                new OracleParameter("Unit", OracleDbType.Varchar2) { Value = unit },
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("Cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetDataSetBySpFromBondDB("GetBondCityInvestment", paramArray).Tables[0];
        }

        public DataTable GetCityBondBottomGrid(string provinceValue, DateTime start, DateTime end, string columnList, string ciBondFlag, string issOrMatFlag, string provOrIssValue)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("ProvOrIssValue", OracleDbType.Varchar2,100) { Value = provinceValue ,Direction = ParameterDirection.InputOutput},
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)start }, 
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)end} , 
                                new OracleParameter("ColumnList", OracleDbType.Varchar2,500) { Value = columnList },
                                new OracleParameter("CiBondFlag", OracleDbType.Varchar2) { Value = ciBondFlag },
                                new OracleParameter("ProvOrIss", OracleDbType.Varchar2) { Value = provOrIssValue },
                                new OracleParameter("IssOrMatFlag", OracleDbType.Varchar2) { Value = issOrMatFlag },
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("Cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            return GetDataSetBySpFromBondDB("GetBondCityIvestmentDetail", paramArray).Tables[0];
        }
        #endregion

        #region UnderWritersRanking
        public DataTable GetUnderWriterAnalysis(string bondClass, DateTime start, DateTime end, string unit, string order, out int outPara)
        {
            //start.ToString("yyyy-MM-dd hh:mm:ss tt")
            var paramArray = new[]
            {
                new OracleParameter("BondClass", OracleDbType.Varchar2) {Value = bondClass},
                new OracleParameter("StartDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp) start},
                new OracleParameter("EndDate", OracleDbType.TimeStamp) {Value = (OracleTimeStamp) end},
                new OracleParameter("Culture", OracleDbType.Varchar2)
                {
                    Value = Thread.CurrentThread.CurrentUICulture.Name
                },
                new OracleParameter("Unit", OracleDbType.Varchar2) {Value = unit},
                new OracleParameter("Order", OracleDbType.Varchar2) {Value = order},
                new OracleParameter("OutPara", OracleDbType.Int32,ParameterDirection.Output) {Value = null},
                new OracleParameter("cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
            };
            object o;
            var table= GetDataSetBySpWithOutParaFromBondDb("GetUnderWriterAnalysis", paramArray, out o).Tables[0];
            if (o == DBNull.Value)
            {
                outPara = 0;
            }
            else
            {
                outPara = Convert.ToInt32(o.ToString());  
            }
            return table;
        }

        public DataTable GetUnderWriterBond(string bondClass, DateTime start, DateTime end, string underWriterId)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("BondClass", OracleDbType.Varchar2) { Value = bondClass },
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)start }, 
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)end} , 
                                new OracleParameter("UnderWriterId", OracleDbType.Varchar2) { Value = underWriterId },
                                new OracleParameter("Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
                            };
            return GetDataSetBySpFromBondDB("GetUnderWriterBond", paramArray).Tables[0];
        }

        public DataTable GetUnderWriterDetail(string bondClass, DateTime start, DateTime end, string unit, string underWriterId)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("BondClass", OracleDbType.Varchar2) { Value = bondClass },
                                new OracleParameter("StartDate", OracleDbType.TimeStamp) { Value =(OracleTimeStamp) start }, 
                                new OracleParameter("EndDate", OracleDbType.TimeStamp) { Value =(OracleTimeStamp) end} , 
                                new OracleParameter("UnderWriterId", OracleDbType.Varchar2) { Value = underWriterId },
                                new OracleParameter("Unit", OracleDbType.Varchar2) { Value = unit },
                                new OracleParameter("cur", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
                            };
            return GetDataSetBySpFromBondDB("GetUnderWriterDetail", paramArray).Tables[0];
        }
        #endregion

        #region ABS Detail

        public DataTable GetAbsBondList(DateTime startDate, DateTime endDate, string bondClass, string couponClass, string option, string bondRating, string isBondCode, string bondCodeOrIss, string columnList, int startPage, int pageSize, out int total)
        {
            var paramArray = new[]
                            {
                                new OracleParameter("P_StartDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)startDate }, 
                                new OracleParameter("P_EndDate", OracleDbType.TimeStamp) { Value = (OracleTimeStamp)endDate }, 
                                new OracleParameter("P_BondClass", OracleDbType.Varchar2) { Value = bondClass }, 
                                new OracleParameter("P_CouponClass", OracleDbType.Varchar2) { Value = couponClass} , 
                                new OracleParameter("P_Option", OracleDbType.Varchar2) { Value = option },
                                new OracleParameter("P_BondRating", OracleDbType.Varchar2) { Value = bondRating },
                                new OracleParameter("P_IsBondeCode", OracleDbType.Varchar2) { Value = isBondCode },
                                new OracleParameter("P_BondCodeOrIss", OracleDbType.NVarchar2) { Value = bondCodeOrIss },
                                new OracleParameter("P_Culture", OracleDbType.Varchar2) { Value = Thread.CurrentThread.CurrentUICulture.Name },
                                new OracleParameter("P_ColumnList", OracleDbType.Varchar2) { Value = columnList },
                                new OracleParameter("P_StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("P_PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("P_TOTAL", OracleDbType.Int32,ParameterDirection.Output) { Value = 0 },
                                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            using (var cmaDb = new ZCXEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    cmaDb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmaDb.Database.Connection.ConnectionString);
                    spCmd.CommandText = "GetAbsListBondInfo";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    total = Convert.ToInt32(spCmd.Parameters["P_TOTAL"].Value.ToString());
                    return ds.Tables[0];
                }
            }
        }


        public V_BOND_ABS GetBondInfoByUnicode(long bond_Uni_Code)
        {
            using (var zcxDb = new ZCXEntities())
            {
                return zcxDb.V_BOND_ABS.FirstOrDefault(b => b.BOND_UNI_CODE == bond_Uni_Code);
            }
        }

        public List<BOND_SIZE_CHAN> GetBondSizeChans(long bond_Uni_Code)
        {
            OracleParameter[] paramArray =
            {
                new OracleParameter("P_Bond_Uni_Code", bond_Uni_Code),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return DataTableSerializer.ToList<BOND_SIZE_CHAN>(GetZCXDataSetBySp("GetBondSizeChangeByUniCode", paramArray).Tables[0]);
        }

        public List<RATE_ORG_CRED_HIS> GetIssuerRatingHisByUniCode(long bond_Uni_Code)
        {
            OracleParameter[] paramArray =
            {
                new OracleParameter("P_Bond_Uni_Code", bond_Uni_Code),
                new OracleParameter("P_Culture", Thread.CurrentThread.CurrentUICulture.Name),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return DataTableSerializer.ToList<RATE_ORG_CRED_HIS>(GetZCXDataSetBySp("GetIssuerRatingByBondUniCode", paramArray).Tables[0]);
        }

        public List<BondRatingHist> GetBondrRatingHisByUniCode(long bond_Uni_Code)
        {
            OracleParameter[] paramArray =
            {
                new OracleParameter("P_Bond_Uni_Code", bond_Uni_Code),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return DataTableSerializer.ToList<BondRatingHist>(GetZCXDataSetBySp("GetBondRatingByBondUniCode", paramArray).Tables[0]);
        }
        public List<string> GetAbsRateHis()
        {
            OracleParameter[] paramArray =
            {
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            var dt = GetZCXDataSetBySp("GetAbsRateHis", paramArray).Tables[0];
            var list=new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                list.Add(dt.Rows[i][0].ToString());
            }
            return list;
        }

        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetZCXDataSetBySp(string inName, OracleParameter[] inParms)
        {
            using (var cnEDB = new ZCXEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    DataSet ds = null;

                    cnEDB.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cnEDB.Database.Connection.ConnectionString);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    if (inParms != null)
                        spCmd.Parameters.AddRange(inParms);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    ds = new DataSet();
                    da.Fill(ds);

                    return ds;
                }
            }
        }
        #endregion
    }
}
