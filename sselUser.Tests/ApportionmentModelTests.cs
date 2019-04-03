using LNF;
using LNF.Impl.Testing;
using LNF.Web.User.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace sselUser.Tests
{
    [TestClass]
    public class ApportionmentModelTests
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
        public void CanUpdateBillingData()
        {
            var model = new ApportionmentModel(ServiceProvider.Current) { Period = DateTime.Parse("2019-03-01"), ClientID = 1116 };
            model.UpdateBillingData();
            Assert.IsTrue(model.Success);
            Assert.AreEqual(0, model.Errors.Count());
        }
    }
}
