using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension
{
    public class OpenMarketOperation
    {
        public string _tabName = null;
        // private String _opFlag = null;

        public List<string> _dataTypes = new List<string>();
        public List<string> _colNames = new List<string>();
        public List<string> _values = new List<string>();

        public string _ric = null;

        public StringBuilder _bulkSqlStatments = new StringBuilder();



        public OpenMarketOperation(string tableName)
        {
            _tabName = tableName;
        }

        public void setRic(string ric)
        {
            _ric = ric;
        }

        public void resetValues()
        {
            _values.Clear();
        }

        public int getColIdx(string colNames)
        {
            return _colNames.IndexOf(colNames);
        }

        public string getColType(int idx)
        {
            return _dataTypes[idx].ToString();
        }

        public StringBuilder getbulkSqlStatments()
        {
            return _bulkSqlStatments;
        }

        public void appendColNames(string colName, string colType)
        {

            if (!_colNames.Contains(colName))
            {
                if (colType == null) colType = "String";
                _dataTypes.Add(colType);
                _colNames.Add(colName);
            }
        }

        public void appendValues(string values)
        {
            _values.Add(values);
        }
    }
}
