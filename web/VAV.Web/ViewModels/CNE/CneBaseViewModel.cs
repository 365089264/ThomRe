using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;
using VAV.DAL.Services;

namespace VAV.Web.ViewModels.CNE
{
    public abstract  class CneBaseViewModel
    {
        //CneService
        public int ReportId { get; set; }
        public abstract void Initialization();
    }
}