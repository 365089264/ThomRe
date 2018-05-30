using System;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using VAV.Model.Data;

namespace VAV.Web.Common
{
    public class JsonTableExcelResult : ActionResult
    {
        private readonly JsonExcelParameter _arg;
        private readonly Workbook _workbook;
        private readonly Worksheet _worksheet;
        private readonly Cells _cells;
        private readonly JsonTable _jTable;
        private Style _titleStyle;
        private Style _headerStyle;
        private Style _signitureStyle;
        protected Style _textStyle;
        private Style _firstColumnStyle;

        public JsonTableExcelResult(JsonExcelParameter parameter)
        {
            _arg = parameter;
            _jTable = _arg.Table;
            _workbook = new Workbook();
            _worksheet = _workbook.Worksheets[0];
            _worksheet.Name = _arg.TableName.Length > 30 ? _arg.TableName.Substring(0, 30) : _arg.TableName;
            _cells = _worksheet.Cells;
            BuildStyle();
        }

        protected virtual void BuildStyle()
        {
            _titleStyle = new Style
            {
                HorizontalAlignment = TextAlignmentType.Center,
                VerticalAlignment = TextAlignmentType.Center
            };
            _titleStyle.Font.Name = "Calibri";
            _titleStyle.Font.Size = 14;
            _titleStyle.Font.Color = Color.FromArgb(153, 153, 255);
            _titleStyle.Number = 0;
            _titleStyle.Pattern = BackgroundType.Solid;
            _titleStyle.Font.IsBold = true;

            _headerStyle = new Style
                               {
                                   HorizontalAlignment = TextAlignmentType.Center,
                                   VerticalAlignment = TextAlignmentType.Center
                               };
            _headerStyle.Font.Name = "Calibri";
            _headerStyle.Font.Size = 11;
            _headerStyle.Number = 0;
            _headerStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            _headerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            _headerStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            _headerStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            _headerStyle.Borders[BorderType.Horizontal].LineStyle = CellBorderType.Thin;
            _headerStyle.Borders[BorderType.Vertical].LineStyle = CellBorderType.Thin;
            _headerStyle.ForegroundColor = Color.FromArgb(153, 153, 255);
            _headerStyle.Pattern = BackgroundType.Solid;
            _headerStyle.Font.IsBold = true;

            _firstColumnStyle = new Style
            {
                HorizontalAlignment = TextAlignmentType.Left,
                VerticalAlignment = TextAlignmentType.Center
            };
            _firstColumnStyle.Font.Name = "Calibri";
            _firstColumnStyle.Font.Size = 11;
            _firstColumnStyle.Number = 0;
            _firstColumnStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            _firstColumnStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            _firstColumnStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            _firstColumnStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            _firstColumnStyle.Borders[BorderType.Horizontal].LineStyle = CellBorderType.Thin;
            _firstColumnStyle.Borders[BorderType.Vertical].LineStyle = CellBorderType.Thin;
            _firstColumnStyle.ForegroundColor = Color.FromArgb(153, 153, 255);
            _firstColumnStyle.Pattern = BackgroundType.Solid;
            _firstColumnStyle.Font.IsBold = true;

            _signitureStyle= new Style
                               {
                                   HorizontalAlignment = TextAlignmentType.Left,
                                   VerticalAlignment = TextAlignmentType.Center
                               };
            _signitureStyle.Font.Name = "Calibri";
            _signitureStyle.Font.Color = Color.Orange;
            _signitureStyle.Number = 0;
            _signitureStyle.Pattern = BackgroundType.Solid;
            _signitureStyle.Font.IsBold = true;

            _textStyle = new Style();
            _textStyle.Font.Name = "Calibri";
            _textStyle.Font.Size = 11;
            _textStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            _textStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            _textStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            _textStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            _textStyle.Pattern = BackgroundType.Solid;
            _textStyle.Number = 4;
            _textStyle.Font.IsBold = false;
        }
        
        /// <summary>
        /// Enables processing of the result of an action method by a custom type that inherits from the <see cref="T:System.Web.Mvc.ActionResult"/> class.
        /// </summary>
        /// <param name="context">The context in which the result is executed. The context information includes the controller, HTTP content, request context, and route data.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            try
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", HttpUtility.UrlEncode(Resources.Global.ExcelOutputFileName, Encoding.UTF8)));
                HttpContext.Current.Response.ContentType = "application/ms-excel";
                BuildTable();
                _workbook.Save(HttpContext.Current.Response.OutputStream, SaveFormat.Excel97To2003);
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BuildTable()
        {
            //Print Title
            WriteCell(0, 0, 1, _jTable.ColumTemplate.Count, _arg.TableName, _titleStyle);
            //Print Date
            foreach (var tuple in _arg.SubTitle)
            {
                WriteCell(1, tuple.Item1, 1, 1, tuple.Item2,null);
            }
            for (int i = 0; i < _jTable.ColumTemplate.Count; i++)
            {
                var style = _cells[1, i].GetStyle();
                style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                _cells[1, i].SetStyle(style);
            }
            //Draw Table Headers
            var rowPointer = 2;
            var colPointer = 0;
            if (_jTable.ExtraHeaders != null)
            {
                foreach (var jsonExtraColumn in _jTable.ExtraHeaders)
                {
                    WriteCell(rowPointer, colPointer, 1, jsonExtraColumn.ColSpan, jsonExtraColumn.Name, _headerStyle);
                    colPointer += jsonExtraColumn.ColSpan;
                }
                rowPointer++;
            }
            colPointer = 0;
            foreach (var column in _jTable.ColumTemplate)
            {
                WriteCell(rowPointer, colPointer++, 1, 1, column.Name, _headerStyle);
            }
            rowPointer++;
            //Draw Table Body
            foreach (var row in _jTable.RowData)
            {
                colPointer = 0;
                foreach (var column in _jTable.ColumTemplate)
                {
                    var valueText = row.ContainsKey(column.ColumnName) ? row[column.ColumnName].Replace("&nbsp;", " ") : "";
                    decimal v;
                    var cStyle = colPointer == 0 ? _firstColumnStyle : _textStyle;
                    if (decimal.TryParse(valueText, out v))
                    {
                        WriteCell(rowPointer, colPointer++, 1, 1, v, cStyle);                        
                    }
                    else
                    {
                        WriteCell(rowPointer, colPointer++, 1, 1, valueText, cStyle);                        
                    }
                }
                rowPointer++;
            }
            //Draw Source
            WriteCell(rowPointer, 0, 1, 1, _arg.Source, _signitureStyle);
            _worksheet.AutoFitColumns();
            _worksheet.IsGridlinesVisible = false;
        }

        private void WriteCell(int startRow, int startCol,int rowSpan, int colSpan, object value,Style style)
        {
            var cell = _cells[startRow, startCol];
            cell.PutValue(value);
            _cells.Merge(startRow, startCol, rowSpan, colSpan);
            if (style != null)
            {
                cell.SetStyle(style);
            }
        }
    }
}