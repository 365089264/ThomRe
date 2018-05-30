using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using VAV.DAL.CnE;
using VAV.Model.Data;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.Web.Localization;
using VAV.Model.Data.CnE;
using VAV.Model.Chart;
namespace VAV.Web.Controllers
{
    public class CNEController : CnEBaseController
    {
        public override ActionResult GetReport(int id)
        {
            var reportInfo = ReportService.GetReportInfoById(id);
            ViewBag.ID = reportInfo.ReportId;
            ViewBag.Name = reportInfo.DisplayName;
            if (reportInfo.ViewName == "Coal")
            {
                var coalRepo = new CoalRepository();
                var chart = coalRepo.GetChartLegendByReportId(id);
                ViewBag.ChartYLabel = chart.ChartYLabel_DisplayName;
                ViewBag.ChartTitle = chart.ChartTitle_DisplayName;
                ViewBag.Legend = chart.Legend;
                ViewBag.Unit = chart.Unit;
                ViewBag.Filters = coalRepo.GetDataFiltersWhichHasDataByReportId(id, reportInfo.TableName);
            }
            return PartialView(reportInfo.ViewName);
        }

        public ActionResult GetRSNews(DateTime startDate, DateTime endDate, string ntitle, int pageNo, bool isHTML = false)
        {
            int pageCount;
            List<CommodityNews> list = NewRespository.GetCommodityNewsData(startDate, endDate, ntitle, pageNo, out pageCount);
            List<CommodityNews> newsList = list.Select(t => new CommodityNews() { TimeString = t.NewsTime.ToString("yyyy-MM-dd   HH:mm:ss"), NewsId = t.NewsId, NewsTitle = t.NewsTitle }).ToList<CommodityNews>();

            var result = new
            {
                Data = newsList,
                Total = pageCount,
                CurrentPage = pageNo,
                PageSize = 15
            };
            return isHTML ? Json(result, "text/html", JsonRequestBehavior.AllowGet) : Json(result, JsonRequestBehavior.AllowGet);

        }
        public string GetSingleNews(string newID)
        {
            string content = NewRespository.GetSingleNewsContent(newID);
            return content.Replace("\r\n", "</br>");
        }
        [Localization]
        public ActionResult ExportOutputChartData()
        {
            var data = Respository.GetSDLocalRefinerExcelData();
            var index = Respository.GetSDLocalRefineryExcelIndex();
            var valuation = Respository.GetSDLocalRefineryExcelValuation();
            var stock = Respository.GetSDLocalRefineryExcelStock();
            return new SDLRChartExcelResult(data, index, valuation, stock);
        }

        [Localization]
        public ActionResult GetSDLocalRefineryChartData(DateTime start, DateTime end, bool isHTML = false)
        {
            var result = Respository.GetSDLocalRefineryChartData();
            string defaultCode;
            var companyList = GetSdLocalRefineryCompany(CultureHelper.IsEnglishCulture(), out defaultCode);
            string reDate;
            var dailyOutputTable = ConvertToDicForOutput(start, end, defaultCode, out reDate);
            var deviceInfoTable = ConvertToDicForDevice(CultureHelper.IsEnglishCulture(), defaultCode, reDate);
            var data = new { chart = result, companyList, deviceInfoTable, dailyOutputTable, reDate };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }


        [Localization]
        public JsonResult GetSDLocalRefineryIndex()
        {
            return Json(Respository.GetSDLocalRefineryIndex(), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public JsonResult GetSDLocalRefineryValue()
        {
            return Json(Respository.GetSDLocalRefineryValuation(), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public JsonResult GetSDLocalRefineryStock()
        {
            return Json(Respository.GetSDLocalRefineryStock(), JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetBoottomTable(DateTime start, DateTime end, string code)
        {
            string reDate;
            var dailyOutputTable = ConvertToDicForOutput(start, end, code, out reDate);
            var deviceInfoTable = ConvertToDicForDevice(CultureHelper.IsEnglishCulture(), code, reDate);
            var data = new { deviceInfoTable, dailyOutputTable, reDate };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForOutput(DateTime start, DateTime end, string code, string company)
        {
            if (string.IsNullOrEmpty(company)) company = code;
            string reDate;
            var data = Respository.GetSdLocalRefineryDailyOutputTable(start, end, code, out reDate);
            return new ExcelResult(data.AsEnumerable().AsQueryable(),
                                    GetOutputHeader(),
                                    GetOutputColumn(),
                                    company,
                                    company
                                    );
        }

        [Localization]
        public ActionResult ExportExcelForDevice(string reDate, string code, string company)
        {
            if (string.IsNullOrEmpty(company)) company = code;
            var data = Respository.GetSdLocalRefineryDeviceInfoTable(CultureHelper.IsEnglishCulture(), code, reDate);
            return new ExcelResult(data.AsEnumerable().AsQueryable(),
                                    GetDeviceHeader(),
                                    GetDeviceColumn(),
                                    company,
                                    company
                                    );
        }


        public JsonTable GetSdLocalRefineryCompany(bool isEnglish, out string defaultCode)
        {
            var result = Respository.GetSdLocalRefineryCompany(isEnglish);
            defaultCode = result.Rows[0][0].ToString();
            return BuidJsonTable(result);
        }

        public JsonTable BuidJsonTable(DataTable table)
        {
            var jTable = new JsonTable();
            var columns = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columns[i] = table.Columns[i].ColumnName;
            }
            foreach (DataRow row in table.Rows)
            {
                var currentRow = new Dictionary<string, string>();
                foreach (var column in columns)
                {
                    currentRow.Add(column, row[column].ToString());
                }
                jTable.RowData.Add(currentRow);
            }
            return jTable;
        }
        private List<Dictionary<string, string>> ConvertToDicForOutput(DateTime start, DateTime end, string code, out string reDate)
        {
            var dt = Respository.GetSdLocalRefineryDailyOutputTable(start, end, code, out reDate);
            var data = new List<Dictionary<string, string>>();
            foreach (DataRow row in dt.Rows)
            {
                var currentRow = new Dictionary<string, string>
                {
                    {"ReDate", UIGenerator.FormatCellValue(row["ReDate"], "datetime")},
                    {"ProcessCapacity", UIGenerator.FormatCellValue(row["ProcessCapacity"], "decimal")},
                    {"Gasoline", UIGenerator.FormatCellValue(row["Gasoline"], "decimal")},
                    {"Diesel", UIGenerator.FormatCellValue(row["Diesel"], "decimal")}
                };
                data.Add(currentRow);
            }
            return data;
        }

        private List<Dictionary<string, string>> ConvertToDicForDevice(bool isEnglish, string code, string reDate)
        {
            var data = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(reDate)) return data;

            var dt = Respository.GetSdLocalRefineryDeviceInfoTable(isEnglish, code, reDate);
            foreach (DataRow row in dt.Rows)
            {
                var currentRow = new Dictionary<string, string>
                {
                    {"Device", UIGenerator.FormatCellValue(row["Device"], "")},
                    {"YieldByTon", UIGenerator.FormatCellValue(row["YieldByTon"], "decimal")},
                    {"YieldByBarrel", UIGenerator.FormatCellValue(row["YieldByBarrel"], "decimal")}
                };
                data.Add(currentRow);
            }
            return data;
        }

        private string[] GetOutputHeader()
        {
            var header = new[] {
                Resources.CnE.CNE_SdR_DailyOutputDate,
                Resources.CnE.CNE_SdR_DailyOutputCapacity,
                Resources.CnE.CNE_SdR_DailyOutputGasoline,
                Resources.CnE.CNE_SdR_DailyOutputDiesel
            };
            return header;
        }

        private string[] GetOutputColumn()
        {
            var columns = new[] {
                "ReDate",
                "ProcessCapacity",
                "Gasoline",
                "Diesel"
            };
            return columns;
        }

        private string[] GetDeviceHeader()
        {
            var header = new[] {
                     Resources.CnE.CNE_SdR_Device,
                     Resources.CnE.CNE_SdR_DeviceYieldByTon,
                     Resources.CnE.CNE_SdR_YieldByBarrel
            };
            return header;
        }
        private string[] GetDeviceColumn()
        {
            var columns = new[] {
                     "Device",
                     "YieldByTon",
                     "YieldByBarrel"
            };
            return columns;
        }

        #region Coal
        [Localization]
        public ActionResult GetCoalTableData(int reportId, int currentPage, string queryStr, string order, bool isHTML = false)
        {
            string key;
            var jsonTable = BuildCoalJsonTable(reportId, currentPage, queryStr, order, out key);
            return isHTML ? Json(jsonTable, "text/html", JsonRequestBehavior.AllowGet) : Json(jsonTable, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public ActionResult GetCoalData(int reportId, int currentPage, string queryStr, string order, bool isHTML = false)
        {
            string key;
            var table = BuildCoalJsonTable(reportId, currentPage, queryStr, order, out key);
            var chart = new List<object> { new { data = BuildCoalChartData(reportId, key), key = key } };
            var data = new { table, chart };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetCoalChartDetailList(int reportId, string keyList)
        {
            var dataSeries = new List<object>();
            var keys = keyList.Split(new[] { "@_@" }, StringSplitOptions.None);
            foreach (var key in keys)
            {
                var data = BuildCoalChartData(reportId, key);
                dataSeries.Add(new { data, key });
            }
            return Json(dataSeries, JsonRequestBehavior.AllowGet);
        }

        private JsonTable BuildCoalJsonTable(int reportId, int currentPage, string queryStr, string order, out string selectKey)
        {
            var coalRepository = new CoalRepository();
            var columns = coalRepository.GetColumnDefinitionByReportId(reportId);
            var totalRows = 0;
            var rptInfo = ReportService.GetReportInfoById(reportId);
            if (string.IsNullOrWhiteSpace(queryStr))
            {
                var filterObj = coalRepository.GetDataFiltersWhichHasDataByReportId(reportId, rptInfo.TableName);
                if (filterObj.PrimaryDropdown != null && filterObj.PrimaryDropdown.Items.Count > 0)
                {
                    queryStr = filterObj.PrimaryDropdown.FieldName + "= '" +
                               filterObj.PrimaryDropdown.Items.First(x => x.Selected).Value + "' ";
                }
                if (filterObj.SecondDropdown != null && filterObj.SecondDropdown.Items.Count > 0)
                {
                    queryStr += " and " + filterObj.SecondDropdown.FieldName + "= '" +
                               filterObj.SecondDropdown.Items.First(x => x.Selected).Value + "' ";
                }
            }
            var dt = coalRepository.GetPagedTableData(columns, rptInfo.TableName, order, queryStr, currentPage, out totalRows);
            if (dt.Rows.Count > 0)
            {
                selectKey = dt.Rows[0]["KeyWord"].ToString();
            }
            else
            {
                selectKey = string.Empty;
            }
            var jsonTable = BuildJsonTable(dt, columns, totalRows, currentPage, 50);
            return jsonTable;
        }
        [Localization]
        public ActionResult GetSubFilter(int sfilterId, string selectedPrimaryItem)
        {
            var filter = CoalRepository.GetSubDropdownByFilterId(sfilterId, selectedPrimaryItem);
            return Json(filter, JsonRequestBehavior.AllowGet);
        }

        private object BuildCoalChartData(int reportId, string key)
        {
            var table = CoalRepository.GetCoalChartDataTable(reportId, key);
            var data = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                var point = new List<object>
                {
                    (Convert.ToDateTime(row[0])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(row[1])
                };
                data.Add(point.ToArray());
            }
            return data;
        }

        public ActionResult GetCoalChartData(int reportId, string key)
        {
            var data = BuildCoalChartData(reportId, key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPortSchedularPagedData(string strOrder, string strWhere, int pageIndex)
        {
            int recordCount = 0;
            var tb = CoalRepository.GetCoalPortSchedularPagedData("Coal_port_traffic_MaxTable", "area_uni_code,area_chi_name,end_date,A001,A002,A003,A004,A005,A006,A007,A008,A009,A010,A011,A012,A013,par_name", strOrder, strWhere, pageIndex, 1, 50, out recordCount);
            var jtb = BuidJsonTable(tb);
            jtb.CurrentPage = pageIndex;
            jtb.Total = recordCount;
            jtb.PageSize = 50;
            return Json(jtb, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public JsonResult GetPageChartData(string key)
        {
            var ChartData = GetCoalChartData(key);
            return Json(ChartData, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public JsonResult GetPageDataFirst(string strOrder, string strWhere, int pageIndex)
        {
            int recordCount = 0;
            var tb = CoalRepository.GetCoalPortSchedularPagedData("Coal_port_traffic_MaxTable", "area_uni_code,area_chi_name,end_date,A001,A002,A003,A004,A005,A006,A007,A008,A009,A010,A011,A012,A013,par_name", strOrder, strWhere, pageIndex, 1, 50, out recordCount);
            var jtb = BuidJsonTable(tb);
            jtb.CurrentPage = pageIndex;
            jtb.Total = recordCount;
            jtb.PageSize = 50;
            var ChartData = GetCoalChartData(tb.Rows[0]["area_uni_code"].ToString());
            object dto = new { Chart = ChartData, Table = jtb };
            return Json(dto, JsonRequestBehavior.AllowGet);
        }
        [Localization]
        public ExcelResult ExportExcelForPortSchedularTable(string strOrder, string strWhere, int pageIndex, string strHeader, int isChart = 0, string key = "", int term = 0, string startTime = "")
        {
            strOrder = isChart == 0 ? (string.IsNullOrEmpty(strOrder) ? "area_uni_code desc" : strOrder) : (string.IsNullOrEmpty(strOrder) ? "END_DATE desc" : " END_DATE desc," + strOrder);
            string tableName = isChart == 0 ? "Coal_port_traffic_MaxTable" : "Coal_port_traffic";
            int pageSize = isChart == 0 ? 50 : 1000;
            pageIndex = isChart == 0 ? pageIndex : 1;
            string starttime = !string.IsNullOrEmpty(startTime) ? DateTime.Parse(startTime).AddMonths(term).ToString("yyyy-MM-dd") : "";
            string endTime = startTime;
            string filterWhere = term == -100 ? "area_uni_code='" + key + "'" : "area_uni_code='" + key + "' and END_DATE>='" + starttime + "' and END_DATE<='" + endTime + "'";
            strWhere = isChart == 0 ? strWhere : filterWhere;
            int total;
            var tableData = CoalRepository.GetCoalPortSchedularPagedData(tableName, "area_chi_name,end_date,A001,A002,A003,A004,A005,A006,A007,A008,A009,A010,A011,A012,A013,par_name", strOrder, strWhere, pageIndex, 1, pageSize, out total);
            string[] strs = strHeader.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var excelTable = new DataTable();
            var columns = new List<DataColumn>();
            strs.ToList().ForEach((s) => { columns.Add(new DataColumn(s)); });
            excelTable.Columns.Clear();
            excelTable.Columns.AddRange(columns.ToArray());
            var itor = tableData.Rows.GetEnumerator();
            while (itor.MoveNext())
            {
                DataRow dr = (DataRow)itor.Current;
                DataRow excelRow = excelTable.NewRow();
                for (int i = 0; i < tableData.Columns.Count; i++)
                {
                    excelRow[i] = dr[i];
                }
                excelTable.Rows.Add(excelRow);
            }
            string name = Resources.CnE.Coal_Schedular;
            var data = excelTable.AsEnumerable().AsQueryable();
            return new ExcelResult(data, strs, strs, name, name);
        }
        private object GetCoalChartData(string key)
        {
            var chart = new ChartJsonData();
            var dataSeries = new List<object>();
            var dataA004 = new List<object>();
            var dataA005 = new List<object>();
            var dataA007 = new List<object>();
            var entrys = CoalRepository.GetCoalTrafficPortData(key);
            entrys.ForEach(t =>
            {
                var point = new List<object>
                {
                    (Convert.ToDateTime(t.end_date)).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(t.A004)
                };
                dataA004.Add(point.ToArray());
                var pointA005 = new List<object>
                {
                    (Convert.ToDateTime(t.end_date)).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(t.A005)
                };
                dataA005.Add(pointA005.ToArray());
                var pointA007 = new List<object>
                {
                    (Convert.ToDateTime(t.end_date)).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(t.A007)
                };
                dataA007.Add(pointA007.ToArray());

            });
            dataSeries.Add(new { data = dataA004.ToArray(), name = entrys[0].PortName + "-" + Resources.CnE.Coal_Traffic_Railway, marker = new { enabled = false } });
            dataSeries.Add(new { data = dataA005.ToArray(), name = entrys[0].PortName + "-" + Resources.CnE.Coal_Traffic_Road, marker = new { enabled = false } });
            dataSeries.Add(new { data = dataA007.ToArray(), name = entrys[0].PortName + "-" + Resources.CnE.Coal_Traffic_Trough, marker = new { enabled = false } });
            chart.SeriesData = dataSeries.ToArray();

            return chart.ToJson();

        }

        [Localization]
        public ActionResult ExportExcelForCoalDetail(int reportId, int currentPage, string filters, string order, string name)
        {
            string key;
            var table = BuildCoalJsonTable(reportId, currentPage, filters, order, out key);
            var jP = new JsonExcelParameter { Table = table, TableName = name, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public ActionResult ExportExcelForCoalChartDetail(int reportId, string key, string name)
        {
            var table = CoalRepository.GetCoalChartDataTable(reportId, key);
            var coalRepo = new CoalRepository();
            var chart = coalRepo.GetChartLegendByReportId(reportId);
            return new ExcelResult(table.AsEnumerable().AsQueryable(),
                                   new[] { Resources.Global.Date, chart.ChartYLabel_DisplayName },
                                   new[] { "PointDate", "PointValue" },
                                   name,
                                   name
                                   );
        }
        #endregion


    }
}
