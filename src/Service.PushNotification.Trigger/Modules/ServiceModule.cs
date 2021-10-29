﻿using Autofac;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.WithdrawalProcessor.Domain.Models;
using Service.InternalTransfer.Domain.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.PushNotification.Client;
using Service.PushNotification.Trigger.Jobs;

namespace Service.PushNotification.Trigger.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), Program.LogFactory);

            var queueName = "PushNotification.Trigger";

            builder.RegisterMyServiceBusSubscriberSingle<SwapMessage>(serviceBusClient, SwapMessage.TopicName, queueName, TopicQueueType.Permanent);
            builder.RegisterPushNotificationClient(Program.Settings.PushNotificationGrpcServiceUrl);
            
            builder.RegisterMyServiceBusSubscriberSingle<Transfer>(serviceBusClient, Transfer.TopicName, 
                queueName, TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusSubscriberSingle<Withdrawal>(serviceBusClient, Withdrawal.TopicName, 
                queueName, TopicQueueType.Permanent);
            builder.RegisterMyServiceBusSubscriberSingle<Deposit>(serviceBusClient, Deposit.TopicName,
                queueName, TopicQueueType.Permanent);


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
                Program.ReloadedSettings(e => e.AuthServiceBusHostPort), 
                Program.LogFactory.CreateLogger("AuthServiceBus"));

            builder.RegisterInstance(authServiceBus).SingleInstance();
            
            builder.RegisterMyServiceBusSubscriberSingle<SessionAuditEvent>(authServiceBus, SessionAuditEvent.TopicName, 
                queueName, TopicQueueType.Permanent);
        }
    }
}