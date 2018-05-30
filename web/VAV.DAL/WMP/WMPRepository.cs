using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using VAV.Model.Data.WMP;
using VAV.Entities;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Model.Data;


namespace VAV.DAL.WMP
{
    public class WMPRepository : WMPBaseRepository
    {
        private static IEnumerable<WMPBankOption> _bankTypeOptions;
        private static IEnumerable<WMPAreaOption> _areaOptions;

        [Dependency]
        public IFileService FileService { get; set; }

        public WMPRepository()
        {
            
            if (_bankTypeOptions == null)
                _bankTypeOptions = GetWmpBankOption();

            if (_areaOptions == null)
                _areaOptions = GetWmpAreaOption();
        }

        public CmaFile GetBankFileData(int id)
        {
            return FileService.GetFileById(id, "WMP_PROSP");
        }

        public CmaFile GetReportData(int id)
        {
            return FileService.GetFileById(id, "WMP_REP");
        }

        public IEnumerable<WMPBankTypeOption> GetWmpBankTypeOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from prod in wmpdb.BANK_FIN_PRD
                        join org in wmpdb.ORG_PROFILE on prod.BANK_ID equals org.ORGCODE
                        join re in wmpdb.GEN_REF on org.ORG_MTYPE.Trim() equals re.REF_CODE
                        where re.CLS_CODE == 104 && prod.ISVALID == 1
                        select new WMPBankTypeOption { Code = re.REF_CODE, TypeName = re.REF_NAME.Trim() }).Distinct().ToList().OrderBy(p => p.Code);
            }
        }

        public IEnumerable<WMPAreaOption> GetWmpAreaOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.v_WMP_AREA_DIC.Select(c => new WMPAreaOption { RegionName = c.PROV_NAME.Trim(),  Code = c.ISS_AREA_CODE, CityName = c.ISS_AREA.Trim()}).Distinct().ToList();
            }
        }

        public IEnumerable<WMPProvinceOption> GetWmpProvinceOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from area in wmpdb.v_WMP_AREA_DIC
                        select new WMPProvinceOption { Code = area.PROV_NAME.Trim(), Name = area.PROV_NAME.Trim() }).Distinct().ToList();
            }
        }

        public IEnumerable<WMPCityOption> GetWmpCityOption(string provName)
        {
            if (provName == "all")
                return _areaOptions.Select(c => new WMPCityOption { Code = c.Code, Name = c.CityName }).Distinct().ToList();
            return _areaOptions.Where(c => provName.Contains(c.RegionName)).Select(c => new WMPCityOption { Code = c.Code, Name = c.CityName.Trim() }).Distinct().ToList();
        }

        public IEnumerable<WMPBankOption> GetWmpBankOption()
        {
            var pRefcursor = new OracleParameter
            {
                OracleDbType = OracleDbType.RefCursor,
                Direction = ParameterDirection.Output
            };

            // this is vital to set when using ref cursors

            // this is an output parameter so we must indicate that fact
            var paramArray = new[]
                            {
                                pRefcursor
                            };
            var ret = GetDataSetBySp("GetWMPBankOptions", paramArray).Tables[0];

            return (from DataRow row in ret.Rows select new WMPBankOption {BankId = row["BANK_ID"].ToString(), BankName = row["BANK_NAME"].ToString(), TypeCode = row["REF_CODE"].ToString()}).ToList();
        }

        public DataTable GetWmpBankDataPaging(bool includeTimeSpan, string bankType, string bank, int currency, string yieldType, string prodSate,
                                        string term, string initAmount, string investType, string yield, DateTime startDate,
                                        DateTime endDate, string prodName, string isQdii, string columns, string order, int startPage, int pageSize, string area, out int total)
        {
            if (!columns.Contains("PRD_NAME"))
                columns += ",PRD_NAME";
            
            var paramArray = new[]
                            {
                                new OracleParameter("@IncludeTimeSpan", OracleDbType.Int32) { Value = includeTimeSpan?1:0},
                                new OracleParameter("@BankType", OracleDbType.Varchar2) { Value = bankType },
                                new OracleParameter("@Bank", OracleDbType.Clob) { Value = bank },
                                new OracleParameter("@Currency", OracleDbType.Int32) { Value = currency },
                                new OracleParameter("@YieldType", OracleDbType.Varchar2) { Value = yieldType },
                                new OracleParameter("@ProductState", OracleDbType.Varchar2) { Value = prodSate },
                                new OracleParameter("@Term", OracleDbType.Varchar2) { Value = term },
                                new OracleParameter("@InitAmount", OracleDbType.Varchar2) { Value = initAmount },
                                new OracleParameter("@InvestType", OracleDbType.Varchar2) { Value = investType },
                                new OracleParameter("@Yield", OracleDbType.Varchar2) { Value = yield },
                                new OracleParameter("@StartDate", OracleDbType.TimeStamp) { Value =(OracleTimeStamp)startDate  }, 
                                new OracleParameter("@EndDate", OracleDbType.TimeStamp) { Value =(OracleTimeStamp)endDate }, 
                                new OracleParameter("@ProductName", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = prodName} ,
                                new OracleParameter("@Area", OracleDbType.Clob){ Value = area} , 
                                new OracleParameter("@IS_QDII",OracleDbType.NVarchar2,ParameterDirection.InputOutput){Value=isQdii},
                                new OracleParameter("@ColumnList", OracleDbType.Varchar2) { Value = columns },
                                new OracleParameter("@Order_col", OracleDbType.Varchar2,200) { Value = order,Direction = ParameterDirection.InputOutput},
                                new OracleParameter("@StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySp("GetWMPBankDataPaging", paramArray, "@Total",out value).Tables[0];
            total = value.ToString()!="null" ? Convert.ToInt32(value.ToString()) : 0;
            
            return table;
        }

        public DataTable GetWmpTrustPaging(bool includeTimeSpan, string queryDateType, string orgType,
                                       string org, string trustType, string invField, string yield,
                                       string prodState, string term, string issueAmount, string minCap, DateTime startDate,
                                       DateTime endDate, string prodName, string isPe, string isTot,
                                       string columns, string order, int startPage, int pageSize, out int total)
        {
            if (!columns.Contains("PRD_NAME"))
                columns += ",PRD_NAME";
            var paramArray = new[]
                            {
                                new OracleParameter("@IncludeTimeSpan", OracleDbType.Int32) { Value = includeTimeSpan?1:0},
                                new OracleParameter("@QueryDateType", OracleDbType.Varchar2) { Value = queryDateType },
                                new OracleParameter("@OrgType", OracleDbType.Varchar2) { Value = orgType },
                                new OracleParameter("@Org", OracleDbType.Varchar2,org.Length,ParameterDirection.InputOutput) { Value = org },
                                new OracleParameter("@TrustType", OracleDbType.NVarchar2) { Value = trustType },
                                new OracleParameter("@InvField", OracleDbType.Varchar2) { Value = invField },
                                new OracleParameter("@ProductState", OracleDbType.Varchar2) { Value = prodState },
                                new OracleParameter("@Term", OracleDbType.NVarchar2) { Value = term },
                                new OracleParameter("@MinCap", OracleDbType.Varchar2) { Value = minCap },
                                new OracleParameter("@IssueAmount", OracleDbType.Varchar2) { Value = issueAmount },
                                new OracleParameter("@Yield", OracleDbType.Varchar2) { Value = yield },
                                new OracleParameter("@StartDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)startDate }, 
                                new OracleParameter("@EndDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)endDate }, 
                                new OracleParameter("@ProductName", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = prodName} , 
                                new OracleParameter("@IS_PE",OracleDbType.NVarchar2,ParameterDirection.InputOutput){Value = isPe},
                                new OracleParameter("@IS_TOT",OracleDbType.NVarchar2,ParameterDirection.InputOutput){Value = isTot},
                                new OracleParameter("@ColumnList", OracleDbType.Varchar2) { Value = columns },
                                new OracleParameter("@Order_col", OracleDbType.Varchar2,200) { Value = order,Direction = ParameterDirection.InputOutput},
                                new OracleParameter("@StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySp("GetWMPTrustPaging", paramArray, "@Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public IEnumerable<WMPCurrencyOption> GetWmpCurrencyOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from prod in wmpdb.v_WMP_BANK_PROD
                        select new WMPCurrencyOption { Type = prod.ENTR_CURNCY_TYPE, Name = prod.ENTR_CURNCY_NAME }).Distinct().ToList().OrderBy(p => p.Type);
            }
        }

        public IEnumerable<WMPYieldOption> GetWmpYieldOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from prod in wmpdb.BANK_FIN_DETAIL
                        where prod.ISVALID == 1 && prod.PRD_TYPE != null
                        select new WMPYieldOption { Type = prod.PRD_TYPE, Name = prod.PRD_TYPE_NAME }).Distinct().ToList().OrderBy(p => p.Type);
            }
        }

        public IEnumerable<WMPInvestOption> GetWmpInvestOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from re in wmpdb.GEN_REF
                        where re.CLS_CODE == 1119
                        select new WMPInvestOption { Type = re.REF_CODE, Name = re.REF_NAME }).Distinct().ToList().OrderBy(p => Convert.ToInt32(p.Type));
            }
        }

        public IEnumerable<WMPBankOption> GetWmpBankOptionByType(string typeCode)
        {
            if (typeCode == "all")
                return _bankTypeOptions.ToList();
            var typeCodes = typeCode.Split(',');
            return _bankTypeOptions.Where(p => typeCodes.Contains(p.TypeCode)).Select(p => p).ToList();
        }

        public BANK_FIN_DETAIL GetBankFinDetailByInnerCode(int code)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.BANK_FIN_DETAIL.FirstOrDefault(x => x.INNER_CODE == code);
            }
        }
        public BANK_FIN_PRD GetBankFinProdByInnerCode(int code)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.BANK_FIN_PRD.FirstOrDefault(x => x.INNER_CODE == code);
            }
        }

        public v_WMP_BANK_PROD GetViewProdByInnerCode(int code)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.v_WMP_BANK_PROD.FirstOrDefault(x => x.INNER_CODE == code);
            }
        }

        public ORG_PROFILE GetOrgProfileByOrgCode(int code)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.ORG_PROFILE.FirstOrDefault(x => x.ORGCODE == code);
            }
        }

        public IEnumerable<WMPReportTeypOption> GetWmpReportTypeOption()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                var types = wmpdb.FIN_PRD_RPT.Where(r => r.ISVALID == 1).Select(r => r.RPT_TYPE).AsEnumerable().Select(r => r.Value.ToString()).Distinct().ToList();
                return (from reff in wmpdb.GEN_REF
                        where reff.CLS_CODE == 8012 && types.Contains(reff.REF_CODE)
                        select new WMPReportTeypOption { Type = reff.REF_CODE, Name = reff.REF_NAME.Trim() }).Distinct().ToList();
            }
        }

        public IEnumerable<WMPBankReport> GetWmpReport(DateTime startDate, DateTime endDate, int type, int pageNo, int pageSize, out int total)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                var query = wmpdb.FIN_PRD_RPT
                    .Where(r => r.WRITEDATE >= startDate && r.WRITEDATE <= endDate && r.ISVALID == 1)
                    .Select(r => r).Distinct();

                if (type != -1) //all
                    query = query.Where(r => r.RPT_TYPE == type);

                var tt = query.AsEnumerable().Select(
                    r => new WMPBankReport
                    {
                        SEQ = r.SEQ,
                        RPT_TITLE = r.RPT_TITLE,
                        RPT_TYPE = GetTypeName(r.RPT_TYPE),
                        WRITEDATE = ((DateTime)r.WRITEDATE).ToString("yyyy-MM-dd"),
                        RPT_SRC = r.RPT_SRC,
                        RPT_ID = r.RPT_ID
                    }).OrderByDescending(r => r.WRITEDATE);

                total = tt.Count();

                if (pageNo != -1) //need to pagination
                    return tt.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                return tt.ToList();
            }
        }

        public string GetReportPath(int id)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.FIN_PRD_RPT.Where(r => r.SEQ == id).Select(r => r.ACCE_ROUTE).FirstOrDefault();
            }
        }

        private string GetTypeName(long? type)
        {
            string t = type == null ? "" : type.ToString();
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.GEN_REF.Where(re => re.CLS_CODE == 8012 && re.REF_CODE == t).Select(re => re.REF_NAME).FirstOrDefault();
            }
        }

        public DataTable GetAmountTrendData(DateTime start, DateTime end, string category, string bank, string area)
        {
            var paramArray = new[]
                {
                    new OracleParameter("@StartDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)start}, 
                    new OracleParameter("@EndDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)end}, 
                    new OracleParameter("@Category_prd",OracleDbType.Varchar2){Value = category},
                    new OracleParameter("@Bank",OracleDbType.Clob){Value = bank},
                    new OracleParameter("@Area",OracleDbType.Clob){Value = area},
                    new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                };
            return GetDataSetBySp("GetWMPTreadSummary", paramArray).Tables[0];
        }

        public DataTable GetTrendCountData(DateTime start, DateTime end, string category, string bank, string area)
        {
            var paramArray = new[]
                {
                    new OracleParameter("@StartDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)start}, 
                    new OracleParameter("@EndDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)end}, 
                    new OracleParameter("@Category_wmp",OracleDbType.Varchar2){Value = category},
                    new OracleParameter("@Bank",OracleDbType.Clob){Value = bank},
                    new OracleParameter("@Area",OracleDbType.Clob){Value = area},
                    new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                };
            return GetDataSetBySp("GetWmpTrendCount", paramArray).Tables[0];
        }

        public DataTable GetTrendBottomTable(DateTime start, DateTime end, string bank, string area, string currency, string yieldType, string term, string investType, string yield, string columnList, int currentPage, int pageSize, out int total)
        {
            var paramArray = new[]
                {
                    new OracleParameter("@BankType", OracleDbType.Varchar2) { Value = "all" },
                    new OracleParameter("@Bank", OracleDbType.Clob) { Value = bank },
                    new OracleParameter("@Area", OracleDbType.Clob){ Value = area} , 
                    new OracleParameter("@Currency", OracleDbType.Varchar2) { Value = currency },
                    new OracleParameter("@YieldType", OracleDbType.Varchar2) { Value = yieldType },
                    new OracleParameter("@Term", OracleDbType.Varchar2) { Value = term },
                    new OracleParameter("@ProdName", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = null} ,
                    new OracleParameter("@InvestType", OracleDbType.Varchar2) { Value = investType },
                    new OracleParameter("@Yield", OracleDbType.Varchar2) { Value = yield },
                    new OracleParameter("@StartDate",OracleDbType.TimeStamp,ParameterDirection.InputOutput){Value =(OracleTimeStamp)start },
                    new OracleParameter("@EndDate",OracleDbType.TimeStamp,ParameterDirection.InputOutput){Value =(OracleTimeStamp)end },
                    new OracleParameter("@ColumnList", OracleDbType.Varchar2) { Value = columnList },
                    new OracleParameter("@StartPage", OracleDbType.Int32) { Value = currentPage },
                    new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                    new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                    new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                };
            object value;
            var table = GetDataSetBySp("GetWMPTrendDetails", paramArray, "@Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public List<SelectListItem> GetWmpGenRefByCode(int code)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from org in wmpdb.GEN_REF
                        where org.CLS_CODE == code && org.ISVALID == 1
                        select new SelectListItem { Text = org.REF_NAME, Value = org.REF_CODE }).ToList();
            }
        }

        public List<SelectListItem> GetWmpOrgType()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from p in wmpdb.v_WMP_TRUST
                        select new { p.ORG_TYPE_CD, p.ORG_TYPE_NAME }).Distinct().OrderBy(
                               x => x.ORG_TYPE_CD).AsEnumerable().Select(x => new SelectListItem { Text = x.ORG_TYPE_NAME, Value = x.ORG_TYPE_CD.ToString() }).ToList();
            }
        }

        public List<SelectListItem> GetTrustCompany(string type)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                if (type == "all")
                {
                    return (from p in wmpdb.TRUST_PROFILE
                            where p.ORGCODE != null && p.ISVALID == 1 && p.IS_PE != 1
                            select new { p.ORGNAME, p.ORGCODE }).Distinct().OrderBy(
                               x => x.ORGNAME).AsEnumerable().Select(x => new SelectListItem { Text = x.ORGNAME, Value = x.ORGCODE.ToString(), Selected = true }).ToList();
                }
                int t = Convert.ToInt32(type);
                return (from p in wmpdb.v_WMP_TRUST
                        where p.ORGCODE != null && p.ORG_TYPE_CD == t && p.IS_PE != 1
                    select new { p.ORGNAME, p.ORGCODE }).Distinct().OrderBy(
                        x => x.ORGNAME).AsEnumerable().Select(x => new SelectListItem { Text = x.ORGNAME, Value = x.ORGCODE.ToString(), Selected = true }).ToList();
            }
        }

        #region trust products

        public v_WMP_TRUST GetTrustWmpDetailByInnerCode(int innerCode)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.v_WMP_TRUST.FirstOrDefault(x => x.INNER_CODE == innerCode);
            }
        }

        #endregion

        #region Yield trend

        public DataTable GetYieldTrendDetail(DateTime start, DateTime end, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType, string term, string columnList, int currentPage, int pageSize, out int total)
        {
            var paramArray = new[]
                {
                    new OracleParameter("@BankType", OracleDbType.Varchar2) { Value = bankType },
                    new OracleParameter("@Bank", OracleDbType.Clob) { Value = bank },
                    new OracleParameter("@Area", OracleDbType.Clob){ Value = area} , 
                    new OracleParameter("@Currency", OracleDbType.Varchar2) { Value = currency },
                    new OracleParameter("@YieldType", OracleDbType.Varchar2) { Value = yieldType },
                    new OracleParameter("@Term", OracleDbType.Varchar2) { Value = term },
                    new OracleParameter("@ProdName", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = prodName} ,
                    new OracleParameter("@InvestType", OracleDbType.Varchar2) { Value = investType },
                    new OracleParameter("@Yield", OracleDbType.Varchar2) { Value = "all" },
                    new OracleParameter("@StartDate",OracleDbType.TimeStamp,ParameterDirection.InputOutput){Value =(OracleTimeStamp)start },
                    new OracleParameter("@EndDate",OracleDbType.TimeStamp,ParameterDirection.InputOutput){Value =(OracleTimeStamp)end },
                    new OracleParameter("@ColumnList", OracleDbType.Varchar2) { Value = columnList },
                    new OracleParameter("@StartPage", OracleDbType.Int32) { Value = currentPage },
                    new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                    new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                    new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                };
            object value;
            var table = GetDataSetBySp("GetWMPTrendDetails", paramArray, "@Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }

        public DataTable GetYieldTrendChartData(DateTime start, DateTime end, string bankType, string bank, string prodName, string area, string currency, string yieldType, string investType, string term)
        {
            var paramArray = new[]
                {
                    new OracleParameter("@StartDate",OracleDbType.TimeStamp,ParameterDirection.InputOutput){Value= (OracleTimeStamp)start},
                    new OracleParameter("@EndDate",OracleDbType.TimeStamp,ParameterDirection.InputOutput){Value= (OracleTimeStamp)end},
                    new OracleParameter("@BankType",OracleDbType.Varchar2){Value = bankType},
                    new OracleParameter("@Bank",OracleDbType.Clob){Value = bank},
                    new OracleParameter("@ProdName",OracleDbType.Varchar2,ParameterDirection.InputOutput){Value = prodName},
                    new OracleParameter("@Area",OracleDbType.Clob){Value = area},
                    new OracleParameter("@Currency",OracleDbType.Varchar2){Value = currency},
                    new OracleParameter("@YieldType",OracleDbType.Varchar2){Value = yieldType},
                    new OracleParameter("@InvestType",OracleDbType.Varchar2){Value = investType},
                    new OracleParameter("@Term",OracleDbType.Varchar2){Value = term},
                    new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                };
            return GetDataSetBySp("GetWMPYieldTrend", paramArray).Tables[0];
        }

        #endregion

        #region CFP WMP
        
        public v_WMP_CFP GetBrokerWmpDetailByInnerCode(int innerCode)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.v_WMP_CFP.FirstOrDefault(x => x.INNER_CODE == innerCode);
            }
        }

        public List<SelectListItem> GetWmpBrokerOrgType()
        {
            List<SelectListItem> list;

            using (var wmpdb = new Genius_HistEntities())
            {
                var ol = (from o in wmpdb.CFP_ISS_ORG.Where(o => o.ISVALID == 1 && o.ENDDATE == null && o.ORG_CLS == 63)
                          join dateList in
                              ((from m in wmpdb.CFP_ISS_ORG
                                where m.ISVALID == 1 && m.ENDDATE == null && m.ORG_CLS == 63
                                group m by m.ORGCODE into orgList
                                select new { ORGCODE = orgList.Key, STARTDATE = orgList.Max(re => re.STARTDATE) })) on o.ORGCODE equals dateList.ORGCODE
                          where o.ISVALID == 1 && o.ENDDATE == null && o.ORG_CLS == 63 && o.STARTDATE == dateList.STARTDATE
                          orderby o.ORGNAME
                          select new { o.ORGCODE, o.ORGNAME }).Distinct().ToList();
                var l = ol.Select(t => new {t.ORGCODE, t.ORGNAME}).Distinct().ToList();
                list = l.Select(x => new SelectListItem {Text = x.ORGNAME, Value = x.ORGCODE.ToString(), Selected = true}).ToList();
            }

            list.Add(new SelectListItem { Text = "其它", Value = "0", Selected = true });
            return list;
        }

        public List<SelectListItem> GetWmpBrokerProductType()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from o in wmpdb.v_WMP_CFP
                        where o.ISVALID == 1
                        select new { o.PROD_TYPE_NAME, o.CFP_TYPE })
                    .Distinct().OrderBy(x => x.CFP_TYPE).AsEnumerable()
                    .Select(x => new SelectListItem { Text = x.PROD_TYPE_NAME, Value = x.CFP_TYPE }).ToList();
            }
        }

        public List<SelectListItem> GetWmpBrokerInvestType()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from o in wmpdb.v_WMP_CFP
                        where o.ISVALID == 1
                        select new { o.INVEST_NAME, o.INVEST_CLS })
                    .Distinct().OrderBy(x => x.INVEST_CLS).AsEnumerable()
                    .Select(x => new SelectListItem { Text = x.INVEST_NAME, Value = x.INVEST_CLS }).ToList();
            }
        }

        public List<SelectListItem> GetWmpBrokerLowestType()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from o in wmpdb.v_WMP_CFP
                        where o.ISVALID == 1
                        select new {o.LOWEST_VAL_CODE,o.LOWEST_VAL_NAME })
                    .Distinct().OrderBy(x => x.LOWEST_VAL_CODE).AsEnumerable()
                    .Select(x => new SelectListItem { Text = x.LOWEST_VAL_NAME, Value = x.LOWEST_VAL_CODE }).ToList().Distinct().ToList();
            }
        }

        public List<SelectListItem> GetWmpBrokerQdiiType()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from o in wmpdb.v_WMP_CFP
                        where o.ISVALID == 1
                        select new { o.IS_QDII, o.IS_QDII_NAME })
                    .Distinct().OrderBy(x => x.IS_QDII).AsEnumerable()
                    .Select(x => new SelectListItem { Text = x.IS_QDII_NAME, Value = x.IS_QDII.ToString() }).ToList();
            }
        }

        public List<SelectListItem> GetWmpBrokerProdStateType()
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return (from o in wmpdb.v_WMP_CFP
                        where o.ISVALID == 1
                        select new { o.PROD_STATE_CODE, o.PROD_STATE })
                    .Distinct().AsEnumerable()
                    .Select(x => new SelectListItem { Text = x.PROD_STATE, Value = x.PROD_STATE_CODE }).ToList();
            }
        }

        public List<SelectListItem> GetWmpBrokerBankType()
        {
            List<SelectListItem> list;

            using (var wmpdb = new Genius_HistEntities())
            {
                var ol = (from o in wmpdb.CFP_ISS_ORG.Where(o => o.ISVALID == 1 && o.ORG_CLS == 23)
                          join dateList in
                              ((from m in wmpdb.CFP_ISS_ORG
                                where m.ISVALID == 1 && m.ORG_CLS == 23
                                group m by m.ORGCODE into orgList
                                select new { ORGCODE = orgList.Key, STARTDATE = orgList.Max(re => re.STARTDATE) })) on o.ORGCODE equals dateList.ORGCODE
                          where o.ISVALID == 1 && o.ORG_CLS == 23 && o.STARTDATE == dateList.STARTDATE
                          orderby o.ORGNAME
                          select new { o.ORGCODE, o.ORGNAME }).Distinct().ToList();

                var l = ol.Select(t => new { t.ORGCODE, t.ORGNAME }).Distinct().ToList();
                list = l.Select(x => new SelectListItem { Text = x.ORGNAME, Value = x.ORGCODE.ToString(), Selected = true }).ToList();
            }

            list.Add(new SelectListItem { Text = "其它", Value = "0", Selected = true });
            return list;
        }

        public DataTable GetWmpBrokerPaging(string orgs, string prodType, string investType, string lowest, string isQdii, string prodState, string banks, bool includeDate, string queryDateType, DateTime startDate, DateTime endDate, string prodName, string pmName, string columns, string order, int startPage, int pageSize, out int total)
        {
            if (!columns.Contains("CFPNAME"))
                columns += ",CFPNAME";

            var paramArray = new[]
                            {
                                new OracleParameter("@IncludeTimeSpan", OracleDbType.Int32) { Value = includeDate?1:0},
                                new OracleParameter("@QueryDateType", OracleDbType.Varchar2) { Value = queryDateType },
                                new OracleParameter("@OrgType", OracleDbType.Varchar2,ParameterDirection.InputOutput) { Value = orgs },
                                new OracleParameter("@BankType", OracleDbType.Varchar2,ParameterDirection.InputOutput) { Value = banks },
                                new OracleParameter("@ProdType", OracleDbType.NVarchar2) { Value = prodType },
                                new OracleParameter("@InvCls", OracleDbType.NVarchar2) { Value = investType },
                                new OracleParameter("@ProductState", OracleDbType.Varchar2) { Value = prodState },
                                new OracleParameter("@LowestVal", OracleDbType.Varchar2) { Value = lowest },
                                new OracleParameter("@StartDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)startDate }, 
                                new OracleParameter("@EndDate", OracleDbType.TimeStamp,ParameterDirection.InputOutput) { Value = (OracleTimeStamp)endDate }, 
                                new OracleParameter("@ProductName", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value = prodName },
                                new OracleParameter("@InvManager", OracleDbType.NVarchar2,ParameterDirection.InputOutput) { Value =pmName },
                                new OracleParameter("@IS_QDII", OracleDbType.Varchar2) { Value = isQdii },
                                new OracleParameter("@ColumnList", OracleDbType.Varchar2) { Value = columns },
                                new OracleParameter("@Order_col", OracleDbType.Varchar2,200) { Value = order,Direction = ParameterDirection.InputOutput},
                                new OracleParameter("@StartPage", OracleDbType.Int32) { Value = startPage },
                                new OracleParameter("@PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("@Total", OracleDbType.Int32,ParameterDirection.Output),
                                new OracleParameter("@cur",OracleDbType.RefCursor,ParameterDirection.Output)
                            };
            object value;
            var table = GetDataSetBySp("GetWMPCFPPaging", paramArray, "@Total", out value).Tables[0];
            total = value.ToString() != "null" ? Convert.ToInt32(value.ToString()) : 0;

            return table;
        }


        public DataTable GetWmpBrokerNetWorthData(int id, DateTime startDate, DateTime endDate)
        {
            var paramArray = new[]
            {
                new OracleParameter("@id_code", OracleDbType.Int32) {Value = id},
                new OracleParameter("@startDate", OracleDbType.Varchar2) {Value = startDate.ToString("dd-MMM-yyyy")},
                new OracleParameter("@endDate", OracleDbType.Varchar2) {Value = endDate.ToString("dd-MMM-yyyy")},
                new OracleParameter("@worth_return",OracleDbType.RefCursor,ParameterDirection.Output)
            };
            return GetDataSetBySp("GetBrokerNetWorth", paramArray).Tables[0];
        }


        public List<BrokerFee> GetWmpBrokerFeeById(int id)
        {
            var paramArray = new[]
                        {
                            new OracleParameter("@id_code", OracleDbType.Int32) { Value = id },
                            new OracleParameter("@fee_return",OracleDbType.RefCursor,ParameterDirection.Output)
                        };
            var table = GetDataSetBySp("GetBrokerFee", paramArray).Tables[0];
            return IPP.DataTableSerializer.ToList<BrokerFee>(table);
         
        }

        public List<BrokerFinIdx> GetWmpBrokerFinIdxById(int id)
        {
            var paramArray = new[]
            {
                new OracleParameter("@id_code", OracleDbType.Int32) { Value = id },
                new OracleParameter("@idx_return",OracleDbType.RefCursor,ParameterDirection.Output)
            };
            var table = GetDataSetBySp("GetBrokerFinIdx", paramArray).Tables[0];
            return IPP.DataTableSerializer.ToList<BrokerFinIdx>(table);
        }

        #endregion

        public DataTable GetDiscContentById(int id)
        {
            return GetDataSetBySp("GetDiscById", new[]
            {
                new OracleParameter("@id_code", OracleDbType.Int32) { Value = id }, 
                new OracleParameter("@cur", OracleDbType.RefCursor, ParameterDirection.Output)
            
            }).Tables[0];
        }

        public List<CFP_PROFITSHEET> GetCfpProfitsheets(int id)
        {
            using (var wmpdb = new Genius_HistEntities())
            {
                return wmpdb.CFP_PROFITSHEET.Where(x => x.ISVALID == 1 && x.INNER_CODE == id).ToList();
            }
        }

        public CmaFile GetBrokerDisc(string id)
        {
            return FileService.GetFileById(Convert.ToInt32(id), "WMP_DISC");
        }
        /// <summary>  
        /// DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time"> DateTime时间格式</param>  
        /// <returns>Unix时间戳格式</returns>  
        public  int ConvertDateTimeInt(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        public  String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssfff");
        }
    }
}
