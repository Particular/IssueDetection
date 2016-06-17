using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NServiceBus.Hosting.Helpers;
using NServiceBus.Saga;

namespace IndexPruner
{
    public class Program
    {
        public const string ConnectionStringName = "sagas";

        private static CloudTable GetTable(string connectionString, string sagaTypeName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            return client.GetTableReference(sagaTypeName);
        }

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            var sagaTypes = FindAllSagaTypes();
            if (sagaTypes.Length == 0)
            {
                Console.WriteLine(
                    "No Saga types found! Did you put all the dlls with sagas into the same directory this executable resides?");
                return;
            }

            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString ??
                                   FetchConnectionString(args);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine(
                    $"Provide a connection string in the standard 'connectionStrings' App.config section with following name: '{ConnectionStringName}'");
                return;
            }

            foreach (var sagaType in sagaTypes)
            {
                Console.WriteLine($"Prunning indexes of the following saga: {sagaType}");
                var table = GetTable(connectionString, sagaType.Name);

                TableContinuationToken token = null;
                do
                {
                    var segment = await table
                        .ExecuteQuerySegmentedAsync(new TableQuery<SecondaryIndexTableEntity>(), token)
                        .ConfigureAwait(false);

                    token = segment.ContinuationToken;

                    var deletes = new List<Task<ITableEntity>>();
                    foreach (var entity in segment)
                    {
                        if (IsSecondaryIndex(entity))
                        {
                            deletes.Add(table.TryDelete(entity));
                            if (deletes.Count > 32)
                            {
                                await FlushDeletes(deletes).ConfigureAwait(false);
                            }
                        }
                    }

                    await FlushDeletes(deletes).ConfigureAwait(false);
                } while (token != null);
            }
        }

        private static async Task FlushDeletes(List<Task<ITableEntity>> deletes)
        {
            var entities = await Task.WhenAll(deletes.ToArray());
            var failures = entities.Where(e => ReferenceEquals(e, Extensions.Empty) == false).ToArray();

            foreach (var failure in failures)
            {
                Console.WriteLine("Following secondary indexes' entities couldn't be removed:");
                Console.WriteLine($"* {failure.PartitionKey}/{failure.RowKey}");
            }
        }

        private static bool IsSecondaryIndex(SecondaryIndexTableEntity entity)
        {
            Guid g;
            var hasGuidPartitionKey = Guid.TryParse(entity.PartitionKey, out g);
            var hasSagaIdSet = entity.SagaId != Guid.Empty;
            return hasSagaIdSet && !hasGuidPartitionKey;
        }

        private static string FetchConnectionString(string[] args)
        {
            CloudStorageAccount storageAccount;
            var first = args?[0];
            return CloudStorageAccount.TryParse(first, out storageAccount) ? first : null;
        }

        private static Type[] FindAllSagaTypes()
        {
            var scanner = new AssemblyScanner
            {
                ThrowExceptions = false
            };
            return scanner.GetScannableAssemblies()
                .Assemblies.Concat(AppDomain.CurrentDomain.GetAssemblies())
                .Where(asm => asm.IsDynamic == false)
                .Distinct()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IContainSagaData).IsAssignableFrom(t))
                .Where(t => t != typeof(IContainSagaData) && t != typeof(ContainSagaData))
                .ToArray();
        }

        class SecondaryIndexTableEntity : TableEntity
        {
            public Guid SagaId { get; set; }
        }
    }
}