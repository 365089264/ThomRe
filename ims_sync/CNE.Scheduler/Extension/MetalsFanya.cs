using System;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace CNE.Scheduler.Extension
{
    public class MetalsFanya : BaseDataHandle
    {
        public void Updatefanya(string url)
        {
            WebRequest myReq = WebRequest.Create(url);
            var proxy = new WebProxy(ConfigurationManager.AppSettings["MySqlProxyAddress"], Convert.ToInt32(ConfigurationManager.AppSettings["MySqlProxyPort"]));
            myReq.Proxy = proxy;
            WebResponse myRes = myReq.GetResponse();
            Stream resStream = myRes.GetResponseStream();
            if (resStream == null) return;
            var sr = new StreamReader(resStream, Encoding.UTF8);
            string responseFromServer = sr.ReadToEnd();

            sr.Close();
            myRes.Close();
            if (string.IsNullOrEmpty(responseFromServer))
            {
                return;
            }

            var arr = responseFromServer.Split('$');
            var quotationDate = arr[1];
            var quotationTime = arr[2];
            var varietyAndPrice = arr[0].Split('#');
            var dt = new DataTable();
            dt.Columns.Add("ID");
            dt.Columns.Add("Code");
            dt.Columns.Add("OpenPrice");
            dt.Columns.Add("HighestPrice");
            dt.Columns.Add("LowestPrice");
            dt.Columns.Add("BuyOnePrice");
            dt.Columns.Add("SaleOnePrice");
            dt.Columns.Add("LatestPrice");
            dt.Columns.Add("Change");
            dt.Columns.Add("TurnoverAmount");
            dt.Columns.Add("OrderAmount");
            dt.Columns.Add("SettlementPrice");
            dt.Columns.Add("YesterdaySettlement");
            dt.Columns.Add("BuyAmountOne");
            dt.Columns.Add("SaleAmountOne");
            dt.Columns.Add("Turnover");
            dt.Columns.Add("OrderKey");
            dt.Columns.Add("Name");
            dt.Columns.Add("QuotationDate");
            dt.Columns.Add("QuotationTime");

            foreach (var variable in varietyAndPrice)
            {
                var arrDr = variable.Split('|');
                var count = arrDr.Count();
                if (count!=17)
                {
                    continue;
                }
                var dr = dt.NewRow();
                for (int i = 0; i < count; i++)
                {
                    dr[i + 1] = arrDr[i];
                }
                dr[18] = quotationDate;
                dr[19] = quotationTime;
                dt.Rows.Add(dr);
            }


            using (var sqlbulkcopy = new SqlBulkCopy(Connectionstr, SqlBulkCopyOptions.UseInternalTransaction) { DestinationTableName = "[CnE].[cne].[FanyaMetals]" })
            {
                sqlbulkcopy.WriteToServer(dt);
                sqlbulkcopy.Close();
            }
        }
    }
}
