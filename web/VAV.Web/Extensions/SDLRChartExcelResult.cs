using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using System.Text;
using VAV.Model.Data;
using VAV.Web.Localization;

namespace VAV.Web.Extensions
{
    
    /// <summary>
    /// Excel result class
    /// </summary>
    public class SDLRChartExcelResult : ActionResult
    {
        private readonly bool _isInEnglish;
        private Tuple<List<Tuple<DateTime, double, double>>, List<Tuple<DateTime, double>>> _data;
        private List<Tuple<DateTime, decimal, decimal>> _index;
        private List<Tuple<DateTime, decimal, decimal>> _valuation;
        private List<Tuple<DateTime, double, double, double, double>> _stock;
        public SDLRChartExcelResult(Tuple<List<Tuple<DateTime, double, double>>, List<Tuple<DateTime, double>>> data,
            List<Tuple<DateTime, decimal, decimal>> index, List<Tuple<DateTime, decimal, decimal>> valuation, List<Tuple<DateTime, double, double, double, double>> stock)
        {
            _isInEnglish = CultureHelper.IsEnglishCulture();
            _data = data;
            _index = index;
            _valuation = valuation;
            _stock = stock;
        }
    
        /// <summary>
        /// Execute the Excel Result. 
        /// </summary>
        /// <param name="context">Controller context.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            //Currently it's only xls format is needed, add different extensions if more format is required.
            //string name = isXlsFormat ? this.fileName + ".xls" : this.fileName + ".xlsx";
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];
            worksheet.Name = Resources.CnE.GDT_ChartOuputName;
            int rowNum = 0;
            int colNum = 0;
            ExcelUtil.IsDisplayEnglish = _isInEnglish;
            ExcelUtil.WriteReportHeader(worksheet.Name,worksheet.Cells,3,ref rowNum,0);
            ExcelUtil.WriteReportParameters(worksheet.Cells, rowNum, colNum++, string.Format(ExcelUtil.QueryPeriod, _data.Item1.First().Item1.ToString(ExcelUtil.DateFormat), _data.Item1.Last().Item1.ToString(ExcelUtil.DateFormat)));
            ExcelUtil.WriteReportParameters(worksheet.Cells, rowNum++, colNum, Resources.CnE.CNE_SdR_DeviceYieldByTon);
            ExcelUtil.WriteHeadersWithExtraHeaders(worksheet.Cells, null, new[] { Resources.Global.Date,Resources.CnE.Gasoline,Resources.CnE.Diesel }, ref rowNum, out colNum);
            --rowNum;
            foreach (var t in _data.Item1)
            {
                ExcelUtil.WriteValues(worksheet.Cells, ++rowNum, 0, t.Item1.ToString(), t.Item1.GetType(), "yyyy-MM");
                ExcelUtil.WriteValues(worksheet.Cells, rowNum, 1, t.Item2.ToString(), t.Item2.GetType());
                ExcelUtil.WriteValues(worksheet.Cells, rowNum, 2, t.Item3.ToString(), t.Item3.GetType());
            }
            ++rowNum;
            ExcelUtil.WriteReportSigniture(worksheet.Cells,ref rowNum,Resources.Global.Source);
            ExcelUtil.ApplyingWidthAndHeightForCells(worksheet.Cells,null);
            workbook.Worksheets.Add(SheetType.Worksheet);
            var worksheet2 = workbook.Worksheets[1];
            worksheet2.Name = Resources.CnE.Operation;
            rowNum = 0;
            colNum = 0;
            ExcelUtil.WriteReportHeader(worksheet2.Name, worksheet2.Cells, 2, ref rowNum, 0);
            ExcelUtil.WriteReportParameters(worksheet2.Cells, rowNum, colNum++, string.Format(ExcelUtil.QueryPeriod, _data.Item2.First().Item1.ToString(ExcelUtil.DateFormat), _data.Item2.Last().Item1.ToString(ExcelUtil.DateFormat)));
            ExcelUtil.WriteReportParameters(worksheet2.Cells, rowNum++, colNum, "%");
            ExcelUtil.WriteHeadersWithExtraHeaders(worksheet2.Cells, null, new[] { Resources.Global.Date, Resources.CnE.Operation }, ref rowNum, out colNum);
            --rowNum;
            foreach (var t in _data.Item2)
            {
                ExcelUtil.WriteValues(worksheet2.Cells, ++rowNum, 0, t.Item1.ToString(), t.Item1.GetType());
                ExcelUtil.WriteValues(worksheet2.Cells, rowNum, 1, (t.Item2*100).ToString(), t.Item2.GetType());
            }
            ++rowNum;
            ExcelUtil.WriteReportSigniture(worksheet2.Cells, ref rowNum, Resources.Global.Source);
            ExcelUtil.ApplyingWidthAndHeightForCells(worksheet2.Cells, null);

            workbook.Worksheets.Add(SheetType.Worksheet);
            var worksheet3 = workbook.Worksheets[2];
            worksheet3.Name = Resources.CnE.Index;
            rowNum = 0;
            colNum = 0;
            ExcelUtil.WriteReportHeader(worksheet3.Name, worksheet3.Cells, 3, ref rowNum, 0);
            ExcelUtil.WriteReportParameters(worksheet3.Cells, rowNum++, colNum, string.Format(ExcelUtil.QueryPeriod, _index.First().Item1.ToString(ExcelUtil.DateFormat), _index.Last().Item1.ToString(ExcelUtil.DateFormat)));
            ExcelUtil.WriteHeadersWithExtraHeaders(worksheet3.Cells, null, new[] { Resources.Global.Date, Resources.CnE.Gasoline, Resources.CnE.Diesel }, ref rowNum, out colNum);
            --rowNum;
            foreach (var t in _index)
            {
                ExcelUtil.WriteValues(worksheet3.Cells, ++rowNum, 0, t.Item1.ToString(), t.Item1.GetType());
                ExcelUtil.WriteValues(worksheet3.Cells, rowNum, 1, t.Item2.ToString(), t.Item2.GetType());
                ExcelUtil.WriteValues(worksheet3.Cells, rowNum, 2, t.Item3.ToString(), t.Item3.GetType());
            }
            ++rowNum;
            ExcelUtil.WriteReportSigniture(worksheet3.Cells, ref rowNum, Resources.Global.Source);
            ExcelUtil.ApplyingWidthAndHeightForCells(worksheet3.Cells, null);

            workbook.Worksheets.Add(SheetType.Worksheet);
            var worksheet4 = workbook.Worksheets[3];
            worksheet4.Name = Resources.CnE.Valuation;
            rowNum = 0;
            colNum = 0;
            ExcelUtil.WriteReportHeader(worksheet4.Name, worksheet4.Cells, 3, ref rowNum, 0);
            ExcelUtil.WriteReportParameters(worksheet4.Cells, rowNum, colNum++, string.Format(ExcelUtil.QueryPeriod, _valuation.First().Item1.ToString(ExcelUtil.DateFormat), _valuation.Last().Item1.ToString(ExcelUtil.DateFormat)));
            ExcelUtil.WriteReportParameters(worksheet4.Cells, rowNum++, colNum, Resources.CnE.CNYTonne);
            ExcelUtil.WriteHeadersWithExtraHeaders(worksheet4.Cells, null, new[] { Resources.Global.Date, Resources.CnE.Gasoline, Resources.CnE.Diesel }, ref rowNum, out colNum);
            --rowNum;
            foreach (var t in _valuation)
            {
                ExcelUtil.WriteValues(worksheet4.Cells, ++rowNum, 0, t.Item1.ToString(), t.Item1.GetType());
                ExcelUtil.WriteValues(worksheet4.Cells, rowNum, 1, t.Item2.ToString(), t.Item2.GetType());
                ExcelUtil.WriteValues(worksheet4.Cells, rowNum, 2, t.Item3.ToString(), t.Item3.GetType());
            }
            ++rowNum;
            ExcelUtil.WriteReportSigniture(worksheet4.Cells, ref rowNum, Resources.Global.Source);
            ExcelUtil.ApplyingWidthAndHeightForCells(worksheet4.Cells, null);

            workbook.Worksheets.Add(SheetType.Worksheet);
            var worksheet5 = workbook.Worksheets[4];
            worksheet5.Name = Resources.CnE.Stock;
            rowNum = 0;
            colNum = 0;
            ExcelUtil.WriteReportHeader(worksheet5.Name, worksheet5.Cells, 5, ref rowNum, 0);
            ExcelUtil.WriteReportParameters(worksheet5.Cells, rowNum, colNum++, string.Format(ExcelUtil.QueryPeriod, _stock.First().Item1.ToString(ExcelUtil.DateFormat), _stock.Last().Item1.ToString(ExcelUtil.DateFormat)));
            ExcelUtil.WriteReportParameters(worksheet5.Cells, rowNum++, colNum, Resources.CnE.CNE_SdR_DeviceYieldByTon);
            ExcelUtil.WriteHeadersWithExtraHeaders(worksheet5.Cells, null, new[] { Resources.Global.Date, Resources.CnE.Gasoline, Resources.CnE.Diesel, Resources.CnE.Gasoline + " " + Resources.CnE.StockCapacityRate, Resources.CnE.Diesel + " " + Resources.CnE.StockCapacityRate }, ref rowNum, out colNum);
            --rowNum;
            foreach (var t in _stock)
            {
                ExcelUtil.WriteValues(worksheet5.Cells, ++rowNum, 0, t.Item1.ToString(), t.Item1.GetType());
                ExcelUtil.WriteValues(worksheet5.Cells, rowNum, 1, t.Item2.ToString(), t.Item2.GetType());
                ExcelUtil.WriteValues(worksheet5.Cells, rowNum, 2, t.Item3.ToString(), t.Item3.GetType());
                ExcelUtil.WriteValues(worksheet5.Cells, rowNum, 3, t.Item4.ToString(), t.Item4.GetType());
                ExcelUtil.WriteValues(worksheet5.Cells, rowNum, 4, t.Item5.ToString(), t.Item5.GetType());
            }
            ++rowNum;
            ExcelUtil.WriteReportSigniture(worksheet5.Cells, ref rowNum, Resources.Global.Source);
            ExcelUtil.ApplyingWidthAndHeightForCells(worksheet5.Cells, null);

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", HttpUtility.UrlEncode(_isInEnglish ? "Report.xls" : "报表.xls", Encoding.UTF8)));
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            workbook.Save(HttpContext.Current.Response.OutputStream, SaveFormat.Excel97To2003);
            HttpContext.Current.Response.End();

        }
    }
}