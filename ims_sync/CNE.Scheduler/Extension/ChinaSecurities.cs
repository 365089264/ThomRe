using System;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Oracle.ManagedDataAccess.Client;

namespace CNE.Scheduler.Extension
{
    public class ChinaSecurities
    {
        private readonly string _connectionstr = ConfigurationManager.AppSettings["VAVConnStr"];
        readonly string _tenUrl = ConfigurationManager.AppSettings["ChinaSecuritiesTenUrl"];
        readonly string _noticeUrl = ConfigurationManager.AppSettings["ChinaSecuritiesNoticeUrl"];
        private int _numInsert;
        public void SyncData(StringBuilder sbSync)
        {
            sbSync.AppendFormat("Source [Type: {0} , TenUrl: {1}] ,NoticeUrl {2} \n", "XML",
                                _tenUrl, _noticeUrl);
            sbSync.AppendFormat("Destination [Type: {0} Address: {1}]\n", "Oracle",
                                 _connectionstr);
            int tenMaxId = 0;
            using (var con = new OracleConnection(_connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand("SELECT MAX(id) FROM ChinaSecuritiesTen", con);
                object objTemp = cmd.ExecuteScalar();

                if (objTemp != DBNull.Value)
                {
                    tenMaxId = Convert.ToInt32(objTemp);
                }
                con.Close();
            }
            StringBuilder sb=new StringBuilder();
            InsertChinaSecuritiesTable(tenMaxId, "ChinaSecuritiesTen", _tenUrl, sb);
            sb.Append("</ol></p>");
            if (_numInsert == 0)
            {
                sbSync.Append("Table ChinaSecuritiesTen no data insert\r\n");
            }
            else
            {
                sbSync.Append("<p>Table :ChinaSecuritiesTen insert rows:" + _numInsert + "<ol>" + sb);
            }
            _numInsert = 0;
            int noticeMaxId = 0;
            using (var con = new OracleConnection(_connectionstr))
            {
                con.Open();
                var cmd = new OracleCommand("SELECT MAX(id) FROM ChinaSecuritiesNotice", con);
                object objTemp = cmd.ExecuteScalar();

                if (objTemp != DBNull.Value)
                {
                    noticeMaxId = Convert.ToInt32(objTemp);
                }
                con.Close();
            }
            sb = new StringBuilder();
            InsertChinaSecuritiesTable(noticeMaxId, "ChinaSecuritiesNotice", _noticeUrl, sb);
            sb.Append("</ol></p>");
            if (_numInsert == 0)
            {
                sbSync.Append("Table ChinaSecuritiesNotice no data insert\r\n");
            }
            else
            {
                sbSync.Append("<p>Table :ChinaSecuritiesNotice insert rows:" + _numInsert + "<ol>" + sb);
            }
            
        }

        private void InsertChinaSecuritiesTable(int maxId, string destinationTable, string xmlUrl, StringBuilder sbSync)
        {
            
            var isOver = false;
            var os = XmlExtension.LoadFromXml(xmlUrl, typeof(oschina)) as oschina;

            if (os == null) return;
            using (var con = new OracleConnection(_connectionstr))
            {
                con.Open();
                foreach (var r in os.rowslist)
                {
                    if (r.id == 0) continue;
                    if (r.id <= maxId)
                    {
                        isOver = true;
                        break;
                    }
                    sbSync.Append("<li>id:" + r.id + " insert");
                    _numInsert++;
                    var operationSql = "insert into " + destinationTable + " values(:ID,:TITLE,:URL,to_date('" + r.published + "','yyyy/mm/dd hh24:mi:ss'),:ORGCODE,sysdate)";
                    var cmd = new OracleCommand(operationSql, con);
                    cmd.Parameters.Add(new OracleParameter(":ID", OracleDbType.Int32) { Value = r.id });
                    cmd.Parameters.Add(new OracleParameter(":TITLE", OracleDbType.NVarchar2) { Value = r.title });
                    cmd.Parameters.Add(new OracleParameter(":URL", OracleDbType.NVarchar2) { Value = r.url });
                    cmd.Parameters.Add(new OracleParameter(":ORGCODE", OracleDbType.Varchar2) { Value = r.orgcode });

                    try
                    {
                        cmd.ExecuteNonQuery();
                        sbSync.Append("<span style=\"color:green;\">success!</span></li>");

                    }
                    catch (OracleException e)
                    {
                        con.Close();
                        throw new Exception(e.Message);
                    }
                }

                con.Close();
            }



            if (!isOver && os.currentpage != os.totalpage)
            {
                xmlUrl = xmlUrl.Replace(os.currentpage + ".xml", os.currentpage + 1 + ".xml");
                InsertChinaSecuritiesTable(maxId, destinationTable, xmlUrl,sbSync);
            }
        }

    }
    public class XmlExtension
    {
        public static object LoadFromXml(string filePath, Type type)
        {
            object result;
            try
            {
                var reader = new XmlTextReader(filePath);
                var xmlSerializer = new XmlSerializer(type);
                result = xmlSerializer.Deserialize(reader);
            }
            catch (Exception ee)
            {
                result = null;
            }
            return result;
        }
    }
    [XmlRoot("oschina", IsNullable = false)]
    public class oschina
    {
        [XmlElementAttribute("table")]
        public string table
        {
            get;
            set;
        }

        [XmlElement("condition")]
        public string condition
        {
            get;
            set;
        }

        [XmlElement("totalrecord")]
        public int totalrecord
        {
            get;
            set;
        }
        [XmlElement("pagesize")]
        public int pagesize
        {
            get;
            set;
        }
        [XmlElement("totalpage")]
        public int totalpage
        {
            get;
            set;
        }
        [XmlElement("currentpage")]
        public int currentpage
        {
            get;
            set;
        }

        [XmlArray("rowslist")]
        public row[] rowslist
        {
            get;
            set;
        }
    }
    [XmlRoot("row")]
    public class row
    {

        [XmlElement("id")]
        public int id
        {
            get;
            set;
        }

        [XmlElement("title")]
        public string title
        {
            get;
            set;
        }

        [XmlElement("url")]
        public string url
        {
            get;
            set;
        }
        [XmlElement("stract")]
        public string stract
        {
            get;
            set;
        }
        [XmlElement("published")]
        public string published
        {
            get;
            set;
        }
        [XmlElement("orgcode")]
        public string orgcode
        {
            get;
            set;
        }
    }
}
