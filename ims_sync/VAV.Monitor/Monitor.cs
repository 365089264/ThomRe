using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace VAV.Monitor
{
    public class ServiceMonitor
    {
        /// <summary>
        /// Return first event in time.
        /// </summary>
        /// <param name="mintue"></param>
        /// <returns></returns>
        public static bool MonitorWindowsEventLog(int mintue, string[] machines, string rpcUsername, string rpcPassword, out string result)
        {
            result = string.Empty;
            StringBuilder debugTrace = new StringBuilder();
            debugTrace.AppendLine("$MonitorResult$");
            int totalErrorCount = 0;
            foreach (var machine in machines)
            {
                debugTrace.AppendLine("");
                string[] logs = new string[] { "Application", "System" };
                foreach (string log in logs)
                {
                    int errorCount = 0;
                    debugTrace.AppendLine(string.Format("Start monitoring {0}[{1}] from {2}", machine, log, Environment.MachineName));

                    if (string.Compare(machine, Environment.MachineName, true) == 0)
                    {
                        EventLog myLog = new EventLog();
                        myLog.Log = log;
                        List<string> errorList = new List<string>();
                        foreach (EventLogEntry entry in myLog.Entries)
                        {
                            //EventLogEntryType include：
                            //Error 
                            //FailureAudit 
                            //Information 
                            //SuccessAudit 
                            //Warning 
                            string temp = string.Empty;

                            if (entry.TimeGenerated.AddMinutes(mintue) > DateTime.Now &&
                                (entry.EntryType == EventLogEntryType.Error ||
                                 entry.EntryType == EventLogEntryType.Warning))
                            {
                                errorList.Add(machine + " |" + log + " |" +
                                         entry.EntryType.ToString() + " | " + entry.TimeWritten.ToString() + "\r\n"
                                         + entry.Message);
                                errorCount++;
                                totalErrorCount++;
                            }
                        }
                        if (errorList.Count > 0)
                        {
                            debugTrace.AppendLine(
                                string.Format("Found {0} errors on {1}[{2}]",
                                              errorList.Count, machine, log));
                            for (int i = 0; i < errorList.Count; i++)
                            {
                                debugTrace.AppendLine(string.Format("Error {0}: {1}", i + 1, errorList[i]));
                            }
                        }
                    }
                    else
                    {
                        string[] eventTypes = new string[] { "Warning", "Error" };
                        string eventLevel = "4";
                        foreach (var eventType in eventTypes)
                        {
                            switch (eventType)
                            {
                                case "Information":
                                    eventLevel = "4";
                                    break;
                                case "Warning":
                                    eventLevel = "3";
                                    break;
                                case "Error":
                                    eventLevel = "2";
                                    break;
                            }
                            String queryString = string.Format("*[System/Level={0}]", eventLevel);

                            SecureString pw = new SecureString();
                            for (int i = 0; i < rpcPassword.Length; i++)
                            {
                                pw.AppendChar(rpcPassword[i]);
                            }
                            pw.MakeReadOnly();
                            EventLogSession session = new EventLogSession(
                                machine, // Remote Computer Machine Name
                                "", // Domain
                                rpcUsername, // Username
                                pw,
                                SessionAuthentication.Default);
                            pw.Dispose();
                            // Query the Application log on the remote computer.
                            EventLogQuery query = new EventLogQuery(log, PathType.LogName, queryString);
                            query.Session = session;

                            try
                            {
                                EventLogReader logReader = new EventLogReader(query);
                                List<EventRecord> errorList = new List<EventRecord>();
                                for (EventRecord eventInstance = logReader.ReadEvent();
                                     null != eventInstance;
                                     eventInstance = logReader.ReadEvent())
                                {
                                    if (((DateTime)eventInstance.TimeCreated).AddMinutes(mintue) > DateTime.Now)
                                    //&& (eventInstance.LevelDisplayName == EventLogEntryType.Error || eventInstance.LevelDisplayName == EventLogEntryType.Warning))
                                    {
                                        errorList.Add(eventInstance);
                                        errorCount++;
                                        totalErrorCount++;
                                    }
                                }
                                if (errorList.Count > 0)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    debugTrace.AppendLine(
                                    string.Format("Found {0} errors on {1}[{2}]", errorList.Count, machine, log));
                                    for (int i = 0; i < errorList.Count; i++)
                                    {
                                        EventLogRecord logEntry = (EventLogRecord)errorList[i];
                                        debugTrace.AppendLine(string.Format("Error {0}: {1}", i + 1,
                                                                            Environment.MachineName + " |"
                                                                            + machine + " |"
                                                                            + log + " |"
                                                                            + logEntry.LevelDisplayName + " | "
                                                                            + logEntry.TimeCreated));
                                        debugTrace.AppendLine(errorList[i].FormatDescription());
                                        debugTrace.AppendLine(logEntry.ContainerLog);
                                    }
                                }
                                //else
                                //{
                                //    debugTrace.AppendLine(string.Format("No error found on {0}", machine));
                                //}
                            }
                            catch (Exception e)
                            {
                                result = string.Format("Exception {0}[{1}]: {2}", machine, log, e.Message);
                                return false;
                            }
                        }
                    }
                    if (errorCount == 0)
                    {
                        debugTrace.AppendLine(string.Format("No error found on machine {0}[{1}]",
                                                            machine,
                                                            log));
                    }
                }
            }
            if (totalErrorCount > 0)
            {
                debugTrace.Replace("$MonitorResult$", "Total count of error: " + totalErrorCount);
                result = debugTrace.ToString();
                return false;
            }
            else
            {
                //debugTrace.Replace("$MonitorResult$", "No error found on all servers");
                result = "No error found on all servers";//debugTrace.ToString();
                return true;
            }
        }

        /// <summary>
        /// "server=127.0.0.1;database=dbTest;user id=sa;pwd=sa")
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <returns></returns>
        public static bool MonitorSqlServerDb(string sqlstr, out string result)
        {
            bool blresult = true;
            result = string.Empty;
            using (SqlConnection conn = new SqlConnection(sqlstr))
            {
                try
                {
                    conn.Open();
                    blresult = true;
                }
                catch (SqlException ex)
                {

                    result = ex.InnerException == null ? ex.Message : ex.Message + ", " + ex.InnerException.Message;
                    blresult = false;
                }
                finally
                {
                    conn.Close();
                }
            }

            return blresult;
        }

        /// <summary>
        /// "Driver={Adaptive Server Enterprise};server=10.185.132.40;port=4074;uid=reporter;password=reporter;")
        /// </summary>
        /// <param name="conStr"></param>
        /// <returns></returns>
        public static bool MonitorSybaseDb(string conStr, out string result)
        {
            bool blresult = true;
            result = string.Empty;
            string temp = string.Empty;
            using (OdbcConnection conn = new OdbcConnection(conStr))
            {
                try
                {
                    conn.Open();
                    blresult = true;
                }
                catch (Exception ex)
                {

                    result = ex.InnerException == null ? ex.Message : ex.Message + ", " + ex.InnerException.Message;
                    blresult = false;
                }
                finally
                {
                    conn.Close();
                }
            }

            return blresult;
        }

        public static bool MonitorWebsiteOrWebService(string url, out string result)
        {
            result = string.Empty;
            HttpWebRequest Request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse Response = null;

            try
            {
                Response = Request.GetResponse() as HttpWebResponse;

                if (Response.StatusCode == HttpStatusCode.OK)
                    return true;
                else
                    return false;
            }
            catch (WebException ex)
            {
                result = ex.InnerException == null ? ex.Message : ex.Message + ", " + ex.InnerException.Message;
                return false;
            }
            finally
            {
                if (Response != null)
                    Response.Close();
            }
        }

        public static bool MonitorRmds(string ipAddress, int port, out string result)
        {
            result = string.Empty;

            try
            {
                TcpClient tcpSocket = new TcpClient(ipAddress, port);
                if (tcpSocket.Connected)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                result = ex.InnerException == null ? ex.Message : ex.Message + ", " + ex.InnerException.Message;
                return false;
            }
        }
    }
}
