using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Data.SqlClient;

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
            tableMapping.IsContainFile = (node.Attributes["isContainFile"] != null) && bool.Parse(node.Attributes["isContainFile"].Value);
            tableMapping.IsContainFilePath = (node.Attributes["isContainFilePath"] != null) && bool.Parse(node.Attributes["isContainFilePath"].Value);
            tableMapping.IsCheckFileSynced = (node.Attributes["isCheckFileSynced"] != null) && bool.Parse(node.Attributes["isCheckFileSynced"].Value);
            tableMapping.FilePath = (node.Attributes["filePath"] == null) ? "" : node.Attributes["filePath"].Value;
            tableMapping.PathColumn = (node.Attributes["pathColumn"] == null) ? "" : node.Attributes["pathColumn"].Value;
            tableMapping.DestinationFilePath = (node.Attributes["destinationFilePath"] == null) ? "" : node.Attributes["destinationFilePath"].Value;
            
            var columnMappingNodes = node.SelectNodes("column-mapping");

            //20141119 yy move it here from if estimate
            string tableNameSource = node.Attributes["source"].Value;
            this._sourceTableName=string.IsNullOrEmpty(this._sourceTableName)? tableNameSource:this._sourceTableName+","+tableNameSource;

            if (node.Attributes["isSqlServer"] != null && node.Attributes["isSync"] != null && node.Attributes["isSqlServer"].Value == "true" && node.Attributes["isSync"].Value == "true")
            {
                string columnname;
                //move up yy 
                string tableNameDes = node.Attributes["destination"].Value;
                string dbConn = node.ParentNode.ParentNode.SelectSingleNode("source-db/conn").FirstChild.Value;
                List<string> columsNameSource = GetColumnsName(dbConn, tableNameSource);
                List<string> columsNameDestination = GetColumnsName(DestinationDb.Conn, tableNameDes);
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
            {
                if (valueAttribute.InnerText.ToLower() == "sysdate")
                {
                    value = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
                }
                else if (valueAttribute.InnerText.ToLower() == "getdate()")
                {
                    value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");
                }
                else
                {
                    value = valueAttribute.InnerText;
                }
            }

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
