using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Luna.DataSync.Setting.Entity;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;
using System.Globalization;

namespace Luna.DataSync.Setting
{
    public class FtpSyncXmlManager
    {
        private string _xml;
        private DbSetting _sourceDb;
        private int _sqlCommandTimeout = 1000;
        private string _fileSavePath;
        private string _fileName;
        private string _hostname;
        private string _username;
        private string _password;
        private string _targetDir;
        private List<FtpSyncTableMapping> _tableMappings = new List<FtpSyncTableMapping>();
        private DateTime _lastSyncTime;
        private DateTime _currentSyncTime;
        private string _formatString = "yyyy-MM-dd hh:mm:ss tt";
        private string _oracleFormatString = "dd-MMM-yyyy hh:mm:ss tt";

        #region Constructors
        public FtpSyncXmlManager(string xml, DateTime lastSyncTime, DateTime currentSyncTime)
        {
            this._xml = xml;
            this._currentSyncTime = currentSyncTime;
            this._lastSyncTime = lastSyncTime;
        }
        #endregion
        public void Init()
        {
            LoadSettings();
        }
        public DataSet Execute()
        {
            var ds = new DataSet();
            if (_sourceDb.Type.ToString() == "ORACLE")
            {
                using (var con = new OracleConnection(_sourceDb.Conn))
                {
                    foreach (var mapping in TableMappings)
                    {
                        using (var cmd = new OracleCommand())
                        {
                            cmd.Connection = con;
                            var sb = new StringBuilder();
                            sb.Append("select ");
                            foreach (var column in mapping.ColumnMappings)
                            {
                                sb.Append(column.Source + ",");
                            }
                            //_lastSyncTime = Convert.ToDateTime("2015-01-10 11:00:00 PM");
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(" from " + mapping.Source);
                            sb.Append(" where " + mapping.Filter.Replace("{LastSyncTime}", _lastSyncTime.ToString(_oracleFormatString, CultureInfo.InvariantCulture)).Replace("{CurrentSyncTime}", _currentSyncTime.ToString(_oracleFormatString, CultureInfo.InvariantCulture)));
                            cmd.CommandText = sb.ToString();
                            cmd.CommandTimeout = _sqlCommandTimeout;
                            var da = new OracleDataAdapter(cmd);
                            da.Fill(ds, mapping.Destination);
                        }
                    }
                }
            }
            else
            {

                using (var con = new SqlConnection(_sourceDb.Conn))
                {
                    foreach (var mapping in TableMappings)
                    {
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            var sb = new StringBuilder();
                            sb.Append("select ");
                            foreach (var column in mapping.ColumnMappings)
                            {
                                sb.Append(column.Source + ",");
                            }
                            //_lastSyncTime = Convert.ToDateTime("2015-01-10 11:00:00 PM");
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(" from " + mapping.Source);
                            sb.Append(" where " + mapping.Filter.Replace("{LastSyncTime}", _lastSyncTime.ToString(_formatString, CultureInfo.InvariantCulture)).Replace("{CurrentSyncTime}", _currentSyncTime.ToString(_formatString, CultureInfo.InvariantCulture)));
                            cmd.CommandText = sb.ToString();
                            cmd.CommandTimeout = _sqlCommandTimeout;
                            var da = new SqlDataAdapter(cmd);
                            da.Fill(ds, mapping.Destination);
                        }
                    }
                }

            }

            return ds;
        }


        public DbSetting SourceDb
        {
            get { return _sourceDb; }
        }
        public IEnumerable<FtpSyncTableMapping> TableMappings
        {
            get { return _tableMappings; }
        }

        public int SqlCommandTimeout
        {
            get { return _sqlCommandTimeout; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public string FileSavePath
        {
            get { return _fileSavePath; }
        }
        public string HostName
        {
            get { return _hostname; }
        }
        public string Password
        {
            get { return _password; }
        }
        public string UserName
        {
            get { return _username; }
        }
        public string TargetDir
        {
            get { return _targetDir; }
        }

        public string GetDateTimeFormatString(string format)
        {
            return _formatString = format;
        }

        #region Private Methods

        private void LoadSettings()
        {
            var doc = new XmlDocument();
            doc.LoadXml(_xml);

            // Retrieve source DB setting
            var sourceDbNode = doc.SelectSingleNode("settings/source-db");
            if (sourceDbNode == null)
                throw new Exception("Cannot find source DB setting.");
            _sourceDb = LoadDbInfo(sourceDbNode);


            // Retrieve sql command timeout setting
            var sqlCommandTimeoutNode = doc.SelectSingleNode("settings/sql-command-timeout");
            if (sqlCommandTimeoutNode != null)
            {
                int timeout;
                if (int.TryParse(sqlCommandTimeoutNode.InnerText, out timeout))
                    _sqlCommandTimeout = timeout;
            }

            var fileSavePath = doc.SelectSingleNode("settings/fileSavePath");
            if (fileSavePath != null)
            {
                _fileSavePath = fileSavePath.InnerText;
            }

            var hostName = doc.SelectSingleNode("settings/hostname");
            if (hostName != null)
            {
                _hostname = hostName.InnerText;
            }

            var userName = doc.SelectSingleNode("settings/username");
            if (userName != null)
            {
                _username = userName.InnerText;
            }

            var password = doc.SelectSingleNode("settings/password");
            if (password != null)
            {
                _password = password.InnerText;
            }

            var targetDir = doc.SelectSingleNode("settings/targetDir");
            if (targetDir != null)
            {
                _targetDir = targetDir.InnerText;
            }

            var fileName = doc.SelectSingleNode("settings/fileName");
            if (fileName != null)
            {
                _fileName = fileName.InnerText;
            }

            var tableMappingNodes = doc.SelectNodes("settings/mappings/table-mapping");
            if (tableMappingNodes != null)
                foreach (XmlNode tableMappingNode in tableMappingNodes)
                    _tableMappings.Add(GetTableMapping(tableMappingNode));//获取所有的表名称

        }
        #endregion
        private FtpSyncTableMapping GetTableMapping(XmlNode node)
        {
            var source = string.Empty;
            var destination = string.Empty;
            var filter = string.Empty;
            var isKeepObsoleteDestinationData = "false";

            // TODO: error handling
            source = node.Attributes["source"].InnerText;

            var destinationAttribute = node.Attributes["destination"];
            if (destinationAttribute != null)
                destination = node.Attributes["destination"].InnerText;
            else // If destination attribute is not specified, we assume it should be the same as source.
                destination = source;

            var filterNode = node.SelectSingleNode("filter");
            if (filterNode != null)
                filter = filterNode.InnerText;

            var isKeepObsoleteDestinationDataAttribute = node.Attributes["isKeepObsoleteDestinationData"];
            if (isKeepObsoleteDestinationDataAttribute != null)
                isKeepObsoleteDestinationData = isKeepObsoleteDestinationDataAttribute.InnerText;

            FtpSyncTableMapping tableMapping = new FtpSyncTableMapping(source, destination, filter, isKeepObsoleteDestinationData);
            tableMapping.IsContainFile = (node.Attributes["isContainFile"] == null) ? false : bool.Parse(node.Attributes["isContainFile"].Value);
            var columnMappingNodes = node.SelectNodes("column-mapping");

            if (node.Attributes["isSqlServer"] != null && node.Attributes["isSync"] != null && node.Attributes["isSqlServer"].Value == "true" && node.Attributes["isSync"].Value == "true")
            {
                string columnname;
                string tableNameSource = node.Attributes["source"].Value;
                string dbConn = node.ParentNode.ParentNode.SelectSingleNode("source-db/conn").FirstChild.Value;
                List<string> columsNameSource = GetColumnsName(dbConn, tableNameSource);
                if (columnMappingNodes != null)
                {
                    foreach (XmlNode columnMappingNode in columnMappingNodes)
                    {
                        tableMapping.Add(GetColumnMapping(columnMappingNode));
                        columnname = columnMappingNode.Attributes["source"].Value;
                        columsNameSource.Remove(columnname);
                    }
                }
                return tableMapping;
            }
            else
            {
                if (columnMappingNodes != null)
                    foreach (XmlNode columnMappingNode in columnMappingNodes)
                        tableMapping.Add(GetColumnMapping(columnMappingNode));

                return tableMapping;
            }
        }

        private List<string> GetColumnsName(string dbConn, string tableName)
        {
            using (SqlConnection conn = new SqlConnection(dbConn))
            {
                List<string> columnsname = new List<string>();
                conn.Open();
                string cmdText = string.Format("select name from syscolumns where id = object_id('{0}') ", tableName);
                SqlCommand cmd = new SqlCommand(cmdText, conn);
                SqlDataReader thisSqlDataReader = cmd.ExecuteReader();
                while (thisSqlDataReader.Read())
                {
                    columnsname.Add(thisSqlDataReader[0].ToString());
                }
                return columnsname;
            }
        }

        private ColumnMapping GetColumnMapping(XmlNode node)
        {
            var source = string.Empty;
            string sourceTableAlias = null;
            var destination = string.Empty;
            string value = null;
            var isKey = "false";
            var isImmutableKey = "false";

            // TODO: error handling
            source = node.Attributes["source"].InnerText;

            var destinationAttribute = node.Attributes["destination"];
            if (destinationAttribute != null)
                destination = node.Attributes["destination"].InnerText;
            else // If destination attribute is not specified, we assume it should be the same as source.
                destination = source;

            var sourceTableAliasAttribute = node.Attributes["sourceTableAlias"];
            if (sourceTableAliasAttribute != null)
                sourceTableAlias = sourceTableAliasAttribute.InnerText;

            var valueAttribute = node.Attributes["value"];
            if (valueAttribute != null)
                value = valueAttribute.InnerText;

            var isKeyAttribute = node.Attributes["isKey"];
            if (isKeyAttribute != null)
                isKey = isKeyAttribute.InnerText;

            var isImmutableKeyAttribute = node.Attributes["isImmutableKey"];
            if (isImmutableKeyAttribute != null)
                isImmutableKey = isImmutableKeyAttribute.InnerText;

            return new ColumnMapping(source, sourceTableAlias, destination, value, isKey, isImmutableKey);
        }

        #region
        private DbSetting LoadDbInfo(XmlNode node)
        {
            var typeNode = node.SelectSingleNode("type");
            if (typeNode == null || string.IsNullOrEmpty(typeNode.InnerText))
                throw new Exception("Cannot find type info under the node: " + node);

            var connNode = node.SelectSingleNode("conn");
            if (connNode == null || string.IsNullOrEmpty(connNode.InnerText))
                throw new Exception("Cannot find conn info under the node: " + node);

            return new DbSetting(typeNode.InnerText, connNode.InnerText);
        }
        #endregion

    }

}
