using Autofac;
using Service.PushNotification.Trigger.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.PushNotification.Trigger.Client
{
    public static class AutofacHelper
    {
        public static void RegisterPushNotificationTriggerClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new PushNotificationTriggerClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
