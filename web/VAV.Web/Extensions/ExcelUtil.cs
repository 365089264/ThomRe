using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Cells;
using System.Drawing;
using VAV.Model.Data;
using VAV.Web.Localization;

namespace VAV.Web.Extensions
{
    public static class ExcelUtil
    {
        private static readonly Color HeaderColor = Color.FromArgb(153, 153, 255);
        public static bool IsDisplayEnglish = false;
        public static string DateFormat = "yyyy-MM-dd";
        private static string sumGroupColumnName = string.Empty;
        private static string rowLevelColumnName = string.Empty; 


        public static string QueryMonth
        {
            get
            {
                if (CultureHelper.IsEnglishCulture())
                    return "Query Month: {0}";
                else
                    return "查询月份: {0}";
            }
        }

        public static string QueryPeriod
        {
            get
            {
                if (CultureHelper.IsEnglishCulture())
                    return "Query Interval: {0} ~ {1}";
                else
                    return "查询区间: {0} ~ {1}";
            }
        }

        public static string Unit
        {
            get
            {
                if (CultureHelper.IsEnglishCulture())
                    return "Unit: {0} RMB";
                else
                    return "单位: {0}元";
            }
        }

        public static string GetUnitChineseString(string unit)
        {
            switch (unit)
            {
                case  "100M":
                    return "亿";
                case "M":
                    return "百万";
                case "10K":
                    return "万";
                case "K":
                    return "千";
                case "P":
                    return "%";
            }
            return "亿";
        }

        public static string PercentageUnit
        {
            get
            {
                return CultureHelper.IsEnglishCulture() ? "Unit: %" : "单位: %";
            }
        }


        /// <summary>
        /// Create the spreadsheet.
        /// </summary>
        /// <param name="worksheet">The worksheet.</param>
        /// <param name="worksheetName">Name of the worksheet.</param>
        /// <param name="rowData">Model/data table data.</param>
        /// <param name="headerData">Headers for the grid.</param>
        /// <param name="rowPointers">Model/data table column name.</param>
        /// <param name="extraHeaders">Extra headers for the grid.</param>
        /// <param name="reportParam">Report params to be include in the report.</param>
        /// <param name="isGroupable">if set to <c>true</c> [is groupable].</param>
        /// <param name="isInEnglish">Used for report signiture, query month, unit and extra headers localization.</param>
        /// <param name="specificDateFormat">Default format is "yyyy-MM-dd", assigning specificDateFormat when date format change is needed.</param>
        /// <param name="isSumRowColumnName">Name of the is sum row column.</param>
        /// <param name="groupColumnName">Name of the group column.</param>
        /// <param name="groupedRowLevelColumnName">Name of the grouped row level column.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static Worksheet CreateWorksheet(string source,Worksheet worksheet, string worksheetName, IQueryable rowData, string[] headerData, string[] rowPointers, List<ExtraHeader> extraHeaders = null, ReportParameter reportParam = null, bool isGroupable = false, bool isInEnglish = false, string specificDateFormat = "", string isSumRowColumnName = "", string groupColumnName = "", string groupedRowLevelColumnName = "",string note="")
        {
            try
            {
                int rowNum = 0;
                int colNum = 0;
                int minCol = 1;
                int maxCol = rowPointers == null ? minCol : rowPointers.Length;
                maxCol = maxCol == 1 && headerData == null ? 1 : headerData.Length;
                IsDisplayEnglish = isInEnglish;

                //set default date format, so no need to assign value everywhere.
                DateFormat = string.IsNullOrEmpty(specificDateFormat) ? "yyyy-MM-dd" : specificDateFormat;

                //set default sum group column name, so no need to assign value everywhere.
                sumGroupColumnName = string.IsNullOrEmpty(groupColumnName) ? "CHINESE_NAME" : groupColumnName;
                
                //set default grouped row level column name, so no need to assign value everywhere.
                rowLevelColumnName = string.IsNullOrEmpty(groupedRowLevelColumnName) ? "ROW_LEVEL" : groupedRowLevelColumnName;

                Cells cells = worksheet.Cells;
                worksheet.IsGridlinesVisible = false;

                WriteReportHeader(worksheetName, cells, maxCol, ref rowNum, colNum);

                if (reportParam != null)
                    WriteReportParameters(cells, reportParam, ref rowNum, colNum);

                WriteHeadersWithExtraHeaders(cells, extraHeaders, headerData, ref rowNum, out colNum);

                WriteRowsFromHeaders(cells, rowData, rowPointers, ref rowNum, isGroupable, sumGroupColumnName, isSumRowColumnName);

                WriteReportSigniture(cells, ref rowNum,source);
                if (!string.IsNullOrEmpty(note))
                {
                    WriteReportSigniture(cells,ref rowNum,note);
                }

                ApplyingWidthAndHeightForCells(cells, extraHeaders);

                return worksheet;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static void ApplyingWidthAndHeightForCells(Cells cells, List<ExtraHeader> extraHeaders)
        {
            cells.StandardHeight = 20.25;
            cells.StandardWidth = 12.5;

            cells.SetColumnWidth(0, 30);
            cells.SetRowHeight(0, 24.75);
            cells.SetRowHeight(1, 22.5);

            if (extraHeaders != null)
            {
                for (int i = 0; i <= extraHeaders.Select(h => h.HeaderLevel).Distinct().Count(); i++)
                {
                    cells.SetRowHeight(2 + i, 15);
                }
            }
        }

        public static void WriteReportHeader(string excelWorksheetName, Cells cells, int columnCount, ref int rowNum, int colNum)
        {
            int rowsForHeader = 1;
            WriteValuesAndMergeCells(cells, rowNum, colNum, rowsForHeader, columnCount, excelWorksheetName);
            rowNum += rowsForHeader;
        }

        /// <summary>
        /// Writes report params, for now it only writes start date and unit. Include more values in reportParam and add code below to write values for other params when needed.
        /// </summary>
        public static void WriteReportParameters(Cells cells, ReportParameter reportParam, ref int rowNum, int colNum)
        {
            if (reportParam.EndDate != null && ((DateTime)reportParam.EndDate).ToString(DateFormat) != "1900-01")
                WriteReportParameters(cells, rowNum, colNum, string.Format(QueryPeriod, ((DateTime)reportParam.StartDate).ToString(DateFormat), ((DateTime)reportParam.EndDate).ToString(DateFormat)));
            else
                WriteReportParameters(cells, rowNum, colNum, string.Format(QueryMonth, ((DateTime)reportParam.StartDate).ToString(DateFormat)));
            if(reportParam.Unit != null)
                WriteReportParameters(cells, rowNum, colNum + 1, string.Format(Unit, IsDisplayEnglish ? reportParam.Unit : GetUnitChineseString(reportParam.Unit)));
            rowNum++;
        }

        /// <summary>
        /// This writes report signiture and sets signiture style
        /// </summary>
        /// <param name="cells">The cells.</param>
        /// <param name="rowNum">The row num.</param>
        /// <param name="source">The source.</param>
        public static void WriteReportSigniture(Cells cells, ref int rowNum,string source)
        {
            Cell cell = cells[rowNum, 0];
            cell.PutValue(source);

            var style = GetStyle(cell.GetStyle(), horizontalAlignment: TextAlignmentType.Left, setBorders: false);
            style.Font.Color = Color.Orange;
            style.Font.IsBold = true;
            cell.SetStyle(style);
            rowNum++;
        }


        /// <summary>
        /// Replace special characters. 
        /// </summary>
        /// <param name="value">Value to input.</param>
        /// <returns>Value with special characters replaced.</returns>
        private static string ReplaceSpecialCharacters(string value)
        {
            if (value == null)
                return string.Empty;
            value = value.Replace("’", "'");
            value = value.Replace("“", "\"");
            value = value.Replace("”", "\"");
            value = value.Replace("–", "-");
            value = value.Replace("…", "...");
            value = value.Replace("&nbsp;", " ");
            return value;
        }


        /// <summary>
        /// Write values to the spreadsheet.
        /// </summary>
        /// <param name="cellLocation">Row Column Value.</param>
        /// <param name="strValue">Value to write.</param>
        /// <param name="spreadSheet">Spreadsheet to write to. </param>
        /// <param name="workSheet">Worksheet to write to. </param>
        private static void WriteValuesAndMergeCells(Cells cells, int startRow, int startColumn, int totalRows, int totalColumns, string value)
        {
            Cell cell = cells[startRow, startColumn];
            cell.PutValue(value);

            cells.Merge(startRow, startColumn, totalRows, totalColumns);

            var style = GetStyle(cell.GetStyle(), fontSize: 14, horizontalAlignment: TextAlignmentType.Center, setBorders: false);
            style.Font.Color = HeaderColor;
            style.Font.IsBold = true;
            cell.SetStyle(style);
        }

        public static void WriteReportParameters(Cells cells, int rowNum, int colNum, string value)
        {
            Cell cell = cells[rowNum, colNum];
            cell.PutValue(value);

            var style = GetStyle(cell.GetStyle(), fontSize: 10, horizontalAlignment: TextAlignmentType.Left, verticalAlignment: TextAlignmentType.Bottom, styleNumber: 0, setBorders: false);
            cell.SetStyle(style);
        }

        private static void WriteReportHeaders(Cells cells, int startRow, int startColumn, string value, bool isExtraHeader)
        {
            //should allow empty header to be added for extra header.
            if (string.IsNullOrWhiteSpace(value) && !isExtraHeader)
            {
                return;
            }
            Cell cell = cells[startRow, startColumn];
            cell.PutValue(value);

            var style = GetStyle(cell.GetStyle(), styleNumber: 0, horizontalAlignment: TextAlignmentType.Center);
            
            //set background
            style.ForegroundColor = HeaderColor;
            style.Pattern = BackgroundType.Solid;
            style.Font.IsBold = true;

            //set border
            cell.SetStyle(style);
        }

        /// <summary>
        /// This is a base style for vav reports, other styles extends this by assigning any amount of following params with different values.
        /// </summary>
        /// <param name="style">This is a must provide value, others are optional.</param>
        private static Style GetStyle(Style style, TextAlignmentType horizontalAlignment = TextAlignmentType.Right, TextAlignmentType verticalAlignment = TextAlignmentType.Center,
            string fontName = "Calibri", int fontSize = 11, int styleNumber = 0, bool setBorders = true)
        {
            style.HorizontalAlignment = horizontalAlignment;
            style.VerticalAlignment = verticalAlignment;

            Aspose.Cells.Font font = style.Font;

            //Set the name.
            font.Name = fontName;
            
            //Set the font size.
            font.Size = fontSize;

            style.Number = styleNumber;

            if (setBorders)
            {
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            }

            return style;
        }

        /// <summary>
        /// Write values to the spreadsheet. Auto check cell data type and applying different styles.
        /// </summary>
        public static void WriteValues(Cells cells, int row, int column, string value, Type type = null,string format = "")
        {
            Cell cell = cells[row, column];
            var style = GetStyle(cell.GetStyle());
            
            //Type == null means data is coming from data table, thus the type can not be fetched.
            if(type == null)
            {
                int intValue = 0;
                double doubleValue = 0;
                DateTime dateValue = DateTime.MinValue;

                if (int.TryParse(value, out intValue))
                {
                    style.Number = 1; //0 int
                    style.HorizontalAlignment = TextAlignmentType.Right;
                    cell.PutValue(string.Format("{0:N0}", int.Parse(value)));
                }
                else if (double.TryParse(value, out doubleValue))
                {
                    style.Number = 2; //0.00
                    style.HorizontalAlignment = TextAlignmentType.Right;
                    cell.PutValue(double.Parse(value));
                }
                else if (DateTime.TryParse(value, out dateValue))
                {
                    style.HorizontalAlignment = TextAlignmentType.Left;
                    cell.PutValue(DateTime.Parse(value).ToString(DateFormat));
                }
                else
                {
                    style.Number = 0;
                    style.HorizontalAlignment = TextAlignmentType.Left;
                    cell.PutValue(value);
                }
            }
            else
            {
                if (type == typeof(int))
                {
                    style.Number = 1; //0 int
                    style.HorizontalAlignment = TextAlignmentType.Right;
                    cell.PutValue(string.Format("{0:N0}", int.Parse(value)));
                }
                else if (type == typeof(double) || type == typeof(decimal))
                {
                    style.Number = 4; //0.00
                    style.HorizontalAlignment = TextAlignmentType.Right;
                    cell.PutValue(double.Parse(value));
                }
                else if (type == typeof(DateTime))
                {
                    style.HorizontalAlignment = TextAlignmentType.Left;
                    if (string.IsNullOrWhiteSpace(format))
                    {
                        cell.PutValue(DateTime.Parse(value).ToString(DateFormat));
                    }
                    else
                    {
                        cell.PutValue(DateTime.Parse(value).ToString(format));
                    }
                }
                else
                {
                    style.Number = 0;
                    style.HorizontalAlignment = TextAlignmentType.Left;
                    cell.PutValue(value);
                }
            }
            
            cell.SetStyle(style);
        }

        /// <summary>
        /// Write values to the spreadsheet. This is for applying specific style, define it prior to pass into this function.
        /// </summary>
        private static void WriteValues(Cells cells, int row, int column, string value, Style style)
        {
            Cell cell = cells[row, column];
            cell.PutValue(value);
            cell.SetStyle(style);
        }

        /// <summary>
        /// Write the excel rows for the spreadsheet.
        /// </summary>
        private static void WriteRowsFromHeaders(Cells cells, IQueryable rowData, string[] headerData, ref int rowNum, bool isGroupable, string specificSumGroupColumnName, string isSumRowColumnName)
        {
            if (!string.IsNullOrEmpty(specificSumGroupColumnName))
            {
                sumGroupColumnName = specificSumGroupColumnName;
            }
            
            foreach (object row in rowData)
            {
                int colNum = 0;
                foreach (string header in headerData)
                {
                    Type cellDataType = null;
                    string strValue = string.Empty;
                    
                    if (row.GetType() == typeof(System.Data.DataRow))
                    {
                        var cell = ((System.Data.DataRow) row)[header];
                        strValue = cell.ToString();
                        cellDataType = cell.GetType();
                        if (header == sumGroupColumnName && isGroupable)
                        {
                            if (((System.Data.DataRow)row)[rowLevelColumnName].ToString() == "1")
                            strValue = strValue.Insert(0, "     ");
                        }
                        if (header.Equals("PreviousCouponDate") && string.IsNullOrEmpty(strValue))
                            strValue = "-";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(isSumRowColumnName))
                        {
                            if (row.GetType().GetProperty(header).GetValue(row, null) != null)
                            {
                                strValue = row.GetType().GetProperty(header).GetValue(row, null).ToString();
                                if (header == sumGroupColumnName && isGroupable)
                                {
                                    if (row.GetType().GetProperty(header).GetValue(row, null).ToString() == "1")
                                        strValue = strValue.Insert(0, "     ");
                                }
                                cellDataType = row.GetType().GetProperty(header).GetValue(row, null).GetType();
                            }
                        }
                        else
                        {
                            if ((bool)row.GetType().GetProperty(isSumRowColumnName).GetValue(row, null) && colNum == 0)
                            {
                                if (row.GetType().GetProperty(sumGroupColumnName).GetValue(row, null) != null)
                                    strValue = row.GetType().GetProperty(sumGroupColumnName).GetValue(row, null).ToString();
                                cellDataType = typeof(string);
                            }
                            else
                            {
                                if (row.GetType().GetProperty(header).GetValue(row, null) != null)
                                {
                                    strValue = row.GetType().GetProperty(header).GetValue(row, null).ToString();
                                    cellDataType = row.GetType().GetProperty(header).GetValue(row, null).GetType();
                                }
                            }
                        }
                    }
                    strValue = ReplaceSpecialCharacters(strValue);
                    var isSumRow = string.IsNullOrEmpty(isSumRowColumnName) ? false : (bool)row.GetType().GetProperty(isSumRowColumnName).GetValue(row, null);
                    if (string.Compare(header, sumGroupColumnName, true)==0 || (isSumRow && colNum == 0))
                    {
                        var rowLevelCol = row.GetType().GetProperty(rowLevelColumnName);
                        if (rowLevelCol != null)
                        {
                            if (rowLevelCol.GetValue(row, null).ToString() == "1")
                                strValue = strValue.Insert(0, "     ");
                        }

                        var style = GetStyle(cells[rowNum, colNum].GetStyle());
                        style.ForegroundColor = HeaderColor;
                        style.Pattern = BackgroundType.Solid;
                        style.Font.IsBold = true;
                        style.HorizontalAlignment = TextAlignmentType.Left;
                        WriteValues(cells, rowNum, colNum, strValue, style);
                    }
                    else
                    {
                        if (cellDataType == null)
                            WriteValues(cells, rowNum, colNum, strValue);
                        else
                            WriteValues(cells, rowNum, colNum, strValue, cellDataType);
                    }
                    colNum++;
                }
                rowNum++;
            }
        }


        /// <summary>
        /// Extra header now must be in ExtraHeader format, better simplify the type in future. If it's null, it only writes header for the grid.
        /// </summary>
        public static void WriteHeadersWithExtraHeaders(Cells cells, List<ExtraHeader> extraHeaders, string[] headerData, ref int rowNum, out int colNum)
        {
            int startRow = rowNum;

            if (extraHeaders != null)
            {
                var headerlevels = extraHeaders.Select(h => h.HeaderLevel).Distinct().OrderBy(h => h);
                foreach (var headerlevel in headerlevels)
                {
                    colNum = 1;
                    foreach (var extraHeader in extraHeaders.Where(h => h.HeaderLevel == headerlevel))
                    {
                        string strValue = ReplaceSpecialCharacters(IsDisplayEnglish ? extraHeader.HeaderTextEN : extraHeader.HeaderTextCN);
                        
                        WriteReportHeaders(cells, rowNum, colNum, strValue, true);
                        if (extraHeader.HeaderColumnSpan > 1)
                        {
                            cells.Merge(rowNum, colNum, 1, extraHeader.HeaderColumnSpan);
                            Range range = cells.CreateRange(rowNum, colNum, 1, extraHeader.HeaderColumnSpan);
                            range.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
                            range.SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);
                        }
                        colNum += extraHeader.HeaderColumnSpan;
                    }
                    rowNum++;
                }
            }

            colNum = 0;
            foreach (string header in headerData)
            {
                string strValue = ReplaceSpecialCharacters(header);
                WriteReportHeaders(cells, rowNum, colNum, strValue, false);
                colNum++;
            }
            rowNum++;

            //This is to merge the top left cells if there are multiple lines of header/extra header. Also this sets the border for that range.
            Range rangeTopLeftHeaderCell = cells.CreateRange(startRow, 0, rowNum - startRow, 1);
            rangeTopLeftHeaderCell.Merge();
            rangeTopLeftHeaderCell.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
        }
    }
}
