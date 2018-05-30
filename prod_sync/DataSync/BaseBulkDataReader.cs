using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Luna.DataSync.Setting;
using System.Globalization;

namespace Luna.DataSync.Core
{
    public abstract class BaseBulkDataReader: IBulkDataReader
    {
        #region Prvate Fields
        private string _connString;
        private IDbConnection _conn;
        private DateTime _lastSyncTime;
        private DateTime _currentSyncTime;
        private int _sqlCommandTimeout;
        private List<string> _customBonds;
        #endregion

        #region Consts
        private const string TokenLastSyncTime = "{LastSyncTime}";
        private const string TokenLastSyncDate = "{LastSyncDate}";
        private const string TokenCurrentSyncTime = "{CurrentSyncTime}";
        private const string TokenCustomBonds = "{CustomBonds}";
        private const string TokenFromTime = "{FromTime}";
        private const string TokenCurrentSyncTimestamp = "{CurrentSyncStamp}";
        private const string TokenFromTimestamp = "{FromStamp}";

        private const string DateTimeFormat = "M/d/yyyy h:mm:ss tt";
        private const string TimestampFormat = "M/d/yyyy h:mm:ss.ffffff tt";

        #endregion

        #region Constructors
        public BaseBulkDataReader(
            string connString,
            int sqlCommandTimeout,
            DateTime lastSyncTime,
            DateTime currentSyncTime,
            List<string> customBonds)
        {
            this._connString = connString;
            this._sqlCommandTimeout = sqlCommandTimeout;
            this._lastSyncTime = lastSyncTime;
            this._currentSyncTime = currentSyncTime;
            this._customBonds = customBonds;
        }
        #endregion

        #region IBulkDataReader Members
        public void Init()
        {
            _conn = GetDbConnection(_connString);
            _conn.Open();
        }

        public DataTable ReadAsDataTable(TableMapping tableMapping)
        {
            IDbCommand cmd = GetDbCommand(BuildSelectCmdText(tableMapping), _conn);
            cmd.CommandTimeout = _sqlCommandTimeout;

            IDbDataAdapter adapter = GetDbDataAdapter(cmd);

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return ds.Tables[0];
        }

        public DataTable ReadAsDataTable(TableMapping tableMapping, DateTime from)
        {
            IDbCommand cmd = GetDbCommand(BuildSelectCmdText(tableMapping, from), _conn);
            cmd.CommandTimeout = _sqlCommandTimeout;

            IDbDataAdapter adapter = GetDbDataAdapter(cmd);

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return ds.Tables[0];
        }

        public DataTable ReadWithRowCount(TableMapping tableMapping, DateTime from, int count)
        {
            IDbCommand cmd = GetDbCommand(BuildSelectCmdWithRownumText(tableMapping,from,count), _conn);
            cmd.CommandTimeout = _sqlCommandTimeout;

            IDbDataAdapter adapter = GetDbDataAdapter(cmd);

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return ds.Tables[0];
        }


        public void Close()
        {
            if (_conn != null)
                _conn.Close();
        }
        #endregion

        #region Protected Members
        protected abstract IDbConnection GetDbConnection(string connString);

        protected abstract IDbCommand GetDbCommand(string cmdText, IDbConnection conn);

        protected abstract IDbDataAdapter GetDbDataAdapter(IDbCommand cmd);
        #endregion

        #region Private Methods
        private string BuildSelectCmdText(TableMapping tableMapping)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            if (tableMapping.ColumnMappings.Count() == 0)
            {
                sb.Append("* ");
            }
            else
            {
                foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                    if (!string.IsNullOrEmpty(columnMapping.Source))
                    {
                        if (!string.IsNullOrEmpty(columnMapping.SourceTableAlias))
                            sb.AppendFormat("{0}.{1}, ", columnMapping.SourceTableAlias, columnMapping.Source);
                        else
                            sb.AppendFormat("{0}, ", columnMapping.Source);
                    }

                sb.Remove(sb.Length - 2, 2);
            }

            sb.AppendFormat(" FROM {0} ", tableMapping.Source);

            if (!string.IsNullOrEmpty(tableMapping.Filter))
                sb.AppendFormat(" WHERE {0}", ParseFilterTokens(tableMapping.Filter));

            return sb.ToString();
        }

        /// <summary>
        /// Build select sql for tables with different start time for each table.
        /// </summary>
        /// <param name="tableMapping">table name</param>
        /// <param name="from">start time for this table</param>
        /// <returns>Select sql.</returns>
        private string BuildSelectCmdText(TableMapping tableMapping,DateTime from)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            if (tableMapping.ColumnMappings.Count() == 0)
            {
                sb.Append("* ");
            }
            else
            {
                foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                    if (!string.IsNullOrEmpty(columnMapping.Source))
                    {
                        if (!string.IsNullOrEmpty(columnMapping.SourceTableAlias))
                            sb.AppendFormat("{0}.{1}, ", columnMapping.SourceTableAlias, columnMapping.Source);
                        else
                            sb.AppendFormat("{0}, ", columnMapping.Source);
                    }

                sb.Remove(sb.Length - 2, 2);
            }

            sb.AppendFormat(" FROM {0} ", tableMapping.Source);

            if (!string.IsNullOrEmpty(tableMapping.Filter))
                sb.AppendFormat(" WHERE {0}", ParseFilterTokens(tableMapping.Filter, from));

            return sb.ToString();
        }

        private string BuildSelectCmdWithRownumText(TableMapping tableMapping,DateTime from, int count)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            if (tableMapping.ColumnMappings.Count() == 0)
            {
                sb.Append("* ");
            }
            else
            {
                foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                    if (!string.IsNullOrEmpty(columnMapping.Source))
                    {
                        if (!string.IsNullOrEmpty(columnMapping.SourceTableAlias))
                            sb.AppendFormat("{0}.{1}, ", columnMapping.SourceTableAlias, columnMapping.Source);
                        else
                            sb.AppendFormat("{0}, ", columnMapping.Source);
                    }

                sb.Remove(sb.Length - 2, 2);
            }

            sb.AppendFormat(" FROM {0} ", tableMapping.Source);

            if (!string.IsNullOrEmpty(tableMapping.Filter))
                sb.AppendFormat(" WHERE {0}", ParseFilterTokens(tableMapping.Filter, from, count));

            return sb.ToString();
        }

        private string ParseFilterTokens(string filter)
        {
            string parsedFilter = string.Empty;

            parsedFilter = filter.Replace(TokenLastSyncTime, string.Format(@"'{0}'", _lastSyncTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenLastSyncDate, string.Format(@"'{0}'", _lastSyncTime.Date.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCurrentSyncTime, string.Format(@"'{0}'", _currentSyncTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCustomBonds, string.Format("{0}", _customBonds.Count == 0 ? "null" : string.Join(",", _customBonds.ToArray())));

            return parsedFilter;
        }

        /// <summary>
        /// Parse fileter(where clause), and return it
        /// </summary>
        /// <param name="filter">Written in xml</param>
        /// <param name="from">From when to select data</param>
        /// <returns>Where clause</returns>
        private string ParseFilterTokens(string filter,DateTime from)
        {
            string parsedFilter = string.Empty;

            parsedFilter = filter.Replace(TokenFromTime, string.Format(@"'{0}'", from.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCurrentSyncTime, string.Format(@"'{0}'", _currentSyncTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCustomBonds, string.Format("{0}", _customBonds.Count == 0 ? "null" : string.Join(",", _customBonds.ToArray())))
                .Replace(TokenFromTimestamp, string.Format(@"'{0}'", from.ToString(TimestampFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCurrentSyncTimestamp, string.Format(@"'{0}'", _currentSyncTime.ToString(TimestampFormat, CultureInfo.InvariantCulture)));

            return parsedFilter;
        }

        private string ParseFilterTokens(string filter, DateTime from, int count)
        {
            string parsedFilter = string.Empty;

            parsedFilter = filter.Replace(TokenLastSyncTime, string.Format(@"'{0}'", from.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenLastSyncDate, string.Format(@"'{0}'", from.Date.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCurrentSyncTime, string.Format(@"'{0}'", _currentSyncTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture)))
                .Replace(TokenCustomBonds, string.Format("{0}", _customBonds.Count == 0 ? "null" : string.Join(",", _customBonds.ToArray())));

            return parsedFilter + " and rownum <= " + count ;
        }
        #endregion
    }
}
