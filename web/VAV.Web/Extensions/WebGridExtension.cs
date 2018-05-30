using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using VAV.Model.Data;

namespace VAV.Web.Extensions
{
    public static class WebGridExtension
    {
        public static IHtmlString GetHtmlWithExtraHeader(this WebGrid webGrid, string tableStyle = null,
            string headerStyle = null, string footerStyle = null, string rowStyle = null, string alternatingRowStyle = null,
            string selectedRowStyle = null, string caption = null, bool displayHeader = true, bool fillEmptyRows = false,
            string emptyRowCellValue = null, IEnumerable<WebGridColumn> columns = null, IEnumerable<string> exclusions = null,
            WebGridPagerModes mode = WebGridPagerModes.All, string firstText = null, string previousText = null, string nextText = null,
            string lastText = null, int numericLinksCount = 5, object htmlAttributes = null, string checkBoxValue = "ID",
            IEnumerable<ExtraHeader> extraHeaders = null, bool displayInChinese = false, bool hideIdColumn = false, string hideColumnName = null)
        {

            StringBuilder sb = new StringBuilder();

            if (extraHeaders != null)
            {
                var headerlevels = extraHeaders.Select(h => h.HeaderLevel).Distinct().OrderBy(h => h);

                sb.Append("<thead>");
                foreach (var headerlevel in headerlevels)
                {
                    sb.Append("<tr class='hr'>");
                    sb.Append("<th></th>"); //the first column is always the row name column.
                    foreach (var header in extraHeaders.Where(h=>h.HeaderLevel == headerlevel))
                    {
                        sb.Append(header.HeaderColumnSpan > 0 ? "<th colspan='" + header.HeaderColumnSpan + "' " : "<th ");
                        sb.Append(string.IsNullOrEmpty(header.HeaderStyle) ? string.Empty : "style = " + header.HeaderStyle);
                        sb.Append(">");
                        sb.Append(header.HeaderText);
                        sb.Append("</th>");
                    }
                    sb.Append("</tr>");
                }
            }
            
            var html = webGrid.GetHtml(tableStyle, headerStyle, footerStyle, rowStyle, alternatingRowStyle, selectedRowStyle, caption, displayHeader, fillEmptyRows, emptyRowCellValue, columns, exclusions, mode, firstText, previousText, nextText, lastText, numericLinksCount, htmlAttributes);
            string htmlToReplaceThead = html.ToString();
            //string htmlAfterReplaceThead = htmlToReplaceThead.Replace("</thead>\r\n    <tbody>\r\n", "");
            //string htmlBeforeReplaceTh = htmlAfterReplaceThead.Replace("<th", "<td");
            //string htmlAfterReplaceTh = htmlBeforeReplaceTh.Replace("</th>", "</td>");

            if (!hideIdColumn)
            {
                return MvcHtmlString.Create(htmlToReplaceThead.Replace("<thead>", sb.ToString()));// Replace thead with extra header 
            }
            if (string.IsNullOrEmpty(hideColumnName))
                hideColumnName = "ID";
            string htmlToReplaceIdColumn = htmlToReplaceThead.Replace("<th scope=\"col\">\r\n" + hideColumnName + "            </th>\r\n", "");
            string stringToSelectFirstRow = htmlToReplaceIdColumn.Replace("<thead>", sb.ToString());
            return MvcHtmlString.Create(stringToSelectFirstRow.Replace("<tbody>\r\n        <tr>", "<tbody>\r\n        <tr class='SelectedRow'>"));// Replace thead with extra header
        }
    }
}
