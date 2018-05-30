using System;
using System.Data;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;
using System.IO;
using Aspose.Cells;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace CNE.Scheduler.Extension
{
    public class FtpSyncLoad
    {
        private FtpSyncXmlManager SettingManager;

        private StringBuilder _log;
        public FtpSyncLoad(FtpSyncXmlManager settingManager, StringBuilder sb)
        {
            SettingManager = settingManager;
            _log = sb;
        }

        public void Excute()
        {
            var ds = SettingManager.Execute();
            #region 判断是否下载(不下载的条件：1、只有一张表，且是空表，2、有多张表，都为空)

            if (SettingManager.TableMappings.Count() == 1 && ds.Tables[0].Rows.Count == 0)
            {
                return;
            }
            bool isAllNull = true;
            foreach (var mapping in SettingManager.TableMappings)
            {
                if (ds.Tables[mapping.Destination].Rows.Count > 0)
                {
                    isAllNull = false;
                }
            }
            if (isAllNull) return;

            #endregion
            SyncFile.RegisterLicense();
            string filepath = SettingManager.FileSavePath + SettingManager.FileName.Replace("{fileDateFormat}", DateTime.Now.ToString("yyyyMMdd"));
            var fs = new FileStream(filepath, FileMode.Create);
            var workbook = new Workbook();
            var i = 0;
            foreach (var mapping in SettingManager.TableMappings)
            {
                var arrHeard = new string[mapping.ColumnMappings.Count()];
                var arrRow = new string[mapping.ColumnMappings.Count()];
                var j = 0;
                foreach (var column in mapping.ColumnMappings)
                {
                    arrHeard[j] = column.Destination;
                    arrRow[j] = column.Source;
                    j++;
                }
                if (i == 0)
                {
                    workbook.Worksheets.Clear();
                }
                var worksheet = workbook.Worksheets.Add(mapping.Destination);
                ExcelUtil.CreateWorksheet("", worksheet, worksheet.Name, ds.Tables[mapping.Destination].AsEnumerable().AsQueryable(), arrHeard, arrRow);
                _log.AppendFormat
                       ("{0} rows have been synchronized from {1} table in  DB to {2} sheet in Excel.\r\n",
                        ds.Tables[mapping.Destination].Rows.Count, mapping.Source, mapping.Destination);
                i++;
            }
            SaveFormat saveFormat;
            //Check file format is xls
            if (SettingManager.FileName.Split('.').Last().ToUpper() == "XLS")
            {
                //Set save format optoin to xls
                saveFormat = SaveFormat.Excel97To2003;
            }
            //Check file format is xlsx
            else
            {
                //Set save format optoin to xlsx
                saveFormat = SaveFormat.Xlsx;
            }
            workbook.Save(fs, new XlsSaveOptions(saveFormat));

            #region UploadFile

            string uri = "ftp://" + SettingManager.HostName + "/" + SettingManager.TargetDir + "/" +
                         SettingManager.FileName.Replace("{fileDateFormat}", DateTime.Now.ToString("yyyyMMdd"));
            FtpWebRequest reqFTP = GetRequest(uri, SettingManager.UserName, SettingManager.Password);
            reqFTP.UsePassive = true;
            reqFTP.UseBinary = true;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.ContentLength = fs.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            fs.Close();
            FileInfo localFile = new FileInfo(filepath);
            fs = localFile.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                _log.AppendFormat
                       ("UploadFile:" + SettingManager.FileName.Replace("{fileDateFormat}", DateTime.Now.ToString("yyyyMMdd")) + " . \r\n");
            }
            catch (Exception ex)
            {
                throw new Exception(_log.Append("FTP UploadFile failed,because:" + ex.Message).ToString());
            }
            #endregion

            //delete old file ,if custeel retain 20 files
            var leftCount = 5;
            if (SettingManager.FileSavePath.ToLower().Contains("custeel"))
            {
                leftCount = 20;
            }
            
            localFile.Delete();
            _log.Append("Deleted files : "+DeleteFtpFile(leftCount)+".\n");
        }

        public string DeleteFtpFile(int leftCount)
        {
            List<string> listFiles = ListDirectory(SettingManager.TargetDir, SettingManager.HostName, SettingManager.UserName, SettingManager.Password, "");
            List<FtpFileInfo> files = new List<FtpFileInfo>();
            var regInt = new Regex("[0-9]+");
            foreach (var li in listFiles)
            {
                DateTime dt = GetFileModifyDateTime(SettingManager.HostName, SettingManager.TargetDir, SettingManager.UserName, SettingManager.Password, li);
                files.Add(new FtpFileInfo { FileName = li, ModifyDate = dt, ReportType = regInt.Replace(li, "{num}") });

            }
            var comp = new Comparison<FtpFileInfo>((a, b) =>
            {
                if (a.ModifyDate > b.ModifyDate)
                    return -1;
                else
                    return 1;
            });
            files.Sort(comp);
            List<FtpFileInfo> deleteFiles = new List<FtpFileInfo>();
            var res = files.Select(re => re.ReportType).Distinct();
            foreach (string report in res)
            {
                if (files.Count(re => re.ReportType == report) <= leftCount) continue;
                deleteFiles.AddRange(files.Where(re => re.ReportType == report).Skip(leftCount));
            }
            string ftpPath = "ftp://" + SettingManager.HostName + "/" + SettingManager.TargetDir + "/";
            string result = "";
            foreach (var li in deleteFiles)
            {
                var ftp = GetRequest(ftpPath + li.FileName, SettingManager.UserName, SettingManager.Password);
                ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile; //删除
                ftp.GetResponse();
                result += li.FileName+" ; ";
            }
            if (deleteFiles.Count == 0) result = "No ;";
            return result;
        }

        private static FtpWebRequest GetRequest(string URI, string username, string password)
        {
            //根据服务器信息FtpWebRequest创建类的对象
            FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);
            //提供身份验证信息
            result.Credentials = new System.Net.NetworkCredential(username, password);
            //设置请求完成之后是否保持到FTP服务器的控制连接，默认值为true
            result.KeepAlive = false;
            return result;
        }
        /// <summary>
        /// 搜索远程文件
        /// </summary>
        /// <param name="targetDir"></param>
        /// <param name="hostname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="SearchPattern"></param>
        /// <returns></returns>
        public static List<string> ListDirectory(string targetDir, string hostname, string username, string password, string SearchPattern)
        {
            List<string> result = new List<string>();

            string URI = "FTP://" + hostname + "/" + targetDir + "/" + SearchPattern;

            System.Net.FtpWebRequest ftp = GetRequest(URI, username, password);
            ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectory;
            ftp.UsePassive = true;
            ftp.UseBinary = true;


            string str = GetStringResponse(ftp);
            str = str.Replace("\r\n", "\r").TrimEnd('\r');
            str = str.Replace("\n", "\r");
            if (str != string.Empty)
                result.AddRange(str.Split('\r'));

            return result;
        }

        private static string GetStringResponse(FtpWebRequest ftp)
        {
            //Get the result, streaming to a string
            string result = "";
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                long size = response.ContentLength;
                using (Stream datastream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(datastream, System.Text.Encoding.Default))
                    {
                        result = sr.ReadToEnd();
                        sr.Close();
                    }

                    datastream.Close();
                }

                response.Close();
            }

            return result;
        }

        public static DateTime GetFileModifyDateTime(string ftpServerIP, string ftpFolder, string ftpUserID, string ftpPwd, string fileName)
        {
            FtpWebRequest reqFTP = null;
            try
            {
                if (ftpFolder != "")
                {
                    ftpFolder = ftpFolder.Replace("/", "").Replace("\\", "");
                    ftpFolder = "/" + ftpFolder;
                }
                string ftpPath = "ftp://" + ftpServerIP + ftpFolder + "/" + fileName;

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath));

                reqFTP.UseBinary = true;
                //reqFTP.UsePassive = false;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPwd);

                reqFTP.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                DateTime dt = response.LastModified;

                response.Close();
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    public class FtpFileInfo
    {
        public string ReportType { get; set; }
        public string FileName { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
