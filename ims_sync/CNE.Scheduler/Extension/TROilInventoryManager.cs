using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Exception = System.Exception;
using Aspose.Cells;
using System.Collections.Generic;
using System.Linq;

namespace CNE.Scheduler.Extension
{
    public class TROilInventoryManager : BaseDataHandle
    {
        public void ProcessData(string file, StringBuilder log)
        {
            SyncFile.RegisterLicense();
            ModifyFile(file, log);
            var v = new Workbook(file);

            foreach (Worksheet st in v.Worksheets)
            {
                try
                {
                    BuildUpdateTROilInventorySql(st, "cne.TROilInventory");
                }
                catch (Exception e)
                {
                    log.AppendFormat("Table cne.TROilInventory Sync Data failed, because:{0}", e.Message);
                }
            }
        }

        private void BuildUpdateTROilInventorySql(Worksheet s, string tableName)
        {
            var tb = new DataTable();

            tb.Columns.Add("InfoDate", typeof(DateTime));
            tb.Columns.Add("Mogas", typeof(decimal));
            tb.Columns.Add("Diesel", typeof(decimal));
            tb.Columns.Add("Kero", typeof(decimal));
            tb.Columns.Add("Totalfuel", typeof(decimal));
            tb.Columns.Add("Crude", typeof(decimal));
            tb.Columns.Add("MogasRatio", typeof(decimal));
            tb.Columns.Add("DieselRatio", typeof(decimal));
            tb.Columns.Add("KeroRatio", typeof(decimal));
            tb.Columns.Add("TotalfuelRatio", typeof(decimal));
            tb.Columns.Add("CrudeRatio", typeof(decimal));
            tb.Columns.Add("ChangeDate", typeof(DateTime));
            
            List<string> dateKeys = new List<string>();
            
            for (var i = 3; i <= s.Cells.Count; i++)
            {
                if (s.Cells[i, 0].Value == null)
                {
                    break;
                }
                if (s.Cells[i, 0].Value is DateTime)
                {
                    var rowDate = (DateTime)s.Cells[i, 0].Value;
                    dateKeys.Add((rowDate).ToString("yyyy-MM-dd"));
                }
                else
                {
                    continue;
                }
                var row = tb.NewRow();
                for (var j = 0; j < 11; j++)
                {
                    row[j] = s.Cells[i, j].Value == null ? DBNull.Value : s.Cells[i, j].Value;
                }
                row[11] = DateTime.Now;
                tb.Rows.Add(row);
            }

            DeleteByDate(tableName, "InfoDate", "'" + dateKeys.Aggregate((a, b) => a + ", " + b).Replace(",", "','") + "'");
            WriteToDb(tb, tableName);
        }

        private DateTime DeleteByDate(string tableName, string keyColumn, string dateKeys)
        {
            DateTime retValue;
            using (var conn = new SqlConnection(Connectionstr))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("delete from {0} where {1} in ({2})", tableName, keyColumn, dateKeys);
                conn.Open();
                var o = cmd.ExecuteScalar();
                conn.Close();
                if (o is DateTime)
                {
                    retValue = (DateTime)o;
                }
                else
                {
                    retValue = DateTime.MinValue;
                }
            }
            return retValue;
        }

        private void WriteToDb(DataTable dt, string tableName)
        {
            if (dt.Rows.Count > 0)
            {
                using (var sqlbulkcopy = new SqlBulkCopy(Connectionstr, SqlBulkCopyOptions.UseInternalTransaction) { DestinationTableName = tableName })
                {
                    sqlbulkcopy.WriteToServer(dt);
                }
            }
        }
    }
}
