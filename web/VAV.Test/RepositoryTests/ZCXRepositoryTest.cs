using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VAV.DAL.Fundamental;
using VAV.Entities;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Linq.Dynamic;
using System.Data.SqlClient;

namespace VAV.Test
{
    /// <summary>
    /// Summary description for ZCXRepositoryTest
    /// </summary>
    [TestClass]
    public class ZCXRepositoryTest
    {
        public ZCXRepositoryTest()
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
        public void TestGetCompanyType()
        {
            ZCXRepository target = new ZCXRepository();
            var actual = target.GetCompanyType();
            foreach (var item in actual)
                Console.WriteLine(item.PAR_NAME);
            Assert.AreEqual(15, actual.Count());
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetNonlistIssuer()
        {
            ZCXRepository target = new ZCXRepository();
            int total;
            var actual = target.GetNonlistIssuer(1799, "", "", false, "", 1, 50, out total);
            //foreach (var item in actual)
            //{
            //    Console.WriteLine("{0}\t{1}\t{2}\t", item.COM_UNI_CODE, item.COM_CHI_NAME, item.TYPE_BIG);
            //}
            Console.WriteLine("Total:{0}", total);
            //Console.WriteLine(actual.Count());
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetBondListByComCode()
        {
            ZCXRepository target = new ZCXRepository();
            var actual = target.GetBondListByComCode(200035802);
            foreach (var item in actual)
            {
                Console.WriteLine(item.BondName + ":" + item.IssueDate);
            }
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetIsserByComCode()
        {
            ZCXRepository target = new ZCXRepository();
            var actual = target.GetIsserByComCode(200035802);
            Console.WriteLine(actual.COM_ORGA_FORM);
            Assert.IsNotNull(actual);
        }
        private class ChnCmp : IComparer<RATE_ORG_INFO_F>
        {
            private string prop;
            private bool isasc;
            public ChnCmp(string p, string order)
            {
                prop = p;
                isasc = order == "ASC";
            }
            public int Compare(RATE_ORG_INFO_F x, RATE_ORG_INFO_F y)
            {//x.GetType().
                var s1 = x.GetType().GetProperty(prop).GetValue(x,null).ToString();
                var s2 = y.GetType().GetProperty(prop).GetValue(y, null).ToString();
                if(isasc)
                    return string.Compare(s1, s2, new CultureInfo("zh-CN"), CompareOptions.IgnoreCase);
                else
                    return string.Compare(s1, s2, new CultureInfo("zh-CN"), CompareOptions.IgnoreCase);
            }
        }
        public void test()
        {
            using (var ZCXDB = new ZCXEntities())
            {
                var query = ZCXDB.RATE_ORG_INFO_F.Select("new (COM_CHI_NAME, COM_UNI_CODE)").Take(10);
                Console.WriteLine(query);
                foreach (var item in query)
                    Console.WriteLine(item.ToString());
            }
        }



        public void TestWithData()
        {
            using (var ZCXDB = new ZCXEntities())
            {
                int parCode = 0;
                var q1 = from info in ZCXDB.RATE_ORG_INFO_F
                         where (info.TYPE_BIG_PAR.Value == parCode || parCode == 0)
                         && info.ISVALID.Value == 1
                         select info;
                Console.WriteLine(q1.Count()); 
            }
        }


        public void TestGetIssuerRating()
        {
            var target = new ZCXRepository();
            var actual = target.GetIssuerRating("200000008");
            foreach (var item in actual)
            {
                Console.WriteLine(new JavaScriptSerializer().Serialize(item));
            }
            Assert.AreEqual(actual.Count,5);
        }


        public void TestGetComCodeFromBondCode()
        {
            var target = new ZCXRepository();
            var actual = target.GetComCodeFromBondCode("130014");
            Console.WriteLine(actual);
        }

    }
}
