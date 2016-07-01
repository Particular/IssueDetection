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
            MainAsync(args).GetAwaiter().GetResult();
            Console.WriteLine("Completed. Press any key to exit.");
            Console.ReadKey();
        }

        public static async Task MainAsync(string[] args)
        {
            var sagaDataTypes = FindAllSagaDataTypes();
            if (sagaDataTypes.Length == 0)
            {
                Console.WriteLine(
                    "No Saga Data Types found! Did you put all the dlls with sagas into the same directory this executable resides?");
                return;
            }

            var connectionString = GetConnectionString(args);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine(
                    $"Provide a connection string in the standard 'connectionStrings' App.config section with following name: '{ConnectionStringName}' \r\nAlternatively, provide a connection string as a commandline parameter when running this tool.");
                return;
            }

            foreach (var sagaDataType in sagaDataTypes)
            {
                if (sagaDataType.IsGenericType)
                {
                    Console.WriteLine($"The saga data type `{sagaDataType.FullName}` is a generic class type and cannot be processed");
                    continue;
                }

                Console.WriteLine($"Pruning indexes of the following saga data type: {sagaDataType}");
                var table = GetTable(connectionString, sagaDataType.Name);

                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<SecondaryIndexTableEntity> segment;
                    try
                    {
                        segment = await table
                            .ExecuteQuerySegmentedAsync(new TableQuery<SecondaryIndexTableEntity>(), token)
                            .ConfigureAwait(false);
                    }
                    catch (StorageException e)
                    {
                        if (e.RequestInformation.HttpStatusCode == 404)
                        {
                            Console.WriteLine($"Unable to find saga storage table for saga data type `{sagaDataType.FullName}`. Check that your connection string is correct for the sagas data types being scanned.");
                        }
                        Console.WriteLine("Error occurred:");
                        Console.WriteLine(e.ToString());
                        return;
                    }

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
                                deletes.Clear();
                            }
                        }
                    }

                    await FlushDeletes(deletes).ConfigureAwait(false);
                } while (token != null);
            }
        }

        private static string GetConnectionString(string[] args)
        {
            var conn = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
            {
                conn = ParseConnectionStringFromCommandlineArgs(args);
            }

            return conn;
        }

        private static async Task FlushDeletes(List<Task<ITableEntity>> deletes)
        {
            var entities = await Task.WhenAll(deletes.ToArray());
            var failures = entities.Where(e => ReferenceEquals(e, Extensions.Empty) == false).ToArray();

            foreach (var failure in failures)
            {
                Console.WriteLine("The following secondary index entities couldn't be removed:");
                Console.WriteLine($"* {failure.PartitionKey}/{failure.RowKey}");
            }
        }

        private static bool IsSecondaryIndex(SecondaryIndexTableEntity entity)
        {
            var isSecondaryIndex = entity.PartitionKey.StartsWith("Index_");
            var hasSagaIdSet = entity.SagaId != Guid.Empty;
            return isSecondaryIndex && hasSagaIdSet;
        }

        private static string ParseConnectionStringFromCommandlineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                return null;
            }

            CloudStorageAccount storageAccount;
            var first = args[0];
            
            return CloudStorageAccount.TryParse(first, out storageAccount) ? first : null;
        }

        private static Type[] FindAllSagaDataTypes()
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