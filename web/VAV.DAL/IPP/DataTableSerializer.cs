using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VAV.DAL.IPP
{
    public class DataTableSerializer
    {
        public static List<T> ToList<T>(DataTable dt)
        {
            var list = new List<T>();
            if (dt == null || dt.Rows.Count == 0)
                return list;//return empty list instead of null object
            foreach (DataRow row in dt.Rows)
            {
                var obj = ToEntity<T>(row);
                list.Add(obj);
            }
            return list;
        }

        public static T ToEntity<T>( DataRow row)
        {
            var objType = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in row.Table.Columns)
            {
                var property = objType.GetProperty(column.ColumnName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null || !property.CanWrite)
                {
                    continue;
                }
                var value = row[column.ColumnName];
                if (value == DBNull.Value)
                {
                    value = null;
                }
                else
                {
                    //add what you need.
                    //if (column.DataType == typeof (DateTime))
                    //{
                    //    value = ((DateTime)value).ToString("yyyy-MM-dd");
                    //}

                    // if db stores string but should be datetime, convert it. 
                    if (column.DataType == typeof (string) && property.PropertyType == typeof (DateTime))
                    {
                        value = Convert.ToDateTime(value);   
                    }
                    else if (column.DataType == typeof(Int64) && (property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Nullable<int>)))
                    {
                        value = Convert.ToInt32(value.ToString());
                    }
                    else if (column.DataType == typeof(Int16) && (property.PropertyType == typeof(Boolean)))
                    {
                        value = Convert.ToBoolean(value);
                    }
                    else if (column.DataType == typeof(Double) && (property.PropertyType == typeof(Decimal) || property.PropertyType == typeof(Nullable<Decimal>)))
                    {
                        value = Convert.ToDecimal(value);
                    }
                    else if (column.DataType == typeof(Decimal) && (property.PropertyType == typeof(Int64) || property.PropertyType == typeof(Nullable<Int64>)))
                    {
                        value = Convert.ToInt64(value);
                    }
                    else if (column.DataType == typeof(Decimal) && (property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Nullable<Int32>)))
                    {
                        value = Convert.ToInt32(value);
                    }
                    else if (column.DataType == typeof(Decimal) && (property.PropertyType == typeof(Double) || property.PropertyType == typeof(Nullable<Double>)))
                    {
                        value = Convert.ToDouble(value);
                    }

                }

                property.SetValue(obj, value, null);

            }
            return obj;
        }
    }
}
