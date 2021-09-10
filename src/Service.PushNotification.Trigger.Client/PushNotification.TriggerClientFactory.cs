using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.PushNotification.Trigger.Grpc;

namespace Service.PushNotification.Trigger.Client
{
    [UsedImplicitly]
    public class PushNotificationTriggerClientFactory: MyGrpcClientFactory
    {
        public PushNotificationTriggerClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
