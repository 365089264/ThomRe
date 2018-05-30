using System;
using System.Data;
using System.Text;
using Exception = System.Exception;
using Aspose.Cells;
using Oracle.ManagedDataAccess.Client;


namespace CNE.Scheduler.Extension
{

    public class CusteelMarketingExcelManager : BaseDataHandle
    {

        public void GetCellsBy(string fileName, StringBuilder sb)
        {
            SyncFile.RegisterLicense();
            ModifyFile(fileName, sb);
            Workbook v = new Workbook(fileName);

            foreach (Worksheet st in v.Worksheets)
            {
                try
                {
                    if (st.Name == "Sheet1")
                    {
                        continue;
                    }
                    var count = Import2Dt(st);
                    sb.AppendFormat("[{0} read complete,totle:{1}]", st.Name, count);
                }
                catch (Exception e)
                {
                    throw new Exception(sb.AppendFormat("[{0} read failed,because:{1}]", st.Name, e.Message).ToString());
                }
            }

        }

        protected decimal GetDec(object obj)
        {
            if (obj == null)
                return 0;
            var res = 0M;
            try
            {
                res = Convert.ToDecimal(obj);
            }
            catch
            {

            }

            return res;
        }


        public int Import2Dt(Worksheet ws)
        {

            var cells = ws.Cells;
            var productName = ws.Name;
            var sheetNo = ws.Index;

            var count = 0;
            DateTime maxReDate;
            int maxId;
            using (var con = new OracleConnection(Connectionstr))
            {
                var cmd = new OracleCommand("SELECT max(RE_DATE)  FROM CUSTEELMARKETINGOUTPUTDATA where cnname='" + ws.Name + "'", con);
                var da = new OracleDataAdapter(cmd);
                var ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0][0] == DBNull.Value)
                {
                    return 0;
                }
                maxReDate = Convert.ToDateTime(ds.Tables[0].Rows[0][0]);
            }
            using (var con = new OracleConnection(Connectionstr))
            {
                var cmd = new OracleCommand("SELECT max(ID)  FROM CUSTEELMARKETINGOUTPUTDATA ", con);
                var da = new OracleDataAdapter(cmd);
                var ds = new DataSet();
                da.Fill(ds);
                maxId = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
            }
            
            CuMarketingModel prev = new CuMarketingModel();
            using (var con = new OracleConnection(Connectionstr))
            {
                con.Open();
                for (int i = 4; i < cells.Rows.Count; i++)
                {
                    var curDate = cells[i, 0].Value;
                    if (curDate == null || string.IsNullOrEmpty(curDate.ToString()))
                    {
                        continue;
                    }
                    var reDate = Convert.ToDateTime(curDate);
                    if (reDate <= maxReDate) continue;
                    var temp = new CuMarketingModel(prev);
                    maxId++;
                    temp.id = maxId;
                    temp.code = "CuMar_" + sheetNo.ToString("0000");
                    temp.cnname = productName;
                    temp.re_date = reDate;
                    temp.sheet_no = sheetNo;
                    temp.unit = "吨";
                    temp.Output_YTD = GetDec(cells[i, 1].Value);
                    temp.Sell_Totle_YTD = GetDec(cells[i, 2].Value);
                    temp.Sell_Direct_YTD = GetDec(cells[i, 3].Value);
                    temp.Sell_Distribution_YTD = GetDec(cells[i, 4].Value);
                    temp.Sell_Retail_YTD = GetDec(cells[i, 5].Value);
                    temp.Sell_Branch_YTD = GetDec(cells[i, 6].Value);
                    temp.Sell_Export_YTD = GetDec(cells[i, 7].Value);
                    temp.Stock_Opening = GetDec(cells[i, 8].Value);
                    temp.Stock_Closing = GetDec(cells[i, 9].Value);
                    try
                    {
                        maxId++;
                        string operationSql = "INSERT INTO CusteelMarketingOutputData values(" + maxId +
                            ",'" + temp.code + "','" + temp.cnname + "','" + temp.re_date.ToString("dd-MMM-yyyy") + "',"
                        + temp.sheet_no + ",'" + temp.unit + "'," + temp.Output_Month + "," +
                        temp.Sell_Totle_Month + "," +
                        temp.Sell_Direct_Month + "," +
                        temp.Sell_Distribution_Month + "," +
                        temp.Sell_Retail_Month + "," +
                        temp.Sell_Branch_Month + "," +
                        temp.Sell_Export_Month + "," +
                        temp.Stock_Increase_Month + "," +
                        temp.Output_YTD + "," +
                        temp.Sell_Totle_YTD + "," +
                        temp.Sell_Direct_YTD + "," +
                        temp.Sell_Distribution_YTD + "," +
                        temp.Sell_Retail_YTD + "," +
                        temp.Sell_Branch_YTD + "," +
                        temp.Sell_Export_YTD + "," +
                        temp.Stock_Opening + "," +
                        temp.Stock_Closing + "," +
                        "sysdate) ";
                        var cmd = new OracleCommand(operationSql, con);
                        cmd.ExecuteNonQuery();

                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                    count++;
                }
                con.Close();
                return count;
            }

        }


    }

    public class CuMarketingModel
    {
        public CuMarketingModel()
        {
            PrevPeriod = null;
        }
        public CuMarketingModel(CuMarketingModel prev)
        {
            PrevPeriod = prev;
        }
        private CuMarketingModel PrevPeriod;

        public long id { get; set; }
        public string code { get; set; }
        public string cnname { get; set; }
        public DateTime re_date { get; set; }
        public int sheet_no { get; set; }
        public string unit { get; set; }
        public decimal Output_YTD { get; set; }
        public decimal Sell_Totle_YTD { get; set; }
        public decimal Sell_Direct_YTD { get; set; }
        public decimal Sell_Distribution_YTD { get; set; }
        public decimal Sell_Retail_YTD { get; set; }
        public decimal Sell_Branch_YTD { get; set; }
        public decimal Sell_Export_YTD { get; set; }
        public decimal Stock_Opening { get; set; }
        public decimal Stock_Closing { get; set; }



        #region calc columns
        private decimal GetMonthValue(decimal curValue, decimal prevValue)
        {
            if (this.PrevPeriod == null || this.re_date.Month == 1)
            {
                return curValue;
            }
            else
            {
                return curValue - prevValue;
            }
        }
        public decimal Output_Month { get { return GetMonthValue(this.Output_YTD, this.PrevPeriod.Output_YTD); } }
        public decimal Sell_Totle_Month { get { return GetMonthValue(this.Sell_Totle_YTD, this.PrevPeriod.Sell_Totle_YTD); } }
        public decimal Sell_Direct_Month { get { return GetMonthValue(this.Sell_Direct_YTD, this.PrevPeriod.Sell_Direct_YTD); } }
        public decimal Sell_Distribution_Month { get { return GetMonthValue(this.Sell_Distribution_YTD, this.PrevPeriod.Sell_Distribution_YTD); } }
        public decimal Sell_Retail_Month { get { return GetMonthValue(this.Sell_Retail_YTD, this.PrevPeriod.Sell_Retail_YTD); } }
        public decimal Sell_Branch_Month { get { return GetMonthValue(this.Sell_Branch_YTD, this.PrevPeriod.Sell_Branch_YTD); } }
        public decimal Sell_Export_Month { get { return GetMonthValue(this.Sell_Export_YTD, this.PrevPeriod.Sell_Export_YTD); } }
        public decimal Stock_Increase_Month { get { return GetMonthValue(this.Stock_Closing, this.PrevPeriod.Stock_Closing); } }
        public DateTime SyncTime { get { return DateTime.Now; } }
        #endregion
    }
}
