using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;

namespace Issue117Detector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DocumentStore store = new DocumentStore { ConnectionStringName = "ServiceControl" };
            store.Initialize();
            store.Conventions.MaxNumberOfRequestsPerSession = 2048;

            Console.WriteLine("Creating Temp Indexes (this may take some time)...");

            try
            {

                RavenIndexes.Create(store);
                Thread.Sleep(1000);

                while (store.DatabaseCommands.GetStatistics().StaleIndexes.Length != 0)
                {
                    Thread.Sleep(5000);
                }

                Console.WriteLine("Index creation complete.");
                Console.WriteLine();

                foreach (var group in GetEndpointGroups(store))
                {
                    Console.WriteLine("{0} handled by {1} endpoints:", group.MsgType, group.EndpointCount);
                    Console.WriteLine("  Endpoints:");
                    foreach (var ep in group.Endpoints)
                        Console.WriteLine("    * {0}", ep);
                    Console.WriteLine("  Problem Messages:");

                    var localGroup = group;
                    var problems = GetMessageData(store, group.MsgType)
                        .Select(msg => new
                        {
                            Msg = msg,
                            NotProcessedBy = group.Endpoints.Except(msg.Endpoints).ToArray()
                        })
                        .Where(x => x.NotProcessedBy.Length > 0);

                    if (problems.Any())
                    {
                        foreach (var problem in problems)
                        {
                            Console.WriteLine("    * Msg {0} not processed by {1} endpoints:", problem.Msg.MsgId,
                                problem.NotProcessedBy.Length);
                            foreach (var npb in problem.NotProcessedBy)
                                Console.WriteLine("      {0}", npb);
                        }
                    }
                    else
                    {
                        Console.WriteLine("    None found!");
                    }
                }
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Removing Temp Indexes...");
                RavenIndexes.Remove(store);
                Console.WriteLine("Indexes removed successfully.");
            }

            Console.WriteLine("Complete. Press Enter to exit.");
            Console.ReadLine();
        }

        public static IEnumerable<GroupedEndpoint> GetEndpointGroups(IDocumentStore store)
        {
            return GetAll(store, session => session.Query<GroupedEndpoint>("Issue117Detector/GroupEndpoints", true)
                    .Where(result => result.EndpointCount > 1));
        }

        public static IEnumerable<MsgData> GetMessageData(IDocumentStore store, string messageType)
        {
            return GetAll(store, session => session.Query<MsgData>("Issue117Detector/GroupMsgIds", true))
                .Where(result => result.MsgType == messageType);
        }

        public static IEnumerable<T> GetAll<T>(IDocumentStore store, Func<IDocumentSession, IRavenQueryable<T>> baseQuery)
        {
            int got = 0;

            while (true)
            {
                List<T> batch = null;
                RavenQueryStatistics stats = null;

                using (var session = store.OpenSession())
                {
                    batch = baseQuery(session)
                        .Statistics(out stats)
                        .Skip(got)
                        .Take(1024)
                        .ToList();
                }

                foreach (var group in batch)
                {
                    yield return group;
                }

                got += batch.Count + stats.SkippedResults;

                if (got >= stats.TotalResults)
                    yield break;
            }
        }
    }
}
