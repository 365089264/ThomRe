using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VAV.DAL.Report;

namespace VAV.Test.RepositoryTests
{
    /// <summary>
    /// Summary description for BondInfoRepositoryTest
    /// </summary>
    [TestClass]
    public class BondInfoRepositoryTest
    {
        public BondInfoRepositoryTest()
        {
            //
            // TODO: Add constructor logic here
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
        public void TestGetDimSumBondSummary()
        {
            var start = new DateTime(2013, 1, 11);
            var end = new DateTime(2013, 5, 11);
            BondInfoRepository target = new BondInfoRepository();
            var actual = target.GetDimSumBondSummary(start, end, "Rating_Info", "100M","all","all");
            //var actual = target.GetDimSumBondSummary(bondList, start, end, "Issue_Country", "100M");
            foreach (var item in actual)
                Console.WriteLine(item.TypeName + ":" + item.Type);
            Assert.IsNotNull(actual);
        }
    }
}
