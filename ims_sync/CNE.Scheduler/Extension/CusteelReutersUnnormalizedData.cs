using System;
using System.Collections.Generic;
using System.Text;
using Aspose.Cells;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class CusteelReutersUnnormalizedData : BaseDataHandle
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
                    int insertRows;
                    switch (st.Name)
                    {
                        case "废钢供应和需求量":
                            BuildScrapSteelSupplyDemand(st, "ScrapSteelSupplyDemand", out insertRows);
                            log.Append("Table ScrapSteelSupplyDemand insertRows:" + insertRows + " \r\n");
                            break;
                        case "废钢基地库存":
                            BuildFormalSheetData(st, "ScrapSteelBaseInventory", 5, 2, out  insertRows);
                            log.Append("Table ScrapSteelBaseInventory insertRows:" + insertRows + " \r\n");
                            break;
                        case "矿石保税区库存":
                            BuildFormalSheetData(st, "OreBondedAreaInventory", 5, 2, out  insertRows);
                            log.Append("Table OreBondedAreaInventory insertRows:" + insertRows + " \r\n");
                            break;
                        case "旬度产量":
                            BuildTenDaysPeriodOutputData(st, "TenDaysPeriodOutput", out  insertRows);
                            log.Append("Table TenDaysPeriodOutput insertRows:" + insertRows + " \r\n");
                            break;
                        case "重点月产量":
                            BuildFormalSheetData(st, "KeyMonthOutput", 4, 1, out  insertRows);
                            log.Append("Table KeyMonthOutput insertRows:" + insertRows + " \r\n");
                            break;
                        case "冷热轧及中板排产":
                            BuildFormalSheetData(st, "ColdHotRolledSheet", 4, 1, out  insertRows);
                            log.Append("Table ColdHotRolledSheet insertRows:" + insertRows + " \r\n");
                            break;
                        case "钢厂库存":
                            BuildFormalSheetData(st, "SteelMillInventory", 4, 1, out  insertRows);
                            log.Append("Table SteelMillInventory insertRows:" + insertRows + " \r\n");
                            break;
                        case "行业财务":
                            BuildIndustryFinanceData(st, "IndustryFinance", out  insertRows);
                            log.Append("Table IndustryFinance insertRows:" + insertRows + " \r\n");
                            break;
                        case "中厚板产销存":
                            BuildFormalSheetData(st, "HeavyAndMediumPlate", 5, 2, out  insertRows);
                            log.Append("Table HeavyAndMediumPlate insertRows:" + insertRows + " \r\n");
                            break;
                        case "内矿开工率":
                            BuildFormalSheetData(st, "RateOfOperation", 4, 1, out  insertRows);
                            log.Append("Table RateOfOperation insertRows:" + insertRows + " \r\n");
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.AppendFormat("Custeel Reuters Unnormalized Data[{0} read failed,because:{1}]", st.Name, e.Message);
                    throw;
                }
            }
        }

        private void BuildScrapSteelSupplyDemand(Worksheet s, string tableName, out int insertRows)
        {
            List<string> executeInsertSql = new List<string>();
            var currentMaxDate = GetTopDate(tableName);
            for (var i = 5; i <= s.Cells.Count; i++)
            {
                if (s.Cells[i, 0].Value == null)
                {
                    break;
                }
                if (s.Cells[i, 0].Value is DateTime)
                {
                    var rowDate = (DateTime)s.Cells[i, 0].Value;
                    if (rowDate <= currentMaxDate)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                string sql = "insert into " + tableName + " values('" + Convert.ToDateTime(s.Cells[i, 0].Value).ToString("dd-MMM-yyyy") + "'";
                for (var j = 1; j <= 9; j++)
                {
                    sql += "," + s.Cells[i, j].Value;
                }
                sql += ",sysdate)";
                executeInsertSql.Add(sql);
            }
            insertRows = executeInsertSql.Count;
            WriteToDb(executeInsertSql);
        }

        private DateTime GetTopDate(string tableName)
        {
            DateTime retValue;
            using (var conn = new OracleConnection(Connectionstr))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("select max(MonthDate) from {0}", tableName);
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

        private void WriteToDb(List<string> insertSql)
        {
            if (insertSql.Count > 0)
            {
                using (var con = new OracleConnection(Connectionstr))
                {
                    con.Open();
                    foreach (var insertRow in insertSql)
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

        private void BuildFormalSheetData(Worksheet s, string table, int startDataRow, int itemNameRow, out int insertRows)
        {
            List<string> executeInsertSql = new List<string>();
            var currentMaxDate = GetTopDate(table);
            for (var i = startDataRow; i <= s.Cells.Count; i++)
            {
                if (s.Cells[i, 0].Value == null)
                {
                    break;
                }
                if (s.Cells[i, 0].Value is DateTime)
                {
                    var rowDate = (DateTime)s.Cells[i, 0].Value;
                    if (rowDate <= currentMaxDate)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                for (var j = 1; s.Cells[itemNameRow, j].Value != null; j++)
                {
                    string sql = "insert into " + table + " values('" + Convert.ToDateTime(s.Cells[i, 0].Value).ToString("dd-MMM-yyyy") + "','"
                        + s.Cells[itemNameRow, j].Value.ToString().Trim() + "'";

                    if (s.Cells[i, j].Value is double)
                    {
                        sql += "," + s.Cells[i, j].Value;
                    }
                    else
                    {
                        sql += ",0";
                    }
                    sql += ",sysdate)";
                    executeInsertSql.Add(sql);
                }
            }
            insertRows = executeInsertSql.Count;
            WriteToDb(executeInsertSql);
        }

        private void BuildTenDaysPeriodOutputData(Worksheet s, string tableName, out int insertRows)
        {
            List<string> executeInsertSql = new List<string>();
            var currentMaxDate = GetTopDate(tableName);
            for (var i = 4; i <= s.Cells.Count; i++)
            {
                if (s.Cells[i, 0].Value == null)
                {
                    break;
                }
                DateTime o;
                int  day;
                string item;
                if (!DateTime.TryParse(s.Cells[i, 0].Value.ToString(),out o) )
                {
                    continue;
                }
                switch ((o.Day-1) / 10)
                {
                    case 0:
                        day = 1;
                        item = "上旬";
                        break;
                    case 1:
                        day = 11;
                        item = "中旬";
                        break;
                    default:
                        day = 21;
                        item = "下旬";
                        break;
                }
                var rowDate = new DateTime(o.Year, o.Month, day);
                if (rowDate <= currentMaxDate)
                {
                    continue;
                }
                for (var j = 1; s.Cells[0, j].Value != null; j++)
                {
                    var itemName = s.Cells[0, j].Value.ToString();
                    string sql = "insert into " + tableName + " values('" + rowDate.ToString("dd-MMM-yyyy") + "','"
                        + item + "','" + itemName.Trim() + "'";
                  
                    if (s.Cells[i, j].Value is double)
                    {
                        sql += "," + s.Cells[i, j].Value;
                    }
                    else
                    {
                        sql += ",0";
                    }
                    sql += ",sysdate)";
                    executeInsertSql.Add(sql);
                }

            }
            insertRows = executeInsertSql.Count;
            WriteToDb(executeInsertSql);
        }

        private void BuildIndustryFinanceData(Worksheet s, string tableName, out int insertRows)
        {
            List<string> executeInsertSql = new List<string>(); 
            var currentMaxDate = GetTopDate(tableName);
            for (var i = 4; i <= s.Cells.Count; i++)
            {
                if (s.Cells[i, 0].Value == null)
                {
                    break;
                }
                if (s.Cells[i, 0].Value is DateTime)
                {
                    var rowDate = (DateTime)s.Cells[i, 0].Value;
                    if (rowDate <= currentMaxDate)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                for (var j = 1; s.Cells[1, j].Value != null; j++)
                {
                    string sql = "insert into " + tableName + " values('" + Convert.ToDateTime(s.Cells[i, 0].Value).ToString("dd-MMM-yyyy")
                        + "','" + s.Cells[2, j].Value + "','" + s.Cells[1, j].Value.ToString().Trim()+"'";
                    if (s.Cells[i, j].Value is double)
                    {
                        sql += "," + s.Cells[i, j].Value;
                    }
                    else
                    {
                        sql += ",0";
                    }
                    sql += ",sysdate)";
                    executeInsertSql.Add(sql);
                }

            }
            insertRows = executeInsertSql.Count;
            WriteToDb(executeInsertSql);
        }
    }
}
