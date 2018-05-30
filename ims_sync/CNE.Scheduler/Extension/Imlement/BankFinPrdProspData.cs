using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Extension
{
    public class BankFinPrdProspData : ITableMappingOperation
    {
        public bool Exist(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("I_INNER_CODE", OracleType.Number) { Value = record["INNER_CODE"] };
            paras[1] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("BANK_FIN_PRD_PROSP_DATA_EXIST", 1, paras) == 1;
        }

        public void AddOrUpdate(TableMapping tableMapping, IDataRecord record, int isSynced = 1)
        {
            var paras = new OracleParameter[5];
            paras[0] = new OracleParameter("I_INNER_CODE", OracleType.Number) { Value = record["INNER_CODE"] };
            paras[1] = new OracleParameter("I_CTIME", OracleType.DateTime) { Value = record["CTIME"] };
            paras[2] = new OracleParameter("I_MTIME", OracleType.DateTime) { Value = record["MTIME"] };
            paras[3] = new OracleParameter("I_ACCE_ROUTE", OracleType.VarChar) { Value = record["ACCE_ROUTE"] };
            paras[4] = new OracleParameter("I_ISSYNCED", OracleType.Number) { Value = isSynced };

            DBHelper.ExecuteStorageWithoutRevalue("BANK_FIN_PRD_PROSP_DATA_Update", paras);
        }

        public bool IsFileSynced(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("I_INNER_CODE", OracleType.Number) { Value = record["INNER_CODE"] };
            paras[1] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("BANK_FIN_PRD_PROSP_IsFileSync", 1, paras) == 1;
        }

        public string GetFileId(IDataRecord obj)
        {
            return obj["INNER_CODE"].ToString();
        }
    }
}
