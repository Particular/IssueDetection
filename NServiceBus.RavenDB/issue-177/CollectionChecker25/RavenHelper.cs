using System.Collections.Generic;
using System.Linq;
using Raven.Client.Document;

namespace CollectionChecker25
{
    static internal class RavenHelper
    {
        public static string[] GetDatabaseNames(string url)
        {
            using (var store = CreateAndInitializeStore(url))
            {
                return store.DatabaseCommands.GetDatabaseNames(1024);
            }
        }

        public static List<string> GetIndexTerms(string indexName, string url, string database)
        {
            using (var store = CreateAndInitializeStore(url, database))
            {
                return store.DatabaseCommands.GetTerms(indexName, "Tag", null, 1024).ToList();
            }
        }

        public static DocumentStore CreateAndInitializeStore(string url, string database = null)
        {
            var store = new DocumentStore
            {
                Url = url,
            };
            if (database != null)
            {
                store.DefaultDatabase = database;
            }

            store.Initialize();
            return store;
        }
    }
}