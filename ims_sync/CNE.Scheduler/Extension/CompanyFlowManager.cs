using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class CompanyFlowManager : BaseDataHandle
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
                reader.Close();
                sb.AppendFormat("[{0} import failed,because:{1}]", fileName, e.Message);
            }
        }
        public void GetCellsByFirstSheet(string fileName, StringBuilder sb)
        {
            SyncFile.RegisterLicense();
            ModifyFile(fileName, sb);
            Workbook v = new Workbook(fileName);
            using (OracleConnection conn = new OracleConnection(Connectionstr))
            {
                conn.Open();
                foreach (Worksheet st in v.Worksheets)
                {
                    Cells cells = st.Cells;
                    try
                    {

                        ImportDataTable(cells, st.Name, conn);
                        sb.AppendFormat("[{0} import success,totla:{1}]", st.Name, cells.Rows.Count - 1);

                    }
                    catch (Exception e)
                    {
                      throw new Exception(sb.AppendFormat("[{0} read failed,because:{1}]", st.Name, e.Message).ToString()); 
                    }

                }
                conn.Close();

            }




        }
      
        public List<string> ImportWorkSheet(Cells cells, string sheetName, OracleConnection conn)
        {
            List<string> executeInsertSql = new List<string>();
            DateTime tdate = GetDataTableBySheetName(sheetName, conn);
            //int rowNum = sheetName == "焊接钢管" ? 3 : 4;
            int rowNum = -1;
            for (int i = 0; i < 10; i++)
            {
                DateTime flaTime;
                if (cells[i, 0].Value != null && DateTime.TryParse(cells[i, 0].Value.ToString(), out flaTime))
                {
                    rowNum = i;
                    break;

                }
            }
            if (rowNum == -1) return executeInsertSql;
            for (int i = rowNum; i < cells.Rows.Count; i++)
            {
                if (cells[i, 0].Value == null) continue;
                DateTime sheetTime = Convert.ToDateTime(cells[i, 0].Value);
                if (sheetTime <= tdate) continue;
                for (int j = 1; j <= 8; j++)
                {
                    string sql = "insert into COMPANYFLOW values(";
                    sql += cells[i, 0].Value == null ? "null" : ("'" + Convert.ToDateTime(cells[i, 0].Value).ToString("dd-MMM-yyyy") + "'");
                    sql += cells[rowNum - 1, j].Value == null ? ",null" : (",'" + cells[rowNum - 1, j].Value + "'");
                    sql += ",'" + sheetName + "'";
                    double tempNum;
                    if (cells[i, j].Value == null || cells[i - 1, j].Value == null || (!double.TryParse(cells[i, j].Value.ToString(), out tempNum) && i != rowNum - 1) || (!double.TryParse(cells[i - 1, j].Value.ToString(), out tempNum) && i != rowNum))
                    {
                        sql += ",null";
                    }
                    else
                    {
                        sql += Convert.ToDateTime(cells[i, 0].Value).Month == 1 ? "," + cells[i, j].Value : "," + (Convert.ToDouble(cells[i, j].Value) - Convert.ToDouble(cells[i - 1, j].Value));
                    }
                    sql += ",'吨'";
                    sql += "," + (cells[rowNum - 1, j].Value.ToString().IndexOf("合计", StringComparison.Ordinal) != -1 ? 1 : 0);
                    sql += ",sysdate)";
                    executeInsertSql.Add(sql);
                }
            }
            return executeInsertSql;
        }

        private DateTime GetDataTableBySheetName(string sheetName, OracleConnection conn)
        {
            StringBuilder sb = new StringBuilder("SELECT MAX(re_date)  FROM CompanyFlow  WHERE sheetName='");
            sb.Append(sheetName + "'");
            using (OracleCommand cmd = new OracleCommand(sb.ToString(), conn))
            {
                object o = cmd.ExecuteScalar();
                if (o == DBNull.Value)
                    return DateTime.Parse("1970-1-1");
                return Convert.ToDateTime(o);
            }
        }
        public void ImportDataTable(Cells c, string sheetName, OracleConnection conn)
        {
            List<string> datatable = ImportWorkSheet(c, sheetName, conn);
            if (datatable.Count > 0)
            {
                using (var con = new OracleConnection(Connectionstr))
                {
                    con.Open();
                    foreach (var insertRow in datatable)
                    {
                        try
                        {
                            var cmd = new OracleCommand(insertRow, con);
                            cmd.ExecuteNonQuery();

                        }
                        catch (OracleException e)
                        {
                            con.Close();
                            throw new Exception(e.Message);
                        }
                    }
                    con.Close();
                }
            }
        }
    }
}
