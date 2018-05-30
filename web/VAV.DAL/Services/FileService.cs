using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Data;
using VAV.DAL.Common;
using VAV.Model.Data;
using VAV.Model.Data.Bond;
using VAV.DAL.Report;
using VAV.Model.Data.OpenMarket;
using VAV.Entities;
using VAV.DAL.FileStorageService;


namespace VAV.DAL.Services
{
    /// <summary>
    /// Interface to fetch file
    /// </summary>
    public interface IFileService
    {
        CmaFile GetFileById(long id, string para);
        void UploadFile(string path, string name, byte[] data);
    }


    public class FileStorageService : IFileService
    {
        public CmaFile GetFileById(long id, string para)
        {
            string path = "";
            string fileName = "";
            string fileType = "";

            if (para == "VAV") //for home page download
            {
                path = "|VAV|Home";
                using (var db = new CMAEntities())
                {
                    fileType = db.HOMEFILEs.Where(f => f.ID == id).Select(f => f.FILETYPE).ToList().FirstOrDefault();
                    fileName = id + "." + fileType;
                }
            }
            else if (para == "IPP") //for macro-ecnomic 100 charts and user uploaded file
            {
                using (var db = new IPPEntities())
                {
                    var subPath = (from f in db.FILEINFOs
                                  join t in db.TOPICs on f.TOPICID equals t.ID
                                  join m in db.MODULEINFOs on t.MODULEID equals m.ID
                                  where f.ID == id
                                  select new { m.NAMEEN, t.ID,f.FILETYPE }).ToList().FirstOrDefault();
                    path = "|IPP|" + subPath.NAMEEN + "|" + subPath.ID;
                    fileName = id + "." + subPath.FILETYPE;
                    fileType = subPath.FILETYPE;
                }
            }
            else if (para.Contains("WMP"))//cmafiledb
            {
                using (var db = new Genius_HistEntities())
                {
                    if (para == "WMP_PROSP")
                    {
                        var accRoute = db.BANK_FIN_PRD_PROSP.Where(f => f.INNER_CODE == id).ToList().Select(f => f.ACCE_ROUTE).FirstOrDefault().Replace("\\", "|");
                        path = "|WMP|RROSP|" + accRoute.Substring(0, accRoute.LastIndexOf("|"));
                        fileName = id.ToString() + accRoute.Substring(accRoute.LastIndexOf("."));
                        fileType = accRoute.Substring(accRoute.LastIndexOf(".") + 1);
                    }
                    else if (para == "WMP_REP")
                    {
                        var accRoute = db.FIN_PRD_RPT.Where(f => f.RPT_ID == id).ToList().Select(f => f.ACCE_ROUTE).FirstOrDefault().Replace("\\", "|");
                        path = "|WMP|PRD_RPT|" + accRoute.Substring(0, accRoute.LastIndexOf("|"));
                        fileName = id.ToString() + accRoute.Substring(accRoute.LastIndexOf("."));
                        fileType = accRoute.Substring(accRoute.LastIndexOf(".") + 1);
                    }
                    else if (para == "WMP_DISC")
                    {
                        var accRoute = db.DISC_ACCE_CFP.Where(f => f.SEQ == id).ToList().Select(f => f.ACCE_ROUTE).FirstOrDefault().Replace("\\", "|");
                        var accOrder = db.DISC_ACCE_CFP.Where(f => f.SEQ == id).ToList().Select(f => f.ACCE_ORDER).FirstOrDefault().ToString();
                        var disc_id = db.DISC_ACCE_CFP.Where(f => f.SEQ == id).ToList().Select(f => f.DISC_ID).FirstOrDefault().ToString();
                        path = "|WMP|DISC_CFP|" + accRoute.Substring(0, accRoute.LastIndexOf("|"));
                        fileName = disc_id + "_" + accOrder + accRoute.Substring(accRoute.LastIndexOf("."));
                        fileType = accRoute.Substring(accRoute.LastIndexOf(".") + 1);
                    }
                }
            }
            else if (para == "ZCX")
            {
                using (var db = new ZCXEntities())
                {
                    var accRoute = db.RATE_REP.Where(f => f.RATE_ID == id).ToList().Select(f => f.RATE_FILE_PATH).FirstOrDefault().Replace("/", "|");
                    path = "|ZCX|RATE" + accRoute.Substring(0, accRoute.LastIndexOf("|"));
                    fileName = accRoute.Substring(accRoute.LastIndexOf("|") + 1);
                    fileType = accRoute.Substring(accRoute.LastIndexOf(".") + 1);
                }
            }
            else if (para == "RR")
            {
                using (var db = new ResearchReportEntities())
                {
                    path = "|RR|" + db.ALLFILESINFOes.Where(f => f.FILEID == id).ToList().Select(f => f.BIZCODE + "|" + f.INSTITUTIONINFOCODE + "|" + f.FILETYPECODE).FirstOrDefault();
                    fileName = db.ALLFILESINFOes.Where(f => f.FILEID == id).ToList().Select(f => f.FILEID.ToString() + "." + f.EXTENSION).FirstOrDefault();
                }
            }
            else if (para.ToUpper() == "LOGO")
            {
                using (var db = new ResearchReportEntities())
                {
                    var institution = db.INSTITUTIONINFOes.FirstOrDefault(i => i.ID_C == (decimal) id);
                    if (institution != null)
                    {
                        path = "|RR|Logo";
                        fileName = string.Format("{0}.{1}", institution.CODE, institution.EXTENSION);
                    }
                }
            }

            var storage = new StorageServiceClient();
            var obj = new fileEntity();

            try
            {
                obj = storage.RetriveFileObj(path, fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (obj.fileData==null || obj.fileData.Length == 0)
                return null;

            return new CmaFile { Id = id, Content = obj.fileData, FileType = fileType };
        }

        public void UploadFile(string path, string name, byte[] data)
        {
            if (data == null) return;
            var storage = new StorageServiceClient();

            var f = new fileEntity
            {
                path = path,
                fileData = data,
                fileName = name
            };

            try
            {
                storage.AddFileObj(f);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
