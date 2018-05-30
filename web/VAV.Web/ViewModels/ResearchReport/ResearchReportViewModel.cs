using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Mvc;
using VAV.DAL.Common;
using VAV.Web.ViewModels.Report;
using VAV.DAL.ResearchReport;
using VAV.Entities;
using System.IO;
using System.Linq;


namespace VAV.Web.ViewModels.ResearchReport
{
    public class ResearchReportViewModel : BaseReportViewModel
    {
        //[Dependency]
        private ResearchReportRepository _repository { get; set; }

        public List<INSTITUTIONINFO> INSTITUTIONINFO { get; set; }
        public List<InstitutionViewModel> InstitList { get; set; }
        public IList<string> fileTypeList = new List<string>();

        public ResearchReportViewModel(int id, string user)
            : base(id, user)
        {
            _repository = (ResearchReportRepository)DependencyResolver.Current.GetService(typeof(ResearchReportRepository));
        }

        public override void Initialization()
        {
            var strWhere = "";
            if (ID == 55) //Fx market
            {
                strWhere = "WHERE num<=5 AND BusinessCodeType='FX' ";
            }
            else if (ID == 61) // Fi market
            {
                strWhere += "WHERE num<=5 AND BusinessCodeType IN('FI','MM') ";
            }
            else if (ID == 22003)
            {
                strWhere = "WHERE num<=5 AND BusinessCodeType IN('Agriculture','Energy','Commodities','Metal','Others') ";
            }
            var oracleDBHelper =
                new OracleDBHelper(ConfigurationManager.AppSettings["FileDBConStr"]);
            var sql = "select * FROM GETReseachReportByType " + strWhere +
                      "ORDER BY InstitutionOrder,Code,TypeOrder,fileTypeCTIME,ReportDate DESC,num";
            var dt = oracleDBHelper.GetDataTableBySql(sql);
            //var cmd = new OracleCommand
            //{
            //    Connection = (OracleConnection)(db.Database.Connection),
            //    CommandText = "select * FROM GETReseachReportByType " + strWhere +
            //                  "ORDER BY InstitutionOrder,Code,TypeOrder,fileTypeCTIME,ReportDate DESC,num"
            //};
            //var da = new OracleDataAdapter(cmd);
            //da.Fill(dt);
            InstitList = (from DataRow dr in dt.Rows
                          select new InstitutionViewModel
                          {
                              Code = dr["Code"].ToString(),
                              InstitutionNameCn = dr["InstitutionNameCn"].ToString(),
                              InstitutionNameEn = dr["InstitutionNameEn"].ToString(),
                              Mtime = Convert.ToDateTime(dr["MTIME"]),
                              InstitutionExtension = dr["InstitutionExtension"].ToString(),
                              FileTypeNameCn = dr["FileTypeNameCn"].ToString(),
                              FileTypeNameEn = dr["FileTypeNameEn"].ToString(),
                              FileNameCn = dr["FileNameCn"].ToString(),
                              FileNameEn = dr["FileNameEn"].ToString(),
                              FileId = Convert.ToInt32(dr["FileId"]),
                              Extension = dr["Extension"].ToString(),
                              ReportDate = Convert.ToDateTime(dr["ReportDate"]),
                          }).ToList();
            INSTITUTIONINFO = (from m in InstitList.Select(re => new { re.InstitutionNameCn, re.InstitutionNameEn, re.Mtime, re.Code, Extension = re.InstitutionExtension }).Distinct()
                               select new INSTITUTIONINFO { INSTITUTIONNAMECN = m.InstitutionNameCn, INSTITUTIONNAMEEN = m.InstitutionNameEn, MTIME = m.Mtime, CODE = m.Code, EXTENSION = m.Extension }).Distinct().ToList();
            foreach (var inst in INSTITUTIONINFO)
            {
                inst.LogoPath = GetLogoImagePath(inst);
            }
        }

        private string GetLogoImagePath(INSTITUTIONINFO inst)
        {
            if (string.IsNullOrEmpty(inst.EXTENSION))
                return "";
            var path = string.Format("{0}logo-{1}-{2}.{3}", "~/Cache/", inst.CODE, inst.MTIME.Value.ToFileTime(), inst.EXTENSION);
            var realPath = HttpContext.Current.Server.MapPath(path);
            if (!File.Exists(realPath))
            {
                byte[] buffer = _repository.GetInstLogoBytesByCode(inst.CODE);
                if (buffer == null)
                    return "";
                using (var fs = new FileStream(realPath, FileMode.Create))
                {
                    fs.Write(buffer, 0, buffer.Length);
                }

            }
            return VirtualPathUtility.ToAbsolute(path);
        }
    }
}