using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Extension
{
    public class DiscAcceCfpData : ITableMappingOperation
    {
        public bool Exist(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[3];
            paras[0] = new OracleParameter("I_DISC_ID", OracleType.Number) { Value = record["DISC_ID"] };
            paras[1] = new OracleParameter("I_ACCE_ORDER", OracleType.Number) { Value = record["ACCE_ORDER"] };
            paras[2] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("DISC_ACCE_CFP_DATA_EXIST", 2, paras) == 1;
        }

        public void AddOrUpdate(TableMapping tableMapping, IDataRecord record, int isSynced = 1)
        {
            var paras = new OracleParameter[6];
            paras[0] = new OracleParameter("I_DISC_ID", OracleType.VarChar) { Value = record["DISC_ID"] };
            paras[1] = new OracleParameter("I_ACCE_ORDER", OracleType.Number) { Value = record["ACCE_ORDER"] };
            paras[2] = new OracleParameter("I_CTIME", OracleType.DateTime) { Value = record["CTIME"] };
            paras[3] = new OracleParameter("I_MTIME", OracleType.DateTime) { Value = record["MTIME"] };
            paras[4] = new OracleParameter("I_ACCE_ROUTE", OracleType.VarChar) { Value = record["ACCE_ROUTE"] };
            paras[5] = new OracleParameter("I_ISSYNCED", OracleType.Number) { Value = isSynced };

            DBHelper.ExecuteStorageWithoutRevalue("DISC_ACCE_CFP_DATA_Update", paras);
        }

        public string GetFileId(IDataRecord obj)
        {
            return string.Format("{0}_{1}", obj["DISC_ID"], obj["ACCE_ORDER"]);
        }

        public bool IsFileSynced(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[3];
            paras[0] = new OracleParameter("I_DISC_ID", OracleType.Number) { Value = record["DISC_ID"] };
            paras[1] = new OracleParameter("I_ACCE_ORDER", OracleType.Number) { Value = record["ACCE_ORDER"] };
            paras[2] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("DISC_ACCE_CFP_DATA_IsFileSync", 2, paras) == 1;
        }
    }

}
