using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace VAV.Model.Data.CnE
{
    public class CoalFilterItem
    {
        public CoalFilterItem()
        {
            Items = new List<SelectListItem>();
        }
        public List<SelectListItem> Items { get; set; }
        public int FilterId { get; set; }
        public string Name_CN { get; set; }
        public string Name_EN { get; set; }
        public string FieldName { get; set; }
        public string DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? Name_CN : Name_EN; } }
    }

    public class CoalFilterObj
    {
        public CoalFilterObj(int id)
        {
            ReportId = id;
        }
        public CoalFilterItem PrimaryDropdown { get; set; }
        public CoalFilterItem SecondDropdown { get; set; }
        public int ReportId { get; private set; }
    }

    public class CoalChartLegendDisplay
    {
        public int ReportID { get; set; }
        public string ChartSQL { get; set; }
        public string Unit { get; set; }
        public string Legend { get; set; }
        public string ChartYLabel_CN { get; set; }
        public string ChartYLabel_EN { get; set; }
        public string ChartTitle_CN { get; set; }
        public string ChartTitle_EN { get; set; }

        public CoalChartLegendDisplay(int ReportID, string ChartSQL, string Unit, string Legend, string ChartYLabel_CN, string ChartYLabel_EN, string ChartTitle_CN, string ChartTitle_EN)
        {
            this.ReportID = ReportID;
            this.ChartSQL = ChartSQL;
            this.Unit = Unit;
            this.Legend = Legend;
            this.ChartYLabel_CN = ChartYLabel_CN;
            this.ChartYLabel_EN = ChartYLabel_EN;
            this.ChartTitle_CN = ChartTitle_CN;
            this.ChartTitle_EN = ChartTitle_EN;
        }


        public string ChartYLabel_DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? ChartYLabel_CN : ChartYLabel_EN; } }
        public string ChartTitle_DisplayName { get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? ChartTitle_CN : ChartTitle_EN; } }

    }


}
