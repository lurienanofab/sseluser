using LNF;
using LNF.Impl.Testing;
using LNF.Models.Billing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace sselUser.Tests
{
    [TestClass]
    public class ApportionmentUtilityTests
    {
        protected ContextManager _context;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new ContextManager("127.0.0.1", "jgett");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
        public void CanGetMinimumDays()
        {
            int actual;

            actual = ServiceProvider.Current.Billing.ApportionmentManager.GetMinimumDays(DateTime.Parse("2016-08-01"), 268, 154, 30);
            Assert.AreEqual(8, actual);

            actual = ServiceProvider.Current.Billing.ApportionmentManager.GetMinimumDays(DateTime.Parse("2016-07-01"), 2691, 154, 17);
            Assert.AreEqual(4, actual);

            actual = ServiceProvider.Current.Billing.ApportionmentManager.GetMinimumDays(DateTime.Parse("2016-09-01"), 96, 154, 17);
            Assert.AreEqual(4, actual);
        }
    }
}
