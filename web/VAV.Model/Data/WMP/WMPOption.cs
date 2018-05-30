using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.WMP
{
    public class WMPBankTypeOption
    {
        public string Code { get; set; }
        public string TypeName { get; set; }
    }

    public class WMPBankOption
    {
        public string TypeCode { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
    }

    public class WMPCurrencyOption
    {
        public decimal? Type { get; set; }
        public string Name { get; set; }
    }

    public class WMPYieldOption
    {
        public int? Type { get; set; }
        public string Name { get; set; }
    }

    public class WMPInvestOption
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class WMPReportTeypOption
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class WMPProvinceOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class WMPCityOption
    {
        public int? Code { get; set; }
        public string Name { get; set; }
    }

    public class WMPAreaOption
    {
        public int? Code { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
    }
}
