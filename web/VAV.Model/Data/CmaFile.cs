using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data
{
    /// <summary>
    /// Report Info
    /// </summary>
    public class CmaFile : BaseModel
    {
        public long Id { get; set; }
        public byte[] Content { get; set; }
        public string FileType { get; set; }
    }
}
