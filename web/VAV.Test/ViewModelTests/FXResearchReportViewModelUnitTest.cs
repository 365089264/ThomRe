using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VAV.Web.ViewModels.ResearchReport;
using System.Runtime.CompilerServices;
using VAV.DAL.ResearchReport;
using VAV.Entities;


namespace VAV.Test.ViewModelTests
{
    [TestClass]
    public class FXResearchReportViewModelUnitTest
    {

        [TestMethod]
        public void TestMethod1()
        {
            ResearchReportViewModel model = new ResearchReportViewModel(1,"");
            model.Initialization();
            Console.WriteLine(model.INSTITUTIONINFO.Count());
            Assert.IsNotNull(model);
        }
    }
}
