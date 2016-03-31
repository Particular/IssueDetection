using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Document;

namespace CollectionChecker25
{
    class Program
    {
        private static readonly List<string> RavenCollectionNames = new List<string>
        {
            "OutboxRecords",
            "SagaUniqueIdentities",
            "Subscriptions",
            "TestSagaDatas",
            "TimeoutDatas"
            //add gateway
        };

        private static readonly List<string> NsbCollectionNames = new List<string>
        {
            "OutboxRecord",
            "SagaUniqueIdentity",
            "Subscription",
            "TestSaga",
            "TimeoutData",
            //add gateway
        };

        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CollectionChecker <raven url> <databasename>");
                Console.WriteLine("Example: CollectionChecker http://localhost:8080 Sales");
                return;
            }

            var url = args[0];
            var database = args[1];

            var store = new DocumentStore
            {
                Url = url,
                DefaultDatabase = database
            };

            try
            {
                store.Initialize();
                
                //Helper.PutDummyData(store,database);

                var terms = store.DatabaseCommands.GetTerms("Raven/DocumentsByEntityName", "Tag", null, 1024).ToList();
                
#if DEBUG
                    Console.WriteLine(string.Join(",", terms));
#endif

                var foundRavenCollectionNames = terms.Any((s => RavenCollectionNames.Contains(s)));
                var foundNsbCollectionNames = terms.Any((s => NsbCollectionNames.Contains(s)));

                if (foundNsbCollectionNames && foundRavenCollectionNames)
                {
                    Console.WriteLine("Multiple names of the same collection found. Please update to NServiceBus.RavenDB hotfix 3.0.7 as soon as possible to avoid potential message loss.");
                }
                else
                {
                    Console.WriteLine("Could not find multiple collection naming strategies in use in the Raven database.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not connect to raven: {url}, database {database}");
                throw;
            }
            
        }
    }
}

