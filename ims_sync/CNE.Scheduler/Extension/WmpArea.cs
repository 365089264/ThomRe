using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CNE.Scheduler.Extension
{
    public class WmpArea : BaseDataHandle
    {
        protected DateTime LastTime;

        public void SyncData(StringBuilder sbSync, DateTime lastTime)
        {
            LastTime = lastTime;
            var constr = ConfigurationManager.AppSettings["Genius_Hist"];
            using (var con = new SqlConnection(constr))
            {
                con.Open();
                var cmd = new SqlCommand("[dbo].[UpdateBANK_FIN_DETAIL]", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add(new SqlParameter("@MTime", SqlDbType.DateTime) { Value = lastTime });
                cmd.Parameters.Add("RETURN_VALUE", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();
                var rowCount = int.Parse(cmd.Parameters["RETURN_VALUE"].Value.ToString());
                con.Close();
                sbSync.Append(" Update Rows:" + rowCount+" ;\r\n");
            }
        }
    }
}
