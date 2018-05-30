using Microsoft.Practices.Unity;
using VAV.DAL.Services;

namespace VAV.Web.ViewModels.GDT
{
    public abstract class GdtBaseViewModel
    {
        public int ReportId { get; set; }

        public int ItemId { get; set; }


        [Dependency]
        public GDTService GdtService { get; set; }

        public string ContentId
        {
            get { return ReportId + "_" + ItemId; }
        }

        public string Legend { get; set; }

        public abstract void Initialization();

    }
}