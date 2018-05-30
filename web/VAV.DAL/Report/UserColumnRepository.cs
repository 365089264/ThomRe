using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAV.Entities;

namespace VAV.DAL.Report
{
    public class UserColumnRepository:BaseReportRepository
    {
        public List<USERCOLUMNSETTING>  GetUserColumns(string userID,int reportID)
        {
            using (var cmaDB = new CMAEntities())
            {
                return (from setting in cmaDB.USERCOLUMNSETTINGS where setting.USERID == userID && setting.REPORTID == reportID select setting).ToList();
            }
        }

        public void UpdateUserColumns(string userID, int reportID,string settingName,string columnList)
        {
            using (var cmaDB = new CMAEntities())
            {
                var userSetting = cmaDB.USERCOLUMNSETTINGS.FirstOrDefault(
                    x => x.USERID == userID && x.REPORTID == reportID && x.SETTINGNAME == settingName);
                if (userSetting == null)
                {
                    userSetting = new USERCOLUMNSETTING
                                      {
                                          USERID = userID,
                                          REPORTID = reportID,
                                          SETTINGNAME = settingName,
                                          COLUMNLIST = columnList
                                      };
                    cmaDB.USERCOLUMNSETTINGS.Add(userSetting);
                }
                else
                {
                    userSetting.COLUMNLIST = columnList;
                }
                cmaDB.SaveChanges();
            }
        }
    }
}
