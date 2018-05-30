using System;
using System.Text;
using Aspose.Cells;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace CNE.Scheduler.Extension
{
    public class CofeedManager : BaseDataHandle
    {

        public void ModifyFile(string fileName, StringBuilder sb)
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
                    File.WriteAllText(fileName, str, System.Text.Encoding.UTF8);
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
            //v.Open(fileName ,FileFormatType .Excel97To2003);
            // v.FileName = fileName;
            // Workbook sheet = new Workbook(fileName);
            foreach (Worksheet st in v.Worksheets)
            {
                Cells cells = st.Cells;
                try
                {
                    ImportDataTable(cells);
                    sb.AppendFormat("[{0} import success,Rows:{1}]\n" + "\r\n", DateTime.ParseExact(st.Name, "yyyyMMdd", null).ToString("yyyy-MM-dd"), cells.Rows.Count - 1);
                }
                catch (Exception e)
                {
                    sb.AppendFormat("[{0} import failed,because:{1}]\n" + "\r\n", DateTime.ParseExact(st.Name, "yyyyMMdd", null).ToString("yyyy-MM-dd"), e.Message);
                    throw;
                }

            }
        }
        private DataTable CreateTableSchema()
        {
            DataTable tb = new DataTable();

            tb.Columns.Add("datec");
            tb.Columns.Add("typec");
            tb.Columns.Add("EnglishType");
            tb.Columns.Add("area");
            tb.Columns.Add("city");
            tb.Columns.Add("price");
            tb.Columns.Add("qastand");
            tb.Columns.Add("unit");
            tb.Columns.Add("code");
            return tb;
        }
        public DataTable ImportWorkSheet(Cells cells)
        {
            DataTable tb = CreateTableSchema();

            for (int i = 1; i < cells.Rows.Count; i++)
            {
                DataRow dataRow = tb.NewRow();
                dataRow["datec"] = Convert.ToDateTime(cells[i, 0].Value).ToString("MM/dd/yyyy");
                dataRow["typec"] = cells[i, 1].Value;
                dataRow["EnglishType"] = cells[i, 2].Value;
                dataRow["area"] = cells[i, 3].Value;
                dataRow["city"] = cells[i, 4].Value;
                dataRow["price"] = cells[i, 5].Value;
                dataRow["qastand"] = cells[i, 6].Value;
                dataRow["unit"] = cells[i, 7].Value;
                dataRow["code"] = cells[i, 8].Value;
                tb.Rows.Add(dataRow);
            }
            return tb;
        }
        public void ImportDataTable(Cells c)
        {
            DataTable tableData = ImportWorkSheet(c);
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmdText = "INSERT INTO Cofeed VALUES( ";
                for (var i = 0; i < tableData.Columns.Count; i++)
                {
                    cmdText += ":v" + i + ",";
                }
                cmdText += "sysdate,null)";
                var cmd = new OracleCommand(cmdText, con);

                for (var j = 0; j < tableData.Rows.Count; j++)
                {
                    cmd.Parameters.Clear();
                    for (var i = 0; i < tableData.Columns.Count; i++)
                    {
                        var par = new OracleParameter("v" + i, OracleDbType.NVarchar2)
                        {
                            Direction = ParameterDirection.Input,
                            Value = tableData.Rows[j][i].ToString()
                        };
                        cmd.Parameters.Add(par);
                    }
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }
    }
}
