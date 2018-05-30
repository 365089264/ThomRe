using System;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class EnergyOilShandong : BaseDataHandle
    {

        public EnergyOilShandong(string filename)
        {
            FileName = filename;
        }

        private readonly StringBuilder _log = new StringBuilder();

        public void ImportTheWholeExcel(StringBuilder str)
        {
            SyncFile.RegisterLicense();
            TotalOperatingRate();
            DayProduction();
            MonthProduction();
            Profit();
            TotalStock();
            GasolineIndex();
            DieselIndex();
            GasolineValuation();
            DieselValuation();
            //DeviceStatistics();
            str.Append(_log);

        }

        public void TotalOperatingRate()
        {
            DateTime lastReDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(ReDate) FROM TotalOperatingRate", con);
                lastReDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("2.1山东地炼常减压装置总开工率");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                   

                    try
                    {
                        if(cells[i, 0].Value==null) continue;
                        DateTime currentDate;
                        if (!DateTime.TryParse(cells[i, 0].Value.ToString(), out currentDate)) continue;
                        if (currentDate <= lastReDate) continue;
                        if (cells[i, 1].Value == null) continue;
                        var operatingRate = cells[i, 1].Value;
                        string insertSql = "insert into TotalOperatingRate values('" + currentDate.ToString("dd-MMM-yyyy") + "'," + operatingRate + ",sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table TotalOperatingRate Insert Rows:" + insertCount + " \n");
        }

        public void DayProduction()
        {
            var cells = GetCellsBySheetName("5.1日产量");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    float processCapacity, gasoline, diesel;
                    if (cells[i, 2].Value == null || !float.TryParse(cells[i, 2].Value.ToString(), out processCapacity))
                    {
                        processCapacity = 0;
                    }

                    if (cells[i, 3].Value == null || !float.TryParse(cells[i, 3].Value.ToString(), out gasoline))
                    {
                        gasoline = 0;
                    }

                    if (cells[i, 4].Value == null || !float.TryParse(cells[i, 4].Value.ToString(), out diesel))
                    {
                        diesel = 0;
                    }

                    try
                    {
                        string insertSql = "insert into DayProduction values('" + cells[i, 0].Value + "','" + cells[i, 1].Value + "'," + processCapacity +
                                           "," + gasoline + "," + diesel + ",sysdate,sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table DayProduction Insert Rows:" + insertCount + " \n");
        }

        public void MonthProduction()
        {
            DateTime lastMonth;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(Month) FROM MonthProduction", con);
                lastMonth = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("5.2月产量");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 0].Value == null)
                    {
                        continue;
                    }
                    DateTime currentMonth;
                    if (!DateTime.TryParse(cells[i, 0].Value.ToString(), out currentMonth)) continue;
                    if (currentMonth <= lastMonth) continue;
                    float gasoline, diesel;
                    if (cells[i, 1].Value == null || !float.TryParse(cells[i, 1].Value.ToString(), out gasoline))
                    {
                        gasoline = 0;
                    }
                    if (cells[i, 2].Value == null || !float.TryParse(cells[i, 2].Value.ToString(), out diesel))
                    {
                        diesel = 0;
                    }
                    try
                    {
                        string insertSql = "insert into MONTHPRODUCTION values('" + currentMonth.ToString("dd-MMM-yyyy") + "'," + gasoline + "," +
                                           diesel + ",'万吨',sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table MonthProduction Insert Rows:" + insertCount + " \n");

        }

        public void Profit()
        {
            DateTime lastReDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(ReDate) FROM Profit", con);
                lastReDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("4、利润");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 0].Value == null)
                    {
                        continue;
                    }
                    DateTime currentDate;
                    if (!DateTime.TryParse(cells[i, 0].Value.ToString(), out currentDate)) continue;
                    if (currentDate <= lastReDate) continue;

                    try
                    {
                        string insertSql = "insert into Profit values('" + currentDate.ToString("dd-MMM-yyyy") + "'," + cells[i, 1].Value + "," + cells[i, 2].Value + "," + cells[i, 3].Value
                            + "," + cells[i, 4].Value + "," + cells[i, 5].Value + "," + cells[i, 6].Value + ",'元/吨',sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table Profit Insert Rows:" + insertCount + " \n");
        }

        public void TotalStock()
        {
            DateTime lastReDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(ReDate) FROM TotalStock", con);
                lastReDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("3、总库存");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 0].Value == null)
                    {
                        continue;
                    }
                    DateTime currentDate;
                    if (!DateTime.TryParse(cells[i, 0].Value.ToString(), out currentDate)) continue;
                    if (currentDate <= lastReDate) continue;

                    try
                    {
                        string insertSql = "insert into TotalStock values('" + currentDate.ToString("dd-MMM-yyyy") + "'," + cells[i, 1].Value + "," + cells[i, 2].Value + "," + cells[i, 3].Value + "," + (cells[i, 4].Value ?? "null") + "," + (cells[i, 5].Value ?? "null") + ",'万吨',sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table TotalStock Insert Rows:" + insertCount + " \n");
        }

        public void GasolineIndex()
        {
            DateTime lastReDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(ReDate) FROM GasolineIndex", con);
                lastReDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("汽油指数");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 0].Value == null)
                    {
                        continue;
                    }
                    DateTime currentDate;
                    if (!DateTime.TryParse(cells[i, 0].Value.ToString(), out currentDate)) continue;
                    if (currentDate <= lastReDate) continue;
                    if (cells[i, 1].Value == null) continue;
                    try
                    {
                        string insertSql = "insert into GasolineIndex values('" + currentDate.ToString("dd-MMM-yyyy") + "'," + cells[i, 1].Value + ",sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table GasolineIndex Insert Rows:" + insertCount + " \n");

        }

        public void DieselIndex()
        {
            DateTime lastReDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(ReDate) FROM DieselIndex", con);
                lastReDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("柴油指数");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 0].Value == null)
                    {
                        continue;
                    }
                    DateTime currentDate;
                    if (!DateTime.TryParse(cells[i, 0].Value.ToString(), out currentDate)) continue;
                    if (currentDate <= lastReDate) continue;
                    if (cells[i, 1].Value == null) continue;

                    try
                    {
                        string insertSql = "insert into DieselIndex values(" + cells[i, 1].Value + ",'" + currentDate.ToString("dd-MMM-yyyy") + "',sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table DieselIndex Insert Rows:" + insertCount + " \n");

        }

        public void GasolineValuation()
        {
            DateTime lastPriceDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(PriceDate) FROM GasolineValuation", con);
                lastPriceDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("汽油估价");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 4].Value == null)
                    {
                        continue;
                    }
                    DateTime currentDate;
                    if (!DateTime.TryParse(cells[i, 4].Value.ToString(), out currentDate)) continue;
                    if (currentDate <= lastPriceDate) continue;
                    if (cells[i, 5].Value == null) continue;
                    try
                    {
                        string insertSql = "insert into GasolineValuation values('汽油','92#','" + cells[i, 2].Value + "','" + cells[i, 3].Value + "','" + currentDate.ToString("dd-MMM-yyyy") + "'," + cells[i, 5].Value + "," + (cells[i, 6].Value ?? "null") + ",sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table GasolineValuation Insert Rows:" + insertCount + " \n");
        }

        public void DieselValuation()
        {
            DateTime lastPriceDate;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand(" SELECT  MAX(PriceDate) FROM DieselValuation", con);
                lastPriceDate = Convert.ToDateTime(cmd.ExecuteScalar());
                con.Close();
            }

            var cells = GetCellsBySheetName("柴油估价");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 4].Value == null)
                    {
                        continue;
                    }
                    DateTime currentDate;
                    if (!DateTime.TryParse(cells[i, 4].Value.ToString(), out currentDate)) continue;
                    if (currentDate <= lastPriceDate) continue;
                    if (cells[i, 5].Value == null) continue;
                    try
                    {
                        string insertSql = "insert into DieselValuation values('" + currentDate.ToString("dd-MMM-yyyy") + "','柴油','0#','" + cells[i, 2].Value + "','" + cells[i, 3].Value + "'," + cells[i, 5].Value + "," + (cells[i, 6].Value ?? "null") + ",sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table DieselValuation Insert Rows:" + insertCount + " \n");
        }

        public void DeviceStatistics()
        {
            var cells = GetCellsBySheetName("2.2地炼装置统计");
            var rowscount = cells.Rows.Count;
            int insertCount = 0;
            var company = "";
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                OracleCommand cmd = new OracleCommand("", con);
                for (var i = 1; i < rowscount; i++)
                {
                    if (cells[i, 0].GetStyle().ForegroundColor.R == 79)
                    {
                        company = cells[i, 0].Value.ToString();
                        continue;
                    }
                    if (cells[i, 0].Value.ToString() == "装置") continue;
                    try
                    {
                        var insertSql = "insert into DeviceStatistics values(null,'" + company + "','" + company + "'," + (cells[i, 1].Value == null ? "null" : (cells[i, 1].Value.ToString() == "/" ? "null" : cells[i, 1].Value)) + "," + (cells[i, 2].Value == null ? "null" : (cells[i, 2].Value.ToString() == "/" ? "null" : cells[i, 2].Value)) + ",sysdate)";
                        cmd.CommandText = insertSql;
                        cmd.ExecuteNonQuery();
                        insertCount++;
                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }
                con.Close();
            }
            _log.Append("Table DeviceStatistics Insert Rows:" + insertCount + " \n");
        }
    }
}
