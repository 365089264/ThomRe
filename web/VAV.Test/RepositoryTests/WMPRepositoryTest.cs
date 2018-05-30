using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VAV.DAL.WMP;
using System.Data;
using System.Resources;

namespace VAV.Test.RepositoryTests
{
    [TestClass]
    public class WMPRepositoryTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var start = new DateTime(2013, 10, 1);
            var end = new DateTime(2013, 10, 28);
            WMPRepository target = new WMPRepository();
            var category = "EY";//EY IBT PT C YT
            var actual = target.GetAmountTrendData(start, end, category, "all", "all");
            foreach (DataRow item in actual.Rows)
            {
                //Console.WriteLine("WMP_Trend_{0}_{1}", category, item["Type"].ToString());
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", item["Type"].ToString(), item["Count"], item["CountPercent"], item["Amount"], item["AmountPercent"]);
            }
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetYieldTrendChartData()
        {
            var start = DateTime.Now.AddYears(-1);
            var end = DateTime.Now;
            WMPRepository target = new WMPRepository();
            var actual = target.GetYieldTrendChartData(start, end, null, null, null, null, "1", null, null, null);
            foreach (DataRow item in actual.Rows)
            {
                Console.WriteLine("{0}\t{1}\t{2}", item["start"], item["end"], item["avg"]);
            }
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetWmpBankData()
        {
            var start = DateTime.Now.AddYears(-10);
            var end = DateTime.Now;
            string bank = "200465941";// null "" "200465941"  "200465941,200014081"
            int total = 0;

            WMPRepository target = new WMPRepository();
            var actual = target.GetWmpBankDataPaging(false, "all", string.IsNullOrEmpty(bank) ? "0" : bank, 1, "all", "all", "all", "all", "all", "all",
                start, end, null, "all", "PRD_NAME", null,1, 50,"all", out total);
            foreach (DataRow item in actual.Rows)
            {
                Console.WriteLine(item["PRD_NAME"]);
            }
            Assert.IsNotNull(actual);
        }


        [TestMethod]
        public void TestGetWMPBrokerOrgType()
        {
            WMPRepository target = new WMPRepository();
            var actual = target.GetWmpBrokerOrgType();
            foreach (var item in actual)
            {
                Console.WriteLine(item.Value + "\t" + item.Text);
            }
            Console.WriteLine(actual.Count());
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void TestGetWMPBrokerProductType()
        {
            WMPRepository target = new WMPRepository();
            var actual = target.GetWmpBrokerProductType();
            foreach (var item in actual)
            {
                Console.WriteLine(item.Value + "\t" + item.Text);
            }
            Console.WriteLine(actual.Count());
            Assert.IsNotNull(actual);
        }

    }
}
