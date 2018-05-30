using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Data;
using VAV.DAL.Common;
using VAV.Model.Data;
using VAV.Model.Data.Bond;
using VAV.DAL.Report;
using VAV.Model.Data.OpenMarket;


namespace VAV.DAL.Services
{
	/// <summary>
	/// Report Service
	/// </summary>
	public class ReportService : BaseService
	{
		private Dictionary<int,ReportInfo> _reportInfoDict;

			/// <summary>
		/// Inject StandardReportRepository for report service
		/// </summary>
		[Dependency]
		public IStandardReportRepository StandardReportRepository { get; set; }

		/// <summary>
		/// Inject StandardReportRepository for report service
		/// </summary>
		[Dependency]
		public IOpenMarketReportRepository OpenMarketReportRepository { get; set; }

				/// <summary>
		/// Inject StandardReportRepository for report service
		/// </summary>
		[Dependency]
        public IBondReportRepository BondReportRepository { get; set; }



		/// <summary>
		/// Gets the report by id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="parameter">The parameter.</param>
		/// <returns></returns>
		public BaseReport GetReportById(int reportId, ReportParameter parameter)
		{

			if (reportId > 0)
			{
				var standardReport = new StandardReport(reportId)
				{
					ResultDataTable = StandardReportRepository.GetReportById(reportId, parameter),
					Columns = StandardReportRepository.GetReportColumns(reportId).ToList(),
					ExtraHeaderCollection = StandardReportRepository.GetExtraHeaderById(reportId).ToList()
				};

				if (parameter.Unit != null && parameter.Unit != ConstValues.Unit_100M)
				{
					foreach (var c in standardReport.Columns)
					{
						if (c.ColumnType == null || !c.ColumnType.Equals("decimal")) continue;
						foreach (var r in standardReport.ResultDataTable.AsEnumerable())
						{
							if (r[c.ColumnName] != null && r[c.ColumnName].GetType() != typeof(DBNull))
								r[c.ColumnName] = Convert.ToDecimal(SwitchAmountUnit(parameter.Unit, Convert.ToDouble(r[c.ColumnName])));
						}
					}
				}
				return standardReport;
			}
			return new StandardReport(reportId);
		}

        public BaseReport GetStructureReportById(int reportId, ReportParameter parameter)
        {

            if (reportId > 0)
            {
                var standardReport = new StandardReport(reportId)
                {
                    ResultDataTable = StandardReportRepository.GetStructureReportById(reportId, parameter),
                    Columns = StandardReportRepository.GetReportColumns(reportId).ToList(),
                    ExtraHeaderCollection = StandardReportRepository.GetExtraHeaderById(reportId).ToList()
                };

                
                return standardReport;
            }
            return new StandardReport(reportId);
        }

		/// <summary>
		/// Gets the report info by id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public ReportInfo GetReportInfoById(int id)
		{
			if (_reportInfoDict == null)
			{
				_reportInfoDict = new Dictionary<int, ReportInfo>();
				var infos = StandardReportRepository.GetReportInfo();
				foreach (var reportInfo in infos)
				{
					if(!_reportInfoDict.ContainsKey(reportInfo.ReportId))
					{
						_reportInfoDict.Add(reportInfo.ReportId,reportInfo);
					}
				}
			}
			if(_reportInfoDict.ContainsKey(id))
			{
				return _reportInfoDict[id];
			}
			return new ReportInfo(string.Empty,string.Empty,-1,string.Empty,string.Empty,string.Empty,string.Empty);
		}

		/// <summary>
		/// Get data used by open market report
		/// </summary>
		/// <returns></returns>
		public IEnumerable<OpenMarketRepo> GetOpenMarketRepo(DetailDataReportParams reportPara, GlobalValueParams globalValues)

		{
			IEnumerable<OpenMarketRepo> openMarketRepo = OpenMarketReportRepository.GetOpenMarketRepo(reportPara);
			IEnumerable<OpenMarketRepo> repoRet = SetOptionType(reportPara, openMarketRepo, globalValues);
			return repoRet;
		}

		/// <summary>
		/// set every item's OptionType in openMarketRepo
		/// </summary>
		/// <param name="type"> the type that seleceted from the drop down list</param>
		/// <param name="repo">the object that returned from the service layer</param>
		/// <returns></returns>
		public List<OpenMarketRepo> SetOptionType(DetailDataReportParams reportPara, IEnumerable<OpenMarketRepo> repo, GlobalValueParams globalValues)
		{
			List<OpenMarketRepo> repoRet = new List<OpenMarketRepo>();
            var repoList = repo.Where(r => r.IssueDate != null).Select(r => r).ToList();
            foreach (OpenMarketRepo item in repoList)
			{
				string typeTemp = item.OperationType;
				int flag = 0;
				switch (item.OperationType)
				{
					case ConstValues.Type_CBankBill:
						item.Category = globalValues.Type_CBankBill;
						item.OperationType = globalValues.Type_CBankBill;
						flag = -1;
						break;
					case ConstValues.Type_CBankBillIE:
						item.Category = globalValues.Type_CBankBill;
						item.OperationType = globalValues.Type_CBankBillIE;
						flag = 1;
						break;
					case ConstValues.Type_FMD:
						item.Category = globalValues.Type_FMD;
						item.OperationType = globalValues.Type_FMD;
						flag = 1;
						break;
					case ConstValues.Type_FMDIE:
						item.Category = globalValues.Type_FMD;
						item.OperationType = globalValues.Type_FMDIE;
						flag = -1;
						break;
                    case ConstValues.Type_MLF:
                        item.Category = globalValues.Type_MLF;
                        item.OperationType = globalValues.Type_MLF;
                        flag = 1;
                        break;
                    case ConstValues.Type_MLFIE:
                        item.Category = globalValues.Type_MLFIE;
                        item.OperationType = globalValues.Type_MLFIE;
                        flag = -1;
                        break;
					case ConstValues.Type_ReverseRepo:
						item.Category = globalValues.Type_ReverseRepo;
						item.OperationType = globalValues.Type_ReverseRepo;
						flag = 1;
						break;
					case ConstValues.Type_ReverseRepoIE:
						item.Category = globalValues.Type_ReverseRepo;
						item.OperationType = globalValues.Type_ReverseRepoIE;
						flag = -1;
						break;
					case ConstValues.Type_Repo:
						item.Category = globalValues.Type_Repo;
						item.OperationType = globalValues.Type_Repo;
						flag = -1;
						break;
					case ConstValues.Type_RepoIE:
						item.Category = globalValues.Type_Repo;
						item.OperationType = globalValues.Type_RepoIE;
						flag = 1;
						break;
					default:
						break;
				}
				SetOptionDirection(item, flag, globalValues);
				SetOptionTerm(globalValues.CurrentContext,item);
				item.Volume = SwitchAmountUnit(reportPara.Unit, item.Volume);
				item.Amount = SwitchAmountUnit(reportPara.Unit, item.Amount);
				item.Term =Convert.ToInt32(((DateTime)item.MaturityDate).Subtract((DateTime)item.IssueDate).Duration().Days);
				if (reportPara.Type == ConstValues.Type_All)
				{
					repoRet.Add(item);
				}

				else if ( typeTemp == reportPara.Type || typeTemp == reportPara.Type+"IE")
				{
					repoRet.Add(item);
				}
				else if (reportPara.Type.Split(',').Contains(typeTemp.Replace("IE","")))
				{
					repoRet.Add(item);
				}
			}
			return repoRet;
		}

		public void SetOptionDirection(OpenMarketRepo repo, int flag, GlobalValueParams globalValues)
		{
			if (flag == 1)
			{
				repo.Direction = globalValues.Direction_injection;
			}
			else if (flag == -1)
			{
				repo.Direction = globalValues.Direction_withdrawal;
				repo.Amount = -repo.Amount;
				repo.Volume = -repo.Volume;
			}
		}

		public double? SwitchAmountUnit(string unit, double? num)
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
			return num * multiplier;
		}

		public void SetOptionTerm(string currentConetext, OpenMarketRepo repo)
		{
			switch (currentConetext)
			{
				case ConstValues.english:
					repo.OperationTerm = repo.TermEn;
					break;
				case ConstValues.chinese:
					repo.OperationTerm = repo.TermCn;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Get data used by open market report
		/// </summary>
		/// <returns></returns>
		public IEnumerable<BondIssueRate> GetBondIssueRatesRepo(BondIssueParams reportParams)
		{
			return BondReportRepository.GetBondIssueRatesRepo(reportParams);
		}

		/// <summary>
		/// Get bond issue amouont statitical data
		/// </summary>
		/// <param name="reportParams"></param>
		/// <returns></returns>
		public IEnumerable<BondIssueAmount> GetIssueAmount(BondIssueAmountParams reportParams)
		{
			Dictionary<string, int> typeOrderDic = CacheGet<Dictionary<string, int>>("TypeOrder");

			if (typeOrderDic == null)
			{
				typeOrderDic = BondReportRepository.GetTypeOrder();
				CacheSet("TypeOrder", typeOrderDic);
			}
            return BondReportRepository.GetBondIssueAmountNew(reportParams);
		}

		/// <summary>
		/// Get bond detail by type and date
		/// </summary>
		/// <param name="reportParams"></param>
		/// <returns></returns>
		public IEnumerable<BondDetail> GetBondDetailByTypeAndSubType(BondDetailParams reportParams)
		{
			return BondReportRepository.GetBondDetailByTypeAndSubType(reportParams);
		}


		/// <summary>
		/// Get bond depository data for isssue bond and mutarity bond
		/// </summary>
		/// <param name="param"></param>
		/// <param name="isIssued"></param>
		/// <returns></returns>
		public IEnumerable<BondDetail> GetBondDetailByType(BondDetailParams param)
		{
			return BondReportRepository.GetBondDetailByType(param);
		}
	}
}
