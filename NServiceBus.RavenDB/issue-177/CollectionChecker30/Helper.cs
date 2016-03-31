using Raven.Client.Document;

namespace CollectionChecker30
{
    static internal class Helper
    {
        public static void PutDummyData(DocumentStore store, string database)
        {
            store.Conventions.FindTypeTagName = t => t.Name;
            using (var session = store.OpenSession(database))
            {
                session.Store(new SagaUniqueIdentity());
                session.Store(new SagaUniqueIdentities());
                session.SaveChanges();
            }
        }
    }

    public class SagaUniqueIdentity
    {
    }
    public class SagaUniqueIdentities
    {
    }
}