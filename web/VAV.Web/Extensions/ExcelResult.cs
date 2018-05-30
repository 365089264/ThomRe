using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using VAV.Model.Data;
using Aspose.Cells;
using System.Data;
using System.Text;
using VAV.Web.Localization;

namespace VAV.Web.Extensions
{
    /// <summary>
    /// Excel result class
    /// </summary>
    public class ExcelResult : ActionResult
    {
        private string fileName = "Report";
        private string workSheetName = "sheet1";
        private IQueryable rowData;
        private string[] headerData = null;
        private string[] rowPointers = null;
        private List<ExtraHeader> extraHeaders = null;
        private ReportParameter reportParam = null;
        private bool isXlsFormat = true;
        private bool isGroupable = false;
        private bool isInEnglish = false;
        private string specificDateFormat = string.Empty;
        private string isSumRowColumnName = string.Empty;
        private string sumGroupColumnName = string.Empty;
        private string groupedRowLevelColumnName = string.Empty;
        private string _source;
        private string _note;

        public ExcelResult(IQueryable data, string[] headers, string[] rowKeys, string fileName = "", string workSheetName = "", bool isXlsFormat = false, List<ExtraHeader> extraHeaders = null, ReportParameter reportParam = null, bool isGroupable = false, string specificDateFormat = "", string isSumRowColumnName = "", string sumGroupColumnName = "", string groupedRowLevelColumnName = ""):
            this(Resources.Global.Source,data,headers,rowKeys,fileName,workSheetName,isXlsFormat,extraHeaders,reportParam,isGroupable,specificDateFormat,isSumRowColumnName,sumGroupColumnName,groupedRowLevelColumnName)
        {            
        }


        /// <summary>
        /// Constructor for ExcelResult
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="data">list of model/data table row</param>
        /// <param name="headers">header collection</param>
        /// <param name="rowKeys">column collection</param>
        /// <param name="fileName">default is "Report"</param>
        /// <param name="workSheetName">default is "sheet1"</param>
        /// <param name="isXlsFormat">default is true</param>
        /// <param name="extraHeaders">default is null</param>
        /// <param name="reportParam">default is null</param>
        /// <param name="isGroupable">indicates whether data are grouped (some column is group column) like cdc report first column</param>
        /// <param name="specificDateFormat">default is "yyyy-MM-dd"</param>
        /// <param name="isSumRowColumnName">indicates such row is a summarized row, like sum row in open market detail report, 7D, 14D etc.</param>
        /// <param name="sumGroupColumnName">the name of the column that indicates the sum group name, like "chinese_name" in cdc report.</param>
        /// <param name="groupedRowLevelColumnName">Name of the grouped row level column.</param>
        public ExcelResult(string source, IQueryable data, string[] headers, string[] rowKeys, string fileName = "", string workSheetName = "", bool isXlsFormat = false, List<ExtraHeader> extraHeaders = null, ReportParameter reportParam = null, bool isGroupable = false, string specificDateFormat = "", string isSumRowColumnName = "", string sumGroupColumnName = "", string groupedRowLevelColumnName = "",string note="")
        {
            //Mandatory params;
            this.rowData = data;
            this.headerData = headers;
            this.rowPointers = rowKeys;
            _source = source;
            //Optional params
            this.fileName = fileName;
            this.workSheetName = workSheetName;
            this.isXlsFormat = isXlsFormat;
            this.extraHeaders = extraHeaders;
            this.reportParam = reportParam;
            this.isGroupable = isGroupable;
            isInEnglish = CultureHelper.IsEnglishCulture();
            this.specificDateFormat = specificDateFormat;
            this.isSumRowColumnName = isSumRowColumnName;
            this.sumGroupColumnName = sumGroupColumnName;
            this.groupedRowLevelColumnName = groupedRowLevelColumnName;
            this._note = note;
        }

        /// <summary>
        ///  Gets a value for file name.
        /// </summary>
        public string ExcelFileName
        {
            get { return this.fileName; }
        }

        /// <summary>
        ///  Gets a value for file name.
        /// </summary>
        public string ExcelWorkSheetName
        {
            get { return this.workSheetName; }
        }

        /// <summary>
        /// Gets a value for rows.
        /// </summary>
        public IQueryable ExcelRowData
        {
            get { return this.rowData; }
        }

        /// <summary>
        /// Execute the Excel Result. 
        /// </summary>
        /// <param name="context">Controller context.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            CreateExcel();
        }

        private void CreateExcel()
        {
            try
            {
                //Currently it's only xls format is needed, add different extensions if more format is required.
                //string name = isXlsFormat ? this.fileName + ".xls" : this.fileName + ".xlsx";

                Workbook workbook = new Workbook();
                Worksheet worksheet = workbook.Worksheets[0];

                //Max worksheet.Name allowed by Excel is 31.
                worksheet.Name = this.workSheetName.Length > 30 ? this.workSheetName.Substring(0, 30) : this.workSheetName;

                ExcelUtil.CreateWorksheet(_source, worksheet, workSheetName, rowData, headerData, rowPointers, extraHeaders, reportParam, isGroupable, isInEnglish, specificDateFormat, isSumRowColumnName, sumGroupColumnName, groupedRowLevelColumnName,_note);

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", HttpUtility.UrlEncode(isInEnglish ? "Report.xls" : "报表.xls", Encoding.UTF8)));
                HttpContext.Current.Response.ContentType = "application/ms-excel";
                workbook.Save(HttpContext.Current.Response.OutputStream, SaveFormat.Excel97To2003);
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}