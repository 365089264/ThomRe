using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Text;
using VAV.Entities;
using VAV.Model.Data.CnE;
using System.Data;
using VAV.DAL.IPP;
using Oracle.ManagedDataAccess.Client;

namespace VAV.DAL.CnE
{
    public class CNERespository : BaseRepository
    {
        public object GetSDLocalRefineryIndex()
        {
            var gasoline = new List<object>();
            var diesel = new List<object>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select a.redate,a.gasoline as gasoline,b.dieselindex as Diesel from GasolineIndex a  join DieselIndex  b on a.redate = b.redate order by a.ReDate";

                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var date = reader.GetDateTime(0).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            var pointGasoline = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDecimal(1)
                            };
                            gasoline.Add(pointGasoline);
                            var pointDiesel = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDecimal(2)
                            };
                            diesel.Add(pointDiesel);
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return new { gasoline, diesel };
        }
        public List<Tuple<DateTime, decimal, decimal>> GetSDLocalRefineryExcelIndex()
        {
            var data = new List<Tuple<DateTime, decimal, decimal>>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select a.redate,a.gasoline as gasoline,b.dieselindex as Diesel from GasolineIndex a  join DieselIndex  b on a.redate = b.redate order by a.ReDate";

                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            data.Add(new Tuple<DateTime, decimal, decimal>(reader.GetDateTime(0), reader.GetDecimal(1), reader.GetDecimal(2)));
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return data;
        }
        public object GetSDLocalRefineryValuation()
        {
            var gasoline = new List<object>();
            var diesel = new List<object>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select a.PriceDate,a.Price as gasoline,b.Price as Diesel from GasolineValuation a  join DieselValuation  b on a.PriceDate = b.PriceDate where a.PriceDate>sysdate-365 order by a.PriceDate";

                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var date = reader.GetDateTime(0).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            var pointGasoline = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDecimal(1)
                            };
                            gasoline.Add(pointGasoline);
                            var pointDiesel = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDecimal(2)
                            };
                            diesel.Add(pointDiesel);
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return new { gasoline, diesel };
        }

        public object GetSDLocalRefineryStock()
        {
            var gasolineStock = new List<object>();
            var dieselStock = new List<object>();
            var gasolineRate = new List<object>();
            var dieselRate = new List<object>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select ReDate,GasolineStock,DieselStock,GasolineCapacityRate,DieselCapacityRate from TotalStock order by ReDate";

                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var date = reader.GetDateTime(0).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            var pointGasoline = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(1)
                            };
                            gasolineStock.Add(pointGasoline);
                            var pointDiesel = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(2)
                            };
                            dieselStock.Add(pointDiesel);
                            if (reader[3] !=DBNull.Value)
                            {
                                var pointGasolineRate = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(3)*100
                               
                            };
                                gasolineRate.Add(pointGasolineRate);
                            }

                            if (reader[4] != DBNull.Value)
                            {
                                var pointDieselRate = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(4)*100
                            };
                                dieselRate.Add(pointDieselRate);
                            }
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return new { gasolineStock, dieselStock, gasolineRate, dieselRate };
        }

        public List<Tuple<DateTime, double, double, double, double>> GetSDLocalRefineryExcelStock()
        {
            var oil = new List<Tuple<DateTime, double, double, double, double>>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select ReDate,GasolineStock,DieselStock,nvl(GasolineCapacityRate,0) GasolineCapacityRate,nvl(DieselCapacityRate,0) DieselCapacityRate from TotalStock order by ReDate";
                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var pointline = new Tuple<DateTime, double, double, double, double>(reader.GetDateTime(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3) * 100, reader.GetDouble(4) * 100);
                            oil.Add(pointline);
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return new List<Tuple<DateTime, double, double, double, double>>(oil);
        }

        public List<Tuple<DateTime, decimal, decimal>> GetSDLocalRefineryExcelValuation()
        {
            var data = new List<Tuple<DateTime, decimal, decimal>>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select a.PriceDate,a.Price as gasoline,b.Price as Diesel from GasolineValuation a  join DieselValuation  b on a.PriceDate = b.PriceDate  where a.PriceDate>sysdate-365 order by a.PriceDate";
                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            data.Add(new Tuple<DateTime, decimal, decimal>(reader.GetDateTime(0), reader.GetDecimal(1), reader.GetDecimal(2)));
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return data;
        }

        public object GetSDLocalRefineryChartData()
        {
            var gasoline = new List<object>();
            var diesel = new List<object>();
            var operating = new List<object>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select Month,Gasoline,Diesel from MonthProduction order by Month";

                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var date = reader.GetDateTime(0).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            var pointGasoline = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(1)
                            };
                            gasoline.Add(pointGasoline);
                            var pointDiesel = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(2)
                            };
                            diesel.Add(pointDiesel);
                        }
                    }
                    // Move to second result set and read Posts 
                    cmd.CommandText = "select ReDate,OperatingRate from TotalOperatingRate order by ReDate";
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var date = reader.GetDateTime(0).Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                            var pointOperating = new List<object>
                            {
                                date.TotalMilliseconds,
                                reader.GetDouble(1)*100
                            };
                            operating.Add(pointOperating);
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return new { gasoline, diesel, operating };
        }

        public Tuple<List<Tuple<DateTime, double, double>>, List<Tuple<DateTime, double>>> GetSDLocalRefinerExcelData()
        {
            var oil = new List<Tuple<DateTime, double, double>>();
            var operating = new List<Tuple<DateTime, double>>();
            using (var db = new CneNewEntities())
            {
                // Create a SQL command to execute the sproc 
                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "select Month,Gasoline,Diesel from MonthProduction order by Month";
                try
                {
                    db.Database.Connection.Open();
                    // Run the sproc  
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var pointline = new Tuple<DateTime, double, double>(reader.GetDateTime(0), reader.GetDouble(1), reader.GetDouble(2));
                            oil.Add(pointline);
                        }
                    }
                    // Move to second result set and read Posts 
                    cmd.CommandText = "select ReDate,OperatingRate from TotalOperatingRate order by ReDate";
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var pointOperating = new Tuple<DateTime, double>(reader.GetDateTime(0), reader.GetDouble(1));
                            operating.Add(pointOperating);
                        }
                    }
                    reader.Close();
                }
                finally
                {
                    db.Database.Connection.Close();
                }
            }
            return new Tuple<List<Tuple<DateTime, double, double>>, List<Tuple<DateTime, double>>>(oil, operating);
        }

        public DataTable GetSdLocalRefineryDailyOutputTable(DateTime start, DateTime end, string code, out string reDate)
        {
            using (var db = new CneNewEntities())
            {
                var connection = (OracleConnection)(db.Database.Connection);
                connection.Open();
                var cmd = new OracleCommand
                {
                    Connection = connection,
                    CommandText =
                        "SELECT max(ReDate)FROM View_DeviceStatisticsMap"
                };
                var obj = cmd.ExecuteScalar();
                if (obj == DBNull.Value)
                {
                    reDate = "";
                }
                else
                {
                    reDate = Convert.ToDateTime(obj).ToString("yyyy-MM-dd");
                }
                connection.Close();
            }
            var dt = new DataTable();
            using (var db = new CneNewEntities())
            {
                var cmd = new OracleCommand
                {
                    Connection = (OracleConnection)(db.Database.Connection),
                    CommandText =
                        "SELECT ReDate,ProcessCapacity,Gasoline,Diesel FROM DayProduction where Code='" + code + "' and ReDate>='" + start.ToString("dd-MMM-yyyy") + "' and ReDate<='" + end.ToString("dd-MMM-yyyy") + "' order by ReDate desc"
                };

                var da = new OracleDataAdapter(cmd);
                da.Fill(dt);
                db.Database.Connection.Close();
            }
            return dt;
        }

        public DataTable GetSdLocalRefineryDeviceInfoTable(bool isEnglish, string code, string reDate)
        {
            var dt = new DataTable();
            string sql = isEnglish ? "SELECT code,Device_EN as Device,YieldByTon,YieldByBarrel FROM View_DeviceStatisticsMap where  Code='" + code + "'" : "SELECT code,Device,YieldByTon,YieldByBarrel FROM View_DeviceStatisticsMap where  Code='" + code + "' order by row_num";
            using (var db = new CneNewEntities())
            {
                var cmd = new OracleCommand
                {
                    Connection = (OracleConnection)(db.Database.Connection),
                    CommandText = sql
                };

                var da = new OracleDataAdapter(cmd);
                da.Fill(dt);
                db.Database.Connection.Close();
            }
            return dt;
        }

        public DataTable GetSdLocalRefineryCompany(bool isEnglish)
        {
            var dt = new DataTable();
            string sql = isEnglish ? "SELECT DISTINCT Code,ItemName_EN as ItemName FROM View_DeviceStatisticsMap  ORDER BY Code" : "SELECT DISTINCT Code,ItemName_CN as ItemName FROM View_DeviceStatisticsMap  ORDER BY Code";
            using (var db = new CneNewEntities())
            {
                var cmd = new OracleCommand
                {
                    Connection = (OracleConnection)(db.Database.Connection),
                    CommandText = sql
                };

                var da = new OracleDataAdapter(cmd);
                da.Fill(dt);
                db.Database.Connection.Close();
            }
            return dt;
        }
    }
}
