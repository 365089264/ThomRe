using System.Configuration;
using System;
using System.Linq;
using System.Xml;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace CNE.Scheduler.Extension
{
    public class FxNews : BaseDataHandle
    {
        protected DateTime LastTime;
        protected int InsertRows;
        protected int UpdateRows;

        public void GetWebData(StringBuilder sbSync, DateTime lastSyncTime)
        {
            LastTime = lastSyncTime;
            sbSync.AppendFormat("Source [Type: {0} URL: {1}],User:{2},Password:{3}\n", "FTP",
                                ConfigurationManager.AppSettings["fx168ftpUrl"], ConfigurationManager.AppSettings["fx168user"], ConfigurationManager.AppSettings["fx168password"]);
            sbSync.AppendFormat("Destination [Type: {0} Address: {1}]\n", "Oracle",
                                 Connectionstr);


            var ftpUrl = ConfigurationManager.AppSettings["fx168ftpUrl"];
            var user = ConfigurationManager.AppSettings["fx168user"];
            var password = ConfigurationManager.AppSettings["fx168password"];
            var uri = new Uri(ftpUrl);
            IEnumerable<string> list = OpenDirectory(uri, user, password);
            var enumerable = list as IList<string> ?? list.ToList();
            foreach (var li in enumerable)
            {

                sbSync.Append("<p>XmlFile :" + li + "<ol>");
                uri = new Uri(ftpUrl + li);
                DisplayFileFromServer(uri, user, password, li, sbSync);
                sbSync.Append("</ol></p>");
            }

            sbSync.Append("Table REUTERSNEWSINFO Insert Rows:" + InsertRows + ",Update Rows:" + UpdateRows + " \n");
        }
        private IEnumerable<string> OpenDirectory(Uri serverUri, string user, string password)
        {
            List<string> list;
            FtpWebResponse response = null;
            Stream stream = null;
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Credentials = new NetworkCredential(user, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Proxy = new WebProxy("10.40.14.55", 80);
                response = (FtpWebResponse)request.GetResponse();
                stream = response.GetResponseStream();
                list = FillDirectoryList(stream);
            }
            catch (Exception ee)
            {
                throw new Exception("update fail:Fun OpenDirectory, " + ee);
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

        private List<string> FillDirectoryList(Stream stream)
        {
            var list = new List<string>();
            var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            string[] files = content.Split('\n');
            foreach (var file in files)
            {
                if (file.Contains("A HREF="))
                {
                    DateTime dt;
                    if (DateTime.TryParse(file.Split(new[] { "  " }, StringSplitOptions.None)[0], out dt))
                    {
                        if (dt > LastTime)
                        {
                            var mf = file.Split('>')[1].Replace("</A", "");
                            list.Add(mf);
                        }
                    }

                }
            }
            reader.Close();
            return list;
        }

        public bool DisplayFileFromServer(Uri serverUri, string user, string password, string filename, StringBuilder sbSync)
        {
            // The serverUri parameter should start with the ftp:// scheme.
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            // Get the object used to communicate with the server.
            var request = new WebClient
            {
                Credentials = new NetworkCredential(user, password),
                Proxy = new WebProxy("10.40.14.55", 80)
            };
            // This example assumes the FTP site uses anonymous logon.
            try
            {
                string xmlString = request.DownloadString(serverUri.ToString());
                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlString);
                XmlNodeList list = xmldoc.SelectNodes("//row");
                using (var con = new OracleConnection(Connectionstr))
                {
                    con.Open();
                    OracleCommand cmd ;
                    if (list != null)
                        foreach (var li in list)
                        {
                            cmd = new OracleCommand("", con);
                            var xe = (XmlElement)li;
                            var xnls = xe.ChildNodes;
                            var newsId = xnls[0].InnerText;
                            var newsTitle = xnls[1].InnerText;
                            var newsContent = xnls[2].InnerText;
                            var str = xnls[0].InnerText;
                            var strDate = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2);
                            var newsTime = Convert.ToDateTime(strDate).ToString("yyyy-MM-dd HH:mm:ss");
                            string existStr = "select count(*) from REUTERSNEWSINFO where NEWSID='" + newsId + "'";
                            cmd.CommandText = existStr;
                            int cmdresult = 0;
                            try
                            {

                                object obj = cmd.ExecuteScalar();
                                if (Equals(obj, null) || (Equals(obj, DBNull.Value)))
                                {
                                }
                                else
                                {
                                    cmdresult = int.Parse(obj.ToString());
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
                                sbSync.Append("<li>NewsId:" + newsId + " insert ");
                                InsertRows++;
                                operationSql = "insert into REUTERSNEWSINFO values(:NEWSID,:NEWSTITLE,:NEWSCONTENT,to_date('" + newsTime + "','yyyy-mm-dd hh24:mi:ss'),sysdate)";
                                cmd = new OracleCommand(operationSql, con);
                                cmd.Parameters.Add(new OracleParameter(":NEWSID", OracleDbType.NVarchar2) { Value = newsId });
                                cmd.Parameters.Add(new OracleParameter(":NEWSTITLE", OracleDbType.NVarchar2) { Value = newsTitle });
                                cmd.Parameters.Add(new OracleParameter(":NEWSCONTENT", OracleDbType.NClob) { Value = newsContent });
                            }
                            else
                            {
                                continue;
                                /*
                                sbSync.Append("<li>NewsId:" + newsId + " update ");
                                UpdateRows++;
                                operationSql = "update REUTERSNEWSINFO set NEWSTITLE= :NEWSTITLE,NEWSCONTENT= :NEWSCONTENT,NEWSTIME=to_date('" + newsTime + "','yyyy-mm-dd hh24:mi:ss'),CTIME=sysdate  where NEWSID= :NEWSID";
                                cmd = new OracleCommand(operationSql, con);
                                cmd.Parameters.Add(new OracleParameter(":NEWSTITLE", OracleDbType.NVarchar2) { Value = newsTitle });
                                cmd.Parameters.Add(new OracleParameter(":NEWSCONTENT", OracleDbType.NClob) { Value = newsContent });
                                cmd.Parameters.Add(new OracleParameter(":NEWSID", OracleDbType.NVarchar2) { Value = newsId });
                                */
                            }
                            
                            
                            try
                            {
                                cmd.ExecuteNonQuery();
                                sbSync.Append("<span style=\"color:green;\">success!</span></li>");

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
            catch (WebException ee)
            {
                throw new Exception("update fail:Fun DisplayFileFromServer, " + ee);
            }
            return true;
        }

    }
}