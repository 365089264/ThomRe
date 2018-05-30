namespace VAV.Model.Data.Bond
{
    public class BondCityAndLocal : BaseModel
    {
        public string ProvinceKey { get; set; }
        public string ProvinceValue { get; set; }
        public decimal? EndBalance { get; set; }
        public decimal? InitialBalance { get; set; }
        public int Issues { get; set; }
        public decimal? IssuesPercent { get; set; }
        public decimal? IssuesAmount { get; set; }
        public decimal? IssuesAmountPercent { get; set; }
        public int? MaturityBonds { get; set; }
        public decimal? MaturityAmount { get; set; }
        public int? EndIssues { get; set; }
    }
}
