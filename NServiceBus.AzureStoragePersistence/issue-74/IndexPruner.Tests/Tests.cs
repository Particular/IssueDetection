using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NServiceBus.Saga;
using NServiceBus.SagaPersisters.Azure;
using NUnit.Framework;

namespace IndexPruner.Tests
{
    public class Tests
    {
        private const string CorrelationIdPropertyName = "CorrelationId";
        private readonly CloudStorageAccount _account;
        private readonly string _connectionString;
        private readonly CloudTable _sagaTable;

        public Tests()
        {
            _connectionString = Environment.GetEnvironmentVariable("AzureStoragePersistence.ConnectionString");
            _account = CloudStorageAccount.Parse(_connectionString);
            _sagaTable = _account.CreateCloudTableClient().GetTableReference(typeof(IndexPrunerSaga).Name);
        }

        [SetUp]
        public async Task SetUp()
        {
            await _sagaTable.CreateIfNotExistsAsync().ConfigureAwait(false);
            await DeleteAllEntities(_sagaTable).ConfigureAwait(false);
        }

        [Test]
        public async Task TryRetrieveAfterPruning()
        {
            const string correlationId = "1";
            var id = new Guid("425D9D37-441F-4FF3-8BCB-6A2806D9C88A");

            var saga = new IndexPrunerSaga
            {
                CorrelationId = correlationId,
                Id = id
            };

            var p1 = GetPersister();
            p1.Save(saga);

            await Program.MainAsync(new[] {_connectionString}).ConfigureAwait(false);

            var entities = await _sagaTable.ExecuteQuerySegmentedAsync(new TableQuery(), null).ConfigureAwait(false);
            Assert.IsNotNull(entities.Results.Single(de => de.PartitionKey == id.ToString()));

            var p2 = GetPersister();

            var s1 = p2.Get<IndexPrunerSaga>(CorrelationIdPropertyName, correlationId);
            AssertSaga(s1, id, correlationId);

            var s2 = p2.Get<IndexPrunerSaga>(id);
            AssertSaga(s2, id, correlationId);
        }

        private static void AssertSaga(IndexPrunerSaga saga, Guid id, string correlationId)
        {
            Assert.AreEqual(id, saga.Id);
            Assert.AreEqual(correlationId, saga.CorrelationId);
        }

        private ISagaPersister GetPersister()
        {
            return new AzureSagaPersister(_account, true);
        }

        public class IndexPrunerSaga : ContainSagaData
        {
            [Unique]
            public string CorrelationId { get; set; }
        }

        private static async Task DeleteAllEntities(CloudTable table)
        {
            TableQuerySegment<DynamicTableEntity> segment = null;

            while (segment == null || segment.ContinuationToken != null)
            {
                segment = await table.ExecuteQuerySegmentedAsync(new TableQuery().Take(100), segment?.ContinuationToken);
                foreach (var entity in segment.Results)
                {
                    await table.ExecuteAsync(TableOperation.Delete(entity));
                }
            }
        }
    }
}