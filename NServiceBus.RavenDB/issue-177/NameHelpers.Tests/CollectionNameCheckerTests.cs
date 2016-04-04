using NUnit.Framework;

namespace NameHelpers.Tests
{
    [TestFixture]
    public class CollectionNameCheckerTests
    {
        [Test]
        public void OrderSagaDatasShouldMatchOrderSaga() //saga name: OrderSagaData
        {
            var collections = new[] {"OrderSaga", "OrderSagaDatas"};
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("OrderSagaDatas", collections, out match));
            Assert.AreEqual("OrderSaga", match);
        }

        [Test]
        public void OrderSagasShouldMatchOrderSaga()//saga name: OrderSaga
        {
            var collections = new[] { "OrderSaga", "OrderSagas" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("OrderSagas", collections, out match));
            Assert.AreEqual("OrderSaga", match);
        }

        [Test]
        public void DataOrderSagaDatasShouldMatchOrderSaga()//saga name: DataOrderSagaData
        {
            var collections = new[] { "OrderSaga", "DataOrderSagaDatas" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("DataOrderSagaDatas", collections, out match));
            Assert.AreEqual("OrderSaga", match);
        }

        [Test]
        public void BlueBerriesShouldMatchBlueBerry()//saga name: Blueberry
        {
            var collections = new[] { "BlueBerry", "BlueBerries" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("BlueBerries", collections, out match));
            Assert.AreEqual("BlueBerry", match);
        }

        [Test]
        public void CarsShouldMatchCar()//saga name: Car
        {
            var collections = new[] { "Car", "Cars" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("Cars", collections, out match));
            Assert.AreEqual("Car", match);
        }

        [Test]
        public void OrderDataIdentitiesShouldMatchOrderIdentity()//saga name: OrderDataIdentity
        {
            var collections = new[] { "OrderIdentity", "OrderDataIdentities" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("OrderDataIdentities", collections, out match));
            Assert.AreEqual("OrderIdentity", match);
        }

        [Test]
        public void OrderDataPolicyDatasShouldMatchOrderPolicy()//saga name: OrderDataPolicyData
        {
            var collections = new[] { "OrderPolicy", "OrderDataPolicyDatas" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("OrderDataPolicyDatas", collections, out match));
            Assert.AreEqual("OrderPolicy", match);
        }

        [Test]
        public void DataofDatasPolicyDataDatasDatasShouldMatchofsPolicys()//saga name: DataofDatasPolicyDataDatasData
        {
            var collections = new[] { "ofsPolicys", "DataofDatasPolicyDataDatasDatas" };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch("DataofDatasPolicyDataDatasDatas", collections, out match));
            Assert.AreEqual("ofsPolicys", match);
        }
    }
}
