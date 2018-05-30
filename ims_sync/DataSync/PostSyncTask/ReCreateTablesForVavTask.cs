using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luna.DataSync.Setting;
using MySql.Data.MySqlClient;

namespace Luna.DataSync.Core
{
    public class ReCreateTablesForVavTask: ITask
    {
        private ISettingManager _settingManager;

        public void Init(ISettingManager settingManager)
        {
            this._settingManager = settingManager;
        }

        public void Run()
        {
            if (_settingManager.DestinationDb.Type != DbType.MYSQL)
                throw new Exception("ReCreateTablesForVavTask should be run against MySQL DB.");

            using (MySqlConnection conn = new MySqlConnection(_settingManager.DestinationDb.Conn))
            {
                conn.Open();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("create table bondview_cn  select * from v_bondview_cn_1;");
                sb.AppendFormat("create table bondview_en  select * from v_bondview_en_1;");
                sb.AppendFormat("drop table v_bondview_cn;");
                sb.AppendFormat("drop table v_bondview_en;");
                sb.AppendFormat("alter table bondview_cn rename to v_bondview_cn;");
                sb.AppendFormat("alter table bondview_en   rename to v_bondview_en;");
                using (MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn))
                {
                    cmd.CommandTimeout = this._settingManager.SqlCommandTimeout;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
