using System;
using System.Data;
using VAV.Entities;
using VAV.Model.Data;

namespace VAV.Web.Common
{
    public static class UIGenerator
    {

        public static string FormatCellValue(DataRow row, REPORTCOLUMNDEFINITION column)
        {
            var retValue = string.Empty;
            if(row.Table.Columns.Contains(column.COLUMN_NAME))
            {
                var dataValue = row[column.COLUMN_NAME];

                retValue = FormatCellValue(dataValue, column.COLUMN_TYPE, column.DISPLAY_FORMAT);
            }
            return retValue;
        }

        public static string FormatCellValue(DataRow row, Column column)
        {
            var retValue = string.Empty;
            if (row.Table.Columns.Contains(column.ColumnName))
            {
                var dataValue = row[column.ColumnName];

                retValue = FormatCellValue(dataValue, column.ColumnType, column.ColumnFormat);
            }
            return retValue;
        }

        public static string FormatCellValue(object dataValue, string type)
        {
            string retValue = "";

            if (dataValue != null)
            {
                switch (type)
                {
                    case "datetime":
                        if (dataValue is DateTime)
                        {
                            retValue = ((DateTime)dataValue).ToString("yyyy-MM-dd");
                        }
                        break;
                    case "System.DateTime":
                        if (dataValue is DateTime)
                        {
                            retValue = ((DateTime)dataValue).ToString("yyyy-MM-dd");
                        }
                        break;
                    case "decimal":
                        if (dataValue is double || dataValue is float || dataValue is decimal)
                        {
                            retValue = string.Format("{0:N2}", dataValue);
                        }
                        else if (dataValue is int || dataValue is Int64)
                        {
                            retValue = string.Format("{0:N0}", dataValue);
                        }
                        break;
                    default:
                        retValue = dataValue.ToString();
                        break;
                }
            }
            
            return retValue;
        }

        public static string FormatCellValue(object dataValue, string type, string displayFormat)
        {
            if (string.IsNullOrEmpty(displayFormat))
                return FormatCellValue(dataValue, type);
            if (type == "datetime")
            {
                if (dataValue is DateTime)
                {
                    return((DateTime)dataValue).ToString(displayFormat);
                }
            }
            return string.Format(displayFormat, dataValue);
        }


        public static string AppendTextAlgin(REPORTCOLUMNDEFINITION column)
        {
            var retValue = string.Empty;
            switch (column.COLUMN_TYPE)
            {
                case "datetime":
                case "text":
                    retValue = "class=\"textLeft\"";
                    break;
            }
            return retValue;
        }

        public static string FormatDateTime(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.Value.ToString("yyyy-MM-dd");
            }
            return string.Empty;
        }

        public static object FormatJsonCellValue(object dataValue, string type)
        {
            object retValue = null;

            if (dataValue != null)
            {
                switch (type)
                {
                    case "System.DateTime":
                        if (dataValue is DateTime)
                        {
                            retValue = ((DateTime)dataValue).ToString("yyyy-MM-dd");
                        }
                        break;
                    case "decimal":
                        if (dataValue is double || dataValue is float || dataValue is decimal)
                        {
                            retValue = string.Format("{0:N2}", dataValue);
                        }
                        else if (dataValue is int)
                        {
                            retValue = string.Format("{0:N0}", dataValue);
                        }
                        break;
                    case "System.Boolean":
                        retValue = dataValue;
                        break;
                    default:
                        retValue = dataValue.ToString();
                        break;
                }
            }

            return retValue;
        }

    }
}