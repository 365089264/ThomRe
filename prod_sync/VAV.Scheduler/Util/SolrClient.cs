using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;

namespace VAV.Scheduler.Util
{
    public class SolrClient
    {
        public static string RebuildIndex(string type)
        {
            var solrServer = ConfigurationManager.AppSettings["SolrServer"];
            var ret1 = DataImport(solrServer, "core0");
            var ret2 = DataImport(solrServer, "core1");

            return (ret1 == "Success" && ret2 == "Success") ? "Success" : ret1 + "|" + ret2;
        }

        private static string DataImport(string url, string solrCore )
        {
            string ret = "Success";
            string command = "full-import";

            if (command == "delta")
                command = "delta-import";
           
            HttpWebRequest Request = WebRequest.Create("http://" + url + ":8983/solr/" + solrCore + "/dataimport?command=" + command) as HttpWebRequest;
            HttpWebResponse Response = null;

            try
            {
                Response = Request.GetResponse() as HttpWebResponse;
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("200");
                }
            }
            catch (WebException ex)
            {
                ret = solrCore +  " Sync Fail:" + ex.Message;
            }
            finally
            {
                if (Response != null)
                    Response.Close();
            }

            return ret;
        }
    }
}
