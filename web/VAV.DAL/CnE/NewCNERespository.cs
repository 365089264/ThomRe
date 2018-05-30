using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using VAV.DAL.IPP;
using VAV.Entities;
using VAV.Model.Data.CnE.GDT;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using VAV.Entities;
using VAV.Model.Data.CnE;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleDataAdapter = Oracle.ManagedDataAccess.Client.OracleDataAdapter;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace VAV.DAL.CnE
{
    public class NewCNERespository : NewBaseRepository
    {
        //GetDataSetBySp
        public List<CommodityNews> GetCommodityNewsData(DateTime startTime, DateTime endTime, string ntitle, int pageIndex, out int recordCount)
        {
            string strWhere = "NewsTime>='" + startTime.ToString("dd-MMM-yyyy") + "' and NewsTime<'" + endTime.AddDays(1).ToString("dd-MMM-yyyy") + "'";
            if (!string.IsNullOrEmpty(ntitle))
            {
                strWhere += "and NewsTitle like '%" + ntitle + "%'";
            }

            var dataTable = GetDataPaged("REUTERSNEWSINFO", "NewsId,NewsTitle,NewsTime", " NewsTime desc", strWhere, pageIndex, 15, 1, 0, out recordCount);
            var newsList = DataTableSerializer.ToList<CommodityNews>(dataTable);
            return newsList;
        }
        public string GetSingleNewsContent(string newsID)
        {
            OracleParameter[] paras =
            {
                new OracleParameter("P_NewsId", newsID),
                new OracleParameter("P_CUR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };
            DataSet dset = GetDataSetBySp("GetSingleNews", paras);
            return dset.Tables[0].Rows[0][0].ToString();
        }
    }
}
