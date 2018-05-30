using System;
using System.Collections.Generic;
using System.Threading;
using VAV.Entities;

namespace VAV.Web.ViewModels.Report
{
    public class InstitutionViewModel
    {
        public string Code { get; set; }
        public string InstitutionNameCn { get; set; }
        public string InstitutionNameEn { get; set; }
        public DateTime? Mtime { get; set; }
        public string InstitutionExtension { get; set; }
        public string InstiDisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? InstitutionNameCn : InstitutionNameEn; }
        }
        public string FileTypeNameCn { get; set; }
        public string FileTypeNameEn { get; set; }
        public string FileTypeDisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? FileTypeNameCn : FileTypeNameEn; }
        }
        public int FileId { get; set; }
        public string FileNameCn { get; set; }
        public string FileNameEn { get; set; }
        public string FileDisplayName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? FileNameCn : FileNameEn; }
        }
        public string Extension { get; set; }
        public DateTime? ReportDate { get; set; }
    }
}