using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNEToolsEntities
{
    /// <summary>
    /// 
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// 
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// 
        /// </summary>
        Success = 0,

        /// <summary>
        /// 
        /// </summary>
        Fail = 1,
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class SCHEDULERLOG
    {
        /// <summary>
        /// 
        /// </summary>
        public JobStatus JobStatus
        {
            set { this.STATUS = (int)value; }
            get { return (JobStatus)this.STATUS; }
        }
    }
}
