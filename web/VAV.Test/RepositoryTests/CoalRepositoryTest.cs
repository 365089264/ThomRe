using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VAV.DAL.CnE;
using VAV.Entities;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Linq.Dynamic;
using System.Data.SqlClient;
using System.Data;

namespace VAV.Test
{

    [TestClass]
    public class CoalRepositoryTest
    {
        public CoalRepositoryTest()
        {
            //
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetDataFiltersByReportID()
        {
            var rid = 10003;
            var res = new CoalRepository().GetDataFiltersByReportId(rid);

            var fom = "isprim:{0},txt:{1},val:{2},issel:{3}";

            if (res.PrimaryDropdown != null && res.PrimaryDropdown.Items != null)
            {
                foreach (var item in res.PrimaryDropdown.Items)
                {
                    Console.WriteLine(string.Format(fom, 1, item.Text, item.Value, item.Selected));
                }
            }

            if (res.SecondDropdown != null && res.SecondDropdown.Items != null)
            {
                foreach (var item in res.SecondDropdown.Items)
                {
                    Console.WriteLine(string.Format(fom, 0, item.Text, item.Value, item.Selected));
                }
            }

        }

        [TestMethod]
        public void TestGetPagedTableData()
        {
            //var rid = 10001;
            //var tbName = "VIEW_INDU_COAL_TYPE_PRI";
            //var sort = "code";
            //var where = "";
            //var pIndex = 1;
            //var count = 0;

            //var res = new CoalRepository().GetPagedTableData(rid, tbName, sort, where, pIndex, out count);
            //Console.WriteLine("totle count:" + count.ToString());
            //var str = "";
            //foreach (DataColumn item in res.Columns)
            //{
            //    str += item.ColumnName + "   | ";
            //}
            //Console.WriteLine(str);
            //foreach (DataRow dr in res.Rows)
            //{
            //    str = string.Join("   | ", dr.ItemArray);
            //    Console.WriteLine(str);
            //}
        }

        [TestMethod]
        public void TestGetCoalChartDataTable()
        {
            var rid = 10009;
            var kk = "AREA_UNI_CODE^|^'401200366'||MAC_IDX_PAR^|^'46'";
            var res = CoalRepository.GetCoalChartDataTable(rid, kk);
 
            var str = "";
            foreach (DataColumn item in res.Columns)
            {
                str += item.ColumnName + "   | ";
            }
            Console.WriteLine(str);
            foreach (DataRow dr in res.Rows)
            {
                str = string.Join("   | ", dr.ItemArray);
                Console.WriteLine(str);
            }
        }

    }
}
