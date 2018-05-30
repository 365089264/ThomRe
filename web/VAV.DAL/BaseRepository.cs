using System.Data;
using VAV.Entities;
using Oracle.ManagedDataAccess.Client;

namespace VAV.DAL
{
    public class BaseRepository
    {

        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetDataSetBySpFromBondDB(string inName, OracleParameter[] inParms)
        {
            using (var bonddb = new BondDBEntities())
            {
                using (var cmd = new OracleCommand())
                {
                    DataSet ds = null;

                    bonddb.Database.Connection.Open();
                    cmd.Connection = (OracleConnection)bonddb.Database.Connection;
                    cmd.CommandText = inName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    if (inParms != null)
                        cmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(cmd);
                    ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        /// <summary>
        /// Get data by store procedure
        /// </summary>
        /// <param name="inName">sp name</param>
        /// <param name="inParms">parameters</param>
        /// <returns></returns>
        protected DataSet GetDataSetBySpFromBondDB(string inName, OracleParameter[] inParms, string outName, out object value)
        {
            using (var bonddb = new BondDBEntities())
            {
                using (var cmd = new OracleCommand())
                {
                    bonddb.Database.Connection.Open();
                    cmd.Connection = (OracleConnection)(bonddb.Database.Connection);
                    cmd.CommandText = inName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    if (inParms != null)
                        cmd.Parameters.AddRange(inParms);

                    var da = new OracleDataAdapter(cmd);
                    var ds = new DataSet();
                    da.Fill(ds);
                    value = cmd.Parameters[outName].Value;

                    return ds;
                }
            }
        }
        protected DataSet GetDataSetBySpWithOutParaFromBondDb(string inName, OracleParameter[] inParms, out object outPara)
        {
            using (var bonddb = new BondDBEntities())
            {
                using (var spCmd = new OracleCommand())
                {
                    bonddb.Database.Connection.Open();
                    spCmd.Connection = (OracleConnection)(bonddb.Database.Connection);
                    spCmd.CommandText = inName;
                    spCmd.CommandType = CommandType.StoredProcedure;
                    spCmd.CommandTimeout = 0;
                    spCmd.Parameters.AddRange(inParms);

                  

                    var da = new OracleDataAdapter(spCmd);
                    var ds = new DataSet();
                    da.Fill(ds);

                    outPara = spCmd.Parameters["OutPara"].Value;

                    return ds;
                }
            }
        }
        public void Dispose()
        {
        }
    }
}
