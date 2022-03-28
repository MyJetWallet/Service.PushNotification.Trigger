using Autofac;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.WithdrawalProcessor.Domain.Models;
using Service.ClientProfile.Domain.Models;
using Service.InternalTransfer.Domain.Models;
using Service.KYC.Domain.Models.Messages;
using Service.Liquidity.Converter.Domain.Models;
using Service.PushNotification.Client;
using Service.PushNotification.Trigger.Jobs;

namespace Service.PushNotification.Trigger.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);

            var queueName = "PushNotification.Trigger";

            builder.RegisterMyServiceBusSubscriberSingle<SwapMessage>(serviceBusClient, SwapMessage.TopicName,
                queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterPushNotificationClient(Program.Settings.PushNotificationGrpcServiceUrl);

            builder.RegisterMyServiceBusSubscriberSingle<Transfer>(serviceBusClient, Transfer.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<Withdrawal>(serviceBusClient, Withdrawal.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<Deposit>(serviceBusClient, Deposit.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<KycProfileUpdatedMessage>(serviceBusClient, KycProfileUpdatedMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<SessionAuditEvent>(serviceBusClient, SessionAuditEvent.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<ClientProfileUpdateMessage>(serviceBusClient, ClientProfileUpdateMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);

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

            builder
                .RegisterType<KycPushNotification>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<TwoFaPushNotification>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}