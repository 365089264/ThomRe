using System;
using System.Collections.Generic;
using System.Linq;
using VAV.Entities;

namespace VAV.DAL.Report
{
    public class BaseReportRepository : BaseRepository
    {
        private static IEnumerable<LOCALIZATION> _localizations;
        private static IEnumerable<REPORTCOLUMNDEFINITION> _columnDefination;
        private static IEnumerable<COLUMNDEFINITION> _gdtcolumnDefination;
        private static IEnumerable<TYPEORDER> _typeOrder;

        public BaseReportRepository()
        {
            using (var bonddb = new BondDBEntities())
            {
                if (_typeOrder == null)
                {
                    _typeOrder = bonddb.TYPEORDER.ToArray();
                }
            }
            using (var CMADB = new CMAEntities())
            {
                if (_localizations == null)
                {
                    _localizations = CMADB.LOCALIZATIONs.ToArray();
                }
                if (_columnDefination == null)
                {
                    _columnDefination = CMADB.REPORTCOLUMNDEFINITIONs.ToArray();
                }
              
            }
            using (var CMADB = new CMAEntities())
            {
                if (_gdtcolumnDefination == null)
                {
                    _gdtcolumnDefination = CMADB.COLUMNDEFINITIONs.ToArray();
                }
            }
        }

        public IEnumerable<LOCALIZATION> GetLocalization()
        {
            return _localizations;
        }

        public IEnumerable<LOCALIZATION> GetLocalization(string tableName,string tableCode)
        {
            return _localizations.Where(x => x.TABLE_NAME == tableName && x.TABLE_CD == tableCode).ToArray();
        }

        public string GetChineseName(string tableName,string tableCode)
        {
            var cnName = GetLocalization(tableName, tableCode).FirstOrDefault();
            return cnName == null ? string.Empty : cnName.CHINESE_NAME;
        }

        public IEnumerable<REPORTCOLUMNDEFINITION> GetColumnDefinitionByReportId(int reportID)
        {
            return _columnDefination.Where(x => x.REPORT_ID == reportID).OrderBy(x=>x.COLUMN_INDEX);
        }

        public IEnumerable<REPORTCOLUMNDEFINITION> GetColumnDefinition(int reportID, string[] columns)
        {
            var columeDefinationList = new List<REPORTCOLUMNDEFINITION>();
            
            foreach(var c in columns)
            {
                var current = _columnDefination.Where(x => x.REPORT_ID == reportID && x.COLUMN_NAME == c).FirstOrDefault();
                if (current != null)
                    columeDefinationList.Add(current);
            }
            return columeDefinationList;  
        }

        public IEnumerable<COLUMNDEFINITION> GetGDTColumnDefinition(int itemId)
        {
            return _gdtcolumnDefination.Where(x => x.ITEMID == itemId).OrderBy(x => x.COLUMN_ORDER);
        }

        public int GetOrderByKey(string key)
        {
            return Convert.ToInt32(_typeOrder.Where(o => o.ENGLISH_NAME == key).Select(o => o.TABLE_CD).FirstOrDefault());
        }
    }
}
