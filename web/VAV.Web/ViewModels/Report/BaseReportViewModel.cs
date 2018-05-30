using System.Web;
using VAV.DAL.Services;
using System.Web.Mvc;

namespace VAV.Web.ViewModels.Report
{
    /// <summary>
    /// Base ReportModel
    /// </summary>
    public abstract class BaseReportViewModel
    {
                /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public int ID { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public string UserId { get; set; }

        public ChartService ChartService { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicReportViewModel" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="user">The user.</param>
        protected BaseReportViewModel(int id,string user = "Unknown")
        {
            ID = id;
            UserId = user;
            ChartService = (ChartService)DependencyResolver.Current.GetService(typeof(ChartService));
        }

        /// <summary>
        /// Initializations this instance.
        /// </summary>
        public abstract void Initialization();

        /// <summary>
        /// Initializations the specified req.
        /// </summary>
        /// <param name="req">The req.</param>
        public virtual void Initialization(HttpRequestBase req )
        {
            Initialization();
        }
    }
}