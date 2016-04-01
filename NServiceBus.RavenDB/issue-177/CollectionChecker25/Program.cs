using System;
using System.Collections.Generic;

namespace CollectionChecker25
{
    class Program
    {
        private static readonly List<string> KnownCollectionNames = new List<string>
        {
            "OutboxRecords",
            "OutboxRecord",
            "SagaUniqueIdentities",
            "SagaUniqueIdentity",
            "Subscriptions",
            "Subscription",
            "TimeoutDatas",
            "TimeoutData",
            "GatewayMessages",
            "GatewayMessage"
        };

        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: CollectionChecker <raven url>");
                Console.WriteLine("Example: CollectionChecker http://localhost:8080");
                return;
            }

            var url = args[0];

            Helper.CreatDummyData(url);

            try
            {
                var databases = RavenHelper.GetDatabaseNames(url);
                foreach (var database in databases)
                {
                    CheckDatabase(database, url);
                }

            }
            catch (Exception)
            {
                Console.WriteLine($"Could not connect to raven: {url}.");
                throw;
            }

        }

        private static void CheckDatabase(string database, string url)
        {
            Console.WriteLine($"Checking database {database} for problems.");

            using (var store = RavenHelper.CreateAndInitializeStore(url, database))
            {
                var collectionNamesInIndex = RavenHelper.GetIndexTerms("Raven/DocumentsByEntityName", url, database);

                var timeoutProblem = CheckForDuplicateTimeoutCollections(collectionNamesInIndex);
                var sagaProblem = CheckForDuplicateSagaCollections(collectionNamesInIndex);


                Console.WriteLine(timeoutProblem || sagaProblem
                    ? $"Problems found in database {database}. Please update to NServiceBus.RavenDB hotfix 3.0.7 as soon as possible to avoid potential message loss."
                    : $"No problems found in database {database}.");
            }
        }

        private static bool CheckForDuplicateSagaCollections(List<string> collectionNamesInIndex)
        {
            var found = false;

            foreach (var collectionName in collectionNamesInIndex)
            {
                if (KnownCollectionNames.Contains(collectionName))
                {
                    continue;
                }

                if (collectionName.EndsWith("Datas"))
                {
                    continue;
                }

                var defaultConventionName = collectionName + "Datas";
                if (collectionNamesInIndex.Contains(defaultConventionName))
                {
                    found = true;
                    Console.WriteLine($"Problem! Duplicate saga data collections found: {collectionName}/{defaultConventionName}.");
                }
            }
            return found;
        }

        private static bool CheckForDuplicateTimeoutCollections(List<string> collectionNamesInIndex)
        {
            var found = false;
            if (collectionNamesInIndex.Contains("TimeoutDatas") // default convention
                && collectionNamesInIndex.Contains("TimeoutData")) //NSB convention
            {
                found = true;
                Console.WriteLine("Problem! Duplicate timeout data collections found: TimeoutData/TimeoutDatas");
            }
            return found;
        }
    }
}

