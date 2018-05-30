using System.Linq;
using System.Data;
using Aspose.Cells;
using System.IO;

namespace CNE.Scheduler.Extension
{
    public class SyncFile
    {
        public void UpLoadFileToFtp(DataTable dt, string sheetName, string[] headerData, string[] rowPointers, string filename, string ftpFilePath)
        {

            var uri = ftpFilePath + filename;
            var fs = new FileStream(uri, FileMode.Create);
            RegisterLicense();
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            worksheet.Name = sheetName;
            ExcelUtil.CreateWorksheet("", worksheet, worksheet.Name, dt.AsEnumerable().AsQueryable(), headerData, rowPointers);
            workbook.Save(fs, SaveFormat.Xlsx);
            fs.Close();

        }

        public static void RegisterLicense()
        {
            var license = new License();
            var commonAssembly = System.Reflection.Assembly.Load("VAV.Common");
            var s = commonAssembly.GetManifestResourceStream("VAV.Common.Aspose.Cells.lic");
            license.SetLicense(s);
        }
    }
}
