using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CNE.Scheduler.FileStorgeService;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Extension
{
    public static class FileUtil
    {
        public static Stream GetStreamFromFTP(string ftpUrl)
        {
            var user = ConfigurationManager.AppSettings["fileftpuser"];
            var password = ConfigurationManager.AppSettings["fileftppassword"];
            var ftpUri = new Uri(ftpUrl);
            if (ftpUri.Scheme != Uri.UriSchemeFtp)
            {
                throw new Exception("Ftp schema is wrong:" + ftpUri.Scheme);
            }

            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)WebRequest.Create(ftpUri);
            reqFTP.Credentials = new NetworkCredential(user, password);
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.UseBinary = true;
            reqFTP.UsePassive = true;
            var response = (FtpWebResponse)reqFTP.GetResponse();
            var responseStream = response.GetResponseStream();

            return responseStream;
        }

        public static bool CallStorageService(string path, byte[] fileData, string fileName)
        {
            var fileEntity = new fileEntity
            {
                path = path.Replace("/", "|").Replace(@"\", "|"),
                fileData = fileData,
                fileName = fileName
            };
            var storage = new StorageServiceClient("StorageServiceImplPort");
            return storage.AddFileObj(fileEntity);
        }

    }
}
