using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VAV.Entities;
using VAV.Model.Data;

namespace VAV.Test.RepositoryTests
{
    [TestClass]
    public class StructureReportRepositoryTest
    {
        string culture = "zh-CN";
        int id = 30033;
        string tableName = "cfets_buyoutRepo_instiCatBal";
        [TestMethod]
        public void TestMethod1()
        {
            //cfets_FOGCUYSpRep_FORGCURYSpot
            //301000


            using (var srdb = new SRDBEntities())
            {
                var tableConfig = srdb.TABLE_CONFIG.FirstOrDefault(x => x.TABLENAMEEN == tableName);
                
                    Console.WriteLine(new JavaScriptSerializer().Serialize(tableConfig));
                var columnsCn = tableConfig.FIELDNAMESARRCN.Split(';');
                var columnsEn = tableConfig.FIELDNAMESARREN.Split(';');
                var columnsTypes = tableConfig.FIELDTYPESARR.Split(';');
                Console.WriteLine("{0} {1} {2}",columnsCn.Length,columnsEn.Length,columnsTypes.Length);
            }
        }

        [TestMethod]
        public void TestGetReportColumns()
        {
            var columnList = new List<Column>();
            using (var srdb = new SRDBEntities())
            {
                var tableConfig = srdb.TABLE_CONFIG.FirstOrDefault(x => x.TABLENAMEEN == tableName);
                var columnsCn = tableConfig.FIELDNAMESARRCN.Split(';');
                var columnsEn = tableConfig.FIELDNAMESARREN.Split(';');
                var columnsTypes = tableConfig.FIELDTYPESARR.Split(';');
                if (columnsCn.Length == columnsEn.Length && columnsCn.Length == columnsTypes.Length)
                {
                    for (int i = 0; i < columnsCn.Length; i++)
                    {
                        var column = new Column()
                        {
                            ColumnName = columnsEn[i],
                            ColumnHeaderCN = columnsCn[i],
                            ColumnHeaderEN = columnsEn[i],
                            ColumnType = columnsTypes[i]
                        };
                        columnList.Add(column);
                    }
                }
                foreach (var column in columnList)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(column));
                }
            }
        }

        [TestMethod]
        public void TestGetExtraHeaderById()
        {
            
        }

        [TestMethod]
        public void TestGetReportById()
        {
            
        }

        [TestMethod]
        public void insertReportColumnDefinition()
        {
            var newcolumn = new REPORTCOLUMNDEFINITION()
            {
                
            };
            using (var cmadb = new CMAEntities())
            {
                var reportList = cmadb.REPORTDEFINITIONs.Where(r => r.ID > 30000).ToList();
                var reportColumnDefinitions = new List<REPORTCOLUMNDEFINITION>();
                foreach (var reportdefinition in reportList)
                {
                    using (var srdb = new SRDBEntities())
                    {
                        var tableConfig = srdb.TABLE_CONFIG.FirstOrDefault(x => x.TABLENAMEEN.ToLower() == reportdefinition.TABLE_NAME.ToLower());
                        var columnsCn = tableConfig.FIELDNAMESARRCN.Split(';');
                        var columnsEn = tableConfig.FIELDNAMESARREN.Split(';');
                        var columnsTypes = tableConfig.FIELDTYPESARR.Split(';');
                        if (columnsCn.Length == columnsEn.Length && columnsCn.Length == columnsTypes.Length)
                        {
                            for (int i = 0; i < columnsCn.Length; i++)
                            {
                                if (string.IsNullOrEmpty(columnsCn[i]))
                                {
                                    continue;
                                }
                                var column = new Column()
                                {
                                    ColumnName = columnsEn[i],
                                    ColumnHeaderCN = columnsCn[i],
                                    ColumnHeaderEN = columnsEn[i],
                                    ColumnType = columnsTypes[i]
                                };
                                var reportColumnDefinition = new REPORTCOLUMNDEFINITION();
                                reportColumnDefinition.COLUMN_INDEX = i;
                                reportColumnDefinition.COLUMN_NAME = columnsEn[i];
                                reportColumnDefinition.COLUMN_TYPE = columnsTypes[i] == "number" ? "decimal" : null;
                                reportColumnDefinition.HEADER_TEXT_CN = columnsCn[i];
                                reportColumnDefinition.HEADER_TEXT_EN = columnsEn[i];
                                reportColumnDefinition.REPORT_ID = reportdefinition.ID;
                                reportColumnDefinitions.Add(reportColumnDefinition);

                                //System.InvalidOperationException: 表/视图没有定义主键。实体为只读
                                //cmadb.REPORTCOLUMNDEFINITIONs.Add(reportColumnDefinition);
                            }
                        }
                    }
                    //Console.WriteLine(cmadb.SaveChanges());
                }

                foreach (var reportcolumndefinition in reportColumnDefinitions)
                {
                    //Console.WriteLine(JsonConvert.SerializeObject(reportcolumndefinition));
                    Console.WriteLine("INSERT INTO REPORTCOLUMNDEFINITION (REPORT_ID,COLUMN_INDEX,COLUMN_NAME,COLUMN_TYPE,HEADER_TEXT_CN,HEADER_TEXT_EN) " +
                                      "VALUES({0},{1},'{2}','{3}',N'{4}','{5}');",
                                      reportcolumndefinition.REPORT_ID, reportcolumndefinition.COLUMN_INDEX, reportcolumndefinition.COLUMN_NAME, reportcolumndefinition.COLUMN_TYPE, reportcolumndefinition.HEADER_TEXT_CN, reportcolumndefinition.HEADER_TEXT_EN);
                }


            }
        }


        [TestMethod]
        public void TestGetColumn()
        {
            using (var cmadb = new CMAEntities())
            {
                var columns = cmadb.REPORTCOLUMNDEFINITIONs.ToList();
                Console.WriteLine(columns.Count(x => x.REPORT_ID == 300100));
                foreach (var column in columns)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(column));
                }
            }
        }

    }
}
