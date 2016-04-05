using System.Collections.Generic;
using NUnit.Framework;

namespace NameHelpers.Tests
{
    [TestFixture]
    public class CollectionNameCheckerTests
    {

        [TestCase("OrderSagaData")]
        [TestCase("OrderSaga")]
        [TestCase("DataOrderSagaData")]
        [TestCase("Blueberry")]
        [TestCase("Car")]
        [TestCase("OrderDataIdentity")]
        [TestCase("OrderDataPolicyData")]
        [TestCase("DataofDatasPolicyDataDatasData")]
        public void CanMatchIfStartingWithRaveName(string sagaName)
        {
            var nsbCollectionName = CollectionNamer.NameByNSBConvention(sagaName);
            var ravenCollectionName = CollectionNamer.NameByDefaultRavenConvention(sagaName);

            var collections = new List<string>{ nsbCollectionName, ravenCollectionName };
            string match;
            Assert.IsTrue(CollectionNameChecker.CheckForMatch(ravenCollectionName, collections, out match));
            Assert.AreEqual(nsbCollectionName, match);
        }


        [TestCase("OrderSagaData")]
        [TestCase("OrderSaga")]
        [TestCase("DataOrderSagaData")]
        [TestCase("Blueberry")]
        [TestCase("Car")]
        [TestCase("OrderDataIdentity")]
        [TestCase("OrderDataPolicyData")]
        [TestCase("DataofDatasPolicyDataDatasData")]
        public void CanNotMatchIfStartingWithNSBName(string sagaName)
        {
            var nsbCollectionName = CollectionNamer.NameByNSBConvention(sagaName);
            var ravenCollectionName = CollectionNamer.NameByDefaultRavenConvention(sagaName);

            var collections = new List<string>{ nsbCollectionName, ravenCollectionName };
            string match;
            Assert.IsFalse(CollectionNameChecker.CheckForMatch(nsbCollectionName, collections, out match));
        }
    }
}
