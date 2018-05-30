using System.Data;
using System.Data.OracleClient;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Extension
{
    public class RateRepData : ITableMappingOperation
    {
        public bool Exist(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("I_ID", OracleType.Number) { Value = record["ID"] };
            paras[1] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("RATE_REP_DATA_EXIST", 1, paras) == 1;
        }

        public void AddOrUpdate(TableMapping tableMapping, IDataRecord record, int isSynced = 1)
        {
            var paras = new OracleParameter[5];
            paras[0] = new OracleParameter("I_ID", OracleType.Number) { Value = record["ID"] };
            paras[1] = new OracleParameter("I_CCXEID", OracleType.DateTime) { Value = record["CCXEID"] };
            paras[2] = new OracleParameter("I_RATE_ID", OracleType.Number) { Value = record["RATE_ID"] };
            paras[3] = new OracleParameter("I_RATE_FILE_PATH", OracleType.VarChar) { Value = record["RATE_FILE_PATH"] };
            paras[4] = new OracleParameter("I_ISSYNCED", OracleType.Number) { Value = isSynced };

            DBHelper.ExecuteStorageWithoutRevalue("RATE_REP_DATA_Update", paras);
        }

        public string GetFileId(IDataRecord obj)
        {
            return obj["RATE_ID"].ToString();
        }

        public bool IsFileSynced(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("I_ID", OracleType.Number) { Value = record["ID"] };
            paras[1] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("RATE_REP_DATA_IsFileSync", 1, paras) == 1;
        }
    }

}
