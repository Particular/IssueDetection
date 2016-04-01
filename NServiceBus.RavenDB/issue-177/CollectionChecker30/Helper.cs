using Raven.Client.Document;

namespace CollectionChecker30
{
    static internal class Helper
    {
        public static void CreatDummyData(string url)
        {
            using (var store = RavenHelper.CreateAndInitializeStore(url))
            {
                store.Conventions.FindTypeTagName = t => t.Name;

                store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists("BothProblems");
                using (var session = store.OpenSession("BothProblems"))
                {
                    session.Store(new TimeoutData());
                    session.Store(new TimeoutDatas());
                    session.Store(new TestSaga());
                    session.Store(new TestSagaDatas());
                    session.SaveChanges();
                }

                store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists("OnlyTimeoutProblem");
                using (var session = store.OpenSession("OnlyTimeoutProblem"))
                {
                    session.Store(new TimeoutData());
                    session.Store(new TimeoutDatas());
                    session.Store(new TestSaga());
                    session.SaveChanges();
                }

                store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists("OnlySagaProblem");
                using (var session = store.OpenSession("OnlySagaProblem"))
                {
                    session.Store(new TestSaga());
                    session.Store(new TestSagaDatas());
                    session.Store(new TimeoutData());

                    session.SaveChanges();
                }

                store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists("NoProblemStandardConvention");
                using (var session = store.OpenSession("NoProblemStandardConvention"))
                {
                    session.Store(new TimeoutDatas());
                    session.Store(new TestSagaDatas());
                    session.SaveChanges();
                }

                store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists("NoProblemNSBConvention");
                using (var session = store.OpenSession("NoProblemNSBConvention"))
                {
                    session.Store(new TimeoutData());
                    session.Store(new TestSaga());
                    session.SaveChanges();
                }
            }
        }
    }

    public class TimeoutData
    {
    }
    public class TimeoutDatas
    {
    }
    public class TestSaga
    {
    }
    public class TestSagaDatas
    {
    }

}