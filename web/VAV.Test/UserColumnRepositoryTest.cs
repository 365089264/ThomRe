using VAV.DAL.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VAV.Entities;
using System.Collections.Generic;

namespace VAV.Test
{


    [TestClass()]
    public class UserColumnRepositoryTest
    {


        private TestContext testContextInstance;

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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void GetUserColumnsTest()
        {
            UserColumnRepository target = new UserColumnRepository(); 
            string uuid = string.Empty;
            int reportID = 0; 
            List<USERCOLUMNSETTING> actual;
            actual = target.GetUserColumns(uuid, reportID);
            Assert.IsNotNull(actual);
        }


        [TestMethod()]
        public void UpdateUserColumnsTest()
        {
            UserColumnRepository target = new UserColumnRepository(); 
            string userID ="jimmy"; 
            int reportID = 123;
            string settingName = "Initial Test"; 
            string columnList = "xxx2"; 
            target.UpdateUserColumns(userID, reportID, settingName, columnList);
            var jimmyList = target.GetUserColumns(userID, reportID);
            Assert.AreEqual(jimmyList.Count,1);
        }
    }
}
