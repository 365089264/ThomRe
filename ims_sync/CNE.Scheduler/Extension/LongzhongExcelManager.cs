using System;
using System.Text;
using Aspose.Cells;
using System.IO;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace CNE.Scheduler.Extension
{
    public class LongZhongExcelManager : BaseDataHandle
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
                Cells cells = st.Cells;
                try
                {
                    sb.Append("Sheet:" + st.Name+"\n");
                    ImportDataTable(cells,sb);
                }
                catch (Exception e)
                {
                    sb.AppendFormat("[LongZhong_Yield import failed,because:{0}]", e.Message);
                    throw;
                }

            }

        }


        public void ImportDataTable(Cells cells, StringBuilder sb)
        {
            int insertCount = 0;
            int updateCount = 0;
            for (int i = 1; i < cells.Rows.Count; i++)
            {
                var curMonth = cells[i, 1].Value;
                var curYear = cells[i, 0].Value;

                if (curYear == null || curMonth == null || string.IsNullOrEmpty(curMonth.ToString()))
                {
                    //if there is no month in this row,pass it.
                    continue;
                }
                var endDate = Convert.ToDateTime(curYear.ToString().Replace('年','-')+curMonth.ToString().Replace('月','-')+"01").ToString("dd-MMM-yyyy");

                //the column of month can't be data like 01M,it should be like  1M,no zero
                var rowDate = curYear.ToString() + curMonth;

                using (var con = new OracleConnection(Connectionstr))
                {
                    con.Open();
                    OracleCommand cmd = new OracleCommand("", con);
                    for (int j = 2; j <= cells.Rows[i].LastDataCell.Column; j++)
                    {
                        var code = "LZ00" + j;
                        var cnname = cells[0, j].Value.ToString();
                        var period = rowDate;
                        double yield;
                        if (cells[i, j].Value==null||!double.TryParse(cells[i, j].Value.ToString(), out yield))
                        {
                            continue;
                        }
                        string existStr = "select Yield from LONGZHONG_YIELD where code='" + code +
                                                      "' and CNNAME='" + cnname + "' and period='" + period + "'";
                        cmd.CommandText = existStr;
                        int cmdresult = 0;
                        try
                        {
                            object obj = cmd.ExecuteScalar();
                            if (Equals(obj, null) || (Equals(obj, DBNull.Value)))
                            {
                                cmdresult = 0;
                            }
                            else
                            {
                                if (Double.Parse(obj.ToString()) == yield) continue;
                                cmdresult = 1;
                            }
                        }
                        catch (OracleException e)
                        {
                            con.Close();
                            throw new Exception(e.Message);
                        }
                        string operationSql;
                        if (cmdresult == 0)
                        {
                            insertCount++;
                            operationSql = "insert into LONGZHONG_YIELD values('" + code + "','" + cnname +
                                           "','" + period + "','" + endDate + "',"+yield+",sysdate)";
                        }
                        else
                        {
                            updateCount++;
                            operationSql = "update LONGZHONG_YIELD set yield=" + yield + ",FETCHUNIX=sysdate where code='" + code +
                                                      "' and CNNAME='" + cnname + "' and period='" + period + "'";
                        }
                        cmd.CommandText = operationSql;
                        try
                        {
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
            sb.Append("Table LONGZHONG_YIELD Insert Rows:" + insertCount + ",Update Rows:" + updateCount + "\n \r\n");
        }


    }
}
