using System;
using System.Collections.Generic;
using System.Linq;
using VAV.DAL.Report;
using VAV.Entities;
using VAV.Model.Data;

namespace VAV.DAL.Services
{
    public class UserColumnService
    {
        private readonly UserColumnRepository _userColumnRepository;

        public UserColumnService(UserColumnRepository svc)
        {
            _userColumnRepository = svc;
        }

        public string[] GetUserColumnNames(string userId, int reportId)
        {
            return GetUserColumns(userId, reportId).Select(reportColumnDefinition => reportColumnDefinition.COLUMN_NAME).ToArray();
        }

        public List<UserColumn> GetAllColumnsWithUserSetting(string userId, int reportId, bool isEnglish)
        {
            var columns = new List<UserColumn>();
            var columnKeys = GetUserColumnNames(userId, reportId);
            IEnumerable<REPORTCOLUMNDEFINITION> defaultColumns = null;

            switch (reportId)
            {
                case 52:
                    defaultColumns = _userColumnRepository.GetColumnDefinitionByReportId(10052);
                    break;
                case 53:
                    defaultColumns = _userColumnRepository.GetColumnDefinitionByReportId(10053);
                    break;
                case 58:
                    defaultColumns = _userColumnRepository.GetColumnDefinitionByReportId(10058);
                    break;
                case 66:
                    defaultColumns = _userColumnRepository.GetColumnDefinitionByReportId(10066);
                    break;
                case 74:
                    defaultColumns = _userColumnRepository.GetColumnDefinitionByReportId(10059);
                    break;
                default:
                    defaultColumns = _userColumnRepository.GetColumnDefinitionByReportId(10000);
                    break;
            }

            foreach (var reportColumnDefinition in defaultColumns)
            {
                var column = new UserColumn
                                 {
                                     ID = reportColumnDefinition.COLUMN_NAME,
                                     Checked = Array.IndexOf(columnKeys, reportColumnDefinition.COLUMN_NAME) + 1,
                                     Text = isEnglish ? reportColumnDefinition.HEADER_TEXT_EN : reportColumnDefinition.HEADER_TEXT_CN
                                 };
                columns.Add(column);
            }

            return columns;
        }

        public void SaveUserColumns(string userId, int reportId, string setting)
        {
            _userColumnRepository.UpdateUserColumns(userId, reportId, "General", setting);
        }

        public IEnumerable<REPORTCOLUMNDEFINITION> GetUserColumns(string userId, int reportId)
        {
            var userSetting = _userColumnRepository.GetUserColumns(userId, reportId).FirstOrDefault();
            if (userSetting != null && !string.IsNullOrWhiteSpace(userSetting.COLUMNLIST))
            {
                switch (reportId)
                {
                    case 52:
                        return _userColumnRepository.GetColumnDefinition(10052, userSetting.COLUMNLIST.Split('|'));
                    case 53:
                        return _userColumnRepository.GetColumnDefinition(10053, userSetting.COLUMNLIST.Split('|'));
                    case 58:
                        return _userColumnRepository.GetColumnDefinition(10058, userSetting.COLUMNLIST.Split('|'));
                    case 66:
                        return _userColumnRepository.GetColumnDefinition(10066, userSetting.COLUMNLIST.Split('|'));
                    case 74:
                        return _userColumnRepository.GetColumnDefinition(10059, userSetting.COLUMNLIST.Split('|'));
                    default:
                        return _userColumnRepository.GetColumnDefinition(10000, userSetting.COLUMNLIST.Split('|'));
                }
            }
            return _userColumnRepository.GetColumnDefinitionByReportId(reportId);
        }

        public IEnumerable<COLUMNDEFINITION> GetGDTUserColumns(int itemId)
        {

            return _userColumnRepository.GetGDTColumnDefinition(itemId);
        }


    }
}
