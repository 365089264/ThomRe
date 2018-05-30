using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using System.Data.OracleClient;
using System.Data;

namespace Luna.DataSync.Setting
{
    public class XmlSettingManager : ISettingManager
    {
        private DbSetting _sourceDb;
        private DbSetting _destinationDb;
        private int _sqlCommandTimeout = 1000;
        private List<TableMapping> _tableMappings = new List<TableMapping>();
        private string _xml;
        private List<string> _customBonds = new List<string>();
        private List<Task> _postSyncTasks = new List<Task>();
        private string _sourceTableName;
        private string _dateKeyTable;
        private string _dateKeyColumn;
        private int _deltaHours;



        #region Constructors
        public XmlSettingManager(string xml)
        {
            this._xml = xml;
        }
        #endregion

        #region ISettingManager Members

        public string SourceTableName
        {
            get { return _sourceTableName; }
        }

        public void Init(string DestinationDbConn)
        {
            LoadSettings(DestinationDbConn);
        }

        public DbSetting SourceDb
        {
            get { return _sourceDb; }
        }

        public DbSetting DestinationDb
        {
            get { return _destinationDb; }
        }

        public IEnumerable<TableMapping> TableMappings
        {
            get { return _tableMappings; }
        }

        public int SqlCommandTimeout
        {
            get { return _sqlCommandTimeout; }
        }

        public List<string> CustomBonds
        {
            get { return _customBonds; }
        }

        public List<Task> PostSyncTasks
        {
            get { return _postSyncTasks; }
        }

        public String DateKeyTable
        {
            get { return _dateKeyTable; }
        }

        public String DateKeyColumn
        {
            get { return _dateKeyColumn; }
        }

        public int DeltaHours
        {
            get { return _deltaHours; }
        }

        #endregion

        #region Private Methods
        private void LoadSettings(string DestinationDbConn)
        {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_xml);

            // Retrieve source DB setting
            var sourceDBNode = doc.SelectSingleNode("settings/source-db");
            if (sourceDBNode == null)
                throw new Exception("Cannot find source DB setting.");
            _sourceDb = LoadDbInfo(sourceDBNode);

            // Retrieve destination setting
            var destinationDBNode = doc.SelectSingleNode("settings/destination-db");
            if (destinationDBNode == null)
                throw new Exception("Cannot find destination DB setting.");
            _destinationDb = LoadDbInfo(destinationDBNode);
            if (DestinationDbConn != string.Empty) this._destinationDb.Conn = DestinationDbConn;//read from outside

            // Retrieve sql command timeout setting
            var sqlCommandTimeoutNode = doc.SelectSingleNode("settings/sql-command-timeout");
            if (sqlCommandTimeoutNode != null)
            {
                int timeout = 0;
                if (int.TryParse(sqlCommandTimeoutNode.InnerText, out timeout))
                    _sqlCommandTimeout = timeout;
            }

            var tableMappingNodes = doc.SelectNodes("settings/mappings/table-mapping");
            if (tableMappingNodes != null)
                foreach (XmlNode tableMappingNode in tableMappingNodes)
                    _tableMappings.Add(GetTableMapping(tableMappingNode));//获取所有的表名称

            var postSyncTaskNodes = doc.SelectNodes("settings/post-sync-tasks/task");
            if (postSyncTaskNodes != null)
                foreach (XmlNode postSyncTaskNode in postSyncTaskNodes)
                    _postSyncTasks.Add(GetTask(postSyncTaskNode));

            var assetIdNodes = doc.SelectNodes("settings/custom-bonds/asset-id");
            if (assetIdNodes != null)
                foreach (XmlNode assetIdNode in assetIdNodes)
                    _customBonds.Add(assetIdNode.InnerText);

            var dateKeyNode = doc.SelectSingleNode("settings/date-key");
            if (dateKeyNode != null)
            {
                var tableNode = dateKeyNode.SelectSingleNode("table");
                if (tableNode != null)
                {
                    _dateKeyTable = tableNode.InnerText;
                }
                var columnNode = dateKeyNode.SelectSingleNode("column");
                if (columnNode != null)
                {
                    _dateKeyColumn = columnNode.InnerText;
                }
            }
            var deltaHourNode = doc.SelectSingleNode("settings/delta-hours");
            if (deltaHourNode != null)
            {
                try
                {
                    _deltaHours = int.Parse(deltaHourNode.InnerText);
                }
                catch (Exception e)
                {
                    _deltaHours = 0;
                    Console.WriteLine("delta-hours invalid.");
                }
            }
        }

        private Task GetTask(XmlNode node)
        {
            return new Task(node.Attributes["name"].InnerText);
        }

        private TableMapping GetTableMapping(XmlNode node)
        {
            string source = string.Empty;
            string destination = string.Empty;
            string filter = string.Empty;
            string isKeepObsoleteDestinationData = "false";

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

            TableMapping tableMapping = new TableMapping(source, destination, filter, isKeepObsoleteDestinationData);
            tableMapping.IsSyncFile = (node.Attributes["isSyncFile"] == null) ? false : bool.Parse(node.Attributes["isSyncFile"].Value);
            tableMapping.PathRoot = (node.Attributes["pathRoot"] == null) ? "" : node.Attributes["pathRoot"].Value;
            tableMapping.PathColumn = (node.Attributes["pathColumn"] == null) ? "" : node.Attributes["pathColumn"].Value;
            tableMapping.ExtColumn = (node.Attributes["extColumn"] == null) ? "" : node.Attributes["extColumn"].Value;

            var columnMappingNodes = node.SelectNodes("column-mapping");

            string tableNameSource = node.Attributes["source"].Value;
            //add table if isSync is set
            if (node.Attributes["isSync"] != null && node.Attributes["isSync"].Value == "true")
                this._sourceTableName = string.IsNullOrEmpty(this._sourceTableName) ? tableNameSource : this._sourceTableName + "," + tableNameSource;

            //for table which omit columns
            if (node.Attributes["isOmitColumns"] != null && node.Attributes["isOmitColumns"].Value == "true")
            {
                string columnname;
                string tableNameDes = node.Attributes["destination"].Value;
                string dbConn = node.ParentNode.ParentNode.SelectSingleNode("source-db/conn").FirstChild.Value;
                List<string> columsNameSource = GetColumnsName(dbConn, tableNameSource, _sourceDb.Type);
                List<string> columsNameDestination = GetColumnsName(DestinationDb.Conn, tableNameDes, _destinationDb.Type);
                ColumnMapping singleColumn;
                if (columnMappingNodes != null)
                {
                    foreach (XmlNode columnMappingNode in columnMappingNodes)
                    {
                        tableMapping.Add(GetColumnMapping(columnMappingNode));
                        columnname = columnMappingNode.Attributes["source"].Value;
                        columsNameSource.Remove(columnname);
                        columsNameDestination.Remove(columnname);
                    }
                }

                List<string> colmns = columsNameSource.Intersect(columsNameDestination).ToList();
                for (int i = 0; i < colmns.Count(); i++)
                {
                    singleColumn = new ColumnMapping(colmns[i], null, colmns[i], null, "false", "false");
                    tableMapping.Add(singleColumn);
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

        //use this ugly code for multiple db type, sorry
        private List<string> GetColumnsName(string dbConn, string tableName, DbType dbType)
        {
            if (dbType == DbType.SQLSERVER)
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
            else if (dbType == DbType.ORACLE)
            {
                using (IDbConnection conn = new OracleConnection(dbConn))
                {
                    List<string> columnsname = new List<string>();
                    conn.Open();
                    string cmdText = string.Format("SELECT column_name FROM all_tab_columns WHERE UPPER(table_name) = '{0}'", tableName);
                    IDbCommand cmd = new OracleCommand(cmdText, (OracleConnection)conn);
                    IDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        columnsname.Add(dataReader[0].ToString());
                    }
                    return columnsname;
                }
            }

            return null;
        }

        private ColumnMapping GetColumnMapping(XmlNode node)
        {
            string source = string.Empty;
            string sourceTableAlias = null;
            string destination = string.Empty;
            string value = null;
            string isKey = "false";
            string isImmutableKey = "false";

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
