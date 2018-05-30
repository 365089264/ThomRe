using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.OpenMarket
{
    public class OpenMarketRepo
    {
        public string Code { get; set; }
        public bool IsSumItem { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? MaturityDate { get; set; }       
        public string Direction { get; set; }
        public string OperationType { get; set; }
        public string AssetId { get; set; }
        public double? Volume { get; set; }
        public double? Amount { get; set; }
        public double? PirceRate { get; set; }
        public double? RefRate { get; set; }
        public string OperationTerm { get; set; }
        public int? Term { get; set; }
        public string TermEn { get; set; }
        public string TermCn {get; set;}
        public string Category { get; set; }
    }

    public class MonetaryAndReturnAnalysisSummaryModel
    {
        public string Category { get; set; }
        public DateTime CategoryStartDate { get; set; }
        public DateTime CategoryEndDate { get; set; }
        public double? RepoInjection { get; set; }
        public double? ReverseRepoInjection { get; set; }
        public double? CbbInjection { get; set; }
        public double? FmdInjection { get; set; }
        public double? MlfInjection { get; set; }
        public double? RepoWithdrawal { get; set; }
        public double? ReverseRepoWithdrawal { get; set; }
        public double? CbbWithdrawal { get; set; }
        public double? FmdWithdrawal { get; set; }
        public double? MlfWithdrawal { get; set; }
        public double? NetInjection { get; set; }
        public double? NetWithdrawal { get; set; }
        public double? NetInjectionWithdrawal { get; set; }
        public double? SumInjectionWithdrawal { get; set; }
        public List<OpenMarketRepo> OpenMarketRepoList { get; set; }
    }

    public class RatesAnalysisModel
    {
        public string Category { get; set; }
        public string OperationType { get; set; }
        public string OperationTerm { get; set; }
        public List<OpenMarketRepo> OpenMarketRepoList { get; set; }
    }
}
