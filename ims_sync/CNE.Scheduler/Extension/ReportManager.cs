using System;
using System.Data.OracleClient;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using CNE.Scheduler.Jobs;

namespace CNE.Scheduler.Extension
{
    public class TranslateResult
    {
        public string Translate { get; set; }
        public int IsValid { get; set; }
        public string BusinessCode { get; set; }
        public string FileCode { get; set; }
        public string InsititutionCode { get; set; }
    }
    public class FileDetail
    {
        public int FileId { get; set; }
        public string FileCode { get; set; }
        public string FileNameCN { get; set; }
        public string FileNameEN { get; set; }
        public DateTime UploadTime { get; set; }
        public int IsValid { get; set; }
        public string Ext { get; set; }
        public string InstitutionCode { get; set; }
        public string FileSize { get; set; }
        public int FileOrder { get; set; }
        public string Operator { get; set; }
        public string Comment { get; set; }
        public string CreateTime { get; set; }

        public string BusinessCode { get; set; }
        public string PhysicalPath { get; set; }
        public int? Dm1FileId { get; set; }

        //add by yy 20141216
        public string Author { get; set; }
    }
    public class ReportManager
    {

        //read byte data;
        private byte[] GetFileData(string fileName, StringBuilder sb, out   string filesize)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(fileName);


                double filesizet = (double)bytes.Length / 1024;
                filesize = string.Format("{0:N1}", filesizet) + "k";
                return bytes;
            }
            catch (Exception e)
            {
                sb.Append("[read file failed:" + e.Message + "]");
                filesize = "0K";
                return null;
            }

        }
        private bool IsChineseOrEnglish(string fileName)
        {
            foreach (char c in fileName)
            {
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(c.ToString());
                if (bs.Length > 1)
                {

                    return true;//chinese;
                }
            }
            return false; //english

        }
        public int GetFileOrder()
        {
            object i;
            string sql = "select max(FileOrder) from FileDetail";
            i = DBHelper.ExecuteScaler(sql);
            if (i == null || i == DBNull.Value)
                i = 0;
            else
                i = Convert.ToInt32(i);
            return Convert.ToInt32(i) + 1;
        }



        private string GetFileName(string fileName)
        {

            string extension = Path.GetExtension(fileName);
            fileName = fileName.Replace(extension, "");
            string[] strs = fileName.Split(new char[] { '\\' });
            string fileRealName = strs[strs.Length - 1];
            fileRealName = fileRealName.Substring(fileRealName.IndexOf("$") + 1);
            return fileRealName;
        }


        private string GetRegex(string institutionCode)
        {
            StringBuilder sb = new StringBuilder();
            string sql = "select *  from EMAILKEYWORDS where INSITITUTIONCODE='" + institutionCode + "' order by sortid ";
            DataTable tb = DBHelper.GetDataTableBySql(sql);

            if (tb != null && tb.Rows.Count > 0)
            {
                foreach (DataRow dr in tb.Rows)
                {
                    sb.Append(dr["input"].ToString() + "|");
                }
                return sb.ToString().Substring(0, sb.ToString().Length - 1);
            }
            else
            {

                return "";
            }


        }
        private bool Exists(string filename,string institutionCode, out string match)
        {
            string str = GetRegex(institutionCode);
            str = str.Replace("(", "\\(").Replace(")", "\\)");
            Regex reg = new Regex(str);
            if (reg.IsMatch(filename))
            {
                match = reg.Match(filename).Value;
                return true;
            }
            else
            {
                match = "";
                return false;
            }

        }
        private TranslateResult CreateTranslateByDataRow(DataRow dr)
        {
            TranslateResult result = new TranslateResult();
            result.IsValid = Convert.ToInt32(dr["isvalid"]);
            result.BusinessCode = dr["businessCode"].ToString();
            result.FileCode = dr["typecode"].ToString();
            result.Translate = dr["translate"].ToString();
            result.InsititutionCode = dr["InsititutionCode"].ToString();
            return result;
        }
        private TranslateResult CreateDefaultTranslate(string input)
        {
            TranslateResult result = new TranslateResult();
            result.IsValid = 0;
            result.BusinessCode = "EOthers";
            result.FileCode = "EOthers";
            result.Translate = input;
            result.InsititutionCode = "EOthers";
            return result;
        }
        private TranslateResult Translate(string input)
        {


            string sql = "select translate,isvalid ,businessCode,typecode,InsititutionCode from EMAILKEYWORDS where input=:input";
            OracleParameter[] paras = {
                                           new OracleParameter (":input",input )
                                       };

            DataTable tb = DBHelper.GetDataTableBySql(sql, paras);
            TranslateResult result = null;
            if (tb != null && tb.Rows.Count == 1)
                result = CreateTranslateByDataRow(tb.Rows[0]);
            else
                result = CreateDefaultTranslate(input);

            return result;


        }

        private string GetInstitutionCode(string email)
        {

            string sql = "select * from EMAILKEYWORDS where email like '%" + email + "%'";
            DataTable tb = DBHelper.GetDataTableBySql(sql);
            if (tb != null && tb.Rows.Count > 0)

                return tb.Rows[0]["InsititutionCode"].ToString();
            else
                return "EOthers";



        }

        public static string MD5_Hash(string path)
        {
            using (var get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                MD5CryptoServiceProvider get_md5 = new MD5CryptoServiceProvider();
                byte[] hash_byte = get_md5.ComputeHash(get_file);
                string resule = BitConverter.ToString(hash_byte);
                resule = resule.Replace("-", "");
                return resule;
            }
        }

        //get sequence from oracle db filedetail
        private int GetFileDetailSequence()
        {
            try
            {
                const string sql = "select SQ_FILEDTLS.NEXTVAL FROM SYS.DUAL";
                return Convert.ToInt32(DBHelper.ExecuteScaler(sql));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Get exception when get filedetail sequence. \n {0} \n", ex));
            }
        }

        private void InsertFileAndData(FileDetail fileDetail, string md5 = "")
        {
            var paras = new OracleParameter[19];
            paras[0] = new OracleParameter("I_FILETYPECODE", OracleType.VarChar);
            paras[0].Value = fileDetail.FileCode;
            paras[1] = new OracleParameter("I_FILENAMECN", OracleType.NVarChar, 500);
            paras[1].Value = fileDetail.FileNameCN;
            paras[2] = new OracleParameter("I_FILENAMEEN", OracleType.NVarChar, 500);
            paras[2].Value = fileDetail.FileNameEN;
            paras[3] = new OracleParameter("I_UPLOADDATE", OracleType.DateTime);
            paras[3].Value = fileDetail.UploadTime;
            paras[4] = new OracleParameter("I_REPORTDATE", OracleType.DateTime);
            paras[4].Value = fileDetail.UploadTime;
            paras[5] = new OracleParameter("I_AUTHOR", OracleType.VarChar);
            paras[5].Value = fileDetail.Author;
            paras[6] = new OracleParameter("I_ISVALID", OracleType.Number);
            paras[6].Value = fileDetail.IsValid;
            paras[7] = new OracleParameter("I_EXTENSION", OracleType.VarChar);
            paras[7].Value = fileDetail.Ext;
            paras[8] = new OracleParameter("I_INSTITUTIONINFOCODE", OracleType.VarChar);
            paras[8].Value = fileDetail.InstitutionCode;
            paras[9] = new OracleParameter("I_FILESIZE", OracleType.NVarChar);
            paras[9].Value = fileDetail.FileSize;
            paras[10] = new OracleParameter("I_FILEORDER", OracleType.Number);
            paras[10].Value = fileDetail.FileOrder;
            paras[11] = new OracleParameter("I_COMMENTS", OracleType.NVarChar, 500);
            paras[11].Value = "";
            paras[12] = new OracleParameter("I_OPERATORS", OracleType.NVarChar);
            paras[12].Value = fileDetail.Operator;
            paras[13] = new OracleParameter("I_CTIME", OracleType.DateTime);
            paras[13].Value = fileDetail.CreateTime;
            paras[14] = new OracleParameter("I_BUSINESSTYPE", OracleType.VarChar);
            paras[14].Value = fileDetail.BusinessCode;
            paras[15] = new OracleParameter("I_PHYSICALPATH", OracleType.NVarChar, 500);
            paras[15].Value = fileDetail.PhysicalPath;
            paras[16] = new OracleParameter("I_FILEID", OracleType.Number);
            paras[16].Value = fileDetail.FileId;
            paras[17] = new OracleParameter("I_FILEMD5", OracleType.VarChar);
            paras[17].Value = md5;
            paras[18] = new OracleParameter("I_DM1FILEID", OracleType.Number);
            paras[18].Value = fileDetail.Dm1FileId;

            try
            {
                DBHelper.ExecuteStorageWithoutRevalue("FILEDETAIL_INSERT", paras);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Get exception when insert filedetail. \n file Id: {0} \n {1} \n", fileDetail.FileId, ex));

            }
        }

        private bool IsExistDM1Id(FileDetail fileDetail)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("I_DM1FILEID", OracleType.Number);
            paras[0].Value = fileDetail.Dm1FileId;
            paras[1] = new OracleParameter("O_RESULT", OracleType.Number) { Direction = ParameterDirection.Output };

            try
            {
                return DBHelper.ExecuteStorageWithRevalue("FILEDETAIL_IsExistDM1ID", 1, paras) >= 1;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Get exception when insert filedetail. \n file Id: {0} \n {1} \n", fileDetail.FileId, ex));

            }
        }


        public bool InsertReportToService(EmailInfo info, StringBuilder sb)
        {
            var md5 = ReportManager.MD5_Hash(info.CurrentAttrName);
           
            //处理业务
            if (FileExists(md5))
            {
                sb.Append(string.Format("Email: {0}, attachment name: {1} duplicated with {2}.  synced successfully \n",
                    info.Ename, info.CurrentAttrName, md5));
                return true;
            }
            var fileDetail = new FileDetail();
            fileDetail.InstitutionCode = GetInstitutionCode(info.Ename);
            fileDetail.FileOrder = GetFileOrder();
            fileDetail.Author = "";
            fileDetail.Operator = "";
            if (fileDetail.InstitutionCode == "EOthers")
            {
                sb.Append("[ email:" + info.Ename + " doesn't exist in EMAILKEYWORDS:] \n");

            }
            string filesize = string.Empty;
            byte[] fileData = GetFileData(info.CurrentAttrName, sb, out filesize);
            fileDetail.FileSize = filesize;
            DateTime dt = DateTime.Parse(info.Etime);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            dt = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local);
            fileDetail.UploadTime = dt;
            fileDetail.CreateTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
            string ext = Path.GetExtension(info.CurrentAttrName).Substring(1);
            fileDetail.Ext = ext;

            string attachName = GetFileName(info.CurrentAttrName);
            string attachConfigName = string.Empty;
            //match attachName
            if (Exists(attachName, fileDetail.InstitutionCode, out attachConfigName))
            {

                TranslateResult tranalate = Translate(attachConfigName);
                if (tranalate.BusinessCode == "EOthers" && tranalate.FileCode == "EOthers")
                {
                    return false;
                }
                else
                {
                    if (IsChineseOrEnglish(attachConfigName))
                    {
                        fileDetail.FileNameCN = attachConfigName;
                        fileDetail.FileNameEN = tranalate.Translate;
                    }
                    else
                    {
                        fileDetail.FileNameEN = attachConfigName;
                        fileDetail.FileNameCN = tranalate.Translate;
                    }
                }
                //if email is not exists ,but input is exists ,use input to find institutioncode, most for @ccb.com. yy 20141223
                if (fileDetail.InstitutionCode.Equals("EOthers") &&
                    !fileDetail.InstitutionCode.Equals(tranalate.InsititutionCode))
                {
                    fileDetail.InstitutionCode = tranalate.InsititutionCode;
                }
                fileDetail.IsValid = tranalate.IsValid;
                fileDetail.BusinessCode = tranalate.BusinessCode;
                fileDetail.FileCode = tranalate.FileCode;
            }
            else
            {
                return false;
            }

            var result = SaveResearchReport(fileDetail, fileData, md5);
            if(result)
                sb.Append(string.Format("Email: {0}, attachment name: {1}, fileId: {2} has synced successfully \n", info.Ename, attachName, fileDetail.FileId));
            return result;

        }

        #region
        //call storage service, and insert into filedetail in file db
        public bool InsertReportFile(string path, string instcode, string busicode, string filetypecode, string author,  StringBuilder sb)
        {
            //check file
            if (!File.Exists(path))
            {
                return false;
            }

            FileDetail fileDetail = new FileDetail();
            fileDetail.InstitutionCode = instcode;
            fileDetail.FileOrder = GetFileOrder();

            string filesize = string.Empty;
            byte[] fileData = GetFileData(path, sb, out filesize);
            fileDetail.FileSize = filesize;
            DateTime dt = DateTime.UtcNow;

            fileDetail.UploadTime = dt;

            fileDetail.CreateTime = dt.ToString("yyyy-MM-dd HH:mm:ss");

            fileDetail.BusinessCode = busicode;
            fileDetail.FileCode = filetypecode;

            var attachName = Path.GetFileNameWithoutExtension(path);

            fileDetail.FileNameCN = attachName;
            fileDetail.FileNameEN = attachName;
            fileDetail.Ext = "pdf";
            fileDetail.IsValid = 1;
            fileDetail.Author = author;
            fileDetail.Operator = "";

            var result = SaveResearchReport(fileDetail, fileData);
            if(result)
                sb.AppendFormat("FileId: {0}, FileName: {1} Sync to Storage service successfully.\n", fileDetail.FileId, attachName);

            return result;
        }

        public bool SaveResearchReport(FileDetail detail, byte[] fileData, string md5 = "")
        {
            var fileId = 0;
            //if dm1ID is not null, from DM12DM2 job
            if (detail.Dm1FileId.HasValue)
            {
                var isExistDm1Id = IsExistDM1Id(detail);
                if (isExistDm1Id)
                {
                    fileId = detail.Dm1FileId.Value;
                }
            }
            fileId = fileId == 0 ? GetFileDetailSequence() : fileId;

            var dbPath = string.Format("|{0}|{1}|{2}", detail.BusinessCode, detail.InstitutionCode, detail.FileCode);
            var success = false;
            try
            {
                success = FileUtil.CallStorageService("|RR" +dbPath, fileData, string.Format("{0}.{1}", fileId, detail.Ext));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Get exception when call storage service\n " +
                                                        "webserviceUrl: {0} \n fileId: {1} \n {2} \n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"), fileId, ex));
            }

            if (success)
            {
                detail.FileId = fileId;
                detail.PhysicalPath = dbPath;
                InsertFileAndData(detail, md5);
            }
            else
            {
                throw new Exception(string.Format("Get failed result when call storage service\n " +
                                                        "webserviceUrl: {0} \n fileId: {1}  \n", ConfigHelper.GetEndpointClientAddress("StorageServiceImplPort"), fileId));
            }
            return true;
        }

        public bool IsBocFileExists(string filename)
        {
            string sql = "select count(1) from Filedetail where  filetypecode = 'FXDaily' and institutioninfocode = 'BOC' and businesstype = 'FX' and filenamecn = '" + filename + "'";
            var res = DBHelper.ExecuteScaler(sql);
            int coun = 0;
            int.TryParse(res.ToString(), out coun);
            return coun > 0;
        }
        private bool FileExists(string md5)
        {
            var paras = new OracleParameter[2];
            paras[0] = new OracleParameter("keyword", OracleType.NVarChar);
            paras[0].Value = md5;
            paras[1] = new OracleParameter("reslt", OracleType.Number) { Direction = ParameterDirection.Output };

            if (DBHelper.ExecuteStorageWithRevalue("FileExamine", 1, paras) == 1)
                return true;
            return false;
        }

        #endregion

    }
}
