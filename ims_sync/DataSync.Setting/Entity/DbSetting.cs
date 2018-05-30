using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Setting
{
    public class DbSetting
    {
        private string _type;

        public DbType Type 
        {
            get { return (DbType)Enum.Parse(typeof(DbType), _type.ToUpper()); } 
        }

        public string Conn { get; set; }

        public DbSetting(string type, string conn)
        {
            _type = type;
            Conn = conn;
        }
    }

    public enum DbType
    {
        MYSQL = 0,
        SQLSERVER,
        SYBASE,
        ORACLE
    }
}
