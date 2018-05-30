using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.IO;
using System.Data;

namespace CNE.Scheduler.Extension
{
    public class GetFtpFileData
    {
        protected DateTime LastTime;
        protected int RowsNum = 0;
        private string Connectionstr = ConfigurationManager.AppSettings["CnEConFileData"];
        private Dictionary<string, DateTime> uploadTimeDic = new Dictionary<string, DateTime>();
        public void GetWebData(ref StringBuilder sbSync)
        {
            using (var con = new SqlConnection(Connectionstr))
            {
                var cmd = new SqlCommand("SELECT max([getdate]) FROM [dbo].[ChineseDocs]", con);
                con.Open();
                object obj = cmd.ExecuteScalar();
                LastTime = obj.ToString() == "" ? Convert.ToDateTime("2014-2-16") : Convert.ToDateTime(obj);
                con.Close();
            }

            var ftpUrl = ConfigurationManager.AppSettings["fileftpUrl"];
            var user = ConfigurationManager.AppSettings["fileftpuser"];
            var password = ConfigurationManager.AppSettings["fileftppassword"];
            var uri = new Uri(ftpUrl);
            IEnumerable<string> list = OpenDirectory(uri, user, password);
            var enumerable = list as IList<string> ?? list.ToList();
            sbSync.Append(enumerable.Count() + " File  update!" + "\r\n");
            foreach (var li in enumerable)
            {
                uri = new Uri(ftpUrl + li);
                DisplayFileFromServer(uri, user, password, li, sbSync);
            }
            sbSync.Append(RowsNum + " rows completed  update!" + "\r\n");
        }
        private IEnumerable<string> OpenDirectory(Uri serverUri, string user, string password)
        {
            var list = new List<string>();
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
            catch (Exception)
            {
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
            string[] files = content.Split(new char []{'\n'},StringSplitOptions .RemoveEmptyEntries);
            var table = new DataTable();

            foreach (var file in files)
            {
                //判断文件是否存在
                string[] turple = file.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //12-19-13 04:36PM
                string[] date = turple[0].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                string[] time = null;
                int hour = 0; int minute = 0;
                if (turple[1].Contains("PM"))
                {
                    turple[1] = turple[1].Replace("PM", "");
                    time = turple[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    hour = Convert.ToInt32(time[0]) + 12;
                    minute = Convert.ToInt32(time[1]);
                }
                else if (turple[1].Contains("AM"))
                {
                    turple[1] = turple[1].Replace("AM", "");
                    time = turple[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    hour = Convert.ToInt32(time[0]);
                    minute = Convert.ToInt32(time[1]);
                }
                string filename = turple[3].Replace("\r", "");
                DateTime uploadTime = new DateTime(Convert.ToInt32(date[2]) + 2000, Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), hour, minute, 0);
                //获取文件的上传时间

                //02-26-14  01:21PM              1950798 Southwest021213.pdf
                string fname = file.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[3].Replace("\r","");
                if (uploadTime > LastTime)
                {
                   
                    //  var mf = file.Split('>')[1].Replace("</A", "");
                    list.Add(fname);
                    uploadTimeDic.Add(fname, uploadTime);

                }


            }
            reader.Close();

            return list;
        }


        public bool DisplayFileFromServer(Uri serverUri, string user, string password, string filename,StringBuilder sb)
        {
            // The serverUri parameter should start with the ftp:// scheme.
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(serverUri);
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.UseBinary = true;
            req.UsePassive = true;
            req.Credentials = new NetworkCredential(user, password);
            try
            {
                //判断文件是否存在；
                using (SqlConnection conn = new SqlConnection(Connectionstr))
                {
                    conn.Open();
                    string sql = "select Count(*) from ChineseDocs where pdfName='" + filename + "'";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {


                        object o = cmd.ExecuteScalar();
                        int i = o == null ? 0 : Convert.ToInt32(o);
                        if (i == 1)
                        {
                           // string localfile = Path.Combine(LocalPath, filename);
                            using (FtpWebResponse res = (FtpWebResponse)req.GetResponse())
                            {
                                MemoryStream ms = new MemoryStream();
                                 byte []bytes=new byte [1024];
                                Stream  stream= res.GetResponseStream();  
                                int iLen=0;
                                while ((iLen = stream.Read(bytes, 0, 1024)) > 0)
                                {
                                    ms.Write(bytes ,0 ,iLen);
                                }
                                 //更新回数据库；.Read(ms, 0, res.GetResponseStream().Length);
                                DateTime utime = uploadTimeDic[filename];
                                cmd.CommandText = "update ChineseDocs set fileData=@data,[getdate]=@time where pdfName=@name ";
                                SqlParameter[] paras = { 
                                                         new SqlParameter ("@data",ms .ToArray ()),
                                                         new SqlParameter ("@time",utime ),
                                                         new SqlParameter ("@name",filename)
                                                       };
                                cmd.Parameters.AddRange(paras);
                                cmd.ExecuteNonQuery();
                                
                            }
                        }
                    }
                    conn.Close();
                }




            }
            catch (Exception  e)
            {
                sb.Append("["+e .Message +"]");
            }
            return true;
        }



    }
}

