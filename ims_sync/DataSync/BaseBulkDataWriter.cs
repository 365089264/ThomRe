using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Data;
using Luna.DataSync.Setting;
using System.Diagnostics;
using System.Data.SqlClient;

namespace Luna.DataSync.Core
{
    public abstract class BaseBulkDataWriter : IBulkDataWritter
    {
        #region Private Fields
        private string _connString;
        private int _sqlCommandTimeout;
        private IDbConnection _conn;
        private IDbTransaction _trans;
        #endregion

        #region Constructors
        public BaseBulkDataWriter(string connString, int sqlCommandTimeout)
        {
            this._connString = connString;
            this._sqlCommandTimeout = sqlCommandTimeout;
        }
        #endregion

        #region IBulkDataWriter Members
        public void Init()
        {
            _conn = GetDbConnection(_connString);
            _conn.Open();
        }

        public long Write(DataTable data, TableMapping tableMapping, ref StringBuilder msgBuilder)
        {
            int currentInsertRowCount = 0, currentUpdateRowCount = 0;
            return Write(data, tableMapping, ref msgBuilder, ref currentInsertRowCount, ref currentUpdateRowCount);
        }

        public long Write(DataTable data, TableMapping tableMapping, ref StringBuilder msgBuilder, ref int insertRowCount, ref int updateRowCount)
        {
            if (data.Rows.Count == 0)
                return 0;

            msgBuilder.Append(string.Format("Table sync detail for : {0} \n", tableMapping.Source));
            var destColumnMetaDataList = LoadTableMetaData(tableMapping.Destination);
            ValidateTableMapping(tableMapping, destColumnMetaDataList);

            var destColumnNameTypeMappings = destColumnMetaDataList.ToDictionary(
                columnMetaData => columnMetaData.ColumnName,
                columnMetaData => columnMetaData.ColumnType,
                StringComparer.OrdinalIgnoreCase);

            if (tableMapping.IsContainFile)
            {
                return SaveVarBinaryData(data, tableMapping, destColumnNameTypeMappings);
            }

            if (tableMapping.IsContainFilePath)
            {
                return SaveVarBinaryDataFromPath(data, tableMapping, destColumnNameTypeMappings, tableMapping.FilePath, tableMapping.PathColumn);
            }

            var keyColumnNames = tableMapping.GetKeyColumnNames();
            var sourceDataValues = new Dictionary<KeySet, Dictionary<string, object>>();
            foreach (DataRow dataRow in data.Rows)
            {
                var destColumnNameValueMappings = LoadDestColumnNameValueMappings(dataRow, tableMapping);

                var keySet = new KeySet();
                foreach (var keyColumnName in keyColumnNames)
                {
                    keySet.Add(keyColumnName, destColumnNameTypeMappings[keyColumnName], destColumnNameValueMappings[keyColumnName]);
                }

                if (sourceDataValues.ContainsKey(keySet))
                    throw new Exception(string.Format("The source data (table: {0}) contains rows with duplicated keys", tableMapping.Source));
                
                sourceDataValues.Add(keySet, destColumnNameValueMappings);
            }

            var immutableKeySets = GetImmutableKeySets(sourceDataValues, tableMapping, destColumnNameTypeMappings);
            var destinationKeySets = GetDestinationKeySets(immutableKeySets, tableMapping, destColumnNameTypeMappings, keyColumnNames);

            if (!tableMapping.IsKeepObsoleteDestinationData)
                RemoveObsoleteDestinationData(sourceDataValues, destinationKeySets, tableMapping, destColumnNameTypeMappings);

            return SaveSourceData(sourceDataValues, destinationKeySets, tableMapping, destColumnNameTypeMappings, ref msgBuilder, ref insertRowCount, ref updateRowCount);
        }



        public DateTime GetMaxDateTime(string tableName, string columnName)
        {
            var sql = string.Format("select max({0}) from {1}", columnName, tableName);
            IDbCommand cmd = GetDbCommand(sql, _conn);
            cmd.CommandTimeout = _sqlCommandTimeout;
            var reader = cmd.ExecuteReader();
            reader.Read();
            if ((reader[0] == null) || (reader[0] == DBNull.Value))
            {
                return Convert.ToDateTime("1999-1-1"); 
            }
            else
            {
                 return reader.GetDateTime(0);
            }

        }

        public void Close()
        {
            if (_conn != null)
                _conn.Close();
        }

        public virtual void BeginTrans()
        {
            _trans = _conn.BeginTransaction(GetDbIsolationLevel());
        }

        public void CommitTrans()
        {
            if (_trans != null)
                _trans.Commit();
        }

        public void RollBackTrans()
        {
            if (_trans != null)
                _trans.Rollback();
        }
        #endregion

        #region Protected Members
        protected abstract IDbConnection GetDbConnection(string connString);

        protected abstract IDbCommand GetDbCommand(string cmdText, IDbConnection conn, IDbTransaction trans);

        protected abstract IsolationLevel GetDbIsolationLevel();

        protected abstract string GetTopOneRecordFromTableSQLString(string tableName);

        protected abstract string GetDateTimeFormatString();

        protected abstract string GetDateFormatString();

        protected abstract string GetTimestampFormatString();

        protected abstract IDataParameter GetDataParameter(string paraName, DbColumnType columnType, object value);

        protected abstract string GetParaSpliter();
        protected abstract IDbCommand GetDbCommand(string cmdText, IDbConnection conn);
        #endregion

        #region Private Methods
        private IEnumerable<ColumnMetaData> LoadTableMetaData(string tableName)
        {
            List<ColumnMetaData> columnMetaDataList = new List<ColumnMetaData>();

            using (IDbCommand cmd = GetDbCommand(GetTopOneRecordFromTableSQLString(tableName), _conn, _trans))
            {
                DataTable schemaTable = new DataTable();

                using (IDataReader dataReader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    schemaTable = dataReader.GetSchemaTable();
                }

                foreach (DataRow row in schemaTable.Rows)
                {
                    //20141119 yy the table from GetSchemaTable for mysql ,have no column DataTypeName,only DataType,so we need to check
                    var dt = DbColumnType.INT;
                    if (schemaTable.Columns.Contains("DataTypeName"))
                    {
                        dt = (DbColumnType)Enum.Parse(typeof(DbColumnType), row["DataTypeName"].ToString().ToUpper());
                    }
                    else if (schemaTable.Columns.Contains("ProviderSpecificDataType") && row["ProviderSpecificDataType"].ToString().Contains("OracleClient"))
                    {
                        switch (row["DataType"].ToString())
                        {
                            case "System.Int32":
                            case "System.Int64":
                                dt = DbColumnType.INT;
                                break;
                            case "System.DateTime":
                                if (schemaTable.Columns.Contains("ProviderType") &&
                                    row["ProviderType"].ToString() == "6")
                                {
                                    dt = DbColumnType.DATE;
                                }else if (schemaTable.Columns.Contains("ProviderType") &&
                                    row["ProviderType"].ToString() == "18")
                                {
                                    dt = DbColumnType.TIMESTAMP;
                                }
                                else
                                {
                                    dt = DbColumnType.DATETIME;  
                                }
                                break;
                            case "System.Decimal":
                                dt = DbColumnType.DECIMAL;
                                break;
                            case "System.String":
                                switch (row["ProviderType"].ToString())
                                {
                                    case "3":
                                        dt = DbColumnType.CHAR;
                                        break;
                                    case "4":
                                        dt = DbColumnType.CLOB;
                                        break;
                                    case "12":
                                        dt = DbColumnType.NTEXT;
                                        break;
                                    default:
                                        dt = DbColumnType.VARCHAR2;
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (row["DataType"].ToString())
                        {
                            case "System.Int32":
                            case "System.Int64":
                                dt = DbColumnType.INT;
                                break;
                            case "System.DateTime":
                                dt = DbColumnType.DATETIME;
                                break;
                            case "System.Decimal":
                                dt = DbColumnType.DECIMAL;
                                break;
                            case "System.String":
                                dt = DbColumnType.NVARCHAR;
                                break;
                            default:
                                break;
                        }
                    }

                    columnMetaDataList.Add(new ColumnMetaData()
                    {
                        ColumnName = (string)row["ColumnName"],
                        ColumnType = dt,
                        IsKey = (bool)row["IsKey"]
                    });
                }

                return columnMetaDataList;
            }
        }

        private void ValidateTableMapping(TableMapping tableMapping, IEnumerable<ColumnMetaData> columnMetaDataList)
        {
            StringBuilder errorMsg = new StringBuilder();
            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
            {
                ColumnMetaData column = columnMetaDataList.FirstOrDefault(
                    columnMetaData => string.Compare(columnMetaData.ColumnName, columnMapping.Destination, true) == 0);

                if (column == null)
                    errorMsg.AppendFormat("The table ({0}) does not contains the column ({1}) in the DB", tableMapping.Destination, columnMapping.Destination).AppendLine();
            }

            /* TODO: the following code will be used when the primary keys are added in the related tables for DSOS sync in Luna DB
            foreach (string keyColumnName in tableMapping.GetKeyColumnNames())
            {
                ColumnMetaData keyColumn = columnMetaDataList.FirstOrDefault(
                    columnMetaData => string.Compare(columnMetaData.ColumnName, keyColumnName, true) == 0 && columnMetaData.IsKey);

                if (keyColumn == null)
                    errorMsg.AppendFormat("The table ({0}) does not contains the key column ({1}) in the DB", tableMapping.Destination, keyColumnName).AppendLine();
            }
             * */

            if (errorMsg.Length != 0)
                throw new Exception(
                    string.Format("The table mapping (destination: {0}) is invalid.  Details: {1}", tableMapping.Destination, errorMsg.ToString()));
        }

        private long SaveSourceData(
            Dictionary<KeySet, Dictionary<string, object>> sourceDataValues,
            HashSet<KeySet> destinationKeySets,
            TableMapping tableMapping,
            Dictionary<string, DbColumnType> destColumnNameTypeMappings, ref StringBuilder msgBuilder)
        {
            int currentInsertRowCount = 0, currentUpdateRowCount = 0;
            return SaveSourceData(sourceDataValues, destinationKeySets, tableMapping, destColumnNameTypeMappings, ref msgBuilder, ref currentInsertRowCount, ref currentUpdateRowCount);
        }

        private long SaveSourceData(
            Dictionary<KeySet, Dictionary<string, object>> sourceDataValues,
            HashSet<KeySet> destinationKeySets,
            TableMapping tableMapping,
            Dictionary<string, DbColumnType> destColumnNameTypeMappings, ref StringBuilder msgBuilder, ref int insertRowCount, ref int updateRowCount)
        {
            long numOfRows = 0;
            msgBuilder.Append("<ol>");
            foreach (var kvp in sourceDataValues)
            {
                var sourceKeySet = kvp.Key;
                var destColumnNameValueMappings = kvp.Value;

                var sqlCmdText = destinationKeySets.Contains(sourceKeySet) ?
                    BuildSqlCmdTextForUpdateData(destColumnNameValueMappings, tableMapping, sourceKeySet) :
                    BuildSqlCmdTextForInsertData(destColumnNameValueMappings, tableMapping);

                if (destinationKeySets.Contains(sourceKeySet))
                {
                    updateRowCount++;
                }
                else
                {
                    insertRowCount++;
                }

                var keys = sourceKeySet.Keys.Aggregate("", (current, tuple) => current + string.Format("{0}:{1},", tuple.Item1, tuple.Item3));
                keys = keys.Substring(0, keys.Length - 1);

                using (IDbCommand cmd = GetDbCommand(sqlCmdText, _conn, _trans))
                {
                    foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                    {
                        cmd.Parameters.Add(GetDataParameter(GetParaSpliter() + columnMapping.Destination, destColumnNameTypeMappings[columnMapping.Destination], columnMapping.Value ?? destColumnNameValueMappings[columnMapping.Destination]));
                    }

                    //for where clause if it's update statement
                    if (destinationKeySets.Contains(sourceKeySet))
                    {
                        foreach (var key in sourceKeySet.Keys)
                        {
                            if (!cmd.Parameters.Contains(GetParaSpliter() + key.Item1))
                            {
                                cmd.Parameters.Add(GetDataParameter(GetParaSpliter() + key.Item1, key.Item2, key.Item3));
                            }
                        }
                    }

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("{0} sync failed\n Exception detail: {1}\n", keys, ex));
                    }
                }

                msgBuilder.Append("<li>" + keys + " sync succeed</li>");
                numOfRows++;

                // Let db server have a break when sync is too busy.
                if (numOfRows % 1000 == 0)
                    System.Threading.Thread.Sleep(100);
            }

            msgBuilder.Append("</ol>");
            return numOfRows;
        }

       
        private string BuildSqlCmdTextForInsertData(
            Dictionary<string, object> destColumnNameValueMappings,
            TableMapping tableMapping)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} ( ", tableMapping.Destination);
            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                sb.AppendFormat("{0}, ", columnMapping.Destination);

            sb.Remove(sb.Length - 2, 2);
            sb.Append(") VALUES (");

            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings) //build sql: insert into xx_table (filed1, ...., filedn) values (:filed1, ...., :fieldn)
                sb.AppendFormat("{0}, ", GetParaSpliter() + columnMapping.Destination);
            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            return sb.ToString();
        }

        private string BuildSqlCmdTextForUpdateData(
            Dictionary<string, object> destColumnNameValueMappings,
            TableMapping tableMapping,
            KeySet sourceKeySet)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("UPDATE {0} SET ", tableMapping.Destination);
            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                sb.AppendFormat(
                    "{0} = {1}, ",
                    columnMapping.Destination,
                    GetParaSpliter() + columnMapping.Destination);
            sb.Remove(sb.Length - 2, 2);

            sb.Append(" WHERE ");
            foreach (var key in sourceKeySet.Keys)
            {
                if (string.IsNullOrEmpty(destColumnNameValueMappings[key.Item1].ToString()))
                {
                    sb.AppendFormat("{0} is null AND ", key.Item1);
                }
                else
                {
                    sb.AppendFormat("{0} = {1} AND ", key.Item1, GetParaSpliter() + key.Item1);
                }
                
            }
            sb.Remove(sb.Length - 4, 4);

            return sb.ToString();
        }

        private long SaveVarBinaryDataFromPath(DataTable data, TableMapping tableMapping, Dictionary<string, DbColumnType> destColumnNameTypeMappings, string pathPrefix, string pathColumn)
        {
            DataTable table = new DataTable();
            var l = tableMapping.ColumnMappings.ToList();
            l.Add(new ColumnMapping("Content", null, "Content", null, "false", "false"));
            tableMapping.ColumnMappings = l;

            foreach (var c in tableMapping.ColumnMappings)
            {
                table.Columns.Add(c.Source, GetSystemType(destColumnNameTypeMappings[c.Source]));
            }

            long ret = 0;
            int count = 0;
            foreach (DataRow dataRow in data.Rows)
            {
                object[] columnValues = new object[table.Columns.Count];

                int i = 0;
                foreach (DataColumn c in table.Columns)
                {
                    if (c.ColumnName == "Content")
                        columnValues[i++] = GetFileContent(pathPrefix + Convert.ToString(dataRow[pathColumn]));
                    else
                        columnValues[i++] = dataRow[c.ColumnName];
                }

                count++;
                table.Rows.Add(columnValues);
                table.AcceptChanges();

                if (count == 500)
                {
                    ret += SaveVarBinaryData(table, tableMapping, destColumnNameTypeMappings);
                    table = new DataTable();
                    foreach (var c in tableMapping.ColumnMappings)
                    {
                        table.Columns.Add(c.Source, GetSystemType(destColumnNameTypeMappings[c.Source]));
                    }
                    count = 0;
                }
            }

            if (count != 0)
                ret += SaveVarBinaryData(table, tableMapping, destColumnNameTypeMappings);

            return ret;
        }

        private long SaveVarBinaryData(DataTable data, TableMapping tableMapping, Dictionary<string, DbColumnType> destColumnNameTypeMappings)
        {
            var keyColumnNames = tableMapping.GetKeyColumnNames();
            var keyColumnName = keyColumnNames.FirstOrDefault();

            string keyValues = ""; //values for key in table;

            //fetch dest table
            foreach (DataRow dataRow in data.Rows)
            {
                if (!string.IsNullOrEmpty(keyValues))
                    keyValues += ",";

                if (destColumnNameTypeMappings[keyColumnName] == DbColumnType.INT)
                    keyValues += dataRow[keyColumnName];
                else
                    keyValues += "'" + dataRow[keyColumnName] + "'";
            }

            SqlCommand qry = new SqlCommand("SELECT * FROM "
                                                + tableMapping.Destination
                                                + " WHERE " + keyColumnName
                                                + " IN (" + keyValues + ")",
                                                (SqlConnection)_conn,
                                                (SqlTransaction)_trans);
            SqlDataAdapter da = new SqlDataAdapter(qry);
            qry.CommandTimeout = this._sqlCommandTimeout;
            DataSet ds = new DataSet();
            da.Fill(ds, "DestTable");
            DataTable dt = ds.Tables["DestTable"];

            //update desttable
            List<string> updateKeys = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                string filter = "";
                string updateKey = "";
                
                foreach (var key in keyColumnNames)
                {
                    if (filter != "")
                        filter += " AND ";

                    if (destColumnNameTypeMappings[key] == DbColumnType.INT)
                        filter += key + " = " + row[key];
                    else
                        filter += key + " = '" + row[key].ToString() + "'";
                }

                DataRow dr = data.Select(filter).FirstOrDefault();
                if (dr != null)
                {
                    foreach (var key in keyColumnNames)
                        updateKey += dr[key].ToString();
                    updateKeys.Add(updateKey);

                    foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
                    {
                        if (columnMapping.IsKey && columnMapping.IsImmutableKey)
                            continue;
                        row[columnMapping.Destination] = dr[columnMapping.Source];
                    }
                }
            }

            //insert into desttable
            foreach (DataRow row in data.Rows)
            {
                string insertKey = "";
                foreach (var key in keyColumnNames)
                    insertKey += row[key].ToString();

                if (!updateKeys.Contains(insertKey))
                {
                    row.SetAdded();
                    dt.ImportRow(row);
                }
            }

            //build insert and update template
            SqlDataAdapter adapter = new SqlDataAdapter();
            string updatesql = BuildUpateTemplate(tableMapping);
            SqlCommand updateCmd = new SqlCommand(updatesql, (SqlConnection)_conn, (SqlTransaction)_trans);
            int i = 1;
            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
            {
                SqlDbType type = GetColumnType(destColumnNameTypeMappings[columnMapping.Destination]);
                int size = GetColumnSize(destColumnNameTypeMappings[columnMapping.Destination]);

                if (columnMapping.IsKey && columnMapping.IsImmutableKey)
                {
                    var param = updateCmd.Parameters.Add("@Id", type, size, tableMapping.GetKeyColumnNames().FirstOrDefault());
                    param.SourceVersion = DataRowVersion.Original;
                }
                else
                {
                    updateCmd.Parameters.Add("@Field" + i.ToString(), type, size, columnMapping.Source);
                    i++;
                }
            }

            String insertSql = BuildInsertTemplate(tableMapping); 
            SqlCommand insertCommand = new SqlCommand(insertSql, (SqlConnection)_conn, (SqlTransaction)_trans);
            i = 1;
            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
            {
                SqlDbType type = GetColumnType(destColumnNameTypeMappings[columnMapping.Destination]);
                int size = GetColumnSize(destColumnNameTypeMappings[columnMapping.Destination]);
                insertCommand.Parameters.Add("@Field" + i.ToString(), type, size, columnMapping.Source);
                i++;
            }

            updateCmd.CommandTimeout = this._sqlCommandTimeout;
            insertCommand.CommandTimeout = this._sqlCommandTimeout;

            adapter.UpdateCommand = updateCmd;
            adapter.InsertCommand = insertCommand;
            adapter.Update(ds, "DestTable");

            return data.Rows.Count;
        }

        private byte[] GetFileContent(string path)
        {
            byte[] content;

            try
            {
                content = System.IO.File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                return null;
            }

            return content;
        }

        private Type GetSystemType(DbColumnType columnType)
        {
            Type t = null;

            switch (columnType)
            {
                case DbColumnType.INT:
                    t = typeof(int);
                    break;
                case DbColumnType.DATETIME:
                    t = typeof(DateTime);
                    break;
                case DbColumnType.NVARCHAR:
                    t = typeof(string);
                    break;
                case DbColumnType.VARBINARY:
                    t = typeof(byte[]);
                    break;
                case DbColumnType.BIGINT:
                    t = typeof(long);
                    break;
                default:
                    break;
            }

            return t;
        }

        private SqlDbType GetColumnType(DbColumnType columnType)
        {
            var typeList = Enum.GetValues(typeof(SqlDbType)).Cast<Enum>().ToList();

            foreach (var t in typeList)
            {
                if (t.ToString().ToUpper() == columnType.ToString())
                    return (SqlDbType)t;
            }

            return 0;
        }

        private int GetColumnSize(DbColumnType columnType)
        {
            switch (columnType)
            {
                case DbColumnType.VARBINARY:
                    return int.MaxValue;
                case DbColumnType.DATETIME:
                    return 8;
                case DbColumnType.INT:
                    return 4;
            }

            return 0;
        }

        private string BuildUpateTemplate(TableMapping tableMapping)
        {
            //Build update template
            string sql = @" UPDATE " + tableMapping.Destination + " SET ";
            string colSet = "";
            int i = 1;
            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
            {
                if (columnMapping.IsKey && columnMapping.IsImmutableKey)
                    continue;

                if (!string.IsNullOrEmpty(colSet))
                    colSet += ",";

                colSet += "[" + columnMapping.Destination + "]" + " = " + "@Field" + i.ToString();
                i++;
            }

            return sql + colSet + " WHERE " + tableMapping.GetKeyColumnNames().FirstOrDefault() + " = @Id";
        }

        private string BuildInsertTemplate(TableMapping tableMapping)
        {
            String sql = @" INSERT INTO " + tableMapping.Destination; 
            string colSet = "";
            string valueSet = "";
            int i = 1;

            foreach (ColumnMapping columnMapping in tableMapping.ColumnMappings)
            {
                if (!string.IsNullOrEmpty(colSet))
                {
                    colSet += ",";
                    valueSet += ",";
                }

                colSet += "[" + columnMapping.Destination + "]";
                valueSet += "@Field" + i.ToString();
                i++;
            }

            return sql + "(" + colSet + ") VALUES (" + valueSet + ")";
        }

       

        private Dictionary<string, object> LoadDestColumnNameValueMappings(IDataReader data, TableMapping tableMapping)
        {
            Dictionary<string, object> destColumnNameValueMappings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < data.FieldCount; index++)
            {
                string sourceColumnName = data.GetName(index);
                string destColumnName = tableMapping[sourceColumnName];
                destColumnNameValueMappings[destColumnName] = data.GetValue(index);
            }

            return destColumnNameValueMappings;
        }

        private Dictionary<string, object> LoadDestColumnNameValueMappings(DataRow dataRow, TableMapping tableMapping)
        {
            Dictionary<string, object> destColumnNameValueMappings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < dataRow.Table.Columns.Count; index++)
            {
                string sourceColumnName = dataRow.Table.Columns[index].ColumnName;
                string destColumnName = tableMapping[sourceColumnName];
                destColumnNameValueMappings[destColumnName] = dataRow[index];
            }

            return destColumnNameValueMappings;
        }

        private HashSet<KeySet> GetDestinationKeySets(
            HashSet<KeySet> immutableKeySets,
            TableMapping tableMapping,
            Dictionary<string, DbColumnType> destColumnNameTypeMappings,
            IEnumerable<string> keyColumnNames)
        {
            var keySets = new HashSet<KeySet>();

            var chunkIndex = 0;
            var chunkSize = 1000;
            while (chunkIndex * chunkSize < immutableKeySets.Count)
            {
                var chunkedImmutableKeySets = immutableKeySets.Skip(chunkIndex++ * chunkSize).Take(chunkSize);

                using (IDbCommand cmd = GetDbCommand(
                    BuildSqlCmdTextForLoadDestinationKeySets(
                        chunkedImmutableKeySets,
                        tableMapping,
                        keyColumnNames), _conn, _trans))
                {
                    cmd.CommandTimeout = _sqlCommandTimeout;

                    using (IDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var keySet = new KeySet();
                            foreach (var keyColumnName in keyColumnNames)
                            {
                                keySet.Add(keyColumnName, destColumnNameTypeMappings[keyColumnName], dataReader[keyColumnName]);
                            }

                            if (keySets.Contains(keySet))
                                throw new Exception(
                                    string.Format(
                                    "The destination table ({0}) contains data with duplicated keys ({1})",
                                    tableMapping.Destination,
                                    keySet.ToString()));

                            keySets.Add(keySet);
                        }
                    }
                }
            }

            return keySets;
        }

        private string BuildSqlCmdTextForLoadDestinationKeySets(
            IEnumerable<KeySet> immutableKeySets,
            TableMapping tableMapping,
            IEnumerable<string> keyColumnNames)
        {
            var immutableKeyColumnName = tableMapping.GetImmutableKeyColumnName();

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");
            foreach (var keyColumnName in keyColumnNames)
                sb.AppendFormat(" {0}, ", keyColumnName);

            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(" FROM {0} ", tableMapping.Destination);

            sb.AppendFormat(
                " WHERE {0} in ({1})",
                immutableKeyColumnName,
                string.Join(
                    ",",
                    immutableKeySets.Select(
                        keySet => BuildSqlStringForValue(keySet.Keys.First().Item3, keySet.Keys.First().Item2)).ToArray()));

            return sb.ToString();
        }

        private void RemoveObsoleteDestinationData(
          Dictionary<KeySet, Dictionary<string, object>> sourceDataValues,
          HashSet<KeySet> destinationKeySets,
          TableMapping tableMapping,
          Dictionary<string, DbColumnType> destColumnNameTypeMappings)
        {
            foreach (var destKeySet in destinationKeySets)
            {
                if (!sourceDataValues.ContainsKey(destKeySet))
                {
                    using (IDbCommand cmd = GetDbCommand(
                        BuildSqlCmdTextForRemoveObsoleteDestinationData(tableMapping, destKeySet),
                        _conn,
                        _trans))
                    {
                        foreach (var key in destKeySet.Keys)
                        {
                            if (string.IsNullOrEmpty(key.Item3.ToString()))
                            {
                                cmd.CommandText = cmd.CommandText.Replace(key.Item1 + " = " + GetParaSpliter() + key.Item1,key.Item1+ " is null");
                            }
                            else
                            {
                                cmd.Parameters.Add(GetDataParameter(GetParaSpliter() + key.Item1, key.Item2, key.Item3));  
                            }
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private string BuildSqlCmdTextForRemoveObsoleteDestinationData(TableMapping tableMapping, KeySet destKeySet)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("DELETE FROM {0} ", tableMapping.Destination);

            sb.Append(" WHERE ");

            foreach (var key in destKeySet.Keys)
                sb.AppendFormat(" {0} = {1} AND ", key.Item1, GetParaSpliter() + key.Item1);

            sb.Remove(sb.Length - 4, 4);

            return sb.ToString();
        }

        private HashSet<KeySet> GetImmutableKeySets(
            Dictionary<KeySet, Dictionary<string, object>> sourceDataValues,
            TableMapping tableMapping,
            Dictionary<string, DbColumnType> destColumnNameTypeMappings)
        {
            var immutableKeySets = new HashSet<KeySet>();
            var immutableKeyColumnName = tableMapping.GetImmutableKeyColumnName();

            foreach (var row in sourceDataValues.Values)
            {
                KeySet keySet = new KeySet();
                keySet.Add(immutableKeyColumnName, destColumnNameTypeMappings[immutableKeyColumnName], row[immutableKeyColumnName]);

                if (!immutableKeySets.Contains(keySet))
                    immutableKeySets.Add(keySet);
            }

            return immutableKeySets;
        }

        private string BuildSqlStringForValue(object value, DbColumnType type)
        {
            if (value == null || value.GetType() == typeof(System.DBNull))
                return "null";
            // HACK: a special case where the Asset_ID of GovCorp..Asset view is binary type in EJV DSOS, 
            // but the assetId of Asset table in Luna DB is varchar,
            // and we cannot convert the byte[] to string directly using toString method
            if (value.GetType() == typeof(byte[]) && (type == DbColumnType.CHAR || type == DbColumnType.VARCHAR || type == DbColumnType.VARCHAR2))
            {
                return string.Format("'{0}'", ((byte[])value).ToStringBySybaseStandard());
            }

            if (type == DbColumnType.CHAR ||
                type == DbColumnType.VARCHAR || type == DbColumnType.TEXT || type == DbColumnType.VARCHAR2)
                return string.Format("'{0}'", value.ToString().Replace("'", "''")); // Escape the single quote in the string text.

            if (type == DbColumnType.NVARCHAR || type == DbColumnType.NTEXT || type == DbColumnType.NCHAR)
                return string.Format("N'{0}'", value.ToString().Replace("'", "''")); // Escape the single quote in the string text.

            if (type == DbColumnType.DATETIME || type == DbColumnType.DATETIME2)
            {
                return value.ToString() == string.Empty ? "null" : string.Format("'{0}'", ((DateTime)value).ToString(GetDateTimeFormatString(), CultureInfo.InvariantCulture));
            }
            if (type == DbColumnType.DATE)
            {
                return value.ToString() == string.Empty ? "null" : string.Format("'{0}'", ((DateTime)value).ToString(GetDateFormatString(), CultureInfo.InvariantCulture));
            }
            if (type == DbColumnType.TIMESTAMP)
            {
                return value.ToString() == string.Empty ? "null" : string.Format("'{0}'", ((DateTime)value).ToString(GetTimestampFormatString(), CultureInfo.InvariantCulture));
            }

            if (type == DbColumnType.FLOAT ||
                type == DbColumnType.BIGINT ||
                type == DbColumnType.INT ||
                type == DbColumnType.NUMERIC ||
                type == DbColumnType.SMALLINT ||
                type == DbColumnType.BINARY ||
                type == DbColumnType.REAL ||
                type == DbColumnType.DECIMAL)
            {
                return value.ToString() == string.Empty ? "null" : value.ToString();
            }

            if (type == DbColumnType.BIT)
                return value.ToString() == string.Empty ? "0" : (Convert.ToInt32(value).ToString());

            throw new Exception("The type is not supported in SQL: " + type.ToString());
        }

        protected object BuildValueForParameters(object value, DbColumnType type)
        {
            if (value == null || value.GetType() == typeof(System.DBNull))
                return DBNull.Value;
            if (value.GetType() == typeof(byte[]) && (type == DbColumnType.CHAR || type == DbColumnType.VARCHAR || type == DbColumnType.VARCHAR2))
            {
                return string.Format("{0}", ((byte[])value).ToStringBySybaseStandard());
            }

            return value;
        }
        #endregion
    }

    public enum DbColumnType
    {
        CHAR = 0,
        VARCHAR,
        VARCHAR2,
        NVARCHAR,
        DATETIME,
        DATETIME2,
        FLOAT,
        INT,
        SMALLINT,
        NUMERIC,
        BINARY,
        BIT,
        REAL,
        DECIMAL,
        TEXT,
        BIGINT,
        NTEXT,
        NCHAR,
        VARBINARY,
        DATE,
        CLOB,
        TIMESTAMP
    }
}
