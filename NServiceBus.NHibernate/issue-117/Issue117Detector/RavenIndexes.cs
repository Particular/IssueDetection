using System.Collections.Generic;
using Raven.Abstractions.Indexing;
using Raven.Client;

namespace Issue117Detector
{
    public class GroupedEndpoint
    {
        public string MsgType { get; set; }
        public string[] Endpoints { get; set; }
        public int MsgCount { get; set; }
        public int EndpointCount { get; set; }
    }

    public class MsgData
    {
        public string MsgId { get; set; }
	    public string MsgType { get; set; }
        public string[] Endpoints { get; set; }
    }

    static class RavenIndexes
    {
        const string groupEndpointsIndexName = "Issue117Detector/GroupEndpoints";
        private const string groupMsgIdsIndexName = "Issue117Detector/GroupMsgIds";
        public static void Create(IDocumentStore store)
        {
            store.DatabaseCommands.PutIndex(groupEndpointsIndexName, new IndexDefinition
            {
                Map = @"from msg in docs.ProcessedMessages
                        where msg.MessageMetadata.MessageIntent == 2
                        select new 
                        {
                            MsgType = msg.MessageMetadata.MessageType,
                            Endpoints = new string[] { msg.MessageMetadata.ReceivingEndpoint.Name },
                            MsgCount = 1,
                            EndpointCount = 1
                        }",

                Reduce = @"from result in results
                           group result by result.MsgType into g
                           select new
                           {
                               MsgType = g.Key,
                               Endpoints = g.SelectMany(x => x.Endpoints).Distinct().ToArray(),
                               MsgCount = g.Sum(x => x.MsgCount),
                               EndpointCount = g.SelectMany(x => x.Endpoints).Distinct().Count()
                           }",

                Indexes = new Dictionary<string, FieldIndexing>
                {
                    {"MsgCount", FieldIndexing.Analyzed}
                }
            }, true);


            store.DatabaseCommands.PutIndex(groupMsgIdsIndexName, new IndexDefinition
            {
                Map = @"from msg in docs.ProcessedMessages
                        where msg.MessageMetadata.MessageIntent == 2 // Publish
                        select new
                        {
	                        MsgId = msg.MessageMetadata.MessageId,
	                        MsgType = msg.MessageMetadata.MessageType,
	                        Endpoints = new string[] { msg.MessageMetadata.ReceivingEndpoint.Name }
                        }",

                Reduce = @"from result in results
                           group result by result.MsgId into g
                           select new
                           {
	                           MsgId = g.Key,
	                           MsgType = g.First().MsgType,
	                           Endpoints = g.SelectMany(x => x.Endpoints).Distinct().ToArray()
                           }",

                Indexes = new Dictionary<string, FieldIndexing>
                {
                    {"MsgType", FieldIndexing.Analyzed}
                }
            }, true);
        }

        public static void Remove(IDocumentStore store)
        {
            store.DatabaseCommands.DeleteIndex(groupEndpointsIndexName);
            store.DatabaseCommands.DeleteIndex(groupMsgIdsIndexName);
        }
    }
}
