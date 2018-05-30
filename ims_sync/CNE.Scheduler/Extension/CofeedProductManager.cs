using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Aspose.Cells;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Extension
{
    public class CofeedProductManager : BaseDataHandle
    {
        public new void ModifyFile(string fileName, StringBuilder sb)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(fileName);

                string fileContent = reader.ReadToEnd();
                reader.Close();
                Regex reg = new Regex("<meta.+/> ", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                if (reg.IsMatch(fileContent))
                {
                    string str = reg.Replace(fileContent, "");
                    //   File.Delete(fileName );
                    File.WriteAllText(fileName, str, Encoding.UTF8);
                }


            }
            catch (Exception e)
            {
                if (reader != null) reader.Close();
                sb.AppendFormat("[File {0} import failed,because:{1}]", fileName, e.Message);
            }
        }
        public void GetCellsByFirstSheet(string fileName, StringBuilder sb)
        {
            SyncFile.RegisterLicense();
            ModifyFile(fileName, sb);
            Workbook v = new Workbook(fileName);
            //v.Open(fileName ,FileFormatType .Excel97To2003);
            // v.FileName = fileName;
            // Workbook sheet = new Workbook(fileName);
            foreach (Worksheet st in v.Worksheets)
            {
                Cells cells = st.Cells;
                try
                {
                    int total;
                    ImportDataTable(cells, out total);
                    sb.AppendFormat("[Table CofeedProduct import success,Rows:{1},No:{0}]\n" + "\r\n", st.Name, total);
                }
                catch (Exception e)
                {
                    sb.AppendFormat("[Table CofeedProduct import failed,because {0}:{1}]\n" + "\r\n", st.Name, e.Message);
                }

            }




        }
        private DataTable CreateTableSchema()
        {
            DataTable tb = new DataTable();
            tb.Columns.Add("startTime");
            tb.Columns.Add("endTime");
            tb.Columns.Add("weekNo");
            tb.Columns.Add("typeName");
            tb.Columns.Add("typeCount");
            return tb;
        }
        public DataTable ImportWorkSheet(Cells cells)
        {
            DataTable tb = CreateTableSchema();
            DateTime maxTime = GetMaxStartTime();
            for (int i = 1; i < cells.Rows.Count; i++)
            {

                if (cells[i, 0].Value == null)
                {
                    continue;
                }
                else if (Convert.ToDateTime(cells[i, 0].Value) <= maxTime)
                {
                    continue;
                }
                else if (cells[i, 1].Value == null || cells[i, 2].Value==null )
                {
                    continue;
                }

                for (int j = 3; j < cells.Columns.Count; j++)
                {
                    if (cells[0, j].Value == null)
                    {
                        break;
                    }
                    else if (cells[i, j] == null)
                    {
                        break;
                    }
                    DataRow dataRow = tb.NewRow();
                    dataRow["startTime"] = cells[i, 0].Value == null ? DBNull.Value : cells[i, 0].Value;
                    dataRow["endTime"] = cells[i, 1].Value == null ? DBNull.Value : cells[i, 1].Value;
                    dataRow["weekNo"] = cells[i, 2].Value == null ? "" : cells[i, 2].Value.ToString().Substring(cells[i, 2].Value.ToString().IndexOf("第") + 1, cells[i, 2].Value.ToString().IndexOf("周") - cells[i, 2].Value.ToString().IndexOf("第") - 1);
                    dataRow["typeName"] = cells[0, j].Value == null ? DBNull.Value : cells[0, j].Value;
                    dataRow["typeCount"] = cells[i, j] == null ? DBNull.Value : cells[i, j].Value;
                    tb.Rows.Add(dataRow);
                }


            }
            return tb;
        }
        public DateTime GetMaxStartTime()
        {
            object o = null;
            using (OracleConnection conn = new OracleConnection(Connectionstr))
            {
                try
                {
                    conn.Open();
                    string sql = "select max(startTime ) as MaxTime from CofeedProduct";
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        o = cmd.ExecuteScalar();

                    }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    conn.Close();
                }
            }
            return Convert.ToDateTime(o);
        }

        public string CreateInsertSql(DataRow dr)
        {
            StringBuilder sb = new StringBuilder("insert into CofeedProduct values(");
            if (dr["startTime"] != DBNull.Value)
            {
                sb.Append("'" + Convert.ToDateTime(dr["startTime"]).ToString("dd-MMM-yyyy") + "',");
            }
            else
            {
                sb.Append("null,");
            }
            if (dr["endTime"] != DBNull.Value)
            {
                sb.Append("'" + Convert.ToDateTime(dr["endTime"]).ToString("dd-MMM-yyyy") + "',");
            }
            else
            {
                sb.Append("null ,");
            }
            if (dr["weekNo"] != DBNull.Value)
            {
                sb.Append(dr["weekNo"] + ",");
            }
            else
            {
                sb.Append("null ,");
            }
            if (dr["typeName"] != DBNull.Value)
            {
                sb.Append("'" + dr["typeName"] + "',");
            }
            else
            {
                sb.Append("null ,");
            }

            if (dr["typeCount"] != DBNull.Value)
            {
                sb.Append(dr["typeCount"] + ",");
            }
            else
            {
                sb.Append("null,");
            }
            sb.Append("sysdate)");
            return sb.ToString();
        }
        public void ImportDataTable(Cells c,out int total)
        {
            DataTable datatable = ImportWorkSheet(c);
            total = datatable.Rows.Count;
            using (OracleConnection conn = new OracleConnection(Connectionstr))
            {
                conn.Open();
                OracleTransaction tran = conn.BeginTransaction();
                try
                {
                    foreach (DataRow dr in datatable.Rows)
                    {

                        string sql1 = CreateInsertSql(dr);
                        using (OracleCommand cmd = new OracleCommand(sql1, conn))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tran.Commit();
                }
                catch (Exception)
                {

                    tran.Rollback();
                    conn.Close();
                    throw;
                }


            }
        }
    }
}

