using System;
using System.Data;
using System.IO;
using CNE.Scheduler.Extension;
using CNE.Scheduler.Jobs;
using Luna.DataSync.Core;
using Luna.DataSync.Setting;

namespace CNE.Scheduler
{
    public abstract class Ftp2SSJob: CmaJobBase
    {
        protected string Ftp2SS(IDataRecord obj, TableMapping tableMapping, string ftpUrl)
        {
            //1, find on FTP, if exception insert failure record or update the roginal record satus to failure
            //2, call webservice, if success, insert successed record or update previous failed record to successed on destination db
            //when need sync file before lastsyncedTime, if file has been synced, return. then synced.

            var table = TableMappingFactory.BuildTable(tableMapping);

            if (tableMapping.IsCheckFileSynced)
            {
                if (table.IsFileSynced(tableMapping, obj))
                {
                    return "";
                }
            }
            Stream responseStream;

            var fileId = table.GetFileId(obj);
            var file = obj[tableMapping.PathColumn].ToString().Replace(@"\", "/");
            if (string.IsNullOrEmpty(file))
            {
                table.AddOrUpdate(tableMapping, obj, 0);
                return string.Format("sync failed. source table {0} column {1} is empty:  \n", tableMapping.Source, tableMapping.PathColumn);
            }
            var uri = ftpUrl + file;
            try
            {
                responseStream = FileUtil.GetStreamFromFTP(uri);
            }
            catch (Exception ex)
            {
                table.AddOrUpdate(tableMapping, obj, 0);
                return string.Format("{0} sync failed. Get exception from FTP \n source table: {1} \n {2} \n", uri, tableMapping.Source, ex);
            }

            using (var ms = new MemoryStream())
            {
                const int Length = 2048;
                var buffer = new Byte[Length];
                int read = 0;
                while (responseStream != null && (read = responseStream.Read(buffer, 0, Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                var ext = Path.GetExtension(file);
                var subDirectoryPath = file.Substring(0, file.LastIndexOf("/"));
                var path = subDirectoryPath.IndexOf("/") == 0
                    ? string.Format("{0}{1}", tableMapping.DestinationFilePath, subDirectoryPath)
                    : string.Format("{0}/{1}", tableMapping.DestinationFilePath, subDirectoryPath);

                bool success;
                try
                {
                    success = FileUtil.CallStorageService(path, ms.ToArray(), string.Format("{0}{1}", fileId, ext));
                }
                catch (Exception ex)
                {
                    throw new CustomException(string.Format("Get exception when call storage service\n " +
                                                            "webserviceUrl: {0} \n fileId: {1} \n source table: {2} \n  {3} \n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"), fileId, tableMapping.Source, ex));
                }
                finally
                {
                    if (responseStream != null)
                        responseStream.Close();
                }

                try
                {
                    if (success)
                        table.AddOrUpdate(tableMapping, obj);
                }
                catch (Exception ex)
                {
                    throw new CustomException(string.Format("Get exception when insert FileDB service\n " +
                                     "fileId: {0} \n destination table: {1} \n  {2} \n", fileId, tableMapping.Destination, ex));

                }
            }

            return string.Format("{0} sync succeed \n", uri);
        }

    }
}
