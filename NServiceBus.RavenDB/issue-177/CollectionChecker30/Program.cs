using System;
using System.Collections.Generic;
using NameHelpers;

namespace CollectionChecker30
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

            //Helper.CreatDummyData(url);
            
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
            Console.WriteLine();
            Console.WriteLine($"********************Checking database {database} for problems.********************************");

            using (RavenHelper.CreateAndInitializeStore(url, database))
            {
                var collectionNamesInIndex = RavenHelper.GetIndexTerms("Raven/DocumentsByEntityName", url, database);

                var timeoutProblem = CheckForDuplicateTimeoutCollections(collectionNamesInIndex);
                var sagaProblem = CheckForDuplicateSagaCollections(collectionNamesInIndex);

                Console.WriteLine(timeoutProblem || sagaProblem
                    ? $"Problems found in database {database}. There are duplicated timeout and/or saga collections in this database. This is caused by switching between using a connection string and providing a full document store to NSB endpoint using this database. You need to inspect the collections listed above and decided if you can discard the ones currently not in use."
                    : $"No problems found in database {database}.");
            }
            Console.WriteLine($"***************Finished checking database {database} for problems.****************************");
        }
        
        private static bool CheckForDuplicateSagaCollections(List<string> collectionNamesInIndex)
        {
            var found = false;

            foreach (var collectionName in collectionNamesInIndex)
            {
                if (KnownCollectionNames.Contains(collectionName))
                    continue;
                
                string match;
                if (!CollectionNameChecker.CheckForMatch(collectionName, collectionNamesInIndex, out match))
                    continue;

                found = true;
                Console.WriteLine($"Problem! Duplicate saga data collections found: {collectionName}/{match}.");
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

