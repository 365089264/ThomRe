using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.IO;
using DbType = Luna.DataSync.Setting.DbType;


namespace Luna.DataSync.Core
{
    public class DataSynchronizer : IDisposable
    {
        #region Private Fields
        private IBulkDataReader _bulkDataReader;
        private IBulkDataWritter _bulkDataWritter;
        private ISettingManager _settingManager;
        private DateTime _lastSyncTime;
        private DateTime _currentSyncTime;
        private List<ITask> _postSyncTasks = new List<ITask>();
        #endregion

        #region Constructors
        public DataSynchronizer(ISettingManager settingManager, DateTime lastSyncTime, DateTime currentSyncTime)
        {
            _settingManager = settingManager;
            _bulkDataReader = null;
            _bulkDataWritter = null;
            _lastSyncTime = lastSyncTime;
            _currentSyncTime = currentSyncTime;
        }
        #endregion

        #region Public Methods
        public void Init()
        {
            CreateDataReader();
            CreateDataWriter();

            CreatePostSyncTasks();
        }

        public string Sync(IEnumerable<string> srcTables)
        {
            StringBuilder msg = new StringBuilder();
            try
            {
                _bulkDataWritter.BeginTrans();


                foreach (string srcTable in srcTables)
                {
                    TableMapping tableMapping = _settingManager.TableMappings.FirstOrDefault(
                        mapping => string.Compare(mapping.Source, srcTable, true) == 0);

                    if (tableMapping == null)
                        throw new Exception(
                            string.Format("Cannot find the mapping info for the source table: {0}", srcTable));

                    var sourceData = _bulkDataReader.ReadAsDataTable(tableMapping);
                    var findRows = string.Format("<p>Find {0} rows from source : {1}.\n", sourceData.Rows.Count,
                        srcTable);
                    msg.Append(findRows);

                    var syncInfo = "";
                    var detailMsg = new StringBuilder();
                    var successFileNo = 0;
                    var failFileNo = 0;

                   
                    long numOfRows = _bulkDataWritter.Write(sourceData, tableMapping, ref detailMsg);

                    msg.AppendFormat("<b>{0} rows of data succeed.</b><br/>", numOfRows);
                   
                    msg.Append(detailMsg.ToString());
                    TableSynched(this,
                        new TableSynchedEventArgs(tableMapping.Source, tableMapping.Destination, numOfRows, syncInfo));
                }

                _bulkDataWritter.CommitTrans();
            }
            catch (Exception ex)
            {
                _bulkDataWritter.RollBackTrans();
                throw ex;
            }

            RunPostSyncTasks();
            return msg.ToString();
        }


        
        public string Sync(IEnumerable<string> srcTables, int batchSize)
        {
            StringBuilder msg = new StringBuilder();

            var successTableCount = 0;
            var failTableCount = 0;

            foreach (string srcTable in srcTables)
            {
                try
                {

                    TableMapping tableMapping = _settingManager.TableMappings.FirstOrDefault(
                        mapping => string.Compare(mapping.Source, srcTable, true) == 0);

                    if (tableMapping == null)
                        throw new Exception(
                            string.Format("Cannot find the mapping info for the source table: {0}", srcTable));

                    var done = false;
                    //todo: config column in xml
                    var from = _bulkDataWritter.GetMaxDateTime(tableMapping.Destination, "MTIME");
                    var tableFrom = from;
                    var insertRows = 0;
                    var updateRows = 0;
                    var fileSyncDetailMsg = "";

                    var syncInfo = "";
                    var detailMsg = new StringBuilder();
                    var successFileNo = 0;
                    var failFileNo = 0;
                    long numOfRows = 0;
                    while (!done)
                    {
                        _bulkDataWritter.BeginTrans();

                        var sourceData = _bulkDataReader.ReadWithRowCount(tableMapping, from, batchSize);
                        if (sourceData.Rows.Count == 0)
                        {
                            _bulkDataWritter.RollBackTrans();
                            break;
                        }
                        if (sourceData.Rows.Count < batchSize)
                        {
                            done = true;
                        }
                        from = (DateTime)sourceData.Rows[sourceData.Rows.Count - 1]["MTIME"];

                        int currentInsertRowCount = 0, currentUpdateRowCount = 0;
                        var currentSyncRowCount = _bulkDataWritter.Write(sourceData, tableMapping, ref detailMsg,
                            ref currentInsertRowCount, ref currentUpdateRowCount);
                        numOfRows += currentSyncRowCount;
                        insertRows += currentInsertRowCount;
                        updateRows += currentUpdateRowCount;

                        _bulkDataWritter.CommitTrans();

                    }


                    msg.AppendFormat(
                        "<p><b>{0}</b> from {1} to {2}. <span style=\"color:green;\">Insert:{3} rows. Update:{4} rows.</span></p>",
                        srcTable, tableFrom, _currentSyncTime, insertRows, updateRows);
                    TableSynched(this,
                        new TableSynchedEventArgs(tableMapping.Source, tableMapping.Destination, numOfRows, syncInfo));
                    successTableCount++;

                }
                catch (Exception ex)
                {
                    _bulkDataWritter.RollBackTrans();
                    msg.AppendFormat("<p><span style=\"color:red;\">{0} failed.</span><br/>Exception detail:<br/>", srcTable);
                    msg.Append(ex.Message + "</p>");
                    failTableCount++;
                }
            }

            msg.AppendFormat("<h2>{0} tables succeed. {1} tables failed.</h2>", successTableCount, failTableCount);
            RunPostSyncTasks();
            return msg.ToString();
        }
        public string SyncNoMax(IEnumerable<string> srcTables, int batchSize)
        {
            StringBuilder msg = new StringBuilder();

            var successTableCount = 0;
            var failTableCount = 0;

            foreach (string srcTable in srcTables)
            {
                try
                {

                    TableMapping tableMapping = _settingManager.TableMappings.FirstOrDefault(
                        mapping => string.Compare(mapping.Source, srcTable, true) == 0);

                    if (tableMapping == null)
                        throw new Exception(
                            string.Format("Cannot find the mapping info for the source table: {0}", srcTable));

                    var done = false;
                    //todo: config column in xml
                    var from = _lastSyncTime;
                    var tableFrom = from;
                    var insertRows = 0;
                    var updateRows = 0;
                    var fileSyncDetailMsg = "";

                    var syncInfo = "";
                    var detailMsg = new StringBuilder();
                    var successFileNo = 0;
                    var failFileNo = 0;
                    long numOfRows = 0;
                    while (!done)
                    {
                        _bulkDataWritter.BeginTrans();

                        var sourceData = _bulkDataReader.ReadWithRowCount(tableMapping, from, batchSize);
                        if (sourceData.Rows.Count == 0)
                        {
                            _bulkDataWritter.RollBackTrans();
                            break;
                        }
                        if (sourceData.Rows.Count < batchSize)
                        {
                            done = true;
                        }
                        int currentInsertRowCount = 0, currentUpdateRowCount = 0;
                        var currentSyncRowCount = _bulkDataWritter.Write(sourceData, tableMapping, ref detailMsg,
                            ref currentInsertRowCount, ref currentUpdateRowCount);
                        numOfRows += currentSyncRowCount;
                        insertRows += currentInsertRowCount;
                        updateRows += currentUpdateRowCount;

                        _bulkDataWritter.CommitTrans();

                    }


                    msg.AppendFormat(
                        "<p><b>{0}</b> from {1} to {2}. <span style=\"color:green;\">Insert:{3} rows. Update:{4} rows.</span></p>",
                        srcTable, tableFrom, _currentSyncTime, insertRows, updateRows);
                    TableSynched(this,
                        new TableSynchedEventArgs(tableMapping.Source, tableMapping.Destination, numOfRows, syncInfo));
                    successTableCount++;

                }
                catch (Exception ex)
                {
                    _bulkDataWritter.RollBackTrans();
                    msg.AppendFormat("<p><span style=\"color:red;\">{0} failed.</span><br/>Exception detail:<br/>", srcTable);
                    msg.Append(ex.Message + "</p>");
                    failTableCount++;
                }
            }

            msg.AppendFormat("<h2>{0} tables succeed. {1} tables failed.</h2>", successTableCount, failTableCount);
            RunPostSyncTasks();
            return msg.ToString();
        }


        /// <summary>
        /// Sync each table from its own max(mtime) to current sync time, 
        /// and return failed table count to indicate how many tables failed.
        /// </summary>
        /// <param name="srcTables">Tables to be synced</param>
        /// <param name="failedTableCount">Count of failed table</param>
        /// <param name="maxFiled"></param>
        /// <returns></returns>
        public string SyncEachTableFromMaxMtime(IEnumerable<string> srcTables, ref int failedTableCount,
            string maxFiled = "MTIME")
        {
            StringBuilder msg = new StringBuilder();

            var successTableCount = 0;

            foreach (string srcTable in srcTables)
            {
                try
                {

                    TableMapping tableMapping = _settingManager.TableMappings.FirstOrDefault(
                        mapping => string.Compare(mapping.Source, srcTable, true) == 0);

                    if (tableMapping == null)
                        throw new Exception(
                            string.Format("Cannot find the mapping info for the source table: {0}", srcTable));


                    //todo: config column in xml
                    var from = _bulkDataWritter.GetMaxDateTime(tableMapping.Destination, maxFiled);
                    var insertRows = 0;
                    var updateRows = 0;
                    var fileSyncDetailMsg = "";

                    var syncInfo = "";
                    var detailMsg = new StringBuilder();
                    var successFileNo = 0;
                    var failFileNo = 0;
                    long numOfRows = 0;

                    _bulkDataWritter.BeginTrans();

                    var sourceData = _bulkDataReader.ReadAsDataTable(tableMapping, from);







                    int currentInsertRowCount = 0, currentUpdateRowCount = 0;
                    var currentSyncRowCount = _bulkDataWritter.Write(sourceData, tableMapping, ref detailMsg,
                        ref currentInsertRowCount, ref currentUpdateRowCount);
                    numOfRows += currentSyncRowCount;
                    insertRows += currentInsertRowCount;
                    updateRows += currentUpdateRowCount;

                    _bulkDataWritter.CommitTrans();




                    msg.AppendFormat(
                        "<p><b>{0}</b> from {1:M/d/yyyy h:mm:ss.ffffff tt} to {2:M/d/yyyy h:mm:ss.ffffff tt}. <span style=\"color:green;\">Insert:{3} rows. Update:{4} rows.</span></p>",
                        srcTable, @from, _currentSyncTime, insertRows, updateRows);

                    TableSynched(this,
                        new TableSynchedEventArgs(tableMapping.Source, tableMapping.Destination, numOfRows, syncInfo));
                    successTableCount++;

                }
                catch (Exception ex)
                {
                    _bulkDataWritter.RollBackTrans();
                    msg.AppendFormat("<p><span style=\"color:red;\">{0} failed.</span><br/>Exception detail:<br/>", srcTable);
                    msg.Append(ex.Message + "</p>");
                    failedTableCount++;
                }
            }

            msg.AppendFormat("<h2>{0} tables succeed. {1} tables failed.</h2>", successTableCount, failedTableCount);
            RunPostSyncTasks();
            return msg.ToString();
        }

        /// <summary>
        /// Sync each table from its own max(mtime) to current sync time
        /// </summary>
        /// <param name="srcTables">Table names</param>
        /// <returns>Sync message</returns>
        public string SyncEachTableFromMaxMtime(IEnumerable<string> srcTables, string maxFiled = "MTIME")
        {
            var failedTableCount = 0;
            return SyncEachTableFromMaxMtime(srcTables, ref failedTableCount, maxFiled);
        }


        public string FileSync(string fptUrl, Func<IDataRecord, TableMapping, string, string> readCallback)
        {
            var ret = new StringBuilder();
            var currentTable = "Empty Table";
            try
            {
                _bulkDataWritter.BeginTrans();

                foreach (var tableMapping in _settingManager.TableMappings)
                {
                    currentTable = tableMapping.Source;

                    var fileMsg = "";
                    var numOfRows = _bulkDataReader.ReadDataAndCallback(tableMapping, readCallback, fptUrl, out fileMsg);
                    ret.Append(fileMsg);

                    TableSynched(this, new TableSynchedEventArgs(tableMapping.Source, tableMapping.Destination, numOfRows, fileMsg));
                }

                _bulkDataWritter.CommitTrans();
            }
            catch (Exception ex)
            {
                var errorMsg = string.Format("Failed to sync {0}. Error: {1}", currentTable, ex);
                try
                {
                    _bulkDataWritter.RollBackTrans();
                }
                catch
                {

                }
                throw new Exception(errorMsg);
            }
            return ret.ToString();
        }

        public void Close()
        {
            if (_bulkDataReader != null)
                _bulkDataReader.Close();

            if (_bulkDataWritter != null)
                _bulkDataWritter.Close();
        }

        public void Dispose()
        {
            Close();
        }
        #endregion

        #region Events
        public event EventHandler<TableSynchedEventArgs> TableSynched = delegate { };
        public event EventHandler<PostSyncTaskExecutedEventArgs> PostTaskExecuted = delegate { };
        #endregion

        #region Private Methods
        private void RunPostSyncTasks()
        {
            try
            {
                foreach (ITask task in _postSyncTasks)
                {
                    task.Run();
                    PostTaskExecuted(this, new PostSyncTaskExecutedEventArgs(task.GetType().FullName));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Failed to run post tasks. Error: {0}", ex.ToString()));
                throw;
            }
        }

        private void CreateDataReader()
        {
            switch (_settingManager.SourceDb.Type)
            {
                case DbType.SQLSERVER:
                    _bulkDataReader = new SqlServerBulkDataReader(
                        _settingManager.SourceDb.Conn,
                        _settingManager.SqlCommandTimeout,
                        _lastSyncTime,
                        _currentSyncTime,
                        _settingManager.CustomBonds);
                    break;
                case DbType.SYBASE:
                    _bulkDataReader = new SybaseBulkDataReader(
                       _settingManager.SourceDb.Conn,
                       _settingManager.SqlCommandTimeout,
                       _lastSyncTime,
                       _currentSyncTime,
                       _settingManager.CustomBonds);
                    break;
                case DbType.MYSQL:
                    _bulkDataReader = new MySqlBulkDataReader(
                       _settingManager.SourceDb.Conn,
                       _settingManager.SqlCommandTimeout,
                       _lastSyncTime,
                       _currentSyncTime,
                       _settingManager.CustomBonds);
                    break;
                case DbType.ORACLE:
                    _bulkDataReader = new OracleBulkDataReader(
                       _settingManager.SourceDb.Conn,
                       _settingManager.SqlCommandTimeout,
                       _lastSyncTime,
                       _currentSyncTime,
                       _settingManager.CustomBonds);
                    break;
                default:
                    throw new Exception("The type of data reader is not supported: " + _settingManager.SourceDb.Type);
            }

            _bulkDataReader.Init();
        }

        private void CreateDataWriter()
        {
            switch (_settingManager.DestinationDb.Type)
            {
                case DbType.SQLSERVER:
                    _bulkDataWritter = new SqlServerBulkDataWritter(_settingManager.DestinationDb.Conn, _settingManager.SqlCommandTimeout);
                    break;
                case DbType.MYSQL:
                    _bulkDataWritter = new MySqlBulkDataWriter(_settingManager.DestinationDb.Conn, _settingManager.SqlCommandTimeout);
                    break;
                case DbType.ORACLE:
                    _bulkDataWritter = new OracleBulkDataWritter(_settingManager.DestinationDb.Conn, _settingManager.SqlCommandTimeout);
                    break;
                default:
                    throw new Exception("The type of data writer is not supported: " + _settingManager.DestinationDb.Type);
            }

            _bulkDataWritter.Init();
        }

        private void CreatePostSyncTasks()
        {
            foreach (Task taskSetting in _settingManager.PostSyncTasks)
            {
                ITask task = Assembly.GetExecutingAssembly().CreateInstance(taskSetting.Name) as ITask;
                task.Init(_settingManager);

                _postSyncTasks.Add(task);
            }
        }

     
        #endregion
    }
}
