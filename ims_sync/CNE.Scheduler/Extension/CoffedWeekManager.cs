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
    public class CoffedWeekManager : BaseDataHandle
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
                sb.AppendFormat("[{0} import failed,because:{1}]", fileName, e.Message);
            }
        }
        public void GetCellsByFirstSheet(string fileName, StringBuilder sb)
        {

            SyncFile.RegisterLicense();
            ModifyFile(fileName, sb);
            Workbook v = new Workbook(fileName);
            foreach (Worksheet st in v.Worksheets)
            {
                if (st.Name.IndexOf("week", StringComparison.Ordinal) == -1)
                {
                    continue;
                }
                Cells cells = st.Cells;
                try
                {
                    int total;
                    ImportDataTable(cells, out total);
                    sb.AppendFormat("[Table CofeedWeek import success,Rows:{1},No:{0}]\n" + "\r\n", st.Name, total);
                }
                catch (Exception e)
                {
                    sb.AppendFormat("[Table CofeedWeek import failed,because {0}:{1}]\n" + "\r\n", st.Name, e.Message);
                    throw e;
                }

            }






        }
        private DataTable CreateTableSchema()
        {

            DataTable tb = new DataTable();
            tb.Columns.Add("productName_cn");
            tb.Columns.Add("productName_en");
            tb.Columns.Add("weekNo");
            tb.Columns.Add("endTime");
            tb.Columns.Add("areaName");
            tb.Columns.Add("areaNo");
            tb.Columns.Add("isTotal");
            tb.Columns.Add("memo");
            return tb;
        }
        public DataTable ImportWorkSheet(Cells cells)
        {
            DataTable tb = CreateTableSchema();

            for (int i = 1; i < cells.Rows.Count; i++)
            {

                for (int j = 4; j < cells.Columns.Count - 1; j++)
                {
                    if (cells[i, 0] == null || cells[i, 0].Value == null) break;
                    if (cells[i, 2] == null || cells[i, 2].Value == null || cells[i, 2].Value.ToString() == "") continue;
                    DataRow dataRow = tb.NewRow();
                    dataRow["productName_cn"] = cells[i, 0] == null ? DBNull.Value : cells[i, 0].Value;
                    dataRow["productName_en"] = cells[i, 1] == null ? DBNull.Value : cells[i, 1].Value;
                    dataRow["weekNo"] = cells[i, 2].Value.ToString().Substring(cells[i, 2].Value.ToString().IndexOf("第", StringComparison.Ordinal) + 1, cells[i, 2].Value.ToString().IndexOf("周", StringComparison.Ordinal) - cells[i, 2].Value.ToString().IndexOf("第", StringComparison.Ordinal) - 1);
                    dataRow["endTime"] = cells[i, 3] == null ? DBNull.Value : cells[i, 3].Value;
                    dataRow["areaName"] = cells[0, j] == null ? DBNull.Value : cells[0, j].Value;
                    dataRow["areaNo"] = cells[i, j].Value ?? DBNull.Value;
                    var cell = cells[0, j];
                    dataRow["isTotal"] = cell != null && cell.Value.ToString().IndexOf("全国", StringComparison.Ordinal) != -1 ? 1 : 0;
                    dataRow["memo"] = cells[i, cells.Columns.Count - 1] == null ? DBNull.Value : cells[i, cells.Columns.Count - 1].Value;
                    tb.Rows.Add(dataRow);
                }

            }
            return tb;
        }

        public string CreateDeleteSql(DataRow dr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("delete from CofeedWeek where ");
            if (dr["productName_cn"] != DBNull.Value)
            {
                sb.Append("productName_cn='" + dr["productName_cn"] + "' and ");
            }
            else
            {
                sb.Append("productName_cn is null and ");
            }
            if (dr["productName_en"] != DBNull.Value)
            {
                sb.Append("productName_en='" + dr["productName_en"] + "' and ");
            }
            else
            {
                sb.Append("productName_en is null and ");
            }
            if (dr["weekNo"] != DBNull.Value)
            {
                sb.Append("weekNo=" + dr["weekNo"] + " and ");
            }
            else
            {
                sb.Append(" weekNo is null and ");
            }
            if (dr["endTime"] != DBNull.Value)
            {
                sb.Append("endTime='" + Convert.ToDateTime(dr["endTime"]).ToString("dd-MMM-yyyy") + "' and ");
            }
            else
            {
                sb.Append("endTime is null and ");
            }
            if (dr["areaName"] != DBNull.Value)
            {
                sb.Append("areaName='" + dr["areaName"] + "' and ");
            }
            else
            {
                sb.Append("areaName is null and ");
            }
            if (dr["isTotal"] != DBNull.Value)
            {
                sb.Append(" isTotal=" + dr["isTotal"]);
            }
            else
            {
                sb.Append("isTotal is null ");
            }
            return sb.ToString();

        }
        public string CreateInsertSql(DataRow dr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into CofeedWeek values(");
            if (dr["productName_cn"] != DBNull.Value)
            {
                sb.Append("'" + dr["productName_cn"].ToString().Trim() + "',");
            }
            else
            {
                sb.Append("null,");
            }
            if (dr["productName_en"] != DBNull.Value)
            {
                sb.Append("'" + dr["productName_en"] + "',");
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
            if (dr["areaName"] != DBNull.Value)
            {
                sb.Append("'" + dr["areaName"] + "',");
            }
            else
            {
                sb.Append("null ,");
            }
            if (dr["areaNo"] != DBNull.Value)
            {
                sb.Append(dr["areaNo"] + ",");
            }
            else
            {
                sb.Append("null,");
            }
            if (dr["isTotal"] != DBNull.Value)
            {
                sb.Append(dr["isTotal"] + ",");
            }
            else
            {
                sb.Append("null,");
            }

            if (dr["memo"] != DBNull.Value)
            {
                sb.Append("'" + dr["memo"] + "',");
            }
            else
            {
                sb.Append("null,");
            }
            sb.Append("sysdate)");
            return sb.ToString();
        }
        public void ImportDataTable(Cells c, out int total)
        {
            DataTable datatable = ImportWorkSheet(c);
            total = datatable.Rows.Count;
            string ex = "";
            using (OracleConnection conn = new OracleConnection(Connectionstr))
            {

                conn.Open();
                OracleTransaction tran = conn.BeginTransaction();
                try
                {

                    foreach (DataRow dr in datatable.Rows)
                    {

                        string sql = CreateDeleteSql(dr);
                        ex = sql;
                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        }


                        string sql1 = CreateInsertSql(dr);
                        ex = sql1;
                        using (OracleCommand cmd = new OracleCommand(sql1, conn))
                        {
                            cmd.Transaction = tran;
                            cmd.ExecuteNonQuery();
                        }

                    }
                    tran.Commit();
                    conn.Close();
                }
                catch (Exception e)
                {

                    tran.Rollback();
                    conn.Close();
                    throw new Exception("ExecuteSql:" + ex + "failed," + e.Message, e);

                }
            }
        }
    }
}
