using LNF;
using LNF.Billing;
using LNF.Impl.DependencyInjection.Default;
using LNF.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace sselUser.Tests
{
    [TestClass]
    public class ApportionmentUtilityTests
    {
        private IDisposable _uow;

        protected IApportionmentManager ApportionmentManager = ServiceProvider.Current.Use<IApportionmentManager>();

        [TestInitialize]
        public void TestInitialize()
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();
            _uow = DA.StartUnitOfWork();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _uow.Dispose();
        }

        [TestMethod]
        public void CanGetMinimumDays()
        {
            int actual;

            actual = ApportionmentManager.GetMinimumDays(DateTime.Parse("2016-08-01"), 268, 154, 30);
            Assert.AreEqual(8, actual);

            actual = ApportionmentManager.GetMinimumDays(DateTime.Parse("2016-07-01"), 2691, 154, 17);
            Assert.AreEqual(4, actual);

            actual = ApportionmentManager.GetMinimumDays(DateTime.Parse("2016-09-01"), 96, 154, 17);
            Assert.AreEqual(3, actual);
        }
    }
}
