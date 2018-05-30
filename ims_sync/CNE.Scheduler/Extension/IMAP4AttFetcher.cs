using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActiveUp.Net.Imap4;
using ActiveUp.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using CNE.Scheduler.Extension.Model;

namespace CNE.Scheduler.Extension
{
    public class IMAP4AttFetcher
    {
        /*The main logic of this tool.*/
        /*
         * imap4client is used to connect the server,login,getting mails.
         */
        private Imap4Client _imap4Client = null;
        private string _savePath = "./";
        private string _server = null;
        private int _port = 0;
        private string _userName = null;
        private string _passWord = null;
        private bool _usingSsl = false;
        private List<EmailDetail> emailDetailList = new List<EmailDetail>();
        //private Dictionary<string, string> fetchEmailTime = new Dictionary<string, string>();
        //private Dictionary<string, string> emailList = new Dictionary<string, string>();
        /*The list of attachments that have been got successful.*/
        private List<string> attachmentNames = new List<string>();
        /*According to the requirement of YUANZHONG, write the filters to judge whether the 
         * mail is the mail that we care.
         * Each filter is a BaseFilter type , run the .filter() function it will return a 
         * bool value specifies whether the mail is the MAIL.
         */
        private List<BaseFilter> filters = new List<BaseFilter>();
        /*SyncTime field specifies when we finished receiving mails.*/
        private DateTime _syncTime = DateTime.Now;
        public List<EmailDetail> GetEmailDetailList()
        {
            return emailDetailList;
        }
        /*Constructors*/
        public IMAP4AttFetcher()
        {
            /*Default Constructor*/

        }
        public IMAP4AttFetcher(string server, int port, bool usingSsl, string userName, string passWord, string savePath)
        {
            _server = server;
            _port = port;
            _usingSsl = usingSsl;
            _userName = userName;
            _passWord = passWord;
            _savePath = savePath;

            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }
        public static void execute()
        {
            /*
             * Here is an example of how to use this class.The exectue() function is only an example for 
             * using this tool.The code of this function will not be used.
             */
            IMAP4AttFetcher fet = new IMAP4AttFetcher();/*Do not forget to input the parameters.*/
            fet.ConnectServer("", 0, true, "un", "pwd");
            SenderFilter sf = new SenderFilter(@"chao.luo@thomsonreuters.com");/*using string as rule , judge by sender*/
            fet.AppendFilter(sf);
            TitleFilter tf = new TitleFilter(@"hello thomsonreuters");/*using string as rule , judge by title*/
            fet.Update(DateTime.Now);
            fet.DisconnectServer();


        }
        public void Execute(DateTime lastSyncTime)
        {
            ConnectServer(_server, _port, _usingSsl, _userName, _passWord);
            Update(lastSyncTime);
            DisconnectServer();
        }



        /*
         * ConnectServer() is used to connect imap server using a hostname, 
         * port , username and password.
         */
        protected bool ConnectServer(string serverName, int port, bool usingSsl, string userName, string passWord)
        {
            string returnMessage = null;
            _imap4Client = new Imap4Client();
            if (usingSsl)
            {
                /*using SSL to connect the server*/
                returnMessage = _imap4Client.ConnectSsl(serverName, port);
                DebugMessage(returnMessage);
            }
            else
            {
                returnMessage = _imap4Client.Connect(serverName, port);
                DebugMessage(returnMessage);
            }
            returnMessage = _imap4Client.Login(userName, passWord);
            DebugMessage(returnMessage);
            /*the _imap4Client object should be in the STATUS of already login.*/

            return true;
        }
        /*
         * DisconnectServer() is used to disconnect the connected server.
         */
        protected int DisconnectServer()
        {
            /*Reserved , do nothing.*/
            _imap4Client.Disconnect();
            return 0;
        }
        /*
         *  As the function name this function is used to replace invalid chars in the "fileName" parameter.
         */
        protected string ReplaceInvalidFilenameChars(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;

        }
        protected int GetAttachmentFromMail(string SavePath, ActiveUp.Net.Mail.Message mail)
        {
            foreach (MimePart att in mail.Attachments)
            {
                string fileSavePath = Path.Combine(SavePath, (mail.From.Email + ConvertUniversalTime(mail) + "$" + ReplaceInvalidFilenameChars(att.Filename)).Replace("/", "").Replace(":", "").Replace(" ", ""));
                attachmentNames.Add(fileSavePath);
                File.WriteAllBytes(fileSavePath, att.BinaryContent);
            }
            return 1;
        }

        protected List<string> GetAttachmentFileNameFromMail(string SavePath, ActiveUp.Net.Mail.Message mail)
        {
            List<string> attachNames = new List<string>();
            foreach (MimePart att in mail.Attachments)
            {
                string fileSavePath = Path.Combine(SavePath, (mail.From.Email + ConvertUniversalTime(mail) + "$" + ReplaceInvalidFilenameChars(att.Filename)).Replace("/", "").Replace(":", "").Replace(" ", ""));
                attachNames.Add(fileSavePath);

            }
            return attachNames;
        }
        /*
         * Process one time update including successful and failed process.
         */
        public DateTime Update(DateTime lastSyncDate)
        {
            UpdateMailFromBox("inbox", lastSyncDate);
            UpdateMailFromBox("Spam Mail", lastSyncDate);
            _syncTime = DateTime.UtcNow;
            DebugMessage("Done");
            return _syncTime;
        }

        private string ConvertUniversalTime(ActiveUp.Net.Mail.Message mail)
        {
            DateTime dtOfThisMail = DateTime.Parse(mail.HeaderFields["date"]);
            dtOfThisMail = DateTime.SpecifyKind(dtOfThisMail, DateTimeKind.Local);
            dtOfThisMail = dtOfThisMail.ToUniversalTime();
            return dtOfThisMail.ToString();
        }
        private void UpdateMailFromBox(string box, DateTime lastSyncDate)
        {
            Mailbox inbox = _imap4Client.SelectMailbox(box);
            string strDate = lastSyncDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
            ActiveUp.Net.Mail.Message mail = null;
            int[] ids = inbox.Search("SENTSINCE " + strDate);
            if (ids.Length > 0)
            {
                TimeFilter timeFilter = new TimeFilter();
                timeFilter.SetRule(lastSyncDate);
                this.AppendFilter(timeFilter);
                for (int l = ids.Length - 1; l >= 0; l--)
                {

                    mail = inbox.Fetch.MessageObject(ids[l]);
                    if (IsCorrectMail(mail))
                    {
                        EmailDetail email = new EmailDetail(ids[l], ConvertUniversalTime(mail), mail.Subject, mail.From.Email, box);
                        email.AttachNames.AddRange(GetAttachmentFileNameFromMail(_savePath, mail));
                        emailDetailList.Add(email);
                        GetAttachmentFromMail(_savePath, mail);
                    }
                }
            }
        }
        public void DeleteEmailByID(int id, string  boxName)
        {

            Mailbox inbox = _imap4Client.SelectMailbox(boxName);
            inbox.DeleteMessage(id, true);
           

        }
        public void OpenConnection()
        {
            ConnectServer(_server, _port, _usingSsl, _userName, _passWord);
        }
        public void CloseConnection()
        {
            DisconnectServer();
        }
        protected bool IsCorrectMail(ActiveUp.Net.Mail.Message mail)
        {
            bool bResult = true;
            if (filters.Count == 0)
            {
                DebugMessage("No filter here.");
                bResult = false;
            }
            else
            {
                foreach (BaseFilter filter in filters)
                {
                    bResult = bResult && filter.Filter(mail);
                }
            }
            return bResult;
        }
        /*After the Execute() function User can load the GetAttachmentFileNames() to get the list of
          names of attachments
         */
        public List<string> GetAttachmentFileNames()
        {
            return attachmentNames;
        }
        /*appendfilter(), used to append the new , initialized filter to the filter list.*/
        public void AppendFilter(BaseFilter filter)
        {
            filters.Add(filter);
        }
        /*GetSyncTimestamp() returns the timestamp this time we receive mails.
          It should be loaded after the Update() function.        
         */
        public DateTime GetSyncTime()
        {
            return _syncTime;
        }

        /*Main logic ended.*/
        /*Debug messages*/
        private bool _debugSwitch = false;
        protected void DebugMessage(string debugMsg)
        {
            if (_debugSwitch)
            {
                Console.WriteLine(debugMsg);
            }
        }

    }
    /*The filters*/
    public abstract class BaseFilter
    {
        protected Regex _rule = null;
        protected List<Regex> _rules = new List<Regex>();
        public void AppendRule(Regex rule)
        {
            _rules.Add(rule);
        }
        public void AppendRule(string rule)
        {
            Regex regexRule = new Regex(rule, RegexOptions.Compiled);
            _rules.Add(regexRule);
        }
        public abstract bool Filter(ActiveUp.Net.Mail.Message mail);
        public virtual void SetRule(Regex rule)
        {
            _rule = rule;
            AppendRule(_rule);
        }
        public virtual void SetRule(string rule)
        {
            Regex regexRule = new Regex(rule, RegexOptions.Compiled);
            _rule = regexRule;
            AppendRule(_rule);
        }
    }
    /*Senderfilter, judge whether the mail is we need by sender*/
    public class SenderFilter : BaseFilter
    {
        /*Default constuctor*/
        public SenderFilter()
        {
        }
        /*constructor using regex*/
        public SenderFilter(Regex rule)
        {
            base.SetRule(rule);

        }
        /*constructor using string , load the base class function*/
        public SenderFilter(string rule)
        {
            base.SetRule(rule);

        }

        /**/
        public override bool Filter(Message mail)
        {
            string strSender = null;
            bool bSuccess = false;
            strSender = mail.HeaderFields["from"];
            foreach (Regex r in _rules)
            {
                Match m = r.Match(strSender);
                bSuccess = bSuccess || m.Success;
            }
            //Match m = _rule.Match(strSender);
            return bSuccess;

        }

    }
    /*Judge whether the mail is we need by title AKA subject*/
    public class TitleFilter : BaseFilter
    {
        public TitleFilter()
        {

        }
        /*constructure using regex , must*/
        public TitleFilter(Regex rule)
        {
            base.SetRule(rule);
        }
        /*constructor using string , load the base class function*/
        public TitleFilter(string rule)
        {
            base.SetRule(rule);
        }
        public override bool Filter(Message mail)
        {
            string strTitle = null;
            strTitle = mail.HeaderFields["subject"];
            bool bSuccess = false;
            foreach (Regex r in _rules)
            {
                Match m = r.Match(strTitle);
                bSuccess = bSuccess || m.Success;
            }
            return bSuccess;
        }
    }
    public class TimeFilter : BaseFilter
    {
        DateTime _dtrule;
        public TimeFilter()
        {

        }
        public TimeFilter(DateTime dtRule)
        {
            _dtrule = dtRule;
        }
        public void SetRule(DateTime dtRule)
        {
            _dtrule = dtRule;
        }

        public override bool Filter(Message mail)
        {
            /*Get the timestamp from mail.
             * Attention: the Parse() returns a local time area time 
             * after Parse() load ToUniversalTime() to change the time to 
             * UTC.
             */
            DateTime dtOfThisMail = DateTime.Parse(mail.HeaderFields["date"]);
            dtOfThisMail = DateTime.SpecifyKind(dtOfThisMail, DateTimeKind.Local);
            dtOfThisMail = dtOfThisMail.ToUniversalTime();
            bool bRetval = false;
            /*Compare the two datetime structure , greater than 0 and equal to 0 means that current mail is newer
             *than the timestamp, less than 0 means this mail is older than the timestamp.
             */
            int iResult = DateTime.Compare(dtOfThisMail, _dtrule);
            if (iResult >= 0)
            {
                /*Newer condition*/
                bRetval = true;
            }
            else
            {
                bRetval = false;
            }
            return bRetval;
        }
    }
}
