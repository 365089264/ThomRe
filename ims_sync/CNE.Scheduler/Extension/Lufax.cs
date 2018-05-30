using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aspose.Cells;

namespace CNE.Scheduler.Extension
{
    public class Lufax : BaseDataHandle
    {
        protected DateTime LastTime;
        protected int RowsNum = 0;

        public void GetFtpData(StringBuilder sbSync)
        {
            using (var con = new SqlConnection(Connectionstr))
            {
                var cmd = new SqlCommand("SELECT max([STARTTIME]) FROM [cne].[SCHEDULERLOG] where [JobType]='Lufax_SYNC' and  [Status]=0", con);
                con.Open();
                object obj = cmd.ExecuteScalar();
                LastTime = obj.ToString() == "" ? Convert.ToDateTime("2014-10-01") : Convert.ToDateTime(obj).AddHours(6.5);
                con.Close();
            }

            var ftpUrl = ConfigurationManager.AppSettings["LJSftpUrl"];
            var user = ConfigurationManager.AppSettings["LJSuser"];
            var password = ConfigurationManager.AppSettings["LJSpassword"];
            var uri = new Uri(ftpUrl);
            List<FtpFileAlias> list = OpenDirectory(uri, user, password);
            sbSync.Append(list.Count() + " Excel  update!" + "\r\n");
            var ljsSavePath = ConfigurationManager.AppSettings["LJSSavePath"];
            foreach (var li in list)
            {
                uri = new Uri(ftpUrl + li.HrefName);
                var reDate = li.FileName.Replace("lfex", "").Replace("Lfex", "").Replace(".xlsx", "").Replace(".xls", "");
                GetCellsByFirstSheet(uri, user, password, ljsSavePath + li.FileName, sbSync, reDate);
            }
        }
        private List<FtpFileAlias> OpenDirectory(Uri serverUri, string user, string password)
        {
            List<FtpFileAlias> list;
            FtpWebResponse response = null;
            Stream stream = null;
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Credentials = new NetworkCredential(user, password);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                response = (FtpWebResponse)request.GetResponse();
                stream = response.GetResponseStream();
                list = FillDirectoryList(stream);
            }
            catch (Exception ee)
            {
                throw new Exception(ee.Message);
            }
            finally
            {
                if (response != null)
                    response.Close();
                if (stream != null)
                    stream.Close();
            }
            return list;
        }

        private List<FtpFileAlias> FillDirectoryList(Stream stream)
        {
            var list = new List<FtpFileAlias>();
            var reader = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
            string content = reader.ReadToEnd();
            string[] files = content.Split('\n');
            foreach (var file in files)
            {
                if (file.Contains("A HREF=") && file.Contains(".xls") && (file.Contains("lfex") || file.Contains("Lfex")))
                {
                    DateTime dt;
                    if (DateTime.TryParse(file.Split(new[] { "  " }, StringSplitOptions.None)[0], out dt))
                    {
                        if (dt > LastTime)
                        {
                            var fn = file.Split('>')[1].Replace("</A", "");
                            var hn = file.Split('"')[1];
                            list.Add(new FtpFileAlias { FileName = fn, HrefName = hn });
                        }
                    }
                    continue;
                }
                if (file.Contains(".xls") && (file.Contains("lfex") || file.Contains("Lfex")))
                {
                    var options = RegexOptions.None;
                    var regex = new Regex(@"[ ]{2,}", options);
                    var fileDetail = regex.Replace(file, @" ").Replace("\r", "");
                    var arrDetails = fileDetail.Split(' ');
                    DateTime dt;
                    if (DateTime.TryParseExact(arrDetails[0] + " " + arrDetails[1], "MM-dd-yy h:mmtt", new CultureInfo("en-US"),
                              DateTimeStyles.None, out dt))
                    {
                        if (dt > LastTime)
                        {
                            var fn = arrDetails[3];
                            var hn = arrDetails[3];
                            list.Add(new FtpFileAlias {FileName = fn, HrefName = hn});
                        }
                    }
                }
            }
            reader.Close();
            return list;
        }

        private void ExecuteWrite(DataTable dt, string storedProcedure)
        {
            var strTarget = ConfigurationManager.AppSettings["LJSCon"];
            var conTarget = new SqlConnection(strTarget);
            try
            {
                conTarget.Open();
                var cmdTarget = new SqlCommand
                {
                    Connection = conTarget,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = storedProcedure
                };
                cmdTarget.Parameters.AddWithValue(@"dt", dt);
                cmdTarget.ExecuteNonQuery();
            }
            catch (Exception ee)
            {
                throw new Exception(ee.Message);
            }
            finally
            {
                conTarget.Close();
            }
        }

        public void GetCellsByFirstSheet(Uri serverUri, string user, string password, string fileName, StringBuilder sb, string reDate)
        {
            SyncFile.RegisterLicense();
            // The serverUri parameter should start with the ftp:// scheme.
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return;
            }
            // Get the object used to communicate with the server.
            var request = new WebClient { Credentials = new NetworkCredential(user, password) };
            //var request = new WebClient();
            request.DownloadFile(serverUri.ToString(), fileName);
            var v = new Workbook(fileName);
            try
            {
                var productTable = ExportWorkSheetToProduct(v.Worksheets[0].Cells, reDate);
                ExecuteWrite(productTable, "vav.sp_UpdateLufaxProduct");
                sb.AppendFormat("[{0} Product Export success,totla:{1}]", fileName, v.Worksheets[0].Cells.Rows.Count - 1);
                var detailTable = ExportWorkSheetToDetail(v.Worksheets[1].Cells);
                ExecuteWrite(detailTable, "vav.sp_UpdateLufaxDetail");
                sb.AppendFormat("[ Detail Export success,totla:{0}]",  v.Worksheets[1].Cells.Rows.Count - 1);
            }
            catch (Exception e)
            {
                sb.AppendFormat("[{0} Export failed,because:{1}]", fileName, e.Message);
                throw;
            }




        }

        public DataTable ExportWorkSheetToProduct(Cells cells, string reDate)
        {
            var tb = CreateProductTable();
            for (int i = 1; i < cells.Rows.Count; i++)
            {
                if (cells[i, 0].Value == null) break;
                var dataRow = tb.NewRow();
                dataRow[0] = cells[i, 0].Value;
                dataRow[1] = cells[i, 1].Value;
                dataRow[2] = cells[i, 2].Value;
                dataRow[3] = cells[i, 3].Value;
                dataRow[4] = cells[i, 4].Value;
                if (cells[i, 5].Value != null)
                {
                    var periods = cells[i, 5].Value.ToString().Split('-');
                    if (periods.Length == 1)
                    {
                        dataRow[5] = periods[0];
                        dataRow[6] = periods[0];
                    }
                    else
                    {
                        dataRow[5] = periods[0];
                        dataRow[6] = periods[1];
                    }
                }
                dataRow[7] = cells[i, 6].Value;
                if (cells[i, 7].Value != null)
                {
                    var capitalAmounts = cells[i, 7].Value.ToString().Split('-');
                    if (capitalAmounts.Length == 1)
                    {
                        dataRow[8] = capitalAmounts[0];
                        dataRow[9] = capitalAmounts[0];
                    }
                    else
                    {
                        dataRow[8] = capitalAmounts[0];
                        dataRow[9] = capitalAmounts[1];
                    }
                }
                if (cells[i, 8].Value != null)
                {
                    var rate = 100;
                    if (cells[i, 8].Value.ToString().Contains("%")) rate = 1;
                    var capitalCosts = cells[i, 8].Value.ToString().Replace("%", "").Split('-');
                    if (capitalCosts.Length == 1)
                    {
                        dataRow[10] = Convert.ToDecimal(capitalCosts[0]) * rate;
                        dataRow[11] = Convert.ToDecimal(capitalCosts[0]) * rate;
                    }
                    else
                    {
                        dataRow[10] = Convert.ToDecimal(capitalCosts[0]) * rate;
                        dataRow[11] = Convert.ToDecimal(capitalCosts[1]) * rate;
                    }
                }
                if (cells[i, 9].Value != null)
                {
                    var rate = 100;
                    if (cells[i, 9].Value.ToString().Contains("%")) rate = 1;
                    var expectedRates = cells[i, 9].Value.ToString().Replace("%", "").Split('-');
                    if (expectedRates.Length == 1)
                    {
                        dataRow[12] = Convert.ToDecimal(expectedRates[0]) * rate;
                        dataRow[13] = Convert.ToDecimal(expectedRates[0]) * rate;
                    }
                    else
                    {
                        dataRow[12] = Convert.ToDecimal(expectedRates[0]) * rate;
                        dataRow[13] = Convert.ToDecimal(expectedRates[1]) * rate;
                    }
                }
                dataRow[14] = cells[i, 10].Value;
                dataRow[15] = cells[i, 11].Value;
                dataRow[16] = cells[i, 12].Value;
                dataRow[17] = reDate;
                tb.Rows.Add(dataRow);
            }
            return tb;
        }
        public DataTable ExportWorkSheetToDetail(Cells cells)
        {
            var tb = CreateDetailTable();
            for (int i = 1; i < cells.Rows.Count; i++)
            {
                if (cells[i, 0].Value == null || cells[i, 1].Value == null) continue;
                var dataRow = tb.NewRow();
                dataRow[0] = cells[i, 0].Value;
                dataRow[1] = cells[i, 1].Value;
                dataRow[2] = cells[i, 2].Value;
                dataRow[3] = i;
                tb.Rows.Add(dataRow);
            }
            return tb;
        }

        private DataTable CreateProductTable()
        {
            var tb = new DataTable();
            tb.Columns.Add("ChannelID");
            tb.Columns.Add("CategoryID");
            tb.Columns.Add("ProductName");
            tb.Columns.Add("ProductType");
            tb.Columns.Add("DOMAIN");
            tb.Columns.Add("MinPeriod");
            tb.Columns.Add("MaxPeriod");
            tb.Columns.Add("PeriodUnit");
            tb.Columns.Add("MinCapitalAmount");
            tb.Columns.Add("MaxCapitalAmount");
            tb.Columns.Add("MinCapitalCost");
            tb.Columns.Add("MaxCapitalCost");
            tb.Columns.Add("MinExpectedRate");
            tb.Columns.Add("MaxExpectedRate");
            tb.Columns.Add("Minimal");
            tb.Columns.Add("Rating");
            tb.Columns.Add("States");
            tb.Columns.Add("CreateDate");
            return tb;
        }

        private DataTable CreateDetailTable()
        {
            var tb = new DataTable();
            tb.Columns.Add("ChannelID");
            tb.Columns.Add("Name");
            tb.Columns.Add("VALUE");
            tb.Columns.Add("OrderId");
            return tb;
        }
    }

    public class FtpFileAlias
    {
        public string HrefName { get; set; }
        public string FileName { get; set; }
    }
}