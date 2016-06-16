using System;
using Microsoft.WindowsAzure.Storage;
using NServiceBus.SagaPersisters.Azure;
using NUnit.Framework;

namespace IndexPruner.Tests
{
    public class Tests
    {
        private readonly CloudStorageAccount _account;
        private AzureSagaPersister persister;

        public Tests()
        {
            _account =
                CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureStoragePersistence.ConnectionString"));
        }

        [SetUp]
        public void SetUp()
        {
            persister = new AzureSagaPersister(_account, true);
        }
    }
}