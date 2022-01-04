using LNF.Billing.Apportionment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace sselUser.Tests
{
    [TestClass]
    public class AportionmentRepositoryTests
    {
        [TestMethod]
        public void CanUpdateChildRoomEntryApportionment()
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                var repo = new Repository(conn);
                var period = DateTime.Parse("");
                var clientId = 1234;
                var roomId = 1234;

                repo.UpdateChildRoomEntryApportionment(period, clientId, roomId);
            }
        }
    }
}
