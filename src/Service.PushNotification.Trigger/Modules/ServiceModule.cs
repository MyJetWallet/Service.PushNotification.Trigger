using Autofac;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.WithdrawalProcessor.Client;
using Service.InternalTransfer.Client;
using Service.InternalTransfer.Domain.Models;
using Service.Liquidity.Converter.Client;
using Service.PushNotification.Client;
using Service.PushNotification.Trigger.Jobs;

namespace Service.PushNotification.Trigger.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), ApplicationEnvironment.HostName, Program.LogFactory);

            var queueName = "PushNotification.Trigger";

            builder.RegisterDepositOperationSubscriberBatch(serviceBusClient, queueName);
            builder.RegisterWithdrawalOperationSubscriber(serviceBusClient, queueName);
            builder.RegisterLiquidityConverterServiceBusSubscriber(serviceBusClient, queueName);
            builder.RegisterPushNotificationClient(Program.Settings.PushNotificationGrpcServiceUrl);
            
            builder.RegisterMyServiceBusSubscriberSingle<Transfer>(serviceBusClient, Transfer.TopicName, 
                queueName, TopicQueueType.PermanentWithSingleConnection);

            builder
                .RegisterType<CryptoDepositPushNotification>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<CryptoWithdrawalPushNotification>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<ConvertPushNotification>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<LoginPushNotification>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<TransferPushNotification>()
                .AutoActivate()
                .SingleInstance();

            var authServiceBus = MyServiceBusTcpClientFactory.Create(
                Program.ReloadedSettings(e => e.AuthServiceBusHostPort), ApplicationEnvironment.HostName,
                Program.LogFactory.CreateLogger("AuthServiceBus"));

            builder.RegisterMyServiceBusSubscriberBatch<SessionAuditEvent>(authServiceBus, SessionAuditEvent.TopicName, queueName, TopicQueueType.Permanent);

            builder.RegisterInstance(authServiceBus).SingleInstance();
        }
    }
}