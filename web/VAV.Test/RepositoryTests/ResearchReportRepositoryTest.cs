using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VAV.DAL.ResearchReport;

namespace VAV.Test.RepositoryTests
{
    /// <summary>
    /// Summary description for ResearchReportRepositoryTest
    /// </summary>
    [TestClass]
    public class ResearchReportRepositoryTest
    {
        public ResearchReportRepositoryTest()
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
        public void TestGetInstitutions()
        {
            ResearchReportRepository target = new ResearchReportRepository();
            var actual = target.GetInstitutions().OrderBy(i => i.INSTITUTIONORDER);
            foreach (var item in actual)
            {
                Console.WriteLine("Order:{0,2}\tId:{1,2}\tNameCn:{2}", item.INSTITUTIONORDER, item.ID_C, item.DisplayName);
                foreach (var ftype in item.FILEDETAILs.GroupBy(f => f.FILETYPEINFO).OrderBy(x => x.Key.TYPEORDER).Select(g => new { FileType = g.Key, FileDetails = g }))
                {
                    Console.WriteLine(ftype.FileType.DisplayName);
                    foreach (var file in ftype.FileDetails)
                        Console.WriteLine(file.DisplayName);
                }
            }
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetFileDataById()
        {
            ResearchReportRepository target = new ResearchReportRepository();
            var actual = target.GetFileDataById(12);

            
            
            Assert.IsNotNull(actual);
        }

    }
}
