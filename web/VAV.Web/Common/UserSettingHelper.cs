﻿using System;
using System.Linq;
using System.Text;
using System.Web;
using VAV.Web.UserSetting;

namespace VAV.Web.Common
{
    public static class UserSettingHelper
    {
        public static string GetUserId(HttpRequestBase request)
        {
            var uuid = request.Headers["reutersuuid"];
            if (String.IsNullOrWhiteSpace(uuid))
            {
                uuid = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrWhiteSpace(uuid))
            {
                uuid = "Unknown";
            }
            return uuid;
        }

        public static string GetEikonUserID(HttpRequestBase request)
        {
            var uuid = request.Headers["reutersuuid"];
            if (String.IsNullOrWhiteSpace(uuid))
            {
                var clientIP = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.ServerVariables["REMOTE_ADDR"];
                return clientIP;
            }
            try
            {
                var userManagement = new AAAASUserManagementClient();
                var resp = userManagement.GetAllUserDetails_1(new GetAllUserDetails { uuid = uuid });
                var userID = resp.user.attributeMap.First(x => x.name.ToLower() == "UserId".ToLower()).value;
                return userID;
            }
            catch (Exception e)
            {
                return e.Message + e.StackTrace;
            }
        }

        public static UserAttributeType[] GetUserAttributeMap(HttpRequestBase request)
        {
            var uuid = request.Headers["reutersuuid"];
            if (!String.IsNullOrWhiteSpace(uuid))
            {
                try
                {
                    var userManagement = new AAAASUserManagementClient();
                    var resp = userManagement.GetAllUserDetails_1(new GetAllUserDetails { uuid = uuid });
                    return resp.user.attributeMap;
                }
                catch (Exception )
                {
                    return null;
                }
            }
            return null;
        }

        public static bool IsInternalUser(HttpRequestBase request)
        {
            var eikonUserId = GetEikonUserID(request).ToLower();
            if (string.IsNullOrWhiteSpace(eikonUserId)) return false;
            return eikonUserId.IndexOf("reuters") != -1;
        }

        public static string GetAllUserAttributes(HttpRequestBase request)
        {
            var uuid = request.Headers["reutersuuid"];
            if (string.IsNullOrWhiteSpace(uuid)) return "Unknow";
            try
            {
                var userManagement = new AAAASUserManagementClient();
                var resp = userManagement.GetAllUserDetails_1(new GetAllUserDetails { uuid = uuid });
                var sb = new StringBuilder();
                foreach (var userAttributeType in resp.user.attributeMap)
                {
                    sb.AppendFormat("key:{0} value:{1}|", userAttributeType.name, userAttributeType.value);
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return "Exception";
            }
        }
    }
}
