using System;
using System.Data;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Extension
{
    public interface ITableMappingOperation
    {
        bool Exist(TableMapping tableMapping, IDataRecord record);
        void AddOrUpdate(TableMapping tableMapping, IDataRecord record, int isSynced = 1);
        string GetFileId(IDataRecord obj);
        bool IsFileSynced(TableMapping tableMapping, IDataRecord record);
    }

    public class TableMappingFactory
    {
        public static ITableMappingOperation BuildTable(TableMapping tableMapping)
        {
            switch (tableMapping.Destination)
            {
                case "BANK_FIN_PRD_PROSP_DATA":
                    return new BankFinPrdProspData();
                case "FIN_PRD_RPT_DATA":
                    return new FinPrdRrtData();
                case "DISC_ACCE_CFP_DATA":
                    return new DiscAcceCfpData();
                case "RATE_REP_DATA":
                    return new RateRepData();
                case "RES_INFO":
                    return new ResInfo();
                default:
                    throw new Exception("Luna.DataSync.Core.TableMappingFactory.BuildTable() got a unknow table destination" + tableMapping.Destination);
            }
        }
    }
}
