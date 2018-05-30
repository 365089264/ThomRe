using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using System.IO;
using Luna.DataSync.Core.IME_SS;


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
                    var fileSyncDetailMsg = "";
                    var successFileNo = 0;
                    var failFileNo = 0;

                    if (tableMapping.IsSyncFile)
                    {
                        fileSyncDetailMsg = SyncFile(sourceData, tableMapping, ref successFileNo, ref failFileNo);
                        //sync file between storage service
                    }
                    long numOfRows = _bulkDataWritter.Write(sourceData, tableMapping, ref detailMsg);

                    msg.AppendFormat("<b>{0} rows of data succeed.</b><br/>", numOfRows);
                    if (tableMapping.IsSyncFile)
                    {
                        msg.AppendFormat("<span style=\"color:green;\">{0} files succeed.</span><br/>", successFileNo);
                        if (failFileNo > 0)
                        {
                            msg.AppendFormat("<span style=\"color:red;\">{0} files failed.</span>", failFileNo);
                        }
                        msg.Append("</p>");
                        msg.Append(fileSyncDetailMsg);
                    }
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


                        if (tableMapping.IsSyncFile)
                        {
                            fileSyncDetailMsg = SyncFile(sourceData, tableMapping, ref successFileNo, ref failFileNo);
                            //sync file between storage service
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
                    if (tableMapping.IsSyncFile)
                    {
                        msg.AppendFormat("<span style=\"color:green;\">{0} files succeed.</span><br/>", successFileNo);
                        if (failFileNo > 0)
                        {
                            msg.AppendFormat("<span style=\"color:red;\">{0} files failed.</span>", failFileNo);
                        }
                        msg.Append(fileSyncDetailMsg);
                    }
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
                    var from = DateTime.Now;
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


                        if (tableMapping.IsSyncFile)
                        {
                            fileSyncDetailMsg = SyncFile(sourceData, tableMapping, ref successFileNo, ref failFileNo);
                            //sync file between storage service
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
                    if (tableMapping.IsSyncFile)
                    {
                        msg.AppendFormat("<span style=\"color:green;\">{0} files succeed.</span><br/>", successFileNo);
                        if (failFileNo > 0)
                        {
                            msg.AppendFormat("<span style=\"color:red;\">{0} files failed.</span>", failFileNo);
                        }
                        msg.Append(fileSyncDetailMsg);
                    }
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
        /// Sync each table from its own max(mtime) to current sync time
        /// </summary>
        /// <param name="srcTables">Table names</param>
        /// <returns>Sync message</returns>
        public string SyncEachTableFromMaxMtime(IEnumerable<string> srcTables, string maxFiled = "MTIME")
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





                    if (tableMapping.IsSyncFile)
                    {
                        fileSyncDetailMsg = SyncFile(sourceData, tableMapping, ref successFileNo, ref failFileNo);
                        //sync file between storage service
                    }

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
                    if (tableMapping.IsSyncFile)
                    {
                        msg.AppendFormat("<span style=\"color:green;\">{0} files succeed.</span><br/>", successFileNo);
                        if (failFileNo > 0)
                        {
                            msg.AppendFormat("<span style=\"color:red;\">{0} files failed.</span>", failFileNo);
                        }
                        msg.Append(fileSyncDetailMsg);
                    }
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

        /*Sync file from storage service of .28 to storage service of production*/
        private string SyncFile(System.Data.DataTable sourceData, TableMapping tableMapping, ref int successFileNo, ref int failFileNo)
        {
            var imeClient = new Luna.DataSync.Core.IME_SS.StorageServiceClient();
            var prodClient = new Luna.DataSync.Core.PROD_SS.StorageServiceClient();
            var tableName = tableMapping.Source;

            var msg = new StringBuilder();
            msg.AppendFormat("File sync detail for table:{0} : \n<ol>", tableMapping.Destination);

            foreach (System.Data.DataRow dtRow in sourceData.Rows)
            {
                string path = "";
                string fileName = "";
                string fileId = "";

                var keyColumns = tableMapping.GetKeyColumnNames().ToList();
                fileId = keyColumns.Count == 1 ? dtRow[keyColumns.FirstOrDefault()].ToString() : keyColumns.Aggregate((i, j) => dtRow[i] + "_" + dtRow[j]);

                //ignore if there is no physical path
                //when tablename = INSTITUTIONINFO, phisicalpath = |Logo
                var realPath = tableMapping.Source == "INSTITUTIONINFO" ? "|Logo" : dtRow[tableMapping.PathColumn].ToString();
                if (string.IsNullOrEmpty(realPath))
                    continue;

                realPath = realPath.Replace(@"\", "/");

                var ext = "";
                if (tableName.Equals("FILEDETAIL") || tableName.Equals("INSTITUTIONINFO"))
                {
                    ext = "." + dtRow[tableMapping.ExtColumn];
                    path = tableMapping.PathRoot + (realPath.IndexOf("|") != 0 ? String.Format("|{0}", realPath) : realPath);
                }
                else
                {
                    ext = Path.GetExtension(realPath);
                    path = tableMapping.PathRoot + realPath.Substring(0, realPath.LastIndexOf("/")).Replace("/", "|");
                }

                fileName = fileId + ext;

                fileEntity obj = null;

                obj = imeClient.RetriveFileObj(path, fileName);
                if (obj.fileData == null)
                {
                    //var strException = string.Format("Can not retrive file(ID:{0},path{1},name:{2}) \n from {3} from source table: {4} \n ", fileId, path, fileName, imeClient.Endpoint.Address, tableMapping.Source);
                    //throw new Exception(strException);
                    msg.AppendFormat("<li>{0}:{1}{2} <span style=\"color:red;\">failed</span>.<br/>(Cannot retrive file from {3})</li>", fileId, path, fileName,
                        imeClient.Endpoint.Address);
                    failFileNo++;
                    continue;
                }

                bool success;
                //try
                //{
                //    Luna.DataSync.Core.PROD_SS.fileEntity newObj = new Luna.DataSync.Core.PROD_SS.fileEntity { fileData = obj.fileData, fileName = fileName, path = path };
                //    success = prodClient.AddFileObj(newObj);
                //}
                //catch (Exception ex)
                //{
                //    throw new Exception(string.Format("Get exception when call storage service (write file)\n " +
                //                                            "fileId: {0} in destination table: {1} \n  {2} \n", fileId, tableMapping.Source, ex));
                //}

                var newObj = new Luna.DataSync.Core.PROD_SS.fileEntity { fileData = obj.fileData, fileName = fileName, path = path };
                success = prodClient.AddFileObj(newObj);
                if (!success)
                {
                    //throw new Exception(string.Format("Can not save file(ID:{0},path{1},name:{2}) \n to {3} in destination table: {4}", fileId, path, fileName, imeClient.Endpoint.Address, tableMapping.Source));                   
                    msg.AppendFormat("<li>{0}:{1}{2} <span style=\"color:red;\">failed</span>.<br/>(Cannot save file to {3})</li>", fileId, path, fileName,
                        prodClient.Endpoint.Address);
                    failFileNo++;
                    continue;
                }

                msg.AppendFormat("<li>{0}:{1}{2} sync <span style=\"color:green;\">succeed</span>.</li>", fileId, path, fileName);
                successFileNo++;
                //try
                //{
                //    if (!success)
                //    {
                //        if (tableName.Equals("FILEDETAIL"))
                //            dtRow["ISVALID"] = 0;
                //        else
                //            dtRow["ISSYNCED"] = 0;

                //        syncInfo += string.Format("{0}|{1}, fail \n", path, fileName);
                //    }

                //    sourceData.AcceptChanges();

                //}
                //catch (Exception ex)
                //{
                //    throw new Exception(string.Format("Get exception when update file sync status\n " +
                //                    "fileId: {0} \n destination table: {1} \n  {2} \n", fileId, tableMapping.Destination, ex));
                //}
            }
            msg.Append("</ol>");
            return msg.ToString();
        }

        #endregion
    }
}
