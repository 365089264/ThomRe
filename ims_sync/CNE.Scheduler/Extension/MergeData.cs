using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using System.Data;

namespace CNE.Scheduler.Extension
{
    public class MergeData
    {

        public void Execute(OracleConnection conn, OracleTransaction tran)
        {


            using (OracleCommand cmd = new OracleCommand("delete from GDT_AgricultureMax", conn))
            {
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }

            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "createMaxAgricultrueTable";
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
        }
        public void ExecuteOilInventoryMax(OracleConnection conn, OracleTransaction tran)
        {
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "createMaxOilInventoryTable";
                cmd.CommandTimeout = 300;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
        }
        public void ExecuteMetals(OracleConnection conn, OracleTransaction tran)
        {
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "createMaxMetalTable";
                cmd.CommandTimeout = 300;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
        }

        public void ExecuteCoffedAndMetalMax(OracleConnection conn, OracleTransaction tran)
        {
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "createMaxMetalOutputTable";
                cmd.CommandTimeout = 300;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CREATEMAXAGRICULTRUEOutput";
                cmd.CommandTimeout = 300;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
            //create agricultrue inventory max table 20140710 yy
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "createMaxAgricultrueInventory";
                cmd.CommandTimeout = 300;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
            }
        }

        public void ExecuteLongZhongExcel(OracleConnection conn, OracleTransaction tran)
        {
            ExecuteCneZcxExcel();

            // energy map 2 max , ver. 2.3
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Transaction = tran;
                cmd.Connection = conn;
                cmd.CommandText = "createMaxEnergyYieldTable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.ExecuteNonQuery();
            }
            //chemical map 2 max 
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Transaction = tran;
                cmd.Connection = conn;
                cmd.CommandText = "createMaxChemistryOutputTable";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.ExecuteNonQuery();
            }
        }

        public void ExecuteCneZcxExcel()
        {
            string zcxStr = ConfigurationManager.AppSettings["ZCXConnStr"].ToString();
            using (var zcxConn = new OracleConnection(zcxStr))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    zcxConn.Open();
                    cmd.Connection = zcxConn;
                    cmd.CommandText = "sp_MergeZCXEnergyYieldData";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                    zcxConn.Close();
                }
                //chemical ver. 2.3 rewrite this class
                using (OracleCommand cmd = new OracleCommand())
                {
                    zcxConn.Open();
                    cmd.Connection = zcxConn;
                    cmd.CommandText = "sp_MergeZCXChemistryOutputData";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                    zcxConn.Close();
                }
            }
        }



        public void ExecuteCusteelExcel(OracleConnection conn, OracleTransaction tran)
        {
           
           //create max table for metal invenotry 20140710 yy
           using (OracleCommand cmd = new OracleCommand())
           {
               cmd.Transaction = tran; 
               cmd.Connection = conn;
               cmd.CommandText = "createMaxMetalInventoryTable";
               cmd.CommandType = CommandType.StoredProcedure;
               cmd.CommandTimeout = 300;
               cmd.ExecuteNonQuery();
           }

           //create max table for metal sales 20140710 yy
           using (OracleCommand cmd = new OracleCommand())
           {
               cmd.Transaction = tran; 
               cmd.Connection = conn;
               cmd.CommandText = "createMaxMetalSales";
               cmd.CommandType = CommandType.StoredProcedure;
               cmd.CommandTimeout = 300;
               cmd.ExecuteNonQuery();
           }
             
        }


    }
}
