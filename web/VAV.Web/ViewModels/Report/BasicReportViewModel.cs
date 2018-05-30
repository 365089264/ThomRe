using VAV.Model.Data;
using System.Dynamic;
using System.Collections.Generic;
using System.Web.Helpers;
using System.Collections;
using System.Data;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace VAV.Web.ViewModels.Report
{
    /// <summary>
    /// Basic Report Model
    /// </summary>
    public class BasicReportViewModel : BaseReportViewModel
    {
        private List<Column> columns = new List<Column>();
        public List<Column> Columns
        {
            get { return columns; }
            set { columns = value; }
        }

        //Default setting for sorting is true;
        public bool EnableSort { get; set; }

        //Default setting for paging is true;
        public bool EnablePaging { get; set; }

        /// <summary>
        /// Extra headers to draw on top of the grid.
        /// </summary>
        public List<ExtraHeader> ExtraHeaderCollection
        {
            get
            {
                if (extraHeaderCollection == null)
                    extraHeaderCollection = new List<ExtraHeader>();
                return extraHeaderCollection;
            }
            set { extraHeaderCollection = value; }
        }
        private List<ExtraHeader> extraHeaderCollection;

        /// <summary>
        /// Text to display when no data in the source.
        /// </summary>
        public string EmptyText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicReportViewModel"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public BasicReportViewModel(int id, StandardReport report)
            : base(id)
        {
            this.standardReport = report;
            this.standardReport.IsStatisticalReport = false;
        }

        private StandardReport standardReport;
        public StandardReport StandardReport { get { return standardReport; } }

        public override void Initialization()
        {
            if (standardReport.ResultDataTable.Rows.Count < 0)
            {
                this.EmptyText = "No result matches search criteria.";
            }
        }
    }

    //public BasicReportViewModel GetBasicReportViewModel()
    //{
    //    if (standardReport.ResultDataTable.Rows.Count > 0)
    //    {
    //        var dataDic = ConvertToDictionary(standardReport.ResultDataTable);
    //        this.reportData = ConvertToDynamic(dataDic);
    //        this.columns = standardReport.Columns;
    //        this.extraHeaderCollection = standardReport.ExtraHeaderCollection;
    //    }
    //    else
    //    {
    //        this.EmptyText = "No result matches search criteria.";
    //    }
    //    return this;
    //}

    //private Expression<Func<dynamic, string>> GetColumnFormat(string format, string propertyName)
    //{
    //    var columnPropInfo = typeof(ColumnModel).GetProperty(propertyName);
    //    var entityParam = Expression.Parameter(typeof(ColumnModel), "e");
    //    var formatMethod = typeof(String).GetMethod("Format", new[] { typeof(string), typeof(Object) });
    //    var entityParam2 = Expression.Parameter(typeof(object), "e");
    //    var columnExpr = Expression.MakeMemberAccess(entityParam, columnPropInfo);
    //    var columnExprObj = Expression.Convert(columnExpr, typeof(object));
    //    var formatCall = Expression.Call(formatMethod, Expression.Constant(format), columnExprObj);
    //    var lambda = Expression.Lambda(formatCall, entityParam2) as Expression<Func<dynamic, string>>;
    //    return lambda;
    //}

    //private List<ExtendedWebGridColumn> BuildColumns(List<Column> reportColumns)
    //{
    //    List<ExtendedWebGridColumn> webGridColumns = new List<ExtendedWebGridColumn>();
    //    foreach (Column column in reportColumns)
    //    {
    //        webGridColumns.Add(new ExtendedWebGridColumn
    //        {
    //            ColumnName = column.ColumnName,
    //            Header = column.ColumnHeaderCN,
    //            //Format = item => GetFormat(item, item.ColumnFormatString, item.ColumnName),
    //            ColumnFormatString = column.ColumnFormat,
    //            Style = column.ColumnStyle
    //            //Format = string.IsNullOrEmpty(column.ColumnFormat) ? null : GetColumnFormat(column.ColumnFormat, column.ColumnName).Compile()
    //        });
    //    }
    //    return webGridColumns;
    //}
    //private object GetFormat(object row, string format, string columnName)
    //{
    //    var dataRow = (WebGridRow)row;
    //    return string.Format(format, dataRow[columnName]);
    //}
}