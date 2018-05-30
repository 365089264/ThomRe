using System;
using System.Data;
using System.Text;
using System.Xml;
using CNE.Scheduler.CenterService;
using Exception = System.Exception;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class CusteelData : BaseDataHandle
    {
        public void SyncCusteelData(StringBuilder sbSync)
        {

            var auth = new authorization { username = "lutou", password = "lu123" };
            var dcmClient = new DataCenterMTOMVerifyServiceClient("DataCenterMTOMVerifyServiceImplPort");
            //dcmClient.register(auth);
            var custeelTargetInsertRows = SyncCusteelTarget(auth, dcmClient);
            sbSync.Append("Table CusteelTarget Insert Rows:" + custeelTargetInsertRows + "\n");

            var ds = new DataSet();
            using (var con = new OracleConnection(Connectionstr))
            {
                var cmd = new OracleCommand("SELECT code FROM CusteelTargets ", con);
                var da = new OracleDataAdapter(cmd);
                da.Fill(ds);
            }
            var count = ds.Tables[0].Rows.Count;
            int insertCount = 0;
            int updateCount = 0;
            for (int i = 0; i < count; i++)
            {
                var code = ds.Tables[0].Rows[i][0];
                var dtbytes = dcmClient.getDataSet(code.ToString(), auth);
                string str = Encoding.UTF8.GetString(dtbytes);
                var xml = new XmlDocument();
                xml.LoadXml(str);

                var tbXmlNode = xml.SelectSingleNode("//TABLE");
                var tbElement = (XmlElement)tbXmlNode;
                if (tbElement != null)
                {

                    var desetElement = (XmlElement)xml.SelectSingleNode("//DBSET");

                    if (desetElement != null)
                    {
                        var xnl = xml.SelectNodes("//ROW");
                        if (xnl != null)
                        {
                            using (var con = new OracleConnection(Connectionstr))
                            {
                                con.Open();
                                OracleCommand cmd = new OracleCommand("", con);
                                foreach (var value in xnl)
                                {
                                    var xe = (XmlElement)value;
                                    var xnls = xe.ChildNodes;
                                    var dbSetId = desetElement.GetAttribute("ID");
                                    var time = xnls[0].InnerText;
                                    var price = xnls[1].InnerText;
                                    string existStr = "select count(*) from CUSTEELDATA where code='" + code +
                                                      "' and TIME='" + time + "'";
                                    cmd.CommandText = existStr;
                                    int cmdresult = 0;
                                    try
                                    {

                                        object obj = cmd.ExecuteScalar();
                                        if (Equals(obj, null) || (Equals(obj, DBNull.Value)))
                                        {
                                        }
                                        else
                                        {
                                            cmdresult = int.Parse(obj.ToString());
                                        }
                                    }
                                    catch (OracleException e)
                                    {
                                        con.Close();
                                        throw new Exception(e.Message);
                                    }
                                    string operationSql;
                                    if (cmdresult == 0)
                                    {
                                        insertCount++;
                                        operationSql = "insert into CUSTEELDATA values('" + code + "','" + dbSetId +
                                                       "','" + time + "','" + price + "',sysdate)";
                                    }
                                    else
                                    {
                                        updateCount++;
                                        operationSql = "update CUSTEELDATA set dbSetId='" + dbSetId + "',price='" +
                                                       price + "',SYNCTIME=sysdate  where code='" + code +
                                                       "' and time='" + time + "'";
                                    }
                                    cmd.CommandText = operationSql;
                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (OracleException e)
                                    {
                                        con.Close();
                                        throw new Exception(e.Message);
                                    }

                                }
                                con.Close();
                            }
                        }


                    }
                }
            }
            sbSync.Append("Table CUSTEELDATA Insert Rows:" + insertCount + ",Update Rows:" + updateCount + " \n");
        }

        public int SyncCusteelTarget(authorization auth, DataCenterMTOMVerifyServiceClient dcmClient)
        {
            var bytes = dcmClient.getTargets(auth);
            string str = Encoding.UTF8.GetString(bytes);
            var xml = new XmlDocument();
            xml.LoadXml(str);
            var targetsNodes = xml.SelectNodes("//targets");
            int insertCount = 0;
            //int updateCount = 0;
            if (targetsNodes != null)
            {
                using (var con = new OracleConnection(Connectionstr))
                {
                    con.Open();
                    foreach (var targetsNode in targetsNodes)
                    {
                        var targetsName = ((XmlElement)targetsNode).GetAttribute("name");

                        var nodes = ((XmlElement)targetsNode).ChildNodes;
                        foreach (var node in nodes)
                        {
                            var targetName = ((XmlElement)node).GetAttribute("name");
                            var code = ((XmlElement)node).GetAttribute("code");
                            string existStr = "select count(*) from CUSTEELTARGETS where code='" + code + "'";
                            int cmdresult = 0;
                            using (OracleCommand cmd = new OracleCommand(existStr, con))
                            {
                                try
                                {

                                    object obj = cmd.ExecuteScalar();
                                    if ((Equals(obj, null)) || (Equals(obj, DBNull.Value)))
                                    {
                                    }
                                    else
                                    {
                                        cmdresult = int.Parse(obj.ToString());
                                    }
                                }
                                catch (OracleException e)
                                {
                                    con.Close();
                                    throw new Exception(e.Message);
                                }
                                string operationSql;
                                if (cmdresult == 0)
                                {
                                    insertCount++;
                                    operationSql = "insert into CUSTEELTARGETS values('" + targetsName + "','" +
                                                   targetName + "','" + code + "',sysdate)";
                                }
                                else
                                {
                                    continue;
                                    //updateCount++;
                                    //operationSql = "update CUSTEELTARGETS set targetsName='" + targetsName + "',targetName='" + targetName + "',SYNCTIME=sysdate  where code='" + code + "'";
                                }
                                cmd.CommandText = operationSql;
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch (OracleException e)
                                {
                                    con.Close();
                                    throw new Exception(e.Message);
                                }
                            }
                        }
                    }
                    con.Close();
                }

            }
            return insertCount;
        }
    }
}
