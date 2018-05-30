using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aspose.Cells;
using System.Configuration;

namespace CNE.Scheduler.Extension
{
    public class BaseDataHandle
    {
        protected string FileName = "";

        protected string Connectionstr = "";

        public BaseDataHandle()
        {
            Connectionstr = ConfigurationManager.AppSettings["CnECon"];
        }

        public Cells GetCellsBySheetName(string sheetname)
        {
            var workbook = new Workbook(FileName); //工作簿 
            var ws = workbook.Worksheets[sheetname];

            if (null == ws)
                return null;

            return ws.Cells;
        }

        public void ModifyFile(string fileName, StringBuilder sb)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(fileName);

                string fileContent = reader.ReadToEnd();
                reader.Close();
                Regex reg = new Regex("<meta.+/> ", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
                if (reg.IsMatch(fileContent))
                {
                    string str = reg.Replace(fileContent, "");
                    File.WriteAllText(fileName, str, System.Text.Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                reader.Close();
                sb.AppendFormat("[{0} import failed,because:{1}]", fileName, e.Message);
            }
        }

    }
}
