using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.Model.Data.OpenMarket;

namespace VAV.Web.ViewModels.OpenMarket
{
    public class DetailDataReportViewModel
    {

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public int ID { get; set; }

        /// <summary>
        /// The data that used in view
        /// </summary>
        public List<OpenMarketRepo> Content { get; set; }
    }
}