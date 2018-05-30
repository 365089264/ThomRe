using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    public class OpenMarketRepo
    {
        public string Code { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string Direction { get; set; }
        public string OperationType { get; set; }
        public string AssetId { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PirceRate { get; set; }
        public decimal? RefRate { get; set; }
        public string OperationTerm { get; set; }
        public decimal? Term { get; set; }
        public string TermEn { get; set; }
        public string TermCh {get; set;}
    }
}
