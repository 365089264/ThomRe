using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VAV.Web.Common
{
    public class JsonTableExcelResultWithCustomFormat : JsonTableExcelResult
    {
        public JsonTableExcelResultWithCustomFormat(JsonExcelParameter parameter) : base(parameter)
        {
            
        }

        protected override void BuildStyle()
        {
            base.BuildStyle();

            _textStyle.Custom = "0.0000";
        }
    }
}