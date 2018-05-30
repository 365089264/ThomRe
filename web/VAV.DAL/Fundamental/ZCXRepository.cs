using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VAV.DAL.Common;
using VAV.DAL.IPP;
using VAV.Entities;
using VAV.Model.Data.ZCX;
using System.Linq.Dynamic;
using System.Threading;
using System.Data.SqlClient;
using System.Linq.Expressions;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;
using VAV.Model.Data;
using Oracle.ManagedDataAccess.Client;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;


namespace VAV.DAL.Fundamental
{
	public class ZCXRepository : BaseRepository
	{
        [Dependency]
        public IFileService FileService { get; set; }

		/// <summary>
		/// Get existing company type from table
		/// </summary>
		/// <returns>List of PUB_PAR</returns>
		public IEnumerable<PUB_PAR> GetCompanyType()
		{
			using (var ZCXDB = new ZCXEntities())
			{
                //var types = (from orginfo in ZCXDB.RATE_ORG_INFO_F
                //             select orginfo.TYPE_BIG_PAR).Distinct().ToList();
                var query = from pubpar in ZCXDB.PUB_PAR
                            join orginfo in ZCXDB.RATE_ORG_INFO_F on pubpar.PAR_CODE equals orginfo.TYPE_BIG_PAR
							where pubpar.PAR_SYS_CODE == 1004
							select pubpar;
				return query.Distinct().ToList();
				//return ZCXDB.PUB_PAR.Where(x => x.PAR_CODE == 1004).ToList();
			}
		}

		/// <summary>
		/// Get unlisted issuers
		/// </summary>
		/// <param name="parCode">Company type code</param>
		/// <param name="name">Search by name</param>
		/// <param name="bond">Search by bond </param>
		/// <param name="order">Order by some field,pass strings like "NAME ASC"</param>
		/// <returns>NonlistIssuer list</returns>
		public IEnumerable<NonlistIssuer> GetNonlistIssuer(long parCode, string name, string bond, bool hideNodata, string order,int pageNo,int pageSize,out int total)
		{
			var paramArray = new[]
                            {
                                new OracleParameter("P_parCode", OracleDbType.Int64) { Value = parCode }, 
                                new OracleParameter("P_company", OracleDbType.Varchar2) { Value = name} , 
                                new OracleParameter("P_bondCode", OracleDbType.Varchar2) { Value = bond },
                                new OracleParameter("P_hideNodata", OracleDbType.Int32) { Value = hideNodata?1:0 },
                                new OracleParameter("P_order", OracleDbType.Varchar2) { Value = order },
                                new OracleParameter("P_StartPage", OracleDbType.Int32) { Value = pageNo },
                                new OracleParameter("P_PageSize", OracleDbType.Int32) { Value = pageSize },
                                new OracleParameter("P_Culture", Thread.CurrentThread.CurrentUICulture.Name),
                                new OracleParameter("P_TOTAL", OracleDbType.Int32,ParameterDirection.Output) { Value = 0 },
                                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
                            };
            using (var cmaDb = new ZCXEntities())
            {
                using (OracleCommand spCmd = new OracleCommand())
                {
                    cmaDb.Database.Connection.Open();
                    spCmd.Connection = new OracleConnection(cmaDb.Database.Connection.ConnectionString);
                    spCmd.CommandText = "GetNonlistIssuer";
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;

                    spCmd.Parameters.AddRange(paramArray);

                    OracleDataAdapter da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    total = Convert.ToInt32(spCmd.Parameters["P_TOTAL"].Value.ToString());
                    return DataTableSerializer.ToList<NonlistIssuer>(ds.Tables[0]);
                }
            }
		}

		public IEnumerable<NonlistIssuer> GetAllNonlistIssuer(long parCode, string name, string bond,bool hideNodata)
		{
		    int total;
            return GetNonlistIssuer(parCode, name, bond, hideNodata,null,1,2000,out total);
		}

		public IEnumerable<IssuerFundamental> GetFoundamentalData(string fundamentalType, int companyId, string reportType, string contentType, int interval, string unit)
		{
			switch (fundamentalType)
			{
				case "tabb":
					return GetIssuerTABB(companyId, reportType, contentType, interval, unit);
				case "tacb":
					return GetIssuerTACB(companyId, reportType, contentType, interval, unit);
				case "tapb":
					return GetIssuerTAPB(companyId, reportType, contentType, interval, unit);
			}

			return null;
		}

		public IEnumerable<IssuerFundamental> GetIssuerTABB(int companyId, string reportType, string contentType, int interval, string unit)
		{
			IEnumerable<IssuerFundamental> issuerTABB = null;

			using (var ZCXDB = new ZCXEntities())
			{
				Func<BOND_FIN_BALA_TABB, bool> dateFilter = null;
				Func<BOND_FIN_BALA_TABB, bool> reportTypeFilter = null;
				int multiplier = GetMultiPlier(unit); 
				
				if (reportType == "Y") //1:year report;
				{
					if (interval == 0)
						dateFilter = t => true;
					else
                        dateFilter = t => t.END_DATE >= new DateTime(DateTime.Now.Year - interval, 1, 1);
					
					reportTypeFilter = t => t.SHEET_ATTR_PAR == 1;
				}
				else
				{
					if (interval == 0)
						dateFilter = t => true;
					else
                        dateFilter = t => t.END_DATE >= new DateTime(DateTime.Now.Year - interval, DateTime.Now.Month - (DateTime.Now.Month - 1) % 3, 1);
					reportTypeFilter = t => (t.SHEET_ATTR_PAR == 3 || t.SHEET_ATTR_PAR == 2 || t.SHEET_ATTR_PAR == 6 || t.SHEET_ATTR_PAR == 1);
				}

				switch (contentType)
				{
					case "RawReport":
						issuerTABB = from tabb in (ZCXDB.BOND_FIN_BALA_TABB
									 .Where(b => b.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(b => b.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault())).ToList()
									 select new IssuerFundamental
									 {
										 Field1 = tabb.BS_01 / multiplier,
										 Field2 = tabb.BS_0101 / multiplier,
										 Field3 = tabb.BS_02 / multiplier,
										 Field4 = tabb.BS_03 / multiplier,
										 Field5 = tabb.BS_04 / multiplier,
										 Field6 = tabb.BS_05 / multiplier,
										 Field7 = tabb.BS_06 / multiplier,
										 Field8 = tabb.BS_07 / multiplier,
										 Field9 = tabb.BS_08 / multiplier,
										 Field10 = tabb.BS_09 / multiplier,
										 Field11 = tabb.BS_0901 / multiplier,
										 Field12 = tabb.BS_10 / multiplier,
										 Field13 = tabb.BS_11 / multiplier,
										 Field14 = tabb.BS_12 / multiplier,
										 Field15 = tabb.BS_13 / multiplier,
										 Field16 = tabb.BS_14 / multiplier,
										 Field17 = tabb.BS_15 / multiplier,
										 Field18 = tabb.BS_16 / multiplier,
										 Field19 = tabb.BS_17 / multiplier,
										 Field20 = tabb.BS_39 / multiplier,
										 Field21 = tabb.BS_40 / multiplier,
										 Field22 = tabb.BS_4001 / multiplier,
										 Field23 = tabb.BS_41 / multiplier,
										 Field24 = tabb.BS_42 / multiplier,
										 Field25 = tabb.BS_43 / multiplier,
										 Field26 = tabb.BS_44 / multiplier,
										 Field27 = tabb.BS_45 / multiplier,
										 Field28 = tabb.BS_46 / multiplier,
										 Field29 = tabb.BS_47 / multiplier,
										 Field30 = tabb.BS_48 / multiplier,
										 Field31 = tabb.BS_49 / multiplier,
										 Field32 = tabb.BS_18 / multiplier,
										 Field33 = tabb.BS_19 / multiplier,

										 Field34 = tabb.BS_20 / multiplier,
										 Field35 = tabb.BS_21 / multiplier,
										 Field36 = tabb.BS_22 / multiplier,
										 Field37 = tabb.BS_23 / multiplier,
										 Field38 = tabb.BS_24 / multiplier,
										 Field39 = tabb.BS_25 / multiplier,
										 Field40 = tabb.BS_26 / multiplier,
										 Field41 = tabb.BS_27 / multiplier,
										 Field42 = tabb.BS_28 / multiplier,
										 Field43 = tabb.BS_29 / multiplier,
										 Field44 = tabb.BS_30 / multiplier,
										 Field45 = tabb.BS_31 / multiplier,
										 Field46 = tabb.BS_3101 / multiplier,
										 Field47 = tabb.BS_32 / multiplier,
										 Field48 = tabb.BS_33 / multiplier,
										 Field49 = tabb.BS_34 / multiplier,
										 Field50 = tabb.BS_35 / multiplier,
										 Field51 = tabb.BS_36 / multiplier,
										 Field52 = tabb.BS_50 / multiplier,
										 Field53 = tabb.BS_51 / multiplier,
										 Field54 = tabb.BS_52 / multiplier,
										 Field55 = tabb.BS_53 / multiplier,
										 Field56 = tabb.BS_54 / multiplier,
										 Field57 = tabb.BS_55 / multiplier,
										 Field58 = tabb.BS_37 / multiplier,
										 Field59 = tabb.BS_38 / multiplier,
										 Field60 = tabb.BS_56 / multiplier,

										 Field61 = tabb.BS_57 / multiplier,
										 Field62 = tabb.BS_5701 / multiplier,
										 Field63 = tabb.BS_58 / multiplier,
										 Field64 = tabb.BS_59 / multiplier,
										 Field65 = tabb.BS_60 / multiplier,
										 Field66 = tabb.BS_61 / multiplier,
										 Field67 = tabb.BS_62 / multiplier,
										 Field68 = tabb.BS_63 / multiplier,
										 Field69 = tabb.BS_64 / multiplier,
										 Field70 = tabb.BS_65 / multiplier,
										 Field71 = tabb.BS_66 / multiplier,
										 Field72 = tabb.BS_67 / multiplier,
										 Field73 = tabb.BS_68 / multiplier,
										 Field74 = tabb.BS_69 / multiplier,
										 Field75 = tabb.BS_70 / multiplier,
										 Field76 = tabb.BS_71 / multiplier,
										 Field77 = tabb.BS_72 / multiplier,
										 Field78 = tabb.BS_73 / multiplier,
										 Field79 = tabb.BS_74 / multiplier,
										 Field80 = tabb.BS_75 / multiplier,
										 Field81 = tabb.BS_76 / multiplier,
										 Field82 = tabb.BS_77 / multiplier,
										 Field83 = tabb.BS_78 / multiplier,
										 Field84 = tabb.BS_79 / multiplier,
										 Field85 = tabb.BS_91 / multiplier,
										 Field86 = tabb.BS_92 / multiplier,
										 Field87 = tabb.BS_93 / multiplier,
										 Field88 = tabb.BS_94 / multiplier,
										 Field89 = tabb.BS_95 / multiplier,
										 Field90 = tabb.BS_96 / multiplier,
										 Field91 = tabb.BS_97 / multiplier,
										 Field92 = tabb.BS_98 / multiplier,
										 Field93 = tabb.BS_99 / multiplier,
										 Field94 = tabb.BS_80 / multiplier,
										 Field95 = tabb.BS_81 / multiplier,

										 Field96 = tabb.BS_82 / multiplier,
										 Field97 = tabb.BS_83 / multiplier,
										 Field98 = tabb.BS_84 / multiplier,
										 Field99 = tabb.BS_85 / multiplier,
										 Field100 = tabb.BS_86 / multiplier,
										 Field101 = tabb.BS_87 / multiplier,
										 Field102 = tabb.BS_88 / multiplier,
										 Field103 = tabb.BS_100 / multiplier,
										 Field104 = tabb.BS_101 / multiplier,
										 Field105 = tabb.BS_102 / multiplier,
										 Field106 = tabb.BS_103 / multiplier,
										 Field107 = tabb.BS_104 / multiplier,
										 Field108 = tabb.BS_105 / multiplier,
										 Field109 = tabb.BS_89 / multiplier,
										 Field110 = tabb.BS_90 / multiplier,
										 Field111 = tabb.BS_56 / multiplier,

										 Field112 = tabb.BS_107 / multiplier,
										 Field113 = tabb.BS_108 / multiplier,
										 Field114 = tabb.BS_109 / multiplier,
										 Field115 = tabb.BS_110 / multiplier,
										 Field116 = tabb.BS_111 / multiplier,
										 Field117 = tabb.BS_112 / multiplier,
										 Field118 = tabb.BS_113 / multiplier,
										 Field119 = tabb.BS_114 / multiplier,
										 Field120 = tabb.BS_115 / multiplier,
										 Field121 = tabb.BS_116 / multiplier,
										 Field122 = tabb.BS_117 / multiplier,
										 Field123 = tabb.BS_118 / multiplier,
										 Field124 = tabb.BS_56 / multiplier,
										 Field125 = tabb.BS_120 / multiplier,

                                         EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR, //0-其它；1-年报；2-中报；3-一季报；6-三季报
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "AssetRatio":
						issuerTABB = from tabb in (ZCXDB.BOND_FIN_BALA_TABB
									 .Where(b => b.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(b => b.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault())).ToList()
									 select new IssuerFundamental
									 {
										 Field1 = tabb.BS_01 / tabb.BS_56 * 100,
										 Field2 = tabb.BS_0101 / tabb.BS_56 * 100,
										 Field3 = tabb.BS_02 / tabb.BS_56 * 100,
										 Field4 = tabb.BS_03 / tabb.BS_56 * 100,
										 Field5 = tabb.BS_04 / tabb.BS_56 * 100,
										 Field6 = tabb.BS_05 / tabb.BS_56 * 100,
										 Field7 = tabb.BS_06 / tabb.BS_56 * 100,
										 Field8 = tabb.BS_07 / tabb.BS_56 * 100,
										 Field9 = tabb.BS_08 / tabb.BS_56 * 100,
										 Field10 = tabb.BS_09 / tabb.BS_56 * 100,
										 Field11 = tabb.BS_0901 / tabb.BS_56 * 100,
										 Field12 = tabb.BS_10 / tabb.BS_56 * 100,
										 Field13 = tabb.BS_11 / tabb.BS_56 * 100,
										 Field14 = tabb.BS_12 / tabb.BS_56 * 100,
										 Field15 = tabb.BS_13 / tabb.BS_56 * 100,
										 Field16 = tabb.BS_14 / tabb.BS_56 * 100,
										 Field17 = tabb.BS_15 / tabb.BS_56 * 100,
										 Field18 = tabb.BS_16 / tabb.BS_56 * 100,
										 Field19 = tabb.BS_17 / tabb.BS_56 * 100,
										 Field20 = tabb.BS_39 / tabb.BS_56 * 100,
										 Field21 = tabb.BS_40 / tabb.BS_56 * 100,
										 Field22 = tabb.BS_4001 / tabb.BS_56 * 100,
										 Field23 = tabb.BS_41 / tabb.BS_56 * 100,
										 Field24 = tabb.BS_42 / tabb.BS_56 * 100,
										 Field25 = tabb.BS_43 / tabb.BS_56 * 100,
										 Field26 = tabb.BS_44 / tabb.BS_56 * 100,
										 Field27 = tabb.BS_45 / tabb.BS_56 * 100,
										 Field28 = tabb.BS_46 / tabb.BS_56 * 100,
										 Field29 = tabb.BS_47 / tabb.BS_56 * 100,
										 Field30 = tabb.BS_48 / tabb.BS_56 * 100,
										 Field31 = tabb.BS_49 / tabb.BS_56 * 100,
										 Field32 = tabb.BS_18 / tabb.BS_56 * 100,
										 Field33 = tabb.BS_19 / tabb.BS_56 * 100,

										 Field34 = tabb.BS_20 / tabb.BS_56 * 100,
										 Field35 = tabb.BS_21 / tabb.BS_56 * 100,
										 Field36 = tabb.BS_22 / tabb.BS_56 * 100,
										 Field37 = tabb.BS_23 / tabb.BS_56 * 100,
										 Field38 = tabb.BS_24 / tabb.BS_56 * 100,
										 Field39 = tabb.BS_25 / tabb.BS_56 * 100,
										 Field40 = tabb.BS_26 / tabb.BS_56 * 100,
										 Field41 = tabb.BS_27 / tabb.BS_56 * 100,
										 Field42 = tabb.BS_28 / tabb.BS_56 * 100,
										 Field43 = tabb.BS_29 / tabb.BS_56 * 100,
										 Field44 = tabb.BS_30 / tabb.BS_56 * 100,
										 Field45 = tabb.BS_31 / tabb.BS_56 * 100,
										 Field46 = tabb.BS_3101 / tabb.BS_56 * 100,
										 Field47 = tabb.BS_32 / tabb.BS_56 * 100,
										 Field48 = tabb.BS_33 / tabb.BS_56 * 100,
										 Field49 = tabb.BS_34 / tabb.BS_56 * 100,
										 Field50 = tabb.BS_35 / tabb.BS_56 * 100,
										 Field51 = tabb.BS_36 / tabb.BS_56 * 100,
										 Field52 = tabb.BS_50 / tabb.BS_56 * 100,
										 Field53 = tabb.BS_51 / tabb.BS_56 * 100,
										 Field54 = tabb.BS_52 / tabb.BS_56 * 100,
										 Field55 = tabb.BS_53 / tabb.BS_56 * 100,
										 Field56 = tabb.BS_54 / tabb.BS_56 * 100,
										 Field57 = tabb.BS_55 / tabb.BS_56 * 100,
										 Field58 = tabb.BS_37 / tabb.BS_56 * 100,
										 Field59 = tabb.BS_38 / tabb.BS_56 * 100, 
										 Field60 = tabb.BS_56 / tabb.BS_56 * 100,

										 Field61 = tabb.BS_57 / tabb.BS_56 * 100,
										 Field62 = tabb.BS_5701 / tabb.BS_56 * 100,
										 Field63 = tabb.BS_58 / tabb.BS_56 * 100,
										 Field64 = tabb.BS_59 / tabb.BS_56 * 100,
										 Field65 = tabb.BS_60 / tabb.BS_56 * 100,
										 Field66 = tabb.BS_61 / tabb.BS_56 * 100,
										 Field67 = tabb.BS_62 / tabb.BS_56 * 100,
										 Field68 = tabb.BS_63 / tabb.BS_56 * 100,
										 Field69 = tabb.BS_64 / tabb.BS_56 * 100,
										 Field70 = tabb.BS_65 / tabb.BS_56 * 100,
										 Field71 = tabb.BS_66 / tabb.BS_56 * 100,
										 Field72 = tabb.BS_67 / tabb.BS_56 * 100,
										 Field73 = tabb.BS_68 / tabb.BS_56 * 100,
										 Field74 = tabb.BS_69 / tabb.BS_56 * 100,
										 Field75 = tabb.BS_70 / tabb.BS_56 * 100,
										 Field76 = tabb.BS_71 / tabb.BS_56 * 100,
										 Field77 = tabb.BS_72 / tabb.BS_56 * 100,
										 Field78 = tabb.BS_73 / tabb.BS_56 * 100,
										 Field79 = tabb.BS_74 / tabb.BS_56 * 100,
										 Field80 = tabb.BS_75 / tabb.BS_56 * 100,
										 Field81 = tabb.BS_76 / tabb.BS_56 * 100,
										 Field82 = tabb.BS_77 / tabb.BS_56 * 100,
										 Field83 = tabb.BS_78 / tabb.BS_56 * 100,
										 Field84 = tabb.BS_79 / tabb.BS_56 * 100,
										 Field85 = tabb.BS_91 / tabb.BS_56 * 100,
										 Field86 = tabb.BS_92 / tabb.BS_56 * 100,
										 Field87 = tabb.BS_93 / tabb.BS_56 * 100,
										 Field88 = tabb.BS_94 / tabb.BS_56 * 100,
										 Field89 = tabb.BS_95 / tabb.BS_56 * 100,
										 Field90 = tabb.BS_96 / tabb.BS_56 * 100,
										 Field91 = tabb.BS_97 / tabb.BS_56 * 100,
										 Field92 = tabb.BS_98 / tabb.BS_56 * 100,
										 Field93 = tabb.BS_99 / tabb.BS_56 * 100,
										 Field94 = tabb.BS_80 / tabb.BS_56 * 100,
										 Field95 = tabb.BS_81 / tabb.BS_56 * 100, 

										 Field96 = tabb.BS_82 / tabb.BS_56 * 100,
										 Field97 = tabb.BS_83 / tabb.BS_56 * 100,
										 Field98 = tabb.BS_84 / tabb.BS_56 * 100,
										 Field99 = tabb.BS_85 / tabb.BS_56 * 100,
										 Field100 = tabb.BS_86 / tabb.BS_56 * 100,
										 Field101 = tabb.BS_87 / tabb.BS_56 * 100,
										 Field102 = tabb.BS_88 / tabb.BS_56 * 100,
										 Field103 = tabb.BS_100 / tabb.BS_56 * 100,
										 Field104 = tabb.BS_101 / tabb.BS_56 * 100,
										 Field105 = tabb.BS_102 / tabb.BS_56 * 100,
										 Field106 = tabb.BS_103 / tabb.BS_56 * 100,
										 Field107 = tabb.BS_104 / tabb.BS_56 * 100,
										 Field108 = tabb.BS_105 / tabb.BS_56 * 100,
										 Field109 = tabb.BS_89 / tabb.BS_56 * 100,
										 Field110 = tabb.BS_90 / tabb.BS_56 * 100, 
										 Field111 = tabb.BS_56 / tabb.BS_56 * 100,

										 Field112 = tabb.BS_107 / tabb.BS_56 * 100,
										 Field113 = tabb.BS_108 / tabb.BS_56 * 100,
										 Field114 = tabb.BS_109 / tabb.BS_56 * 100,
										 Field115 = tabb.BS_110 / tabb.BS_56 * 100,
										 Field116 = tabb.BS_111 / tabb.BS_56 * 100,
										 Field117 = tabb.BS_112 / tabb.BS_56 * 100,
										 Field118 = tabb.BS_113 / tabb.BS_56 * 100,
										 Field119 = tabb.BS_114 / tabb.BS_56 * 100,
										 Field120 = tabb.BS_115 / tabb.BS_56 * 100,
										 Field121 = tabb.BS_116 / tabb.BS_56 * 100,
										 Field122 = tabb.BS_117 / tabb.BS_56 * 100,
										 Field123 = tabb.BS_118 / tabb.BS_56 * 100,
										 Field124 = tabb.BS_119 / tabb.BS_56 * 100,
										 Field125 = tabb.BS_120 / tabb.BS_56 * 100,

										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "YoY":
						var funda = ZCXDB.BOND_FIN_BALA_TABB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(f => f.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault()); //取最新的更新的记录

						var fundamentalYoY = ZCXDB.BOND_FIN_BALA_TABB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(f => f.ISVALID == 1).ToList()
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault());

						issuerTABB = from tabb in funda.ToList()
									 join tabbOfLastYear in fundamentalYoY.ToList()
                                     on new { Year = tabb.END_DATE.Value.Year - 1, Month = tabb.END_DATE.Value.Month }
                                     equals new { Year = tabbOfLastYear.END_DATE.Value.Year, Month = tabbOfLastYear.END_DATE.Value.Month } into YoY
									 from tabbYoY in YoY
									 select new IssuerFundamental
									 {
										 Field1 = tabbYoY.BS_01 == 0M ? null : 100 * (tabb.BS_01 - tabbYoY.BS_01) / tabbYoY.BS_01,
										 Field2 = tabbYoY.BS_0101 == 0M ? null : 100 * (tabb.BS_0101 - tabbYoY.BS_0101) / tabbYoY.BS_0101,
										 Field3 = tabbYoY.BS_02 == 0M ? null : 100 * (tabb.BS_02 - tabbYoY.BS_02) / tabbYoY.BS_02,
										 Field4 = tabbYoY.BS_03 == 0M ? null : 100 * (tabb.BS_03 - tabbYoY.BS_03) / tabbYoY.BS_03,
										 Field5 = tabbYoY.BS_04 == 0M ? null : 100 * (tabb.BS_04 - tabbYoY.BS_04) / tabbYoY.BS_04,
										 Field6 = tabbYoY.BS_05 == 0M ? null : 100 * (tabb.BS_05 - tabbYoY.BS_05) / tabbYoY.BS_05,
										 Field7 = tabbYoY.BS_06 == 0M ? null : 100 * (tabb.BS_06 - tabbYoY.BS_06) / tabbYoY.BS_06,
										 Field8 = tabbYoY.BS_07 == 0M ? null : 100 * (tabb.BS_07 - tabbYoY.BS_07) / tabbYoY.BS_07,
										 Field9 = tabbYoY.BS_08 == 0M ? null : 100 * (tabb.BS_08 - tabbYoY.BS_08) / tabbYoY.BS_08,
										 Field10 = tabbYoY.BS_09 == 0M ? null : 100 * (tabb.BS_09 - tabbYoY.BS_09) / tabbYoY.BS_09,
										 Field11 = tabbYoY.BS_0901 == 0M ? null : 100 * (tabb.BS_0901 - tabbYoY.BS_0901) / tabbYoY.BS_0901,
										 Field12 = tabbYoY.BS_10 == 0M ? null : 100 * (tabb.BS_10 - tabbYoY.BS_10) / tabbYoY.BS_10,
										 Field13 = tabbYoY.BS_11 == 0M ? null : 100 * (tabb.BS_11 - tabbYoY.BS_11) / tabbYoY.BS_11,
										 Field14 = tabbYoY.BS_12 == 0M ? null : 100 * (tabb.BS_12 - tabbYoY.BS_12) / tabbYoY.BS_12,
										 Field15 = tabbYoY.BS_13 == 0M ? null : 100 * (tabb.BS_13 - tabbYoY.BS_13) / tabbYoY.BS_13,
										 Field16 = tabbYoY.BS_14 == 0M ? null : 100 * (tabb.BS_14 - tabbYoY.BS_14) / tabbYoY.BS_14,
										 Field17 = tabbYoY.BS_15 == 0M ? null : 100 * (tabb.BS_15 - tabbYoY.BS_15) / tabbYoY.BS_15,
										 Field18 = tabbYoY.BS_16 == 0M ? null : 100 * (tabb.BS_16 - tabbYoY.BS_16) / tabbYoY.BS_16,
										 Field19 = tabbYoY.BS_17 == 0M ? null : 100 * (tabb.BS_17 - tabbYoY.BS_17) / tabbYoY.BS_17,
										 Field20 = tabbYoY.BS_39 == 0M ? null : 100 * (tabb.BS_39 - tabbYoY.BS_39) / tabbYoY.BS_39,
										 Field21 = tabbYoY.BS_40 == 0M ? null : 100 * (tabb.BS_40 - tabbYoY.BS_40) / tabbYoY.BS_40,
										 Field22 = tabbYoY.BS_4001 == 0M ? null : 100 * (tabb.BS_4001 - tabbYoY.BS_4001) / tabbYoY.BS_4001,
										 Field23 = tabbYoY.BS_41 == 0M ? null : 100 * (tabb.BS_41 - tabbYoY.BS_41) / tabbYoY.BS_41,
										 Field24 = tabbYoY.BS_42 == 0M ? null : 100 * (tabb.BS_42 - tabbYoY.BS_42) / tabbYoY.BS_42,
										 Field25 = tabbYoY.BS_43 == 0M ? null : 100 * (tabb.BS_43 - tabbYoY.BS_43) / tabbYoY.BS_43,
										 Field26 = tabbYoY.BS_44 == 0M ? null : 100 * (tabb.BS_44 - tabbYoY.BS_44) / tabbYoY.BS_44,
										 Field27 = tabbYoY.BS_45 == 0M ? null : 100 * (tabb.BS_45 - tabbYoY.BS_45) / tabbYoY.BS_45,
										 Field28 = tabbYoY.BS_46 == 0M ? null : 100 * (tabb.BS_46 - tabbYoY.BS_46) / tabbYoY.BS_46,
										 Field29 = tabbYoY.BS_47 == 0M ? null : 100 * (tabb.BS_47 - tabbYoY.BS_47) / tabbYoY.BS_47,
										 Field30 = tabbYoY.BS_48 == 0M ? null : 100 * (tabb.BS_48 - tabbYoY.BS_48) / tabbYoY.BS_48,
										 Field31 = tabbYoY.BS_49 == 0M ? null : 100 * (tabb.BS_49 - tabbYoY.BS_49) / tabbYoY.BS_49,
										 Field32 = tabbYoY.BS_18 == 0M ? null : 100 * (tabb.BS_18 - tabbYoY.BS_18) / tabbYoY.BS_18,
										 Field33 = tabbYoY.BS_19 == 0M ? null : 100 * (tabb.BS_19 - tabbYoY.BS_19) / tabbYoY.BS_19,

										 Field34 = tabbYoY.BS_20 == 0M ? null : 100 * (tabb.BS_20 - tabbYoY.BS_20) / tabbYoY.BS_20,
										 Field35 = tabbYoY.BS_21 == 0M ? null : 100 * (tabb.BS_21 - tabbYoY.BS_21) / tabbYoY.BS_21,
										 Field36 = tabbYoY.BS_22 == 0M ? null : 100 * (tabb.BS_22 - tabbYoY.BS_22) / tabbYoY.BS_22,
										 Field37 = tabbYoY.BS_23 == 0M ? null : 100 * (tabb.BS_23 - tabbYoY.BS_23) / tabbYoY.BS_23,
										 Field38 = tabbYoY.BS_24 == 0M ? null : 100 * (tabb.BS_24 - tabbYoY.BS_24) / tabbYoY.BS_24,
										 Field39 = tabbYoY.BS_25 == 0M ? null : 100 * (tabb.BS_25 - tabbYoY.BS_25) / tabbYoY.BS_25,
										 Field40 = tabbYoY.BS_26 == 0M ? null : 100 * (tabb.BS_26 - tabbYoY.BS_26) / tabbYoY.BS_26,
										 Field41 = tabbYoY.BS_27 == 0M ? null : 100 * (tabb.BS_27 - tabbYoY.BS_27) / tabbYoY.BS_27,
										 Field42 = tabbYoY.BS_28 == 0M ? null : 100 * (tabb.BS_28 - tabbYoY.BS_28) / tabbYoY.BS_28,
										 Field43 = tabbYoY.BS_29 == 0M ? null : 100 * (tabb.BS_29 - tabbYoY.BS_29) / tabbYoY.BS_29,
										 Field44 = tabbYoY.BS_30 == 0M ? null : 100 * (tabb.BS_30 - tabbYoY.BS_30) / tabbYoY.BS_30,
										 Field45 = tabbYoY.BS_31 == 0M ? null : 100 * (tabb.BS_31 - tabbYoY.BS_31) / tabbYoY.BS_31,
										 Field46 = tabbYoY.BS_3101 == 0M ? null : 100 * (tabb.BS_3101 - tabbYoY.BS_3101) / tabbYoY.BS_3101,
										 Field47 = tabbYoY.BS_32 == 0M ? null : 100 * (tabb.BS_32 - tabbYoY.BS_32) / tabbYoY.BS_32,
										 Field48 = tabbYoY.BS_33 == 0M ? null : 100 * (tabb.BS_33 - tabbYoY.BS_33) / tabbYoY.BS_33,
										 Field49 = tabbYoY.BS_34 == 0M ? null : 100 * (tabb.BS_34 - tabbYoY.BS_34) / tabbYoY.BS_34,
										 Field50 = tabbYoY.BS_35 == 0M ? null : 100 * (tabb.BS_35 - tabbYoY.BS_35) / tabbYoY.BS_35,
										 Field51 = tabbYoY.BS_36 == 0M ? null : 100 * (tabb.BS_36 - tabbYoY.BS_36) / tabbYoY.BS_36,
										 Field52 = tabbYoY.BS_50 == 0M ? null : 100 * (tabb.BS_50 - tabbYoY.BS_50) / tabbYoY.BS_50,
										 Field53 = tabbYoY.BS_51 == 0M ? null : 100 * (tabb.BS_51 - tabbYoY.BS_51) / tabbYoY.BS_51,
										 Field54 = tabbYoY.BS_52 == 0M ? null : 100 * (tabb.BS_52 - tabbYoY.BS_52) / tabbYoY.BS_52,
										 Field55 = tabbYoY.BS_53 == 0M ? null : 100 * (tabb.BS_53 - tabbYoY.BS_53) / tabbYoY.BS_53,
										 Field56 = tabbYoY.BS_54 == 0M ? null : 100 * (tabb.BS_54 - tabbYoY.BS_54) / tabbYoY.BS_54,
										 Field57 = tabbYoY.BS_55 == 0M ? null : 100 * (tabb.BS_55 - tabbYoY.BS_55) / tabbYoY.BS_55,
										 Field58 = tabbYoY.BS_37 == 0M ? null : 100 * (tabb.BS_37 - tabbYoY.BS_37) / tabbYoY.BS_37,
										 Field59 = tabbYoY.BS_38 == 0M ? null : 100 * (tabb.BS_38 - tabbYoY.BS_38) / tabbYoY.BS_38,
										 Field60 = tabbYoY.BS_56 == 0M ? null : 100 * (tabb.BS_56 - tabbYoY.BS_56) / tabbYoY.BS_56,

										 Field61 = tabbYoY.BS_57 == 0M ? null : 100 * (tabb.BS_57 - tabbYoY.BS_57) / tabbYoY.BS_57,
										 Field62 = tabbYoY.BS_5701 == 0M ? null : 100 * (tabb.BS_5701 - tabbYoY.BS_5701) / tabbYoY.BS_5701,
										 Field63 = tabbYoY.BS_58 == 0M ? null : 100 * (tabb.BS_58 - tabbYoY.BS_58) / tabbYoY.BS_58,
										 Field64 = tabbYoY.BS_59 == 0M ? null : 100 * (tabb.BS_59 - tabbYoY.BS_59) / tabbYoY.BS_59,
										 Field65 = tabbYoY.BS_60 == 0M ? null : 100 * (tabb.BS_60 - tabbYoY.BS_60) / tabbYoY.BS_60,
										 Field66 = tabbYoY.BS_61 == 0M ? null : 100 * (tabb.BS_61 - tabbYoY.BS_61) / tabbYoY.BS_61,
										 Field67 = tabbYoY.BS_62 == 0M ? null : 100 * (tabb.BS_62 - tabbYoY.BS_62) / tabbYoY.BS_62,
										 Field68 = tabbYoY.BS_63 == 0M ? null : 100 * (tabb.BS_63 - tabbYoY.BS_63) / tabbYoY.BS_63,
										 Field69 = tabbYoY.BS_64 == 0M ? null : 100 * (tabb.BS_64 - tabbYoY.BS_64) / tabbYoY.BS_64,
										 Field70 = tabbYoY.BS_65 == 0M ? null : 100 * (tabb.BS_65 - tabbYoY.BS_65) / tabbYoY.BS_65,
										 Field71 = tabbYoY.BS_66 == 0M ? null : 100 * (tabb.BS_66 - tabbYoY.BS_66) / tabbYoY.BS_66,
										 Field72 = tabbYoY.BS_67 == 0M ? null : 100 * (tabb.BS_67 - tabbYoY.BS_67) / tabbYoY.BS_67,
										 Field73 = tabbYoY.BS_68 == 0M ? null : 100 * (tabb.BS_68 - tabbYoY.BS_68) / tabbYoY.BS_68,
										 Field74 = tabbYoY.BS_69 == 0M ? null : 100 * (tabb.BS_69 - tabbYoY.BS_69) / tabbYoY.BS_69,
										 Field75 = tabbYoY.BS_70 == 0M ? null : 100 * (tabb.BS_70 - tabbYoY.BS_70) / tabbYoY.BS_70,
										 Field76 = tabbYoY.BS_71 == 0M ? null : 100 * (tabb.BS_71 - tabbYoY.BS_71) / tabbYoY.BS_71,
										 Field77 = tabbYoY.BS_72 == 0M ? null : 100 * (tabb.BS_72 - tabbYoY.BS_72) / tabbYoY.BS_72,
										 Field78 = tabbYoY.BS_73 == 0M ? null : 100 * (tabb.BS_73 - tabbYoY.BS_73) / tabbYoY.BS_73,
										 Field79 = tabbYoY.BS_74 == 0M ? null : 100 * (tabb.BS_74 - tabbYoY.BS_74) / tabbYoY.BS_74,
										 Field80 = tabbYoY.BS_75 == 0M ? null : 100 * (tabb.BS_75 - tabbYoY.BS_75) / tabbYoY.BS_75,
										 Field81 = tabbYoY.BS_76 == 0M ? null : 100 * (tabb.BS_76 - tabbYoY.BS_76) / tabbYoY.BS_76,
										 Field82 = tabbYoY.BS_77 == 0M ? null : 100 * (tabb.BS_77 - tabbYoY.BS_77) / tabbYoY.BS_77,
										 Field83 = tabbYoY.BS_78 == 0M ? null : 100 * (tabb.BS_78 - tabbYoY.BS_78) / tabbYoY.BS_78,
										 Field84 = tabbYoY.BS_79 == 0M ? null : 100 * (tabb.BS_79 - tabbYoY.BS_79) / tabbYoY.BS_79,
										 Field85 = tabbYoY.BS_91 == 0M ? null : 100 * (tabb.BS_91 - tabbYoY.BS_91) / tabbYoY.BS_91,
										 Field86 = tabbYoY.BS_92 == 0M ? null : 100 * (tabb.BS_92 - tabbYoY.BS_92) / tabbYoY.BS_92,
										 Field87 = tabbYoY.BS_93 == 0M ? null : 100 * (tabb.BS_93 - tabbYoY.BS_93) / tabbYoY.BS_93,
										 Field88 = tabbYoY.BS_94 == 0M ? null : 100 * (tabb.BS_94 - tabbYoY.BS_94) / tabbYoY.BS_94,
										 Field89 = tabbYoY.BS_95 == 0M ? null : 100 * (tabb.BS_95 - tabbYoY.BS_95) / tabbYoY.BS_95,
										 Field90 = tabbYoY.BS_96 == 0M ? null : 100 * (tabb.BS_96 - tabbYoY.BS_96) / tabbYoY.BS_96,
										 Field91 = tabbYoY.BS_97 == 0M ? null : 100 * (tabb.BS_97 - tabbYoY.BS_97) / tabbYoY.BS_97,
										 Field92 = tabbYoY.BS_98 == 0M ? null : 100 * (tabb.BS_98 - tabbYoY.BS_98) / tabbYoY.BS_98,
										 Field93 = tabbYoY.BS_99 == 0M ? null : 100 * (tabb.BS_99 - tabbYoY.BS_99) / tabbYoY.BS_99,
										 Field94 = tabbYoY.BS_80 == 0M ? null : 100 * (tabb.BS_80 - tabbYoY.BS_80) / tabbYoY.BS_80,
										 Field95 = tabbYoY.BS_81 == 0M ? null : 100 * (tabb.BS_81 - tabbYoY.BS_81) / tabbYoY.BS_81,

										 Field96 = tabbYoY.BS_82 == 0M ? null : 100 * (tabb.BS_82 - tabbYoY.BS_82) / tabbYoY.BS_82,
										 Field97 = tabbYoY.BS_83 == 0M ? null : 100 * (tabb.BS_83 - tabbYoY.BS_83) / tabbYoY.BS_83,
										 Field98 = tabbYoY.BS_84 == 0M ? null : 100 * (tabb.BS_84 - tabbYoY.BS_84) / tabbYoY.BS_84,
										 Field99 = tabbYoY.BS_85 == 0M ? null : 100 * (tabb.BS_85 - tabbYoY.BS_85) / tabbYoY.BS_85,
										 Field100 = tabbYoY.BS_86 == 0M ? null : 100 * (tabb.BS_86 - tabbYoY.BS_86) / tabbYoY.BS_86,
										 Field101 = tabbYoY.BS_87 == 0M ? null : 100 * (tabb.BS_87 - tabbYoY.BS_87) / tabbYoY.BS_87,
										 Field102 = tabbYoY.BS_88 == 0M ? null : 100 * (tabb.BS_88 - tabbYoY.BS_88) / tabbYoY.BS_88,
										 Field103 = tabbYoY.BS_100 == 0M ? null : 100 * (tabb.BS_100 - tabbYoY.BS_100) / tabbYoY.BS_100,
										 Field104 = tabbYoY.BS_101 == 0M ? null : 100 * (tabb.BS_101 - tabbYoY.BS_101) / tabbYoY.BS_101,
										 Field105 = tabbYoY.BS_102 == 0M ? null : 100 * (tabb.BS_102 - tabbYoY.BS_102) / tabbYoY.BS_102,
										 Field106 = tabbYoY.BS_103 == 0M ? null : 100 * (tabb.BS_103 - tabbYoY.BS_103) / tabbYoY.BS_103,
										 Field107 = tabbYoY.BS_104 == 0M ? null : 100 * (tabb.BS_104 - tabbYoY.BS_104) / tabbYoY.BS_104,
										 Field108 = tabbYoY.BS_105 == 0M ? null : 100 * (tabb.BS_105 - tabbYoY.BS_105) / tabbYoY.BS_105,
										 Field109 = tabbYoY.BS_89 == 0M ? null : 100 * (tabb.BS_89 - tabbYoY.BS_89) / tabbYoY.BS_89,
										 Field110 = tabbYoY.BS_90 == 0M ? null : 100 * (tabb.BS_90 - tabbYoY.BS_90) / tabbYoY.BS_90,
										 Field111 = tabbYoY.BS_106 == 0M ? null : 100 * (tabb.BS_56 - tabbYoY.BS_106) / tabbYoY.BS_106,

										 Field112 = tabbYoY.BS_107 == 0M ? null : 100 * (tabb.BS_107 - tabbYoY.BS_107) / tabbYoY.BS_107,
										 Field113 = tabbYoY.BS_108 == 0M ? null : 100 * (tabb.BS_108 - tabbYoY.BS_108) / tabbYoY.BS_108,
										 Field114 = tabbYoY.BS_109 == 0M ? null : 100 * (tabb.BS_109 - tabbYoY.BS_109) / tabbYoY.BS_109,
										 Field115 = tabbYoY.BS_110 == 0M ? null : 100 * (tabb.BS_110 - tabbYoY.BS_110) / tabbYoY.BS_110,
										 Field116 = tabbYoY.BS_111 == 0M ? null : 100 * (tabb.BS_111 - tabbYoY.BS_111) / tabbYoY.BS_111,
										 Field117 = tabbYoY.BS_112 == 0M ? null : 100 * (tabb.BS_112 - tabbYoY.BS_112) / tabbYoY.BS_112,
										 Field118 = tabbYoY.BS_113 == 0M ? null : 100 * (tabb.BS_113 - tabbYoY.BS_113) / tabbYoY.BS_113,
										 Field119 = tabbYoY.BS_114 == 0M ? null : 100 * (tabb.BS_114 - tabbYoY.BS_114) / tabbYoY.BS_114,
										 Field120 = tabbYoY.BS_115 == 0M ? null : 100 * (tabb.BS_115 - tabbYoY.BS_115) / tabbYoY.BS_115,
										 Field121 = tabbYoY.BS_116 == 0M ? null : 100 * (tabb.BS_116 - tabbYoY.BS_116) / tabbYoY.BS_116,
										 Field122 = tabbYoY.BS_117 == 0M ? null : 100 * (tabb.BS_117 - tabbYoY.BS_117) / tabbYoY.BS_117,
										 Field123 = tabbYoY.BS_118 == 0M ? null : 100 * (tabb.BS_118 - tabbYoY.BS_118) / tabbYoY.BS_118,
										 Field124 = tabbYoY.BS_119 == 0M ? null : 100 * (tabb.BS_56 - tabbYoY.BS_119) / tabbYoY.BS_119,
										 Field125 = tabbYoY.BS_120 == 0M ? null : 100 * (tabb.BS_120 - tabbYoY.BS_120) / tabbYoY.BS_120,

										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "QoQ":
						var fundamental = ZCXDB.BOND_FIN_BALA_TABB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(f => f.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault()); //取最新的更新的记录

						var fundamentalQoQ = ZCXDB.BOND_FIN_BALA_TABB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(f => f.ISVALID == 1).ToList()
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault());

						issuerTABB = from tabb in fundamental.ToList()
									 join tabbOfLastQuarter in fundamentalQoQ.ToList()
									 on GetLastQuarter(tabb.END_DATE) equals tabbOfLastQuarter.END_DATE.Value.ToString("yyyy-MM") into QoQ
									 from tabbQoQ in QoQ
									 select new IssuerFundamental
									 {
										 Field1 = tabbQoQ.BS_01 == 0M ? null : 100 * (tabb.BS_01 - tabbQoQ.BS_01) / tabbQoQ.BS_01,
										 Field2 = tabbQoQ.BS_0101 == 0M ? null : 100 * (tabb.BS_0101 - tabbQoQ.BS_0101) / tabbQoQ.BS_0101,
										 Field3 = tabbQoQ.BS_02 == 0M ? null : 100 * (tabb.BS_02 - tabbQoQ.BS_02) / tabbQoQ.BS_02,
										 Field4 = tabbQoQ.BS_03 == 0M ? null : 100 * (tabb.BS_03 - tabbQoQ.BS_03) / tabbQoQ.BS_03,
										 Field5 = tabbQoQ.BS_04 == 0M ? null : 100 * (tabb.BS_04 - tabbQoQ.BS_04) / tabbQoQ.BS_04,
										 Field6 = tabbQoQ.BS_05 == 0M ? null : 100 * (tabb.BS_05 - tabbQoQ.BS_05) / tabbQoQ.BS_05,
										 Field7 = tabbQoQ.BS_06 == 0M ? null : 100 * (tabb.BS_06 - tabbQoQ.BS_06) / tabbQoQ.BS_06,
										 Field8 = tabbQoQ.BS_07 == 0M ? null : 100 * (tabb.BS_07 - tabbQoQ.BS_07) / tabbQoQ.BS_07,
										 Field9 = tabbQoQ.BS_08 == 0M ? null : 100 * (tabb.BS_08 - tabbQoQ.BS_08) / tabbQoQ.BS_08,
										 Field10 = tabbQoQ.BS_09 == 0M ? null : 100 * (tabb.BS_09 - tabbQoQ.BS_09) / tabbQoQ.BS_09,
										 Field11 = tabbQoQ.BS_0901 == 0M ? null : 100 * (tabb.BS_0901 - tabbQoQ.BS_0901) / tabbQoQ.BS_0901,
										 Field12 = tabbQoQ.BS_10 == 0M ? null : 100 * (tabb.BS_10 - tabbQoQ.BS_10) / tabbQoQ.BS_10,
										 Field13 = tabbQoQ.BS_11 == 0M ? null : 100 * (tabb.BS_11 - tabbQoQ.BS_11) / tabbQoQ.BS_11,
										 Field14 = tabbQoQ.BS_12 == 0M ? null : 100 * (tabb.BS_12 - tabbQoQ.BS_12) / tabbQoQ.BS_12,
										 Field15 = tabbQoQ.BS_13 == 0M ? null : 100 * (tabb.BS_13 - tabbQoQ.BS_13) / tabbQoQ.BS_13,
										 Field16 = tabbQoQ.BS_14 == 0M ? null : 100 * (tabb.BS_14 - tabbQoQ.BS_14) / tabbQoQ.BS_14,
										 Field17 = tabbQoQ.BS_15 == 0M ? null : 100 * (tabb.BS_15 - tabbQoQ.BS_15) / tabbQoQ.BS_15,
										 Field18 = tabbQoQ.BS_16 == 0M ? null : 100 * (tabb.BS_16 - tabbQoQ.BS_16) / tabbQoQ.BS_16,
										 Field19 = tabbQoQ.BS_17 == 0M ? null : 100 * (tabb.BS_17 - tabbQoQ.BS_17) / tabbQoQ.BS_17,
										 Field20 = tabbQoQ.BS_39 == 0M ? null : 100 * (tabb.BS_39 - tabbQoQ.BS_39) / tabbQoQ.BS_39,
										 Field21 = tabbQoQ.BS_40 == 0M ? null : 100 * (tabb.BS_40 - tabbQoQ.BS_40) / tabbQoQ.BS_40,
										 Field22 = tabbQoQ.BS_4001 == 0M ? null : 100 * (tabb.BS_4001 - tabbQoQ.BS_4001) / tabbQoQ.BS_4001,
										 Field23 = tabbQoQ.BS_41 == 0M ? null : 100 * (tabb.BS_41 - tabbQoQ.BS_41) / tabbQoQ.BS_41,
										 Field24 = tabbQoQ.BS_42 == 0M ? null : 100 * (tabb.BS_42 - tabbQoQ.BS_42) / tabbQoQ.BS_42,
										 Field25 = tabbQoQ.BS_43 == 0M ? null : 100 * (tabb.BS_43 - tabbQoQ.BS_43) / tabbQoQ.BS_43,
										 Field26 = tabbQoQ.BS_44 == 0M ? null : 100 * (tabb.BS_44 - tabbQoQ.BS_44) / tabbQoQ.BS_44,
										 Field27 = tabbQoQ.BS_45 == 0M ? null : 100 * (tabb.BS_45 - tabbQoQ.BS_45) / tabbQoQ.BS_45,
										 Field28 = tabbQoQ.BS_46 == 0M ? null : 100 * (tabb.BS_46 - tabbQoQ.BS_46) / tabbQoQ.BS_46,
										 Field29 = tabbQoQ.BS_47 == 0M ? null : 100 * (tabb.BS_47 - tabbQoQ.BS_47) / tabbQoQ.BS_47,
										 Field30 = tabbQoQ.BS_48 == 0M ? null : 100 * (tabb.BS_48 - tabbQoQ.BS_48) / tabbQoQ.BS_48,
										 Field31 = tabbQoQ.BS_49 == 0M ? null : 100 * (tabb.BS_49 - tabbQoQ.BS_49) / tabbQoQ.BS_49,
										 Field32 = tabbQoQ.BS_18 == 0M ? null : 100 * (tabb.BS_18 - tabbQoQ.BS_18) / tabbQoQ.BS_18,
										 Field33 = tabbQoQ.BS_19 == 0M ? null : 100 * (tabb.BS_19 - tabbQoQ.BS_19) / tabbQoQ.BS_19,

										 Field34 = tabbQoQ.BS_20 == 0M ? null : 100 * (tabb.BS_20 - tabbQoQ.BS_20) / tabbQoQ.BS_20,
										 Field35 = tabbQoQ.BS_21 == 0M ? null : 100 * (tabb.BS_21 - tabbQoQ.BS_21) / tabbQoQ.BS_21,
										 Field36 = tabbQoQ.BS_22 == 0M ? null : 100 * (tabb.BS_22 - tabbQoQ.BS_22) / tabbQoQ.BS_22,
										 Field37 = tabbQoQ.BS_23 == 0M ? null : 100 * (tabb.BS_23 - tabbQoQ.BS_23) / tabbQoQ.BS_23,
										 Field38 = tabbQoQ.BS_24 == 0M ? null : 100 * (tabb.BS_24 - tabbQoQ.BS_24) / tabbQoQ.BS_24,
										 Field39 = tabbQoQ.BS_25 == 0M ? null : 100 * (tabb.BS_25 - tabbQoQ.BS_25) / tabbQoQ.BS_25,
										 Field40 = tabbQoQ.BS_26 == 0M ? null : 100 * (tabb.BS_26 - tabbQoQ.BS_26) / tabbQoQ.BS_26,
										 Field41 = tabbQoQ.BS_27 == 0M ? null : 100 * (tabb.BS_27 - tabbQoQ.BS_27) / tabbQoQ.BS_27,
										 Field42 = tabbQoQ.BS_28 == 0M ? null : 100 * (tabb.BS_28 - tabbQoQ.BS_28) / tabbQoQ.BS_28,
										 Field43 = tabbQoQ.BS_29 == 0M ? null : 100 * (tabb.BS_29 - tabbQoQ.BS_29) / tabbQoQ.BS_29,
										 Field44 = tabbQoQ.BS_30 == 0M ? null : 100 * (tabb.BS_30 - tabbQoQ.BS_30) / tabbQoQ.BS_30,
										 Field45 = tabbQoQ.BS_31 == 0M ? null : 100 * (tabb.BS_31 - tabbQoQ.BS_31) / tabbQoQ.BS_31,
										 Field46 = tabbQoQ.BS_3101 == 0M ? null : 100 * (tabb.BS_3101 - tabbQoQ.BS_3101) / tabbQoQ.BS_3101,
										 Field47 = tabbQoQ.BS_32 == 0M ? null : 100 * (tabb.BS_32 - tabbQoQ.BS_32) / tabbQoQ.BS_32,
										 Field48 = tabbQoQ.BS_33 == 0M ? null : 100 * (tabb.BS_33 - tabbQoQ.BS_33) / tabbQoQ.BS_33,
										 Field49 = tabbQoQ.BS_34 == 0M ? null : 100 * (tabb.BS_34 - tabbQoQ.BS_34) / tabbQoQ.BS_34,
										 Field50 = tabbQoQ.BS_35 == 0M ? null : 100 * (tabb.BS_35 - tabbQoQ.BS_35) / tabbQoQ.BS_35,
										 Field51 = tabbQoQ.BS_36 == 0M ? null : 100 * (tabb.BS_36 - tabbQoQ.BS_36) / tabbQoQ.BS_36,
										 Field52 = tabbQoQ.BS_50 == 0M ? null : 100 * (tabb.BS_50 - tabbQoQ.BS_50) / tabbQoQ.BS_50,
										 Field53 = tabbQoQ.BS_51 == 0M ? null : 100 * (tabb.BS_51 - tabbQoQ.BS_51) / tabbQoQ.BS_51,
										 Field54 = tabbQoQ.BS_52 == 0M ? null : 100 * (tabb.BS_52 - tabbQoQ.BS_52) / tabbQoQ.BS_52,
										 Field55 = tabbQoQ.BS_53 == 0M ? null : 100 * (tabb.BS_53 - tabbQoQ.BS_53) / tabbQoQ.BS_53,
										 Field56 = tabbQoQ.BS_54 == 0M ? null : 100 * (tabb.BS_54 - tabbQoQ.BS_54) / tabbQoQ.BS_54,
										 Field57 = tabbQoQ.BS_55 == 0M ? null : 100 * (tabb.BS_55 - tabbQoQ.BS_55) / tabbQoQ.BS_55,
										 Field58 = tabbQoQ.BS_37 == 0M ? null : 100 * (tabb.BS_37 - tabbQoQ.BS_37) / tabbQoQ.BS_37,
										 Field59 = tabbQoQ.BS_38 == 0M ? null : 100 * (tabb.BS_38 - tabbQoQ.BS_38) / tabbQoQ.BS_38,
										 Field60 = tabbQoQ.BS_56 == 0M ? null : 100 * (tabb.BS_56 - tabbQoQ.BS_56) / tabbQoQ.BS_56,

										 Field61 = tabbQoQ.BS_57 == 0M ? null : 100 * (tabb.BS_57 - tabbQoQ.BS_57) / tabbQoQ.BS_57,
										 Field62 = tabbQoQ.BS_5701 == 0M ? null : 100 * (tabb.BS_5701 - tabbQoQ.BS_5701) / tabbQoQ.BS_5701,
										 Field63 = tabbQoQ.BS_58 == 0M ? null : 100 * (tabb.BS_58 - tabbQoQ.BS_58) / tabbQoQ.BS_58,
										 Field64 = tabbQoQ.BS_59 == 0M ? null : 100 * (tabb.BS_59 - tabbQoQ.BS_59) / tabbQoQ.BS_59,
										 Field65 = tabbQoQ.BS_60 == 0M ? null : 100 * (tabb.BS_60 - tabbQoQ.BS_60) / tabbQoQ.BS_60,
										 Field66 = tabbQoQ.BS_61 == 0M ? null : 100 * (tabb.BS_61 - tabbQoQ.BS_61) / tabbQoQ.BS_61,
										 Field67 = tabbQoQ.BS_62 == 0M ? null : 100 * (tabb.BS_62 - tabbQoQ.BS_62) / tabbQoQ.BS_62,
										 Field68 = tabbQoQ.BS_63 == 0M ? null : 100 * (tabb.BS_63 - tabbQoQ.BS_63) / tabbQoQ.BS_63,
										 Field69 = tabbQoQ.BS_64 == 0M ? null : 100 * (tabb.BS_64 - tabbQoQ.BS_64) / tabbQoQ.BS_64,
										 Field70 = tabbQoQ.BS_65 == 0M ? null : 100 * (tabb.BS_65 - tabbQoQ.BS_65) / tabbQoQ.BS_65,
										 Field71 = tabbQoQ.BS_66 == 0M ? null : 100 * (tabb.BS_66 - tabbQoQ.BS_66) / tabbQoQ.BS_66,
										 Field72 = tabbQoQ.BS_67 == 0M ? null : 100 * (tabb.BS_67 - tabbQoQ.BS_67) / tabbQoQ.BS_67,
										 Field73 = tabbQoQ.BS_68 == 0M ? null : 100 * (tabb.BS_68 - tabbQoQ.BS_68) / tabbQoQ.BS_68,
										 Field74 = tabbQoQ.BS_69 == 0M ? null : 100 * (tabb.BS_69 - tabbQoQ.BS_69) / tabbQoQ.BS_69,
										 Field75 = tabbQoQ.BS_70 == 0M ? null : 100 * (tabb.BS_70 - tabbQoQ.BS_70) / tabbQoQ.BS_70,
										 Field76 = tabbQoQ.BS_71 == 0M ? null : 100 * (tabb.BS_71 - tabbQoQ.BS_71) / tabbQoQ.BS_71,
										 Field77 = tabbQoQ.BS_72 == 0M ? null : 100 * (tabb.BS_72 - tabbQoQ.BS_72) / tabbQoQ.BS_72,
										 Field78 = tabbQoQ.BS_73 == 0M ? null : 100 * (tabb.BS_73 - tabbQoQ.BS_73) / tabbQoQ.BS_73,
										 Field79 = tabbQoQ.BS_74 == 0M ? null : 100 * (tabb.BS_74 - tabbQoQ.BS_74) / tabbQoQ.BS_74,
										 Field80 = tabbQoQ.BS_75 == 0M ? null : 100 * (tabb.BS_75 - tabbQoQ.BS_75) / tabbQoQ.BS_75,
										 Field81 = tabbQoQ.BS_76 == 0M ? null : 100 * (tabb.BS_76 - tabbQoQ.BS_76) / tabbQoQ.BS_76,
										 Field82 = tabbQoQ.BS_77 == 0M ? null : 100 * (tabb.BS_77 - tabbQoQ.BS_77) / tabbQoQ.BS_77,
										 Field83 = tabbQoQ.BS_78 == 0M ? null : 100 * (tabb.BS_78 - tabbQoQ.BS_78) / tabbQoQ.BS_78,
										 Field84 = tabbQoQ.BS_79 == 0M ? null : 100 * (tabb.BS_79 - tabbQoQ.BS_79) / tabbQoQ.BS_79,
										 Field85 = tabbQoQ.BS_91 == 0M ? null : 100 * (tabb.BS_91 - tabbQoQ.BS_91) / tabbQoQ.BS_91,
										 Field86 = tabbQoQ.BS_92 == 0M ? null : 100 * (tabb.BS_92 - tabbQoQ.BS_92) / tabbQoQ.BS_92,
										 Field87 = tabbQoQ.BS_93 == 0M ? null : 100 * (tabb.BS_93 - tabbQoQ.BS_93) / tabbQoQ.BS_93,
										 Field88 = tabbQoQ.BS_94 == 0M ? null : 100 * (tabb.BS_94 - tabbQoQ.BS_94) / tabbQoQ.BS_94,
										 Field89 = tabbQoQ.BS_95 == 0M ? null : 100 * (tabb.BS_95 - tabbQoQ.BS_95) / tabbQoQ.BS_95,
										 Field90 = tabbQoQ.BS_96 == 0M ? null : 100 * (tabb.BS_96 - tabbQoQ.BS_96) / tabbQoQ.BS_96,
										 Field91 = tabbQoQ.BS_97 == 0M ? null : 100 * (tabb.BS_97 - tabbQoQ.BS_97) / tabbQoQ.BS_97,
										 Field92 = tabbQoQ.BS_98 == 0M ? null : 100 * (tabb.BS_98 - tabbQoQ.BS_98) / tabbQoQ.BS_98,
										 Field93 = tabbQoQ.BS_99 == 0M ? null : 100 * (tabb.BS_99 - tabbQoQ.BS_99) / tabbQoQ.BS_99,
										 Field94 = tabbQoQ.BS_80 == 0M ? null : 100 * (tabb.BS_80 - tabbQoQ.BS_80) / tabbQoQ.BS_80,
										 Field95 = tabbQoQ.BS_81 == 0M ? null : 100 * (tabb.BS_81 - tabbQoQ.BS_81) / tabbQoQ.BS_81,

										 Field96 = tabbQoQ.BS_82 == 0M ? null : 100 * (tabb.BS_82 - tabbQoQ.BS_82) / tabbQoQ.BS_82,
										 Field97 = tabbQoQ.BS_83 == 0M ? null : 100 * (tabb.BS_83 - tabbQoQ.BS_83) / tabbQoQ.BS_83,
										 Field98 = tabbQoQ.BS_84 == 0M ? null : 100 * (tabb.BS_84 - tabbQoQ.BS_84) / tabbQoQ.BS_84,
										 Field99 = tabbQoQ.BS_85 == 0M ? null : 100 * (tabb.BS_85 - tabbQoQ.BS_85) / tabbQoQ.BS_85,
										 Field100 = tabbQoQ.BS_86 == 0M ? null : 100 * (tabb.BS_86 - tabbQoQ.BS_86) / tabbQoQ.BS_86,
										 Field101 = tabbQoQ.BS_87 == 0M ? null : 100 * (tabb.BS_87 - tabbQoQ.BS_87) / tabbQoQ.BS_87,
										 Field102 = tabbQoQ.BS_88 == 0M ? null : 100 * (tabb.BS_88 - tabbQoQ.BS_88) / tabbQoQ.BS_88,
										 Field103 = tabbQoQ.BS_100 == 0M ? null : 100 * (tabb.BS_100 - tabbQoQ.BS_100) / tabbQoQ.BS_100,
										 Field104 = tabbQoQ.BS_101 == 0M ? null : 100 * (tabb.BS_101 - tabbQoQ.BS_101) / tabbQoQ.BS_101,
										 Field105 = tabbQoQ.BS_102 == 0M ? null : 100 * (tabb.BS_102 - tabbQoQ.BS_102) / tabbQoQ.BS_102,
										 Field106 = tabbQoQ.BS_103 == 0M ? null : 100 * (tabb.BS_103 - tabbQoQ.BS_103) / tabbQoQ.BS_103,
										 Field107 = tabbQoQ.BS_104 == 0M ? null : 100 * (tabb.BS_104 - tabbQoQ.BS_104) / tabbQoQ.BS_104,
										 Field108 = tabbQoQ.BS_105 == 0M ? null : 100 * (tabb.BS_105 - tabbQoQ.BS_105) / tabbQoQ.BS_105,
										 Field109 = tabbQoQ.BS_89 == 0M ? null : 100 * (tabb.BS_89 - tabbQoQ.BS_89) / tabbQoQ.BS_89,
										 Field110 = tabbQoQ.BS_90 == 0M ? null : 100 * (tabb.BS_90 - tabbQoQ.BS_90) / tabbQoQ.BS_90,
										 Field111 = tabbQoQ.BS_106 == 0M ? null : 100 * (tabb.BS_56 - tabbQoQ.BS_106) / tabbQoQ.BS_106,

										 Field112 = tabbQoQ.BS_107 == 0M ? null : 100 * (tabb.BS_107 - tabbQoQ.BS_107) / tabbQoQ.BS_107,
										 Field113 = tabbQoQ.BS_108 == 0M ? null : 100 * (tabb.BS_108 - tabbQoQ.BS_108) / tabbQoQ.BS_108,
										 Field114 = tabbQoQ.BS_109 == 0M ? null : 100 * (tabb.BS_109 - tabbQoQ.BS_109) / tabbQoQ.BS_109,
										 Field115 = tabbQoQ.BS_110 == 0M ? null : 100 * (tabb.BS_110 - tabbQoQ.BS_110) / tabbQoQ.BS_110,
										 Field116 = tabbQoQ.BS_111 == 0M ? null : 100 * (tabb.BS_111 - tabbQoQ.BS_111) / tabbQoQ.BS_111,
										 Field117 = tabbQoQ.BS_112 == 0M ? null : 100 * (tabb.BS_112 - tabbQoQ.BS_112) / tabbQoQ.BS_112,
										 Field118 = tabbQoQ.BS_113 == 0M ? null : 100 * (tabb.BS_113 - tabbQoQ.BS_113) / tabbQoQ.BS_113,
										 Field119 = tabbQoQ.BS_114 == 0M ? null : 100 * (tabb.BS_114 - tabbQoQ.BS_114) / tabbQoQ.BS_114,
										 Field120 = tabbQoQ.BS_115 == 0M ? null : 100 * (tabb.BS_115 - tabbQoQ.BS_115) / tabbQoQ.BS_115,
										 Field121 = tabbQoQ.BS_116 == 0M ? null : 100 * (tabb.BS_116 - tabbQoQ.BS_116) / tabbQoQ.BS_116,
										 Field122 = tabbQoQ.BS_117 == 0M ? null : 100 * (tabb.BS_117 - tabbQoQ.BS_117) / tabbQoQ.BS_117,
										 Field123 = tabbQoQ.BS_118 == 0M ? null : 100 * (tabb.BS_118 - tabbQoQ.BS_118) / tabbQoQ.BS_118,
										 Field124 = tabbQoQ.BS_119 == 0M ? null : 100 * (tabb.BS_56 - tabbQoQ.BS_119) / tabbQoQ.BS_119,
										 Field125 = tabbQoQ.BS_120 == 0M ? null : 100 * (tabb.BS_120 - tabbQoQ.BS_120) / tabbQoQ.BS_120,

										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					default:
						break;
				}

				return issuerTABB.ToList();
			}
		}

		public IEnumerable<IssuerFundamental> GetIssuerTACB(int companyId, string reportType, string contentType, int interval, string unit)
		{
			IEnumerable<IssuerFundamental> issuerTACB = null;

			using (var ZCXDB = new ZCXEntities())
			{
				Func<BOND_FIN_CASH_TACB, bool> dateFilter = null;
				Func<BOND_FIN_CASH_TACB, bool> reportTypeFilter = null;
				int multiplier = GetMultiPlier(unit); 

				if (reportType == "Y") //1:year report;
				{
					if (interval == 0)
						dateFilter = t => true;
					else
                        dateFilter = t => t.END_DATE >= new DateTime(DateTime.Now.Year - interval, 1, 1);

					reportTypeFilter = t => t.SHEET_ATTR_PAR == 1;
				}
				else
				{
					if (interval == 0)
						dateFilter = t => true;
					else
                        dateFilter = t => t.END_DATE >= new DateTime(DateTime.Now.Year - interval, DateTime.Now.Month - (DateTime.Now.Month - 1) % 3, 1);

					reportTypeFilter = t => (t.SHEET_ATTR_PAR == 3 || t.SHEET_ATTR_PAR == 2 || t.SHEET_ATTR_PAR == 6 || t.SHEET_ATTR_PAR == 1);
				}

				switch (contentType)
				{
					case "RawReport":
						issuerTACB = from tabb in (ZCXDB.BOND_FIN_CASH_TACB
									 .Where(b => b.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(b => b.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault())).ToList()
									 select new IssuerFundamental
									 {
										 Field2 = tabb.CF_01 / multiplier,
										 Field3 = tabb.CF_02 / multiplier,
										 Field4 = tabb.CF_03 / multiplier,
										 Field5 = tabb.CF_04 / multiplier,
										 Field6 = tabb.CF_05 / multiplier,
										 Field7 = tabb.CF_06 / multiplier,
										 Field8 = tabb.CF_07 / multiplier,
										 Field9 = tabb.CF_08 / multiplier,
										 Field10 = tabb.CF_09 / multiplier,
										 Field11 = tabb.CF_10 / multiplier,
										 Field12 = tabb.CF_11 / multiplier,
										 Field13 = tabb.CF_12 / multiplier,
										 Field14 = tabb.CF_13 / multiplier,
										 Field15 = tabb.CF_14 / multiplier,
										 Field16 = tabb.CF_15 / multiplier,
										 Field17 = tabb.CF_16 / multiplier,
										 Field18 = tabb.CF_17 / multiplier,
										 Field19 = tabb.CF_18 / multiplier,
										 Field20 = tabb.CF_19 / multiplier,
										 Field21 = tabb.CF_20 / multiplier,
										 Field22 = tabb.CF_21 / multiplier,
										 Field23 = tabb.CF_22 / multiplier,
										 Field24 = tabb.CF_23 / multiplier,
										 Field25 = tabb.CF_24 / multiplier,
										 Field26 = tabb.CF_25 / multiplier,
										 Field27 = tabb.CF_26 / multiplier,
										 Field28 = tabb.CF_27 / multiplier,
										 Field29 = tabb.CF_28 / multiplier,
										 Field31 = tabb.CF_29 / multiplier,
										 Field32 = tabb.CF_30 / multiplier,
										 Field33 = tabb.CF_31 / multiplier,
										 Field34 = tabb.CF_32 / multiplier,
										 Field35 = tabb.CF_33 / multiplier,
										 Field36 = tabb.CF_34 / multiplier,
										 Field37 = tabb.CF_35 / multiplier,
										 Field38 = tabb.CF_36 / multiplier,
										 Field39 = tabb.CF_37 / multiplier,
										 Field40 = tabb.CF_38 / multiplier,
										 Field41 = tabb.CF_39 / multiplier,
										 Field42 = tabb.CF_40 / multiplier,
										 Field43 = tabb.CF_41 / multiplier,
										 Field45 = tabb.CF_42 / multiplier,
										 Field46 = tabb.CF_4201 / multiplier,
										 Field47 = tabb.CF_43 / multiplier,
										 Field48 = tabb.CF_44 / multiplier,
										 Field49 = tabb.CF_45 / multiplier,
										 Field50 = tabb.CF_46 / multiplier,
										 Field51 = tabb.CF_47 / multiplier,
										 Field52 = tabb.CF_48 / multiplier,
										 Field53 = tabb.CF_49 / multiplier,
										 Field54 = tabb.CF_4901 / multiplier,
										 Field55 = tabb.CF_50 / multiplier,
										 Field56 = tabb.CF_51 / multiplier,
										 Field57 = tabb.CF_52 / multiplier,
										 Field59 = tabb.CF_53 / multiplier,
										 Field60 = tabb.CF_54 / multiplier,
										 Field61 = tabb.CF_55 / multiplier,
										 Field62 = tabb.CF_56 / multiplier,
										 Field64 = tabb.CF_57 / multiplier,
										 Field65 = tabb.CF_58 / multiplier,
										 Field66 = tabb.CF_59 / multiplier,
										 Field67 = tabb.CF_60 / multiplier,
										 Field68 = tabb.CF_61 / multiplier,
										 Field69 = tabb.CF_62 / multiplier,
										 Field70 = tabb.CF_63 / multiplier,
										 Field71 = tabb.CF_64 / multiplier,
										 Field72 = tabb.CF_65 / multiplier,
										 Field73 = tabb.CF_66 / multiplier,
										 Field74 = tabb.CF_67 / multiplier,
										 Field75 = tabb.CF_68 / multiplier,
										 Field76 = tabb.CF_69 / multiplier,
										 Field77 = tabb.CF_70 / multiplier,
										 Field78 = tabb.CF_71 / multiplier,
										 Field79 = tabb.CF_72 / multiplier,
										 Field80 = tabb.CF_73 / multiplier,
										 Field81 = tabb.CF_74 / multiplier,
										 Field82 = tabb.CF_75 / multiplier,
										 Field83 = tabb.CF_76 / multiplier,
										 Field84 = tabb.CF_77 / multiplier,
										 Field86 = tabb.CF_78 / multiplier,
										 Field87 = tabb.CF_79 / multiplier,
										 Field88 = tabb.CF_80 / multiplier,
										 Field90 = tabb.CF_81 / multiplier,
										 Field91 = tabb.CF_82 / multiplier,
										 Field92 = tabb.CF_83 / multiplier,
										 Field93 = tabb.CF_84 / multiplier,
										 Field94 = tabb.CF_85 / multiplier,
										 Field95 = tabb.CF_86 / multiplier,

                                         EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR, //0-其它；1-年报；2-中报；3-一季报；6-三季报
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "YoY":
						var funda = ZCXDB.BOND_FIN_CASH_TACB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(f => f.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault()); //取最新的更新的记录

						var fundamentalYoY = ZCXDB.BOND_FIN_CASH_TACB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(f => f.ISVALID == 1).ToList()
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault());

						issuerTACB = from tabb in funda.ToList()
									 join tabbOfLastYear in fundamentalYoY.ToList()
                                     on new { Year = tabb.END_DATE.Value.Year - 1, Month = tabb.END_DATE.Value.Month }
                                     equals new { Year = tabbOfLastYear.END_DATE.Value.Year, Month = tabbOfLastYear.END_DATE.Value.Month } into YoY
									 from tabbYoY in YoY
									 select new IssuerFundamental
									 {
										 Field2 = tabbYoY.CF_01 == 0M ? null : 100 * (tabb.CF_01 - tabbYoY.CF_01) / tabbYoY.CF_01,
										 Field3 = tabbYoY.CF_02 == 0M ? null : 100 * (tabb.CF_02 - tabbYoY.CF_02) / tabbYoY.CF_02,
										 Field4 = tabbYoY.CF_03 == 0M ? null : 100 * (tabb.CF_03 - tabbYoY.CF_03) / tabbYoY.CF_03,
										 Field5 = tabbYoY.CF_04 == 0M ? null : 100 * (tabb.CF_04 - tabbYoY.CF_04) / tabbYoY.CF_04,
										 Field6 = tabbYoY.CF_05 == 0M ? null : 100 * (tabb.CF_05 - tabbYoY.CF_05) / tabbYoY.CF_05,
										 Field7 = tabbYoY.CF_06 == 0M ? null : 100 * (tabb.CF_06 - tabbYoY.CF_06) / tabbYoY.CF_06,
										 Field8 = tabbYoY.CF_07 == 0M ? null : 100 * (tabb.CF_07 - tabbYoY.CF_07) / tabbYoY.CF_07,
										 Field9 = tabbYoY.CF_08 == 0M ? null : 100 * (tabb.CF_08 - tabbYoY.CF_08) / tabbYoY.CF_08,
										 Field10 = tabbYoY.CF_09 == 0M ? null : 100 * (tabb.CF_09 - tabbYoY.CF_09) / tabbYoY.CF_09,
										 Field11 = tabbYoY.CF_10 == 0M ? null : 100 * (tabb.CF_10 - tabbYoY.CF_10) / tabbYoY.CF_10,
										 Field12 = tabbYoY.CF_11 == 0M ? null : 100 * (tabb.CF_11 - tabbYoY.CF_11) / tabbYoY.CF_11,
										 Field13 = tabbYoY.CF_12 == 0M ? null : 100 * (tabb.CF_12 - tabbYoY.CF_12) / tabbYoY.CF_12,
										 Field14 = tabbYoY.CF_13 == 0M ? null : 100 * (tabb.CF_13 - tabbYoY.CF_13) / tabbYoY.CF_13,
										 Field15 = tabbYoY.CF_14 == 0M ? null : 100 * (tabb.CF_14 - tabbYoY.CF_14) / tabbYoY.CF_14,
										 Field16 = tabbYoY.CF_15 == 0M ? null : 100 * (tabb.CF_15 - tabbYoY.CF_15) / tabbYoY.CF_15,
										 Field17 = tabbYoY.CF_16 == 0M ? null : 100 * (tabb.CF_16 - tabbYoY.CF_16) / tabbYoY.CF_16,
										 Field18 = tabbYoY.CF_17 == 0M ? null : 100 * (tabb.CF_17 - tabbYoY.CF_17) / tabbYoY.CF_17,
										 Field19 = tabbYoY.CF_18 == 0M ? null : 100 * (tabb.CF_18 - tabbYoY.CF_18) / tabbYoY.CF_18,
										 Field20 = tabbYoY.CF_19 == 0M ? null : 100 * (tabb.CF_19 - tabbYoY.CF_19) / tabbYoY.CF_19,
										 Field21 = tabbYoY.CF_20 == 0M ? null : 100 * (tabb.CF_20 - tabbYoY.CF_20) / tabbYoY.CF_20,
										 Field22 = tabbYoY.CF_21 == 0M ? null : 100 * (tabb.CF_21 - tabbYoY.CF_21) / tabbYoY.CF_21,
										 Field23 = tabbYoY.CF_22 == 0M ? null : 100 * (tabb.CF_22 - tabbYoY.CF_22) / tabbYoY.CF_22,
										 Field24 = tabbYoY.CF_23 == 0M ? null : 100 * (tabb.CF_23 - tabbYoY.CF_23) / tabbYoY.CF_23,
										 Field25 = tabbYoY.CF_24 == 0M ? null : 100 * (tabb.CF_24 - tabbYoY.CF_24) / tabbYoY.CF_24,
										 Field26 = tabbYoY.CF_25 == 0M ? null : 100 * (tabb.CF_25 - tabbYoY.CF_25) / tabbYoY.CF_25,
										 Field27 = tabbYoY.CF_26 == 0M ? null : 100 * (tabb.CF_26 - tabbYoY.CF_26) / tabbYoY.CF_26,
										 Field28 = tabbYoY.CF_27 == 0M ? null : 100 * (tabb.CF_27 - tabbYoY.CF_27) / tabbYoY.CF_27,
										 Field29 = tabbYoY.CF_28 == 0M ? null : 100 * (tabb.CF_28 - tabbYoY.CF_28) / tabbYoY.CF_28,
										 Field31 = tabbYoY.CF_29 == 0M ? null : 100 * (tabb.CF_29 - tabbYoY.CF_29) / tabbYoY.CF_29,
										 Field32 = tabbYoY.CF_30 == 0M ? null : 100 * (tabb.CF_30 - tabbYoY.CF_30) / tabbYoY.CF_30,
										 Field33 = tabbYoY.CF_31 == 0M ? null : 100 * (tabb.CF_31 - tabbYoY.CF_31) / tabbYoY.CF_31,
										 Field34 = tabbYoY.CF_32 == 0M ? null : 100 * (tabb.CF_32 - tabbYoY.CF_32) / tabbYoY.CF_32,
										 Field35 = tabbYoY.CF_33 == 0M ? null : 100 * (tabb.CF_33 - tabbYoY.CF_33) / tabbYoY.CF_33,
										 Field36 = tabbYoY.CF_34 == 0M ? null : 100 * (tabb.CF_34 - tabbYoY.CF_34) / tabbYoY.CF_34,
										 Field37 = tabbYoY.CF_35 == 0M ? null : 100 * (tabb.CF_35 - tabbYoY.CF_35) / tabbYoY.CF_35,
										 Field38 = tabbYoY.CF_36 == 0M ? null : 100 * (tabb.CF_36 - tabbYoY.CF_36) / tabbYoY.CF_36,
										 Field39 = tabbYoY.CF_37 == 0M ? null : 100 * (tabb.CF_37 - tabbYoY.CF_37) / tabbYoY.CF_37,
										 Field40 = tabbYoY.CF_38 == 0M ? null : 100 * (tabb.CF_38 - tabbYoY.CF_38) / tabbYoY.CF_38,
										 Field41 = tabbYoY.CF_39 == 0M ? null : 100 * (tabb.CF_39 - tabbYoY.CF_39) / tabbYoY.CF_39,
										 Field42 = tabbYoY.CF_40 == 0M ? null : 100 * (tabb.CF_40 - tabbYoY.CF_40) / tabbYoY.CF_40,
										 Field43 = tabbYoY.CF_41 == 0M ? null : 100 * (tabb.CF_41 - tabbYoY.CF_41) / tabbYoY.CF_41,
										 Field45 = tabbYoY.CF_42 == 0M ? null : 100 * (tabb.CF_42 - tabbYoY.CF_42) / tabbYoY.CF_42,
										 Field46 = tabbYoY.CF_4201 == 0M ? null : 100 * (tabb.CF_4201 - tabbYoY.CF_4201) / tabbYoY.CF_4201,
										 Field47 = tabbYoY.CF_43 == 0M ? null : 100 * (tabb.CF_43 - tabbYoY.CF_43) / tabbYoY.CF_43,
										 Field48 = tabbYoY.CF_44 == 0M ? null : 100 * (tabb.CF_44 - tabbYoY.CF_44) / tabbYoY.CF_44,
										 Field49 = tabbYoY.CF_45 == 0M ? null : 100 * (tabb.CF_45 - tabbYoY.CF_45) / tabbYoY.CF_45,
										 Field50 = tabbYoY.CF_46 == 0M ? null : 100 * (tabb.CF_46 - tabbYoY.CF_46) / tabbYoY.CF_46,
										 Field51 = tabbYoY.CF_47 == 0M ? null : 100 * (tabb.CF_47 - tabbYoY.CF_47) / tabbYoY.CF_47,
										 Field52 = tabbYoY.CF_48 == 0M ? null : 100 * (tabb.CF_48 - tabbYoY.CF_48) / tabbYoY.CF_48,
										 Field53 = tabbYoY.CF_49 == 0M ? null : 100 * (tabb.CF_49 - tabbYoY.CF_49) / tabbYoY.CF_49,
										 Field54 = tabbYoY.CF_4901 == 0M ? null : 100 * (tabb.CF_4901 - tabbYoY.CF_4901) / tabbYoY.CF_4901,
										 Field55 = tabbYoY.CF_50 == 0M ? null : 100 * (tabb.CF_50 - tabbYoY.CF_50) / tabbYoY.CF_50,
										 Field56 = tabbYoY.CF_51 == 0M ? null : 100 * (tabb.CF_51 - tabbYoY.CF_51) / tabbYoY.CF_51,
										 Field57 = tabbYoY.CF_52 == 0M ? null : 100 * (tabb.CF_52 - tabbYoY.CF_52) / tabbYoY.CF_52,
										 Field59 = tabbYoY.CF_53 == 0M ? null : 100 * (tabb.CF_53 - tabbYoY.CF_53) / tabbYoY.CF_53,
										 Field60 = tabbYoY.CF_54 == 0M ? null : 100 * (tabb.CF_54 - tabbYoY.CF_54) / tabbYoY.CF_54,
										 Field61 = tabbYoY.CF_55 == 0M ? null : 100 * (tabb.CF_55 - tabbYoY.CF_55) / tabbYoY.CF_55,
										 Field62 = tabbYoY.CF_56 == 0M ? null : 100 * (tabb.CF_56 - tabbYoY.CF_56) / tabbYoY.CF_56,
										 Field64 = tabbYoY.CF_57 == 0M ? null : 100 * (tabb.CF_57 - tabbYoY.CF_57) / tabbYoY.CF_57,
										 Field65 = tabbYoY.CF_58 == 0M ? null : 100 * (tabb.CF_58 - tabbYoY.CF_58) / tabbYoY.CF_58,
										 Field66 = tabbYoY.CF_59 == 0M ? null : 100 * (tabb.CF_59 - tabbYoY.CF_59) / tabbYoY.CF_59,
										 Field67 = tabbYoY.CF_60 == 0M ? null : 100 * (tabb.CF_60 - tabbYoY.CF_60) / tabbYoY.CF_60,
										 Field68 = tabbYoY.CF_61 == 0M ? null : 100 * (tabb.CF_61 - tabbYoY.CF_61) / tabbYoY.CF_61,
										 Field69 = tabbYoY.CF_62 == 0M ? null : 100 * (tabb.CF_62 - tabbYoY.CF_62) / tabbYoY.CF_62,
										 Field70 = tabbYoY.CF_63 == 0M ? null : 100 * (tabb.CF_63 - tabbYoY.CF_63) / tabbYoY.CF_63,
										 Field71 = tabbYoY.CF_64 == 0M ? null : 100 * (tabb.CF_64 - tabbYoY.CF_64) / tabbYoY.CF_64,
										 Field72 = tabbYoY.CF_65 == 0M ? null : 100 * (tabb.CF_65 - tabbYoY.CF_65) / tabbYoY.CF_65,
										 Field73 = tabbYoY.CF_66 == 0M ? null : 100 * (tabb.CF_66 - tabbYoY.CF_66) / tabbYoY.CF_66,
										 Field74 = tabbYoY.CF_67 == 0M ? null : 100 * (tabb.CF_67 - tabbYoY.CF_67) / tabbYoY.CF_67,
										 Field75 = tabbYoY.CF_68 == 0M ? null : 100 * (tabb.CF_68 - tabbYoY.CF_68) / tabbYoY.CF_68,
										 Field76 = tabbYoY.CF_69 == 0M ? null : 100 * (tabb.CF_69 - tabbYoY.CF_69) / tabbYoY.CF_69,
										 Field77 = tabbYoY.CF_70 == 0M ? null : 100 * (tabb.CF_70 - tabbYoY.CF_70) / tabbYoY.CF_70,
										 Field78 = tabbYoY.CF_71 == 0M ? null : 100 * (tabb.CF_71 - tabbYoY.CF_71) / tabbYoY.CF_71,
										 Field79 = tabbYoY.CF_72 == 0M ? null : 100 * (tabb.CF_72 - tabbYoY.CF_72) / tabbYoY.CF_72,
										 Field80 = tabbYoY.CF_73 == 0M ? null : 100 * (tabb.CF_73 - tabbYoY.CF_73) / tabbYoY.CF_73,
										 Field81 = tabbYoY.CF_74 == 0M ? null : 100 * (tabb.CF_74 - tabbYoY.CF_74) / tabbYoY.CF_74,
										 Field82 = tabbYoY.CF_75 == 0M ? null : 100 * (tabb.CF_75 - tabbYoY.CF_75) / tabbYoY.CF_75,
										 Field83 = tabbYoY.CF_76 == 0M ? null : 100 * (tabb.CF_76 - tabbYoY.CF_76) / tabbYoY.CF_76,
										 Field84 = tabbYoY.CF_77 == 0M ? null : 100 * (tabb.CF_77 - tabbYoY.CF_77) / tabbYoY.CF_77,
										 Field86 = tabbYoY.CF_78 == 0M ? null : 100 * (tabb.CF_78 - tabbYoY.CF_78) / tabbYoY.CF_78,
										 Field87 = tabbYoY.CF_79 == 0M ? null : 100 * (tabb.CF_79 - tabbYoY.CF_79) / tabbYoY.CF_79,
										 Field88 = tabbYoY.CF_80 == 0M ? null : 100 * (tabb.CF_80 - tabbYoY.CF_80) / tabbYoY.CF_80,
										 Field90 = tabbYoY.CF_81 == 0M ? null : 100 * (tabb.CF_81 - tabbYoY.CF_81) / tabbYoY.CF_81,
										 Field91 = tabbYoY.CF_82 == 0M ? null : 100 * (tabb.CF_82 - tabbYoY.CF_82) / tabbYoY.CF_82,
										 Field92 = tabbYoY.CF_83 == 0M ? null : 100 * (tabb.CF_83 - tabbYoY.CF_83) / tabbYoY.CF_83,
										 Field93 = tabbYoY.CF_84 == 0M ? null : 100 * (tabb.CF_84 - tabbYoY.CF_84) / tabbYoY.CF_84,
										 Field94 = tabbYoY.CF_85 == 0M ? null : 100 * (tabb.CF_85 - tabbYoY.CF_85) / tabbYoY.CF_85,
										 Field95 = tabbYoY.CF_86 == 0M ? null : 100 * (tabb.CF_86 - tabbYoY.CF_86) / tabbYoY.CF_86,

                                         EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "QoQ":
						var fundamental = ZCXDB.BOND_FIN_CASH_TACB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(f => f.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault()); //取最新的更新的记录

						var fundamentalQoQ = ZCXDB.BOND_FIN_CASH_TACB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(f => f.ISVALID == 1).ToList()
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault());

						issuerTACB = from tabb in fundamental.ToList()
									 join tabbOfLastQuarter in fundamentalQoQ.ToList()
									 on GetLastQuarter(tabb.END_DATE) equals tabbOfLastQuarter.END_DATE.Value.ToString("yyyy-MM") into QoQ
									 from tabbQoQ in QoQ
									 select new IssuerFundamental
									 {
										 Field2 = tabbQoQ.CF_01 == 0M ? null : 100 * (tabb.CF_01 - tabbQoQ.CF_01) / tabbQoQ.CF_01,
										 Field3 = tabbQoQ.CF_02 == 0M ? null : 100 * (tabb.CF_02 - tabbQoQ.CF_02) / tabbQoQ.CF_02,
										 Field4 = tabbQoQ.CF_03 == 0M ? null : 100 * (tabb.CF_03 - tabbQoQ.CF_03) / tabbQoQ.CF_03,
										 Field5 = tabbQoQ.CF_04 == 0M ? null : 100 * (tabb.CF_04 - tabbQoQ.CF_04) / tabbQoQ.CF_04,
										 Field6 = tabbQoQ.CF_05 == 0M ? null : 100 * (tabb.CF_05 - tabbQoQ.CF_05) / tabbQoQ.CF_05,
										 Field7 = tabbQoQ.CF_06 == 0M ? null : 100 * (tabb.CF_06 - tabbQoQ.CF_06) / tabbQoQ.CF_06,
										 Field8 = tabbQoQ.CF_07 == 0M ? null : 100 * (tabb.CF_07 - tabbQoQ.CF_07) / tabbQoQ.CF_07,
										 Field9 = tabbQoQ.CF_08 == 0M ? null : 100 * (tabb.CF_08 - tabbQoQ.CF_08) / tabbQoQ.CF_08,
										 Field10 = tabbQoQ.CF_09 == 0M ? null : 100 * (tabb.CF_09 - tabbQoQ.CF_09) / tabbQoQ.CF_09,
										 Field11 = tabbQoQ.CF_10 == 0M ? null : 100 * (tabb.CF_10 - tabbQoQ.CF_10) / tabbQoQ.CF_10,
										 Field12 = tabbQoQ.CF_11 == 0M ? null : 100 * (tabb.CF_11 - tabbQoQ.CF_11) / tabbQoQ.CF_11,
										 Field13 = tabbQoQ.CF_12 == 0M ? null : 100 * (tabb.CF_12 - tabbQoQ.CF_12) / tabbQoQ.CF_12,
										 Field14 = tabbQoQ.CF_13 == 0M ? null : 100 * (tabb.CF_13 - tabbQoQ.CF_13) / tabbQoQ.CF_13,
										 Field15 = tabbQoQ.CF_14 == 0M ? null : 100 * (tabb.CF_14 - tabbQoQ.CF_14) / tabbQoQ.CF_14,
										 Field16 = tabbQoQ.CF_15 == 0M ? null : 100 * (tabb.CF_15 - tabbQoQ.CF_15) / tabbQoQ.CF_15,
										 Field17 = tabbQoQ.CF_16 == 0M ? null : 100 * (tabb.CF_16 - tabbQoQ.CF_16) / tabbQoQ.CF_16,
										 Field18 = tabbQoQ.CF_17 == 0M ? null : 100 * (tabb.CF_17 - tabbQoQ.CF_17) / tabbQoQ.CF_17,
										 Field19 = tabbQoQ.CF_18 == 0M ? null : 100 * (tabb.CF_18 - tabbQoQ.CF_18) / tabbQoQ.CF_18,
										 Field20 = tabbQoQ.CF_19 == 0M ? null : 100 * (tabb.CF_19 - tabbQoQ.CF_19) / tabbQoQ.CF_19,
										 Field21 = tabbQoQ.CF_20 == 0M ? null : 100 * (tabb.CF_20 - tabbQoQ.CF_20) / tabbQoQ.CF_20,
										 Field22 = tabbQoQ.CF_21 == 0M ? null : 100 * (tabb.CF_21 - tabbQoQ.CF_21) / tabbQoQ.CF_21,
										 Field23 = tabbQoQ.CF_22 == 0M ? null : 100 * (tabb.CF_22 - tabbQoQ.CF_22) / tabbQoQ.CF_22,
										 Field24 = tabbQoQ.CF_23 == 0M ? null : 100 * (tabb.CF_23 - tabbQoQ.CF_23) / tabbQoQ.CF_23,
										 Field25 = tabbQoQ.CF_24 == 0M ? null : 100 * (tabb.CF_24 - tabbQoQ.CF_24) / tabbQoQ.CF_24,
										 Field26 = tabbQoQ.CF_25 == 0M ? null : 100 * (tabb.CF_25 - tabbQoQ.CF_25) / tabbQoQ.CF_25,
										 Field27 = tabbQoQ.CF_26 == 0M ? null : 100 * (tabb.CF_26 - tabbQoQ.CF_26) / tabbQoQ.CF_26,
										 Field28 = tabbQoQ.CF_27 == 0M ? null : 100 * (tabb.CF_27 - tabbQoQ.CF_27) / tabbQoQ.CF_27,
										 Field29 = tabbQoQ.CF_28 == 0M ? null : 100 * (tabb.CF_28 - tabbQoQ.CF_28) / tabbQoQ.CF_28,
										 Field31 = tabbQoQ.CF_29 == 0M ? null : 100 * (tabb.CF_29 - tabbQoQ.CF_29) / tabbQoQ.CF_29,
										 Field32 = tabbQoQ.CF_30 == 0M ? null : 100 * (tabb.CF_30 - tabbQoQ.CF_30) / tabbQoQ.CF_30,
										 Field33 = tabbQoQ.CF_31 == 0M ? null : 100 * (tabb.CF_31 - tabbQoQ.CF_31) / tabbQoQ.CF_31,
										 Field34 = tabbQoQ.CF_32 == 0M ? null : 100 * (tabb.CF_32 - tabbQoQ.CF_32) / tabbQoQ.CF_32,
										 Field35 = tabbQoQ.CF_33 == 0M ? null : 100 * (tabb.CF_33 - tabbQoQ.CF_33) / tabbQoQ.CF_33,
										 Field36 = tabbQoQ.CF_34 == 0M ? null : 100 * (tabb.CF_34 - tabbQoQ.CF_34) / tabbQoQ.CF_34,
										 Field37 = tabbQoQ.CF_35 == 0M ? null : 100 * (tabb.CF_35 - tabbQoQ.CF_35) / tabbQoQ.CF_35,
										 Field38 = tabbQoQ.CF_36 == 0M ? null : 100 * (tabb.CF_36 - tabbQoQ.CF_36) / tabbQoQ.CF_36,
										 Field39 = tabbQoQ.CF_37 == 0M ? null : 100 * (tabb.CF_37 - tabbQoQ.CF_37) / tabbQoQ.CF_37,
										 Field40 = tabbQoQ.CF_38 == 0M ? null : 100 * (tabb.CF_38 - tabbQoQ.CF_38) / tabbQoQ.CF_38,
										 Field41 = tabbQoQ.CF_39 == 0M ? null : 100 * (tabb.CF_39 - tabbQoQ.CF_39) / tabbQoQ.CF_39,
										 Field42 = tabbQoQ.CF_40 == 0M ? null : 100 * (tabb.CF_40 - tabbQoQ.CF_40) / tabbQoQ.CF_40,
										 Field43 = tabbQoQ.CF_41 == 0M ? null : 100 * (tabb.CF_41 - tabbQoQ.CF_41) / tabbQoQ.CF_41,
										 Field45 = tabbQoQ.CF_42 == 0M ? null : 100 * (tabb.CF_42 - tabbQoQ.CF_42) / tabbQoQ.CF_42,
										 Field46 = tabbQoQ.CF_4201 == 0M ? null : 100 * (tabb.CF_4201 - tabbQoQ.CF_4201) / tabbQoQ.CF_4201,
										 Field47 = tabbQoQ.CF_43 == 0M ? null : 100 * (tabb.CF_43 - tabbQoQ.CF_43) / tabbQoQ.CF_43,
										 Field48 = tabbQoQ.CF_44 == 0M ? null : 100 * (tabb.CF_44 - tabbQoQ.CF_44) / tabbQoQ.CF_44,
										 Field49 = tabbQoQ.CF_45 == 0M ? null : 100 * (tabb.CF_45 - tabbQoQ.CF_45) / tabbQoQ.CF_45,
										 Field50 = tabbQoQ.CF_46 == 0M ? null : 100 * (tabb.CF_46 - tabbQoQ.CF_46) / tabbQoQ.CF_46,
										 Field51 = tabbQoQ.CF_47 == 0M ? null : 100 * (tabb.CF_47 - tabbQoQ.CF_47) / tabbQoQ.CF_47,
										 Field52 = tabbQoQ.CF_48 == 0M ? null : 100 * (tabb.CF_48 - tabbQoQ.CF_48) / tabbQoQ.CF_48,
										 Field53 = tabbQoQ.CF_49 == 0M ? null : 100 * (tabb.CF_49 - tabbQoQ.CF_49) / tabbQoQ.CF_49,
										 Field54 = tabbQoQ.CF_4901 == 0M ? null : 100 * (tabb.CF_4901 - tabbQoQ.CF_4901) / tabbQoQ.CF_4901,
										 Field55 = tabbQoQ.CF_50 == 0M ? null : 100 * (tabb.CF_50 - tabbQoQ.CF_50) / tabbQoQ.CF_50,
										 Field56 = tabbQoQ.CF_51 == 0M ? null : 100 * (tabb.CF_51 - tabbQoQ.CF_51) / tabbQoQ.CF_51,
										 Field57 = tabbQoQ.CF_52 == 0M ? null : 100 * (tabb.CF_52 - tabbQoQ.CF_52) / tabbQoQ.CF_52,
										 Field59 = tabbQoQ.CF_53 == 0M ? null : 100 * (tabb.CF_53 - tabbQoQ.CF_53) / tabbQoQ.CF_53,
										 Field60 = tabbQoQ.CF_54 == 0M ? null : 100 * (tabb.CF_54 - tabbQoQ.CF_54) / tabbQoQ.CF_54,
										 Field61 = tabbQoQ.CF_55 == 0M ? null : 100 * (tabb.CF_55 - tabbQoQ.CF_55) / tabbQoQ.CF_55,
										 Field62 = tabbQoQ.CF_56 == 0M ? null : 100 * (tabb.CF_56 - tabbQoQ.CF_56) / tabbQoQ.CF_56,
										 Field64 = tabbQoQ.CF_57 == 0M ? null : 100 * (tabb.CF_57 - tabbQoQ.CF_57) / tabbQoQ.CF_57,
										 Field65 = tabbQoQ.CF_58 == 0M ? null : 100 * (tabb.CF_58 - tabbQoQ.CF_58) / tabbQoQ.CF_58,
										 Field66 = tabbQoQ.CF_59 == 0M ? null : 100 * (tabb.CF_59 - tabbQoQ.CF_59) / tabbQoQ.CF_59,
										 Field67 = tabbQoQ.CF_60 == 0M ? null : 100 * (tabb.CF_60 - tabbQoQ.CF_60) / tabbQoQ.CF_60,
										 Field68 = tabbQoQ.CF_61 == 0M ? null : 100 * (tabb.CF_61 - tabbQoQ.CF_61) / tabbQoQ.CF_61,
										 Field69 = tabbQoQ.CF_62 == 0M ? null : 100 * (tabb.CF_62 - tabbQoQ.CF_62) / tabbQoQ.CF_62,
										 Field70 = tabbQoQ.CF_63 == 0M ? null : 100 * (tabb.CF_63 - tabbQoQ.CF_63) / tabbQoQ.CF_63,
										 Field71 = tabbQoQ.CF_64 == 0M ? null : 100 * (tabb.CF_64 - tabbQoQ.CF_64) / tabbQoQ.CF_64,
										 Field72 = tabbQoQ.CF_65 == 0M ? null : 100 * (tabb.CF_65 - tabbQoQ.CF_65) / tabbQoQ.CF_65,
										 Field73 = tabbQoQ.CF_66 == 0M ? null : 100 * (tabb.CF_66 - tabbQoQ.CF_66) / tabbQoQ.CF_66,
										 Field74 = tabbQoQ.CF_67 == 0M ? null : 100 * (tabb.CF_67 - tabbQoQ.CF_67) / tabbQoQ.CF_67,
										 Field75 = tabbQoQ.CF_68 == 0M ? null : 100 * (tabb.CF_68 - tabbQoQ.CF_68) / tabbQoQ.CF_68,
										 Field76 = tabbQoQ.CF_69 == 0M ? null : 100 * (tabb.CF_69 - tabbQoQ.CF_69) / tabbQoQ.CF_69,
										 Field77 = tabbQoQ.CF_70 == 0M ? null : 100 * (tabb.CF_70 - tabbQoQ.CF_70) / tabbQoQ.CF_70,
										 Field78 = tabbQoQ.CF_71 == 0M ? null : 100 * (tabb.CF_71 - tabbQoQ.CF_71) / tabbQoQ.CF_71,
										 Field79 = tabbQoQ.CF_72 == 0M ? null : 100 * (tabb.CF_72 - tabbQoQ.CF_72) / tabbQoQ.CF_72,
										 Field80 = tabbQoQ.CF_73 == 0M ? null : 100 * (tabb.CF_73 - tabbQoQ.CF_73) / tabbQoQ.CF_73,
										 Field81 = tabbQoQ.CF_74 == 0M ? null : 100 * (tabb.CF_74 - tabbQoQ.CF_74) / tabbQoQ.CF_74,
										 Field82 = tabbQoQ.CF_75 == 0M ? null : 100 * (tabb.CF_75 - tabbQoQ.CF_75) / tabbQoQ.CF_75,
										 Field83 = tabbQoQ.CF_76 == 0M ? null : 100 * (tabb.CF_76 - tabbQoQ.CF_76) / tabbQoQ.CF_76,
										 Field84 = tabbQoQ.CF_77 == 0M ? null : 100 * (tabb.CF_77 - tabbQoQ.CF_77) / tabbQoQ.CF_77,
										 Field86 = tabbQoQ.CF_78 == 0M ? null : 100 * (tabb.CF_78 - tabbQoQ.CF_78) / tabbQoQ.CF_78,
										 Field87 = tabbQoQ.CF_79 == 0M ? null : 100 * (tabb.CF_79 - tabbQoQ.CF_79) / tabbQoQ.CF_79,
										 Field88 = tabbQoQ.CF_80 == 0M ? null : 100 * (tabb.CF_80 - tabbQoQ.CF_80) / tabbQoQ.CF_80,
										 Field90 = tabbQoQ.CF_81 == 0M ? null : 100 * (tabb.CF_81 - tabbQoQ.CF_81) / tabbQoQ.CF_81,
										 Field91 = tabbQoQ.CF_82 == 0M ? null : 100 * (tabb.CF_82 - tabbQoQ.CF_82) / tabbQoQ.CF_82,
										 Field92 = tabbQoQ.CF_83 == 0M ? null : 100 * (tabb.CF_83 - tabbQoQ.CF_83) / tabbQoQ.CF_83,
										 Field93 = tabbQoQ.CF_84 == 0M ? null : 100 * (tabb.CF_84 - tabbQoQ.CF_84) / tabbQoQ.CF_84,
										 Field94 = tabbQoQ.CF_85 == 0M ? null : 100 * (tabb.CF_85 - tabbQoQ.CF_85) / tabbQoQ.CF_85,
										 Field95 = tabbQoQ.CF_86 == 0M ? null : 100 * (tabb.CF_86 - tabbQoQ.CF_86) / tabbQoQ.CF_86,
										 
										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					default:
						break;
				}

				return issuerTACB.ToList();
			}
		}

		public IEnumerable<IssuerFundamental> GetIssuerTAPB(int companyId, string reportType, string contentType, int interval, string unit)
		{
			IEnumerable<IssuerFundamental> issuerTAPB = null;

			using (var ZCXDB = new ZCXEntities())
			{
				Func<BOND_FIN_PROF_TAPB, bool> dateFilter = null;
				Func<BOND_FIN_PROF_TAPB, bool> reportTypeFilter = null;
				int multiplier = GetMultiPlier(unit); 

				if (reportType == "Y") //1:year report;
				{
					if (interval == 0)
						dateFilter = t => true;
					else
                        dateFilter = t => t.END_DATE.Value >= new DateTime(DateTime.Now.Year - interval, 1, 1);

					reportTypeFilter = t => t.SHEET_ATTR_PAR == 1;
				}
				else
				{
					if (interval == 0)
						dateFilter = t => true;
					else
                        dateFilter = t => t.END_DATE.Value >= new DateTime(DateTime.Now.Year - interval, DateTime.Now.Month - (DateTime.Now.Month - 1) % 3, 1);

					reportTypeFilter = t => (t.SHEET_ATTR_PAR == 3 || t.SHEET_ATTR_PAR == 2 || t.SHEET_ATTR_PAR == 6 || t.SHEET_ATTR_PAR == 1);
				}

				switch (contentType)
				{
					case "RawReport":
						issuerTAPB = from tabb in (ZCXDB.BOND_FIN_PROF_TAPB
									 .Where(b => b.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(b => b.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault())).ToList()
									 select new IssuerFundamental
									 {
										 Field1 = tabb.PF_01 / multiplier,
										 Field2 = tabb.PF_02 / multiplier,
										 Field3 = tabb.PF_03 / multiplier,
										 Field4 = tabb.PF_0301 / multiplier,
										 Field5 = tabb.PF_0302 / multiplier,
										 Field6 = tabb.PF_04 / multiplier,
										 Field7 = tabb.PF_0401 / multiplier,
										 Field8 = tabb.PF_0402 / multiplier,
										 Field9 = tabb.PF_0403 / multiplier,
										 Field10 = tabb.PF_0404 / multiplier,
										 Field11 = tabb.PF_0405 / multiplier,
										 Field12 = tabb.PF_05 / multiplier,
										 Field13 = tabb.PF_06 / multiplier,
										 Field14 = tabb.PF_0601 / multiplier,
										 Field15 = tabb.PF_07 / multiplier,
										 Field16 = tabb.PF_08 / multiplier,
										 Field17 = tabb.PF_09 / multiplier,
										 Field18 = tabb.PF_10 / multiplier,
										 Field19 = tabb.PF_11 / multiplier,
										 Field20 = tabb.PF_12 / multiplier,
										 Field21 = tabb.PF_13 / multiplier,
										 Field22 = tabb.PF_14 / multiplier,
										 Field23 = tabb.PF_15 / multiplier,
										 Field24 = tabb.PF_16 / multiplier,
										 Field25 = tabb.PF_17 / multiplier,
										 Field26 = tabb.PF_18 / multiplier,
										 Field27 = tabb.PF_19 / multiplier,
										 Field28 = tabb.PF_20 / multiplier,
										 Field29 = tabb.PF_21 / multiplier,
										 Field30 = tabb.PF_22 / multiplier,
										 Field31 = tabb.PF_23 / multiplier,
										 Field32 = tabb.PF_24 / multiplier,
										 Field33 = tabb.PF_35 / multiplier,
										 Field34 = tabb.PF_26 / multiplier,
										 Field35 = tabb.PF_27 / multiplier,
										 Field36 = tabb.PF_28 / multiplier,
										 Field38 = tabb.PF_29 / multiplier,
										 Field39 = tabb.PF_30 / multiplier,
										 Field40 = tabb.PF_31 / multiplier,
										 Field41 = tabb.PF_3101 / multiplier,
										 Field42 = tabb.PF_32 / multiplier,
										 Field43 = tabb.PF_33 / multiplier,
										 Field44 = tabb.PF_34 / multiplier,
										 Field45 = tabb.PF_35 / multiplier,
										 Field46 = tabb.PF_3501 / multiplier,
										 Field47 = tabb.PF_36 / multiplier,
										 Field48 = tabb.PF_37 / multiplier,
										 Field49 = tabb.PF_38 / multiplier,
										 Field50 = tabb.PF_39 / multiplier,
										 Field51 = tabb.PF_40 / multiplier,
										 Field52 = tabb.PF_41 / multiplier,
										 Field54 = tabb.PF_42 / multiplier,
										 Field55 = tabb.PF_43 / multiplier,
										 Field56 = tabb.PF_44 / multiplier,
										 Field57 = tabb.PF_45 / multiplier,
										 Field59 = tabb.PF_46 / multiplier,
										 Field60 = tabb.PF_47 / multiplier,

										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR, //0-其它；1-年报；2-中报；3-一季报；6-三季报
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "YoY":
						var funda = ZCXDB.BOND_FIN_PROF_TAPB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(f => f.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault()); //取最新的更新的记录

						var fundamentalYoY = ZCXDB.BOND_FIN_PROF_TAPB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(f => f.ISVALID == 1).ToList()
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault());

						issuerTAPB = from tabb in funda.ToList()
									 join tabbOfLastYear in fundamentalYoY.ToList()
                                     on new { Year = tabb.END_DATE.Value.Year - 1, Month = tabb.END_DATE.Value.Month }
                                     equals new { Year = tabbOfLastYear.END_DATE.Value.Year, Month = tabbOfLastYear.END_DATE.Value.Month } into YoY
									 from tabbYoY in YoY
									 select new IssuerFundamental
									 {
										 Field1 = tabbYoY.PF_01 == 0M ? null : 100 * (tabb.PF_01 - tabbYoY.PF_01) / tabbYoY.PF_01,
										 Field2 = tabbYoY.PF_02 == 0M ? null : 100 * (tabb.PF_02 - tabbYoY.PF_02) / tabbYoY.PF_02,
										 Field3 = tabbYoY.PF_03 == 0M ? null : 100 * (tabb.PF_03 - tabbYoY.PF_03) / tabbYoY.PF_03,
										 Field4 = tabbYoY.PF_0301 == 0M ? null : 100 * (tabb.PF_0301 - tabbYoY.PF_0301) / tabbYoY.PF_0301,
										 Field5 = tabbYoY.PF_0302 == 0M ? null : 100 * (tabb.PF_0302 - tabbYoY.PF_0302) / tabbYoY.PF_0302,
										 Field6 = tabbYoY.PF_04 == 0M ? null : 100 * (tabb.PF_04 - tabbYoY.PF_04) / tabbYoY.PF_04,
										 Field7 = tabbYoY.PF_0401 == 0M ? null : 100 * (tabb.PF_0401 - tabbYoY.PF_0401) / tabbYoY.PF_0401,
										 Field8 = tabbYoY.PF_0402 == 0M ? null : 100 * (tabb.PF_0401 - tabbYoY.PF_0402) / tabbYoY.PF_0402,
										 Field9 = tabbYoY.PF_0403 == 0M ? null : 100 * (tabb.PF_0401 - tabbYoY.PF_0403) / tabbYoY.PF_0403,
										 Field10 = tabbYoY.PF_0404 == 0M ? null : 100 * (tabb.PF_0401 - tabbYoY.PF_0404) / tabbYoY.PF_0404,
										 Field11 = tabbYoY.PF_0405 == 0M ? null : 100 * (tabb.PF_0401 - tabbYoY.PF_0405) / tabbYoY.PF_0405,
										 Field12 = tabbYoY.PF_05 == 0M ? null : 100 * (tabb.PF_05 - tabbYoY.PF_05) / tabbYoY.PF_05,
										 Field13 = tabbYoY.PF_06 == 0M ? null : 100 * (tabb.PF_06 - tabbYoY.PF_06) / tabbYoY.PF_06,
										 Field14 = tabbYoY.PF_0601 == 0M ? null : 100 * (tabb.PF_0601 - tabbYoY.PF_0601) / tabbYoY.PF_0601,
										 Field15 = tabbYoY.PF_07 == 0M ? null : 100 * (tabb.PF_07 - tabbYoY.PF_07) / tabbYoY.PF_07,
										 Field16 = tabbYoY.PF_08 == 0M ? null : 100 * (tabb.PF_08 - tabbYoY.PF_08) / tabbYoY.PF_08,
										 Field17 = tabbYoY.PF_09 == 0M ? null : 100 * (tabb.PF_09 - tabbYoY.PF_09) / tabbYoY.PF_09,
										 Field18 = tabbYoY.PF_10 == 0M ? null : 100 * (tabb.PF_10 - tabbYoY.PF_10) / tabbYoY.PF_10,
										 Field19 = tabbYoY.PF_11 == 0M ? null : 100 * (tabb.PF_11 - tabbYoY.PF_11) / tabbYoY.PF_11,
										 Field20 = tabbYoY.PF_12 == 0M ? null : 100 * (tabb.PF_12 - tabbYoY.PF_12) / tabbYoY.PF_12,
										 Field21 = tabbYoY.PF_13 == 0M ? null : 100 * (tabb.PF_13 - tabbYoY.PF_13) / tabbYoY.PF_13,
										 Field22 = tabbYoY.PF_14 == 0M ? null : 100 * (tabb.PF_14 - tabbYoY.PF_14) / tabbYoY.PF_14,
										 Field23 = tabbYoY.PF_15 == 0M ? null : 100 * (tabb.PF_15 - tabbYoY.PF_15) / tabbYoY.PF_15,
										 Field24 = tabbYoY.PF_16 == 0M ? null : 100 * (tabb.PF_16 - tabbYoY.PF_16) / tabbYoY.PF_16,
										 Field25 = tabbYoY.PF_17 == 0M ? null : 100 * (tabb.PF_17 - tabbYoY.PF_17) / tabbYoY.PF_17,
										 Field26 = tabbYoY.PF_18 == 0M ? null : 100 * (tabb.PF_18 - tabbYoY.PF_18) / tabbYoY.PF_18,
										 Field27 = tabbYoY.PF_19 == 0M ? null : 100 * (tabb.PF_19 - tabbYoY.PF_19) / tabbYoY.PF_19,
										 Field28 = tabbYoY.PF_20 == 0M ? null : 100 * (tabb.PF_20 - tabbYoY.PF_20) / tabbYoY.PF_20,
										 Field29 = tabbYoY.PF_21 == 0M ? null : 100 * (tabb.PF_21 - tabbYoY.PF_21) / tabbYoY.PF_21,
										 Field30 = tabbYoY.PF_22 == 0M ? null : 100 * (tabb.PF_22 - tabbYoY.PF_22) / tabbYoY.PF_22,
										 Field31 = tabbYoY.PF_23 == 0M ? null : 100 * (tabb.PF_23 - tabbYoY.PF_23) / tabbYoY.PF_23,
										 Field32 = tabbYoY.PF_24 == 0M ? null : 100 * (tabb.PF_24 - tabbYoY.PF_24) / tabbYoY.PF_24,
										 Field33 = tabbYoY.PF_25 == 0M ? null : 100 * (tabb.PF_35 - tabbYoY.PF_25) / tabbYoY.PF_25,
										 Field34 = tabbYoY.PF_26 == 0M ? null : 100 * (tabb.PF_26 - tabbYoY.PF_26) / tabbYoY.PF_26,
										 Field35 = tabbYoY.PF_27 == 0M ? null : 100 * (tabb.PF_27 - tabbYoY.PF_27) / tabbYoY.PF_27,
										 Field36 = tabbYoY.PF_28 == 0M ? null : 100 * (tabb.PF_28 - tabbYoY.PF_28) / tabbYoY.PF_28,
										 Field38 = tabbYoY.PF_29 == 0M ? null : 100 * (tabb.PF_29 - tabbYoY.PF_29) / tabbYoY.PF_29,
										 Field39 = tabbYoY.PF_30 == 0M ? null : 100 * (tabb.PF_30 - tabbYoY.PF_30) / tabbYoY.PF_30,
										 Field40 = tabbYoY.PF_31 == 0M ? null : 100 * (tabb.PF_31 - tabbYoY.PF_31) / tabbYoY.PF_31,
										 Field41 = tabbYoY.PF_3101 == 0M ? null : 100 * (tabb.PF_3101 - tabbYoY.PF_3101) / tabbYoY.PF_3101,
										 Field42 = tabbYoY.PF_32 == 0M ? null : 100 * (tabb.PF_32 - tabbYoY.PF_32) / tabbYoY.PF_32,
										 Field43 = tabbYoY.PF_33 == 0M ? null : 100 * (tabb.PF_33 - tabbYoY.PF_33) / tabbYoY.PF_33,
										 Field44 = tabbYoY.PF_34 == 0M ? null : 100 * (tabb.PF_34 - tabbYoY.PF_34) / tabbYoY.PF_34,
										 Field45 = tabbYoY.PF_35 == 0M ? null : 100 * (tabb.PF_35 - tabbYoY.PF_35) / tabbYoY.PF_35,
										 Field46 = tabbYoY.PF_3501 == 0M ? null : 100 * (tabb.PF_3501 - tabbYoY.PF_3501) / tabbYoY.PF_3501,
										 Field47 = tabbYoY.PF_36 == 0M ? null : 100 * (tabb.PF_36 - tabbYoY.PF_36) / tabbYoY.PF_36,
										 Field48 = tabbYoY.PF_37 == 0M ? null : 100 * (tabb.PF_37 - tabbYoY.PF_37) / tabbYoY.PF_37,
										 Field49 = tabbYoY.PF_38 == 0M ? null : 100 * (tabb.PF_38 - tabbYoY.PF_38) / tabbYoY.PF_38,
										 Field50 = tabbYoY.PF_39 == 0M ? null : 100 * (tabb.PF_39 - tabbYoY.PF_39) / tabbYoY.PF_39,
										 Field51 = tabbYoY.PF_40 == 0M ? null : 100 * (tabb.PF_40 - tabbYoY.PF_40) / tabbYoY.PF_40,
										 Field52 = tabbYoY.PF_41 == 0M ? null : 100 * (tabb.PF_41 - tabbYoY.PF_41) / tabbYoY.PF_41,
										 Field54 = tabbYoY.PF_42 == 0M ? null : 100 * (tabb.PF_42 - tabbYoY.PF_42) / tabbYoY.PF_42,
										 Field55 = tabbYoY.PF_43 == 0M ? null : 100 * (tabb.PF_43 - tabbYoY.PF_43) / tabbYoY.PF_43,
										 Field56 = tabbYoY.PF_44 == 0M ? null : 100 * (tabb.PF_44 - tabbYoY.PF_44) / tabbYoY.PF_44,
										 Field57 = tabbYoY.PF_45 == 0M ? null : 100 * (tabb.PF_45 - tabbYoY.PF_45) / tabbYoY.PF_45,
										 Field59 = tabbYoY.PF_46 == 0M ? null : 100 * (tabb.PF_46 - tabbYoY.PF_46) / tabbYoY.PF_46,
										 Field60 = tabbYoY.PF_47 == 0M ? null : 100 * (tabb.PF_47 - tabbYoY.PF_47) / tabbYoY.PF_47,

										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					case "QoQ":
						var fundamental = ZCXDB.BOND_FIN_PROF_TAPB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(dateFilter)
									 .Where(f => f.ISVALID == 1)
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault()); //取最新的更新的记录

						var fundamentalQoQ = ZCXDB.BOND_FIN_PROF_TAPB.Where(f => f.COM_UNI_CODE == companyId)
									 .Where(reportTypeFilter)
									 .Where(f => f.ISVALID == 1).ToList()
									 .GroupBy(f => f.END_DATE)
									 .Select(g => g.Where(l => l.CCXEID == g.Max(m => m.CCXEID)).Select(n => n).FirstOrDefault());

						issuerTAPB = from tabb in fundamental.ToList()
									 join tabbOfLastQuarter in fundamentalQoQ.ToList()
									 on GetLastQuarter(tabb.END_DATE) equals tabbOfLastQuarter.END_DATE.Value.ToString("yyyy-MM") into QoQ
									 from tabbQoQ in QoQ
									 select new IssuerFundamental
									 {
										 Field1 = tabbQoQ.PF_01 == 0M ? null : 100 * (tabb.PF_01 - tabbQoQ.PF_01) / tabbQoQ.PF_01,
										 Field2 = tabbQoQ.PF_02 == 0M ? null : 100 * (tabb.PF_02 - tabbQoQ.PF_02) / tabbQoQ.PF_02,
										 Field3 = tabbQoQ.PF_03 == 0M ? null : 100 * (tabb.PF_03 - tabbQoQ.PF_03) / tabbQoQ.PF_03,
										 Field4 = tabbQoQ.PF_0301 == 0M ? null : 100 * (tabb.PF_0301 - tabbQoQ.PF_0301) / tabbQoQ.PF_0301,
										 Field5 = tabbQoQ.PF_0302 == 0M ? null : 100 * (tabb.PF_0302 - tabbQoQ.PF_0302) / tabbQoQ.PF_0302,
										 Field6 = tabbQoQ.PF_04 == 0M ? null : 100 * (tabb.PF_04 - tabbQoQ.PF_04) / tabbQoQ.PF_04,
										 Field7 = tabbQoQ.PF_0401 == 0M ? null : 100 * (tabb.PF_0401 - tabbQoQ.PF_0401) / tabbQoQ.PF_0401,
										 Field8 = tabbQoQ.PF_0402 == 0M ? null : 100 * (tabb.PF_0401 - tabbQoQ.PF_0402) / tabbQoQ.PF_0402,
										 Field9 = tabbQoQ.PF_0403 == 0M ? null : 100 * (tabb.PF_0401 - tabbQoQ.PF_0403) / tabbQoQ.PF_0403,
										 Field10 = tabbQoQ.PF_0404 == 0M ? null : 100 * (tabb.PF_0401 - tabbQoQ.PF_0404) / tabbQoQ.PF_0404,
										 Field11 = tabbQoQ.PF_0405 == 0M ? null : 100 * (tabb.PF_0401 - tabbQoQ.PF_0405) / tabbQoQ.PF_0405,
										 Field12 = tabbQoQ.PF_05 == 0M ? null : 100 * (tabb.PF_05 - tabbQoQ.PF_05) / tabbQoQ.PF_05,
										 Field13 = tabbQoQ.PF_06 == 0M ? null : 100 * (tabb.PF_06 - tabbQoQ.PF_06) / tabbQoQ.PF_06,
										 Field14 = tabbQoQ.PF_0601 == 0M ? null : 100 * (tabb.PF_0601 - tabbQoQ.PF_0601) / tabbQoQ.PF_0601,
										 Field15 = tabbQoQ.PF_07 == 0M ? null : 100 * (tabb.PF_07 - tabbQoQ.PF_07) / tabbQoQ.PF_07,
										 Field16 = tabbQoQ.PF_08 == 0M ? null : 100 * (tabb.PF_08 - tabbQoQ.PF_08) / tabbQoQ.PF_08,
										 Field17 = tabbQoQ.PF_09 == 0M ? null : 100 * (tabb.PF_09 - tabbQoQ.PF_09) / tabbQoQ.PF_09,
										 Field18 = tabbQoQ.PF_10 == 0M ? null : 100 * (tabb.PF_10 - tabbQoQ.PF_10) / tabbQoQ.PF_10,
										 Field19 = tabbQoQ.PF_11 == 0M ? null : 100 * (tabb.PF_11 - tabbQoQ.PF_11) / tabbQoQ.PF_11,
										 Field20 = tabbQoQ.PF_12 == 0M ? null : 100 * (tabb.PF_12 - tabbQoQ.PF_12) / tabbQoQ.PF_12,
										 Field21 = tabbQoQ.PF_13 == 0M ? null : 100 * (tabb.PF_13 - tabbQoQ.PF_13) / tabbQoQ.PF_13,
										 Field22 = tabbQoQ.PF_14 == 0M ? null : 100 * (tabb.PF_14 - tabbQoQ.PF_14) / tabbQoQ.PF_14,
										 Field23 = tabbQoQ.PF_15 == 0M ? null : 100 * (tabb.PF_15 - tabbQoQ.PF_15) / tabbQoQ.PF_15,
										 Field24 = tabbQoQ.PF_16 == 0M ? null : 100 * (tabb.PF_16 - tabbQoQ.PF_16) / tabbQoQ.PF_16,
										 Field25 = tabbQoQ.PF_17 == 0M ? null : 100 * (tabb.PF_17 - tabbQoQ.PF_17) / tabbQoQ.PF_17,
										 Field26 = tabbQoQ.PF_18 == 0M ? null : 100 * (tabb.PF_18 - tabbQoQ.PF_18) / tabbQoQ.PF_18,
										 Field27 = tabbQoQ.PF_19 == 0M ? null : 100 * (tabb.PF_19 - tabbQoQ.PF_19) / tabbQoQ.PF_19,
										 Field28 = tabbQoQ.PF_20 == 0M ? null : 100 * (tabb.PF_20 - tabbQoQ.PF_20) / tabbQoQ.PF_20,
										 Field29 = tabbQoQ.PF_21 == 0M ? null : 100 * (tabb.PF_21 - tabbQoQ.PF_21) / tabbQoQ.PF_21,
										 Field30 = tabbQoQ.PF_22 == 0M ? null : 100 * (tabb.PF_22 - tabbQoQ.PF_22) / tabbQoQ.PF_22,
										 Field31 = tabbQoQ.PF_23 == 0M ? null : 100 * (tabb.PF_23 - tabbQoQ.PF_23) / tabbQoQ.PF_23,
										 Field32 = tabbQoQ.PF_24 == 0M ? null : 100 * (tabb.PF_24 - tabbQoQ.PF_24) / tabbQoQ.PF_24,
										 Field33 = tabbQoQ.PF_25 == 0M ? null : 100 * (tabb.PF_35 - tabbQoQ.PF_25) / tabbQoQ.PF_25,
										 Field34 = tabbQoQ.PF_26 == 0M ? null : 100 * (tabb.PF_26 - tabbQoQ.PF_26) / tabbQoQ.PF_26,
										 Field35 = tabbQoQ.PF_27 == 0M ? null : 100 * (tabb.PF_27 - tabbQoQ.PF_27) / tabbQoQ.PF_27,
										 Field36 = tabbQoQ.PF_28 == 0M ? null : 100 * (tabb.PF_28 - tabbQoQ.PF_28) / tabbQoQ.PF_28,
										 Field38 = tabbQoQ.PF_29 == 0M ? null : 100 * (tabb.PF_29 - tabbQoQ.PF_29) / tabbQoQ.PF_29,
										 Field39 = tabbQoQ.PF_30 == 0M ? null : 100 * (tabb.PF_30 - tabbQoQ.PF_30) / tabbQoQ.PF_30,
										 Field40 = tabbQoQ.PF_31 == 0M ? null : 100 * (tabb.PF_31 - tabbQoQ.PF_31) / tabbQoQ.PF_31,
										 Field41 = tabbQoQ.PF_3101 == 0M ? null : 100 * (tabb.PF_3101 - tabbQoQ.PF_3101) / tabbQoQ.PF_3101,
										 Field42 = tabbQoQ.PF_32 == 0M ? null : 100 * (tabb.PF_32 - tabbQoQ.PF_32) / tabbQoQ.PF_32,
										 Field43 = tabbQoQ.PF_33 == 0M ? null : 100 * (tabb.PF_33 - tabbQoQ.PF_33) / tabbQoQ.PF_33,
										 Field44 = tabbQoQ.PF_34 == 0M ? null : 100 * (tabb.PF_34 - tabbQoQ.PF_34) / tabbQoQ.PF_34,
										 Field45 = tabbQoQ.PF_35 == 0M ? null : 100 * (tabb.PF_35 - tabbQoQ.PF_35) / tabbQoQ.PF_35,
										 Field46 = tabbQoQ.PF_3501 == 0M ? null : 100 * (tabb.PF_3501 - tabbQoQ.PF_3501) / tabbQoQ.PF_3501,
										 Field47 = tabbQoQ.PF_36 == 0M ? null : 100 * (tabb.PF_36 - tabbQoQ.PF_36) / tabbQoQ.PF_36,
										 Field48 = tabbQoQ.PF_37 == 0M ? null : 100 * (tabb.PF_37 - tabbQoQ.PF_37) / tabbQoQ.PF_37,
										 Field49 = tabbQoQ.PF_38 == 0M ? null : 100 * (tabb.PF_38 - tabbQoQ.PF_38) / tabbQoQ.PF_38,
										 Field50 = tabbQoQ.PF_39 == 0M ? null : 100 * (tabbQoQ.PF_39 - tabbQoQ.PF_39) / tabbQoQ.PF_39,
										 Field51 = tabbQoQ.PF_40 == 0M ? null : 100 * (tabb.PF_40 - tabbQoQ.PF_40) / tabbQoQ.PF_40,
										 Field52 = tabbQoQ.PF_41 == 0M ? null : 100 * (tabb.PF_41 - tabbQoQ.PF_41) / tabbQoQ.PF_41,
										 Field54 = tabbQoQ.PF_42 == 0M ? null : 100 * (tabb.PF_42 - tabbQoQ.PF_42) / tabbQoQ.PF_42,
										 Field55 = tabbQoQ.PF_43 == 0M ? null : 100 * (tabb.PF_43 - tabbQoQ.PF_43) / tabbQoQ.PF_43,
										 Field56 = tabbQoQ.PF_44 == 0M ? null : 100 * (tabb.PF_44 - tabbQoQ.PF_44) / tabbQoQ.PF_44,
										 Field57 = tabbQoQ.PF_45 == 0M ? null : 100 * (tabb.PF_45 - tabbQoQ.PF_45) / tabbQoQ.PF_45,
										 Field59 = tabbQoQ.PF_46 == 0M ? null : 100 * (tabb.PF_46 - tabbQoQ.PF_46) / tabbQoQ.PF_46,
										 Field60 = tabbQoQ.PF_47 == 0M ? null : 100 * (tabb.PF_47 - tabbQoQ.PF_47) / tabbQoQ.PF_47,

										 EndDate = tabb.END_DATE.Value,
										 ReportType = tabb.SHEET_ATTR_PAR,
										 UpdateTime = tabb.CCXEID
									 };
						break;
					default:
						break;
				}

				return issuerTAPB.ToList();
			}
		}

		/// <summary>
		/// Get nonlisted issuer by com_code
		/// </summary>
		/// <param name="comCode">COM_UNI_CODE</param>
		/// <returns>IssuerDetail object</returns>
		public IssuerDetail GetIsserByComCode(int comCode)
		{
            OracleParameter[] paramArray =
            {
                new OracleParameter("P_comCode", comCode),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return DataTableSerializer.ToList<IssuerDetail>(GetZCXDataSetBySp("GetIsserByComCode", paramArray).Tables[0]).FirstOrDefault();
		}

        public IEnumerable<FUNDAMENTALFIELDMAPPING> GetFundamentalFiledMapping(string fundamentalType)
		{
			using (var ZCXDB = new ZCXEntities())
			{
				return ZCXDB.FUNDAMENTALFIELDMAPPING.Where(f => f.FUNDAMENATAL_TYPE == fundamentalType).Select(f => f).ToList();
			}
		}

		/// <summary>
		/// Get bondlist from [BondInfoCn] and [BondInfoEn]
		/// </summary>
		/// <param name="comCode">COM_UNI_CODE</param>
		/// <returns>List of IssuerBondInfo</returns>
		public IEnumerable<IssuerBondInfo> GetBondListByComCode(int comCode)
		{
            OracleParameter[] paramArray =
            {
                new OracleParameter("P_ORG_UNI_CODE",OracleDbType.Varchar2){Value = comCode.ToString()},
                new OracleParameter("P_Culture",OracleDbType.Varchar2){Value = Thread.CurrentThread.CurrentUICulture.Name},
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            var dt = GetZCXDataSetBySp("GetBondListByComCode", paramArray).Tables[0];
            if (dt == null || dt.Rows.Count == 0) return new List<IssuerBondInfo>();
		    return DataTableSerializer.ToList<IssuerBondInfo>(dt);
        }

	    public long GetComCodeFromBondCode(string bondCode)
	    {
	        OracleParameter[] paramArray =
	        {
                new OracleParameter("P_BOND_CODE", OracleDbType.Varchar2) { Value = bondCode }, 
	            new OracleParameter("P_CUR", OracleDbType.RefCursor) {Direction = ParameterDirection.Output}
	        };
	        var dt = GetZCXDataSetBySp("GetComCodeFromBondCode", paramArray).Tables[0];
	        if (dt == null || dt.Rows.Count == 0) return 0;
	        return Convert.ToInt64(dt.Rows[0][0]);
	    }


	    private int GetMultiPlier(string unit)
		{
			int multiplier = 1;
			switch (unit)
			{
				case ConstValues.Unit_100M:
					multiplier = 100000000;
					break;
				case ConstValues.Unit_M:
					multiplier = 1000000;
					break;
				case ConstValues.Unit_10K:
					multiplier = 10000;
					break;
				case ConstValues.Unit_K:
					multiplier = 1000;
					break;
				default:
					multiplier = 1; 
					break;
			}

			return multiplier;
		}

        private string GetLastQuarter(DateTime? endTime)
		{
            int year = endTime.Value.Year;
            int month = endTime.Value.Month;

			if (month == 3)
				return (year - 1).ToString() + "-12";
			else
			{
				var t = year.ToString() + "-0" + (month - 3).ToString();
				return year.ToString() + "-0" + (month - 3).ToString();
			}
		}


        /// <summary>
        /// Get issuer rating
        /// </summary>
        /// <param name="id">COM_UNI_CODE</param>
        /// <returns><![CDATA[List<RATE_ORG_CRED_HIS>]]></returns>
        public List<RATE_ORG_CRED_HIS> GetIssuerRating(string id)
	    {
            var paramArray = new[]
            {
                new OracleParameter("P_ID", id),
                new OracleParameter("P_Culture", Thread.CurrentThread.CurrentUICulture.Name),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            return DataTableSerializer.ToList<RATE_ORG_CRED_HIS>(GetZCXDataSetBySp("GetIssuerRating", paramArray).Tables[0]);
	    }

	    public CmaFile GetRatingFileData(long id)
	    {
            return FileService.GetFileById(id, "ZCX");
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

	}
}
