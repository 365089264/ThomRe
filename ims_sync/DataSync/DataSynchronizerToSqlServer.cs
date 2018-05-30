using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Luna.DataSync.Setting;
using System.Diagnostics;
using DbType = Luna.DataSync.Setting.DbType;

namespace Luna.DataSync.Core
{
    public class DataSynchronizerToSqlServer : IDisposable
    {
        #region Private Fields
        private IBulkDataReader _bulkDataReader;
        private readonly ISettingManager _settingManager;
        private readonly DateTime _lastSyncTime;
        private readonly DateTime _currentSyncTime;
        #endregion

        #region Constructors
        public DataSynchronizerToSqlServer(ISettingManager settingManager, DateTime lastSyncTime, DateTime currentSyncTime)
        {
            _settingManager = settingManager;
            _bulkDataReader = null;
            _lastSyncTime = lastSyncTime;
            _currentSyncTime = currentSyncTime;
        }
        #endregion

        #region Public Methods
        public void Init()
        {
            CreateDataReader();
        }

        public void Sync(IEnumerable<string> srcTables)
        {
            try
            {
                foreach (string srcTable in srcTables)
                {
                    TableMapping tableMapping = _settingManager.TableMappings.FirstOrDefault(
                        mapping => String.Compare(mapping.Source, srcTable, StringComparison.OrdinalIgnoreCase) == 0);

                    if (tableMapping == null)
                        throw new Exception(
                            string.Format("Cannot find the mapping info for the source table: {0}", srcTable));

                    long numOfRows = ExecuteWrite(_bulkDataReader.ReadAsDataTable(tableMapping), tableMapping);

                    TableSynched(this, new TableSynchedEventArgs(tableMapping.Source, tableMapping.Destination, numOfRows, ""));
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to sync tables. Error: {0}", ex);
                throw;
            }

        }

        private long ExecuteWrite(DataTable dt, TableMapping tableMapping)
        {
            var strTarget = _settingManager.DestinationDb.Conn;
            var conTarget = new SqlConnection(strTarget);
            try
            {
                conTarget.Open();
                var cmdTarget = new SqlCommand
                {
                    Connection = conTarget,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = tableMapping.Destination
                };
                cmdTarget.Parameters.AddWithValue(@"dt", dt);
                cmdTarget.ExecuteNonQuery();
            }
            catch (Exception ee)
            {
                throw new Exception(ee.Message);
            }
            finally
            {
                conTarget.Close();
            }
            return dt.Rows.Count;
        }


        public void Close()
        {
            if (_bulkDataReader != null)
                _bulkDataReader.Close();

        }

        public void Dispose()
        {
            Close();
        }
        #endregion

        #region Events
        public event EventHandler<TableSynchedEventArgs> TableSynched = delegate { };
        #endregion

        #region Private Methods
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
                default:
                    throw new Exception("The type of data reader is not supported: " + _settingManager.SourceDb.Type);
            }

            _bulkDataReader.Init();
        }

        #endregion
    }
}
