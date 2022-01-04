using LNF.Web.User.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace sselUser.Tests
{
    [TestClass]
    public class ApportionmentModelTests
    {
        [TestMethod]
        public void CanUpdateBillingData()
        {
            var model = new ApportionmentModel() { Period = DateTime.Parse("2019-03-01"), ClientID = 1116 };
            model.UpdateBillingData();
            Assert.IsTrue(model.Success);
            Assert.AreEqual(0, model.Errors.Count());
        }
    }
}
