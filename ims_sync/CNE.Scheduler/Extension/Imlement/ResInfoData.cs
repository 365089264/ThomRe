using System;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using Luna.DataSync.Setting;

namespace CNE.Scheduler.Extension
{
    public class ResInfo : ITableMappingOperation
    {
        const string BusinessCode = "FI";
        const string InstitutionCode = "zcx";
        const string FileCode = "RR";
        public bool Exist(TableMapping tableMapping, IDataRecord record)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("I_RES_ID", OracleType.Number) { Value = record["RES_ID"] };
            paras[1] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            return DBHelper.ExecuteStorageWithRevalue("RES_INFO_DATA_EXIST", 1, paras) == 1;
        }

        //uncomplete
        // get filedetail info
        public void AddOrUpdate(TableMapping tableMapping, IDataRecord record, int isSynced = 1)
        {
            var reportManager = new ReportManager();
            var fileOrder = reportManager.GetFileOrder();
            var fileSize = string.Format("{0:N1}k", Convert.ToDouble(record["RES_FILE_SIZE"]) / 1024);
            var fileName = record["RES_TITLE"].ToString();
            var originalFilePath = record[tableMapping.PathColumn].ToString().Replace(@"\", "/");
            var subDirectoryPath = originalFilePath.Substring(0, originalFilePath.LastIndexOf("/"));
            var filePath = subDirectoryPath.IndexOf("/") == 0
                ? string.Format("{0}{1}", tableMapping.DestinationFilePath, subDirectoryPath)
                : string.Format("{0}/{1}", tableMapping.DestinationFilePath, subDirectoryPath);

            var phisicalPath = filePath.Replace("/", "|").Replace("|RR", "");
            DateTime uploadTime;
            if (!DateTime.TryParse(record["RES_PUBL_DATE"].ToString(), out uploadTime))
                uploadTime = DateTime.Now;
            var paras = new OracleParameter[19];
            paras[0] = new OracleParameter("I_RES_ID", OracleType.Number) { Value = record["RES_ID"] };
            paras[1] = new OracleParameter("I_CCXEID", OracleType.DateTime) { Value = record["CCXEID"] };
            paras[2] = new OracleParameter("I_RES_FILE_PATH", OracleType.VarChar) { Value = record["RES_FILE_PATH"] };
            paras[3] = new OracleParameter("I_ISSYNCED", OracleType.Number) { Value = isSynced };
            paras[4] = new OracleParameter("I_FILETYPECODE", OracleType.VarChar) { Value = FileCode };
            paras[5] = new OracleParameter("I_FILENAMECN", OracleType.NVarChar, 500) { Value = fileName };
            paras[6] = new OracleParameter("I_FILENAMEEN", OracleType.NVarChar, 500) { Value = fileName };
            paras[7] = new OracleParameter("I_UPLOADDATE", OracleType.DateTime) { Value = uploadTime };
            paras[8] = new OracleParameter("I_REPORTDATE", OracleType.DateTime) { Value = uploadTime };
            paras[9] = new OracleParameter("I_AUTHOR", OracleType.VarChar) { Value = record["RESER_NAME"] };
            paras[10] = new OracleParameter("I_ISVALID", OracleType.Number) { Value = 1 };
            paras[11] = new OracleParameter("I_EXTENSION", OracleType.VarChar) { Value = Path.GetExtension(record["RES_FILE_PATH"].ToString().Substring(1)) };
            paras[12] = new OracleParameter("I_INSTITUTIONINFOCODE", OracleType.VarChar) { Value = InstitutionCode };
            paras[13] = new OracleParameter("I_FILESIZE", OracleType.NVarChar) { Value = fileSize };
            paras[14] = new OracleParameter("I_FILEORDER", OracleType.VarChar) { Value = fileOrder };
            paras[15] = new OracleParameter("I_COMMENTS", OracleType.NVarChar, 500) { Value = "" };
            paras[16] = new OracleParameter("I_OPERATORS", OracleType.NVarChar) { Value = "" };
            paras[17] = new OracleParameter("I_BUSINESSTYPE", OracleType.VarChar) { Value = BusinessCode };
            paras[18] = new OracleParameter("I_PHYSICALPATH", OracleType.NVarChar) { Value = phisicalPath };

            DBHelper.ExecuteStorageWithoutRevalue("RES_INFO_DATA_Update", paras);
        }

        public string GetFileId(IDataRecord obj)
        {
            return obj["RES_ID"].ToString();
        }

        public bool IsFileSynced(TableMapping tableMapping, IDataRecord record)
        {
            throw new NotImplementedException();
        }
    }
}
