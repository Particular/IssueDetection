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

                store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists("EverythingIsCrazy");
                using (var session = store.OpenSession("EverythingIsCrazy"))
                {
                    session.Store(new TimeoutData());
                    session.Store(new TimeoutDatas());
                    session.Store(new TestSaga());
                    session.Store(new TestSagaDatas());
                    session.Store(new OutboxRecords());
                    session.Store(new OutboxRecord());
                    session.Store(new SagaUniqueIdentities());
                    session.Store(new SagaUniqueIdentity());
                    session.Store(new Subscriptions());
                    session.Store(new Subscription());
                    session.Store(new GatewayMessages());
                    session.Store(new GatewayMessage());
                    session.Store(new OrderSaga());
                    session.Store(new OrderSagaDatas());
                    session.Store(new OrderSagas());
                    session.Store(new DataOrderSagaDatas());
                    session.Store(new BlueBerry());
                    session.Store(new BlueBerries());
                    session.Store(new Car());
                    session.Store(new Cars());
                    session.Store(new OrderIdentity());
                    session.Store(new OrderDataIdentities());
                    session.Store(new OrderPolicy());
                    session.Store(new OrderDataPolicyDatas());
                    session.Store(new ofsPolicys());
                    session.Store(new DataofDatasPolicyDataDatasDatas());
                    session.SaveChanges();
                }
            }
        }
    }

    public class TimeoutData { }
    public class TimeoutDatas { }
    public class TestSaga { }
    public class TestSagaDatas {}
    public class OutboxRecords { }
    public class OutboxRecord { }
    public class SagaUniqueIdentities { }
    public class SagaUniqueIdentity { }
    public class Subscriptions { }
    public class Subscription { }
    public class GatewayMessages { }
    public class GatewayMessage { }

    public class OrderSaga { }
    public class OrderSagaDatas { }
    public class OrderSagas { }
    public class DataOrderSagaDatas { }
    public class BlueBerry { }
    public class BlueBerries { }
    public class Car { }
    public class Cars { }
    public class OrderIdentity { }
    public class OrderDataIdentities { }
    public class OrderPolicy { }
    public class OrderDataPolicyDatas { }
    public class ofsPolicys { }
    public class DataofDatasPolicyDataDatasDatas { }
}