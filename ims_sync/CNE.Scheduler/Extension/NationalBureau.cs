using System;
using Aspose.Cells;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Text;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class NationalBureau : BaseDataHandle
    {
        public void ImportTheWholeExcel(string filename, StringBuilder sb)
        {
            SyncFile.RegisterLicense();

            
            var workbook = new Workbook(filename); //工作簿 

            var ws = workbook.Worksheets[0];
            var arr = new ArrayList();
            var isContains = new ArrayList();
            if (ws != null)
            {
                int maxId;
                int insertRows = 0;
                using (var con = new OracleConnection(Connectionstr))
                {
                    var cmd = new OracleCommand("SELECT distinct ReportDate FROM NationalBureau ", con);
                    var da = new OracleDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        isContains.Add(ds.Tables[0].Rows[i][0]);
                    }
                }
                using (var con = new OracleConnection(Connectionstr))
                {
                    var cmd = new OracleCommand("SELECT max(ID)  FROM NationalBureau ", con);
                    var da = new OracleDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    maxId = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                }
                var dtColumn = ws.Cells.ExportDataTable(0, 0, 1,
                    ws.Cells.MaxColumn + 1);
                var productName = dtColumn.Rows[0][0].ToString().Trim();
                for (int i = 2; i < ws.Cells.MaxColumn; i++)
                {
                    if (dtColumn.Rows[0][i].ToString() != "")
                    {
                        arr.Add(dtColumn.Rows[0][i]);
                    }
                }

                var dt = ws.Cells.ExportDataTable(1, 0, ws.Cells.MaxRow + 1,
                             ws.Cells.MaxColumn + 1);
                using (var con = new OracleConnection(Connectionstr))
                {
                    con.Open();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i][0].ToString() != "" && dt.Rows[i][1].ToString() != "")
                        {
                            for (int j = 0; j < arr.Count; j++)
                            {
                                if (isContains.Contains(arr[j])) continue;
                                var reportDate = Convert.ToDateTime(arr[j]).ToString("dd-MMM-yyyy");
                                var proAreas = dt.Rows[i][0].ToString().Split(':');
                                var area = proAreas[0];
                                var collectName = proAreas[1];
                                var unit = dt.Rows[i][1];
                                decimal productPrice;
                                if (dt.Rows[i][2 + j] != null)
                                {
                                    decimal d;
                                    if (Decimal.TryParse(dt.Rows[i][2 + j].ToString(), out d))
                                    {
                                        if (d == 0)
                                        {
                                            continue;
                                        }
                                        productPrice = d;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                try
                                {
                                    maxId++;
                                    insertRows++;
                                    string operationSql = "INSERT INTO NATIONALBUREAU values(" + maxId + ",'" + productName + "','" + area + "','" + collectName + "','" + unit + "'," + productPrice + ",'" + reportDate + "',sysdate) ";
                                    var cmd = new OracleCommand(operationSql, con);
                                    cmd.ExecuteNonQuery();

                                }
                                catch (OracleException e)
                                {
                                    con.Close();
                                    throw new Exception(e.Message);
                                }
                            }
                        }
                        else if (dt.Rows[i][0].ToString() != "" && dt.Rows[i][1].ToString() == "")
                        {
                            productName = dt.Rows[i][0].ToString().Trim();
                        }

                    }
                    con.Close();
                }
                sb.Append("Table NATIONALBUREAU insert rows: " + insertRows + ";\r\n");
            }

        }
    }
}
