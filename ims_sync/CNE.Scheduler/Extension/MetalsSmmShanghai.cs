using System;
using System.Data.SqlClient;
using Aspose.Cells;
using System.Data;

namespace CNE.Scheduler.Extension
{
    public class MetalsSmmShanghai : BaseDataHandle
    {
        public MetalsSmmShanghai(string filename)
        {
            FileName = filename;
        }

        public void ImportTheWholeExcel()
        {
            SyncFile.RegisterLicense();
            if (FileName.EndsWith("toThomsonReuters.xlsx"))
            {
                metals_smm_shanghai(1);
            }
            else if (FileName.EndsWith("路透.xlsx"))
            {
                metals_smm_shanghai(0);
            }
        }


        public void metals_smm_shanghai(int language)
        {
            Cells cells = GetCellsBySheetName("Sheet1");
            int rowscount = cells.Rows.Count;
            var currentdate = DateTime.Now;

            var datatable = new DataTable();
            datatable.Columns.Add("id");
            datatable.Columns.Add("productName");
            datatable.Columns.Add("unit");
            datatable.Columns.Add("specification");
            datatable.Columns.Add("grade");
            datatable.Columns.Add("brand");
            datatable.Columns.Add("locationOfSale");
            datatable.Columns.Add("locationOfProduction");
            datatable.Columns.Add("producer");
            datatable.Columns.Add("lowestPrice");
            datatable.Columns.Add("highestPrice");
            datatable.Columns.Add("updateDate");
            datatable.Columns.Add("updateDateTime");
            datatable.Columns.Add("language");

            for (int i = 1; i < rowscount; i++)
            {
                DataRow datarow = datatable.NewRow();
                datarow["productName"] = cells[i, 0].Value;
                datarow["unit"] = cells[i, 1].Value;
                datarow["specification"] = cells[i, 2].Value;
                datarow["grade"] = cells[i, 3].Value;
                datarow["brand"] = cells[i, 4].Value;
                datarow["locationOfSale"] = cells[i, 5].Value;
                datarow["locationOfProduction"] = cells[i, 6].Value;
                datarow["producer"] = cells[i, 7].Value;
                datarow["lowestPrice"] = cells[i, 8].Value;
                datarow["highestPrice"] = cells[i, 9].Value;
                datarow["updateDate"] = cells[i, 10].Value;
                datarow["updateDateTime"] = currentdate;
                datarow["language"] = language;

                datatable.Rows.Add(datarow);
            }

            using (var sqlbulkcopy = new SqlBulkCopy(Connectionstr, SqlBulkCopyOptions.UseInternalTransaction) { DestinationTableName = "[CnE].[cne].[metals_smm_shanghai]" })
            {
                sqlbulkcopy.WriteToServer(datatable);
                sqlbulkcopy.Close();
            }
        }
    }
}
