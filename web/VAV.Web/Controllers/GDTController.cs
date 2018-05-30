using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using VAV.DAL.CnE;
using VAV.DAL.Services;
using VAV.Entities;
using VAV.Model.Data;
using VAV.Web.Common;
using VAV.Web.Extensions;
using VAV.Web.Localization;
using VAV.Model.Data.CnE.GDT;
using VAV.Web.ViewModels.GDT;
using VAV.Model.Chart;

namespace VAV.Web.Controllers
{
    public class GDTController : CnEBaseController
    {

        [Dependency]
        public GDTService NewGdtService { get; set; }

        const int PageSize = 50;

        [Dependency]
        public NewGdtRespository newGdtRespository { get; set; }

        [Dependency]
        public UserColumnService UserColumnService { get; set; }

        [Localization]
        public ActionResult GDTHome(int reportId)
        {
            var reportInfo = ReportService.GetReportInfoById(reportId);
            List<TabNode> nodes = NewGdtService[reportId];
            return PartialView(reportInfo.ViewName, nodes);
        }

        [Localization]
        public ActionResult GetTabItem(int itemId, int reportId)
        {
            var currentNode = NewGdtService[reportId].FirstOrDefault(t => t.ItemID == itemId);
            var viewModelType = System.Type.GetType(currentNode.ViewModelName1);
            GdtBaseViewModel viewModel = null;
            if (viewModelType != null)
            {
                viewModel = (GdtBaseViewModel)Activator.CreateInstance(viewModelType);
                viewModel.ReportId = reportId;
                viewModel.ItemId = itemId;
                viewModel.GdtService = NewGdtService;
                viewModel.Legend = currentNode.Legend;
                viewModel.Initialization();
            }
            else
            {
                ViewBag.itemID = itemId;
                ViewBag.ID = reportId;
                ViewBag.ContentId = reportId + "_" + itemId;
            }
            return PartialView(currentNode.ViewName1, viewModel);
        }

        [Localization]
        public ActionResult GetPriceData(int id, int itemId, string category, string order = "re_date desc,orderKey", int currentPage = 1, int total = 0, string term = "5Y", bool isHTML = false, string key = "")
        {
            string defaultkey;
            var jsonTable = GetPriceTable(id, itemId, currentPage, category, order, out defaultkey);
            var currentTime = jsonTable.RowData.Count > 0 ? jsonTable.RowData[0]["re_date"] : DateTime.Now.ToString();
            var chartTable = new DataTable();

            chartTable = newGdtRespository.GetPriceChart(itemId, string.IsNullOrWhiteSpace(key) ? defaultkey : key,
            currentTime);

            var chartJsonData = BuidPriceChart(chartTable);
            var data = new { chart = chartJsonData, table = jsonTable };
            var jr = isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
            jr.MaxJsonLength = int.MaxValue;
            return jr;
        }

        [Localization]
        public ActionResult ExportExcelForPriceDetail(int reportId, int itemId, int currentPage, string category, string order = "[re_date] desc,[orderKey]")
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, reportId, itemId, 1, 2000, category, order, out total);
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   columnDefinitions.Select(x => x.HearderDisplayName).ToArray(),
                                   columnDefinitions.Select(x => x.ColumnDisplayName).ToArray(),
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }
        [Localization]
        public ActionResult ExportExcelForFanYa(int reportId, int itemId, string key, string term)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var allHeardes = columnDefinitions.Select(x => new { x.COLUMNNAME_EN, x.HearderDisplayName }).ToArray();
            var templateTable = GetGdtTable(columnDefinitions, reportId, itemId, 1, 1000, "keyword='" + key + "'", "keyword desc", out total).Rows[0];
            var tableData = newGdtRespository.GetPriceChartTable(itemId, key, term, templateTable["re_date"].ToString(), 1);
            DataTable dataTable = new DataTable();
            foreach (var header in allHeardes)
            {
                dataTable.Columns.Add(header.HearderDisplayName);
            }
            for (int i = 0; i < tableData.Rows.Count; i++)
            {
                DataRow dr = dataTable.NewRow();
                for (int j = 0; j < tableData.Columns.Count; j++)
                {
                    dr[j] = tableData.Rows[i][j];
                }
                dataTable.Rows.Add(dr);
            }
            var unitHeader = allHeardes.Where(t => t.HearderDisplayName == "Unit" || t.HearderDisplayName == "单位").Select(x => new { x.COLUMNNAME_EN, x.HearderDisplayName }).FirstOrDefault();
            var productHeader = allHeardes.Where(t => t.HearderDisplayName == "ProductName" || t.HearderDisplayName == "品种").Select(x => new { x.COLUMNNAME_EN, x.HearderDisplayName }).FirstOrDefault();
            for (int i = 0; i < tableData.Rows.Count; i++)
            {
                if (unitHeader != null)
                {
                    dataTable.Rows[i][unitHeader.HearderDisplayName] =
                        templateTable.Table.Columns.Contains(unitHeader.COLUMNNAME_EN)
                            ? templateTable[unitHeader.COLUMNNAME_EN]
                            : templateTable.Table.Columns.Contains(unitHeader.COLUMNNAME_EN.ToLower()
                                .Replace("_en", "_cn"))
                                ? templateTable[unitHeader.COLUMNNAME_EN.ToLower().Replace("_en", "_cn")]
                                : templateTable[unitHeader.COLUMNNAME_EN.ToLower().Replace("_en", "")];
                }
                dataTable.Rows[i][productHeader.HearderDisplayName] = templateTable.Table.Columns.Contains(productHeader.COLUMNNAME_EN) ? templateTable[productHeader.COLUMNNAME_EN] :
                    templateTable.Table.Columns.Contains(productHeader.COLUMNNAME_EN.ToLower().Replace("_en", "_cn")) ? templateTable[productHeader.COLUMNNAME_EN.ToLower().Replace("_en", "_cn")] : templateTable[productHeader.COLUMNNAME_EN.ToLower().Replace("_en", "")];
            }
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(),
                                 allHeardes.Select(x => x.HearderDisplayName).ToArray(),
                                 allHeardes.Select(x => x.HearderDisplayName).ToArray(),
                                 MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                 MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                 );

        }
        [Localization]
        public ActionResult ExportExcelForPriceChartDetail(int reportId, int itemId, string key, string term)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var allHeardes = columnDefinitions.Select(x => new { x.COLUMNNAME_EN, x.HearderDisplayName }).ToArray();
            var templateRow = GetGdtTable(columnDefinitions, reportId, itemId, 1, 1, "keyword='" + key + "'", "keyword desc", out total).Rows[0];
            var tableData = new DataTable();
            tableData = newGdtRespository.GetPriceChartTable(itemId, key, term, templateRow["re_date"].ToString(), 1);

            var dataTable = new DataTable();
            foreach (var header in allHeardes)
            {
                dataTable.Columns.Add(header.HearderDisplayName);
            }
            for (int i = 0; i < tableData.Rows.Count; i++)
            {
                var dr = dataTable.NewRow();
                for (int j = 0; j < columnDefinitions.Count(); j++)
                {
                    if (allHeardes[j].COLUMNNAME_EN.ToLower() == "price")
                    {
                        dr[j] = tableData.Rows[i]["price"];
                    }
                    else if (allHeardes[j].COLUMNNAME_EN.ToLower() == "re_date")
                    {
                        dr[j] = tableData.Rows[i]["re_date"];
                    }
                    else
                    {
                        dr[j] = templateRow[j];
                    }
                }
                dataTable.Rows.Add(dr);
            }
            return new ExcelResult(dataTable.AsEnumerable().AsQueryable(),
                                   allHeardes.Select(x => x.HearderDisplayName).ToArray(),
                                   allHeardes.Select(x => x.HearderDisplayName).ToArray(),
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult GetPriceDetail(int id, int itemId, int currentPage, string category, string order = "[re_date] desc,[orderKey]")
        {
            string key;
            var jsonTable = GetPriceTable(id, itemId, currentPage, category, order, out key);
            return Json(jsonTable, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetPriceChartDetail(int itemId, string key, string term, string reDate)
        {
            var chartTable = new DataTable();

            chartTable = newGdtRespository.GetPriceChart(itemId, key, reDate, term);

            var jsonChart = BuidPriceChart(chartTable);
            return Json(jsonChart, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPriceChartDetailList(int itemId, string keyList)
        {
            var dataSeries = new List<object>();
            var keys = keyList.Split(',');
            foreach (var key in keys)
            {
                var data = new List<object>();
                var dataTable = new DataTable();

                dataTable = newGdtRespository.GetPriceChartTable(itemId, key, "All", DateTime.Now.ToString("yyyy-MM-dd"));

                foreach (DataRow row in dataTable.Rows)
                {
                    var point = new List<object>
                    {
                        (Convert.ToDateTime(row["re_date"])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                        Convert.ToDouble(row["price"])
                    };
                    data.Add(point.ToArray());
                }
                dataSeries.Add(new { data = data.ToArray(), name = key, marker = new { enabled = false } });
            }
            var jr = Json(dataSeries, JsonRequestBehavior.AllowGet);
            jr.MaxJsonLength = int.MaxValue;
            return jr;
        }

        public DataTable GetGdtTable(COLUMNDEFINITION[] columnDefinitions, int reportId, int itemId, int currentPage, int pageSize, string category, string order, out int total)
        {
            var currentNode = NewGdtService[reportId].FirstOrDefault(t => t.ItemID == itemId);
            var strWhere = currentNode.TableFilter1;
            if (!string.IsNullOrEmpty(category)) strWhere += " and " + category;
            var dt = new DataTable();

            order = order.Replace('[', ' ').Replace(']', ' ');
            dt = newGdtRespository.GetDataPaged(currentNode.TableName1,
                                                            columnDefinitions.Select(x => x.ColumnDisplayName).Aggregate((a, b) => a + "," + b),
                                                            order,
                                                            strWhere,
                                                            currentPage,
                                                            pageSize,
                                                            1,
                                                            0,
                                                            out total
                                                            );

            return dt;
        }

        private JsonTable GetPriceTable(int id, int itemId, int currentPage, string category, string order, out string defaultkey)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, id, itemId, currentPage, PageSize, category, order, out total);
            defaultkey = tableData.Rows.Count > 0 ? tableData.Rows[0]["KeyWord"].ToString() : "";
            return BuidJsonTable(tableData, columnDefinitions, total, currentPage, PageSize);
        }

        private object BuidPriceChart(DataTable table)
        {
            var chart = new ChartJsonData();
            var dataSeries = new List<object>();
            var data = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                var point = new List<object>
                {
                    (Convert.ToDateTime(row["re_date"])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(row["price"])
                };
                data.Add(point.ToArray());
            }
            dataSeries.Add(new { data = data.ToArray(), name = Resources.CnE.GDT_PriceChartPriceName, marker = new { enabled = false } });
            chart.SeriesData = dataSeries.ToArray();
            return chart.ToJson();
        }

        #region  output


        [Localization]
        public ActionResult GetOutputData(int id, int itemId, string category, string order = "KeyWord,LatestDate desc", int currentPage = 1, int total = 0, bool isHTML = false)
        {
            string defaultkey;
            var jsonTable = GetOutputTable(id, itemId, currentPage, category, order, out defaultkey);
            var dataTable = new DataTable();
            dataTable = newGdtRespository.GetOutputChart(itemId, defaultkey);
            var chartJsonData = BuidOutputChart(dataTable);
            var data = new { chart = chartJsonData, table = jsonTable };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForOutputDetail(int reportId, int itemId, int currentPage, string category, string order = "KeyWord,LatestDate  desc")
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, reportId, itemId, 1, 2000, category, order, out total);
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   columnDefinitions.Select(x => x.HearderDisplayName).ToArray(),
                                   columnDefinitions.Select(x => x.ColumnDisplayName).ToArray(),
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult ExportExcelForOutputChartDetail(int reportId, int itemId, string key)
        {

            var tableData = newGdtRespository.GetOutputChartTable(itemId, key);
            var allHeardes = new string[tableData.Columns.Count];
            int i = 0;
            foreach (DataColumn dc in tableData.Columns)
            {
                allHeardes[i] = dc.ColumnName;
                i++;
            }
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   allHeardes,
                                   allHeardes,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult GetOutputDetail(int id, int itemId, int currentPage, string category, string order = "KeyWord,LatestDate  desc")
        {
            string key;
            var jsonTable = GetOutputTable(id, itemId, currentPage, category, order, out key);
            return Json(jsonTable, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetOutputChartDetail(int itemId, string key, string term, string reDate)
        {
            var tableData = newGdtRespository.GetOutputChart(itemId, key);
            var jsonChart = BuidOutputChart(tableData);
            return Json(jsonChart, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOutputChartDetailList(int itemId, string keyList)
        {
            var dataSeries = new List<object>();
            var keys = keyList.Split(',');
            foreach (var key in keys)
            {
                var data = new List<object>();
                var dataTable = newGdtRespository.GetOutputChartTable(itemId, key);

                foreach (DataRow row in dataTable.Rows)
                {
                    DateTime re_date = Convert.ToDateTime(row["re_date"]);
                    DateTime re_dateonlyym = new DateTime(re_date.Year, re_date.Month, 1);

                    var point = new List<object>
                    {
                        (re_dateonlyym).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                        Convert.ToDouble(row["output"])
                    };
                    data.Add(point.ToArray());
                }
                dataSeries.Add(new { data = data.ToArray(), name = key, marker = new { enabled = false } });
            }
            return Json(dataSeries, JsonRequestBehavior.AllowGet);
        }

        private JsonTable GetOutputTable(int id, int itemId, int currentPage, string category, string order, out string defaultkey)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, id, itemId, currentPage, PageSize, category, order, out total);
            defaultkey = tableData.Rows.Count > 0 ? tableData.Rows[0]["KeyWord"].ToString() : "";
            return BuidJsonTable(tableData, columnDefinitions, total, currentPage, PageSize);
        }

        private object BuidOutputChart(DataTable table)
        {
            var chart = new ChartJsonData();
            var dataSeries = new List<object>();
            var data = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                var date = Convert.ToDateTime(row["re_date"]);
                var mDate = new DateTime(date.Year, date.Month, 1);
                var point = new List<object>
                {
                    mDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(row["output"])
                };
                data.Add(point.ToArray());

            }
            dataSeries.Add(new { data = data.ToArray(), name = Resources.CnE.GDT_ChartOuputName, tooltip = new { valueDecimals = 2 }, marker = new { enabled = false } });
            chart.SeriesData = dataSeries.ToArray();
            return chart.ToJson();
        }

        #endregion

        #region  inventory


        [Localization]
        public ActionResult GetInventoryData(int id, int itemId, string category, string order = "[KeyWord],[LatestDate] desc", int currentPage = 1, int total = 0, bool isHTML = false)
        {
            string defaultkey;
            var jsonTable = GetInventoryTable(id, itemId, currentPage, category, order, out defaultkey);
            var chartJsonData = BuidInventoryChart(newGdtRespository.GetInventoryChart(itemId, defaultkey));
            var data = new { chart = chartJsonData, table = jsonTable };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForInventoryDetail(int reportId, int itemId, int currentPage, string category, string order = "[KeyWord],[LatestDate]  desc")
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, reportId, itemId, 1, 2000, category, order, out total);
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   columnDefinitions.Select(x => x.HearderDisplayName).ToArray(),
                                   columnDefinitions.Select(x => x.ColumnDisplayName).ToArray(),
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult ExportExcelForInventoryChartDetail(int reportId, int itemId, string key)
        {

            var tableData = newGdtRespository.GetInventoryChartTable(itemId, key);
            var allHeardes = new string[tableData.Columns.Count];
            int i = 0;
            foreach (DataColumn dc in tableData.Columns)
            {
                allHeardes[i] = dc.ColumnName;
                i++;
            }
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   allHeardes,
                                   allHeardes,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult GetInventoryDetail(int id, int itemId, int currentPage, string category, string order = "[KeyWord],[LatestDate]  desc")
        {
            string key;
            var jsonTable = GetInventoryTable(id, itemId, currentPage, category, order, out key);
            return Json(jsonTable, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetInventoryChartDetail(int itemId, string key, string term, string reDate)
        {
            var jsonChart = BuidInventoryChart(newGdtRespository.GetInventoryChart(itemId, key));
            return Json(jsonChart, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInventoryChartDetailList(int itemId, string keyList)
        {
            var dataSeries = new List<object>();
            var keys = keyList.Split(',');
            foreach (var key in keys)
            {
                var data = new List<object>();
                var dataTable = newGdtRespository.GetInventoryChartTable(itemId, key);
                foreach (DataRow row in dataTable.Rows)
                {
                    var point = new List<object>
                    {
                        (Convert.ToDateTime(row["re_date"])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                        Convert.ToDouble(row["inventory"])
                    };
                    data.Add(point.ToArray());
                }
                dataSeries.Add(new { data = data.ToArray(), name = key, marker = new { enabled = false } });
            }
            return Json(dataSeries, JsonRequestBehavior.AllowGet);
        }

        private JsonTable GetInventoryTable(int id, int itemId, int currentPage, string category, string order, out string defaultkey)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, id, itemId, currentPage, PageSize, category, order, out total);
            defaultkey = tableData.Rows.Count > 0 ? tableData.Rows[0]["KeyWord"].ToString() : "";
            return BuidJsonTable(tableData, columnDefinitions, total, currentPage, PageSize);
        }

        private object BuidInventoryChart(DataTable table)
        {
            var chart = new ChartJsonData();
            var dataSeries = new List<object>();
            var data = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                var point = new List<object>
                {
                    (Convert.ToDateTime(row["re_date"])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(row["inventory"])
                };
                data.Add(point.ToArray());

            }
            dataSeries.Add(new { data = data.ToArray(), name = Resources.CnE.GDT_ChartInventoryName, tooltip = new { valueDecimals = 2 }, marker = new { enabled = false } });
            chart.SeriesData = dataSeries.ToArray();
            return chart.ToJson();
        }

        #endregion

        #region  sales


        [Localization]
        public ActionResult GetSalesData(int id, int itemId, string category, string order = "KeyWord,LatestDate desc", int currentPage = 1, int total = 0, bool isHTML = false)
        {
            string defaultkey;
            var jsonTable = GetSalesTable(id, itemId, currentPage, category, order, out defaultkey);
            var chartJsonData = BuidSalesChart(newGdtRespository.GetSalesChart(itemId, defaultkey));
            var data = new { chart = chartJsonData, table = jsonTable };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForSalesDetail(int reportId, int itemId, int currentPage, string category, string order = "KeyWord,LatestDate  desc")
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, reportId, itemId, 1, 2000, category, order, out total);
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   columnDefinitions.Select(x => x.HearderDisplayName).ToArray(),
                                   columnDefinitions.Select(x => x.ColumnDisplayName).ToArray(),
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult ExportExcelForSalesChartDetail(int reportId, int itemId, string key)
        {

            var tableData = newGdtRespository.GetSalesChartTable(itemId, key);
            var allHeardes = new string[tableData.Columns.Count];
            int i = 0;
            foreach (DataColumn dc in tableData.Columns)
            {
                allHeardes[i] = dc.ColumnName;
                i++;
            }
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   allHeardes,
                                   allHeardes,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        [Localization]
        public ActionResult GetSalesDetail(int id, int itemId, int currentPage, string category, string order = "[KeyWord],[LatestDate]  desc")
        {
            string key;
            var jsonTable = GetSalesTable(id, itemId, currentPage, category, order, out key);
            return Json(jsonTable, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult GetSalesChartDetail(int itemId, string key, string term, string reDate)
        {
            var jsonChart = BuidSalesChart(newGdtRespository.GetSalesChart(itemId, key));
            return Json(jsonChart, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSalesChartDetailList(int itemId, string keyList)
        {
            var dataSeries = new List<object>();
            var keys = keyList.Split(',');
            foreach (var key in keys)
            {
                var data = new List<object>();
                var dataTable = newGdtRespository.GetSalesChartTable(itemId, key);
                foreach (DataRow row in dataTable.Rows)
                {
                    var point = new List<object>
                    {
                        (Convert.ToDateTime(row["re_date"])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                        Convert.ToDouble(row["Sales"])
                    };
                    data.Add(point.ToArray());
                }
                dataSeries.Add(new { data = data.ToArray(), name = key, marker = new { enabled = false } });
            }
            return Json(dataSeries, JsonRequestBehavior.AllowGet);
        }

        private JsonTable GetSalesTable(int id, int itemId, int currentPage, string category, string order, out string defaultkey)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            int total;
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtTable(columnDefinitions, id, itemId, currentPage, PageSize, category, order, out total);
            defaultkey = tableData.Rows.Count > 0 ? tableData.Rows[0]["KeyWord"].ToString() : "";
            return BuidJsonTable(tableData, columnDefinitions, total, currentPage, PageSize);
        }

        private object BuidSalesChart(DataTable table)
        {
            var chart = new ChartJsonData();
            var dataSeries = new List<object>();
            var data = new List<object>();
            foreach (DataRow row in table.Rows)
            {
                var point = new List<object>
                {
                    (Convert.ToDateTime(row["re_date"])).Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds,
                    Convert.ToDouble(row["Sales"])
                };
                data.Add(point.ToArray());

            }
            dataSeries.Add(new { data = data.ToArray(), name = Resources.CnE.GDT_ChartSalesName, tooltip = new { valueDecimals = 2 }, marker = new { enabled = false } });
            chart.SeriesData = dataSeries.ToArray();
            return chart.ToJson();
        }

        #endregion

        #region Balance Table



        [Localization]
        public ActionResult GetBalanceTabletData(string category, string region, int reportID, bool isQuery = false)
        {

            var header = new List<object>();

            List<BallanceFilter> productFiltes = newGdtRespository.GetProductFilters(reportID);
            category = string.IsNullOrEmpty(category) ? (productFiltes.Count == 0 ? "" : productFiltes.First().Item_Value) : category;
            List<BallanceFilter> AreaFiltes = newGdtRespository.GetAreaFilters(reportID, category);
            region = string.IsNullOrEmpty(region) ? (AreaFiltes.Count == 0 ? "" : AreaFiltes.First().Item_Value) : region;
            List<string> columnNames = newGdtRespository.GetBallanceColumnNames(reportID, region, category);
            header.Add(new List<object> { new { Colspan = 1, ColName = Resources.Global.SupplyDemand }, new { Colspan = 2, ColName = columnNames[2] }, new { Colspan = 2, ColName = columnNames[1] }, new { Colspan = 2, ColName = columnNames[0] } });
            header.Add(new List<object> { new { Rowspan = 2, ColName = "" }, new { Colspan = 1, ColName = "CNGOIC" }, new { Colspan = 1, ColName = "USDA" }, new { Colspan = 1, ColName = "CNGOIC" }, new { Colspan = 1, ColName = "USDA" }, new { Colspan = 1, ColName = "CNGOIC" }, new { Colspan = 1, ColName = "USDA" } });
            var balanceCategory = new List<object>();
            balanceCategory.AddRange(productFiltes.Select(t => new { Code = t.Item_Value, ItemName = t.DisplayName }));
            var balanceRegion = new List<object>();
            balanceRegion.AddRange(AreaFiltes.Select(t => new { Code = t.Item_Value, ItemName = t.DisplayName }));
            var filterData = new { balanceCategory, balanceRegion };
            var data = newGdtRespository.GetPartitionBallenceData(region, category, columnNames);
            var objData = data.Select(t => new { sup = t.SupplyDemand, cn1 = t.cn1, sd1 = t.usd1, cn2 = t.cn2, sd2 = t.usd2, cn3 = t.cn3, sd3 = t.usd3 });
            if (isQuery)
            {
                return Json(new { header, body = objData }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { filterData, TableData = new { header, body = objData } }, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportBalanceTabletData(string category, string region, int reportID)
        {
            if (string.IsNullOrEmpty(category) || category == "null") category = "0";
            if (string.IsNullOrEmpty(region) || region == "null") region = "0";
            List<string> columnNames = newGdtRespository.GetBallanceColumnNames(reportID, region, category);
            var data = newGdtRespository.GetPartitionBallenceData(region, category, columnNames);
            List<ExtraHeader> headers = new List<ExtraHeader>();
            //headers.Add(new ExtraHeader() { HeaderTextCN = "供需项", HeaderColumnSpan = 1, HeaderLevel = 1, HeaderTextEN = "供需项" });
            headers.Add(new ExtraHeader() { HeaderTextCN = columnNames[2], HeaderColumnSpan = 2, HeaderLevel = 1, HeaderTextEN = columnNames[2] });
            headers.Add(new ExtraHeader() { HeaderTextCN = columnNames[1], HeaderColumnSpan = 2, HeaderLevel = 1, HeaderTextEN = columnNames[1] });
            headers.Add(new ExtraHeader() { HeaderTextCN = columnNames[0], HeaderColumnSpan = 2, HeaderLevel = 1, HeaderTextEN = columnNames[0] });
            return new ExcelResult(Resources.Global.Source, data.AsEnumerable().AsQueryable(),
                                   new string[] { Resources.CnE.GDT_SupplyDemandItem, "CNGOIC", "USDA", "CNGOIC", "USDA", "CNGOIC", "USDA" },
                                    new string[] { "SupplyDemand", "cn1", "usd1", "cn2", "usd2", "cn3", "usd3" }
                                   ,
                                    "", Resources.CnE.GDT_SupplyDemandTable, false,
                                  headers, null, false, ""

                                    );

        }

        [Localization]
        public ActionResult GetBalanceRegionData(string category, int reportID)
        {
            var areaFiltes = newGdtRespository.GetAreaFilters(reportID, category);
            var balanceRegion = new List<object>();
            balanceRegion.AddRange(areaFiltes.Select(t => new { Code = t.Item_Value, ItemName = t.DisplayName }));
            return Json(new { balanceRegion }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  EnergyInventory


        [Localization]
        public ActionResult GetEnergyInventoryData(int id, int itemId, string order = "LatestDate desc", bool isHTML = false)
        {
            var currentNode = NewGdtService[id].FirstOrDefault(t => t.ItemID == itemId);
            var strWhere = currentNode.TableFilter1;
            var jsonTable = GetEnergyInventoryTable(id, itemId, order);
            var chartJsonData = BuidInventoryChart(newGdtRespository.GetOilInventoryChart(itemId, strWhere));
            var data = new { chart = chartJsonData, table = jsonTable };
            return isHTML ? Json(data, "text/html", JsonRequestBehavior.AllowGet) : Json(data, JsonRequestBehavior.AllowGet);
        }

        [Localization]
        public ActionResult ExportExcelForEnergyInventoryTable(int reportId, int itemId, string order = "LatestDate desc", bool isHTML = false)
        {
            var jsonTable = GetEnergyInventoryTable(reportId, itemId, order);
            var reportName = MenuService.GetMenuNodeByReportId(reportId).DisplayName;
            var jP = new JsonExcelParameter { Table = jsonTable, TableName = reportName, Source = Resources.Global.Source };
            return new JsonTableExcelResult(jP);
        }

        [Localization]
        public ActionResult ExportExcelForEnergyInventoryChart(int reportId, int itemId)
        {
            var currentNode = NewGdtService[reportId].FirstOrDefault(t => t.ItemID == itemId);
            var strWhere = currentNode.TableFilter1;
            var tableData = newGdtRespository.GetOilInventoryChart(itemId, strWhere);
            var allHeardes = new string[tableData.Columns.Count];
            int i = 0;
            foreach (DataColumn dc in tableData.Columns)
            {
                allHeardes[i] = dc.ColumnName;
                i++;
            }
            return new ExcelResult(tableData.AsEnumerable().AsQueryable(),
                                   allHeardes,
                                   allHeardes,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName,
                                   MenuService.GetMenuNodeByReportId(reportId).DisplayName
                                   );
        }

        private JsonTable GetEnergyInventoryTable(int id, int itemId, string order)
        {
            var columns = UserColumnService.GetGDTUserColumns(itemId);
            var columnDefinitions = columns as COLUMNDEFINITION[] ?? columns.ToArray();
            var tableData = GetGdtEnergyTable(columnDefinitions, id, itemId, order);
            return BuidJsonTable(tableData, columnDefinitions, 0, 0, 0);
        }

        public DataTable GetGdtEnergyTable(COLUMNDEFINITION[] columnDefinitions, int reportId, int itemId, string order)
        {
            var currentNode = NewGdtService[reportId].FirstOrDefault(t => t.ItemID == itemId);
            var strWhere = currentNode.TableFilter1;
            var dt = newGdtRespository.GetEnergyInvntoryData(currentNode.TableName1,
                                                columnDefinitions.Select(x => x.ColumnDisplayName).Aggregate((a, b) => a + "," + string.Format("{0}", b)),
                                                order,
                                                strWhere
                                                );
            return dt;
        }
        #endregion
    }
}
