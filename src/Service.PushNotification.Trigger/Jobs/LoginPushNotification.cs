using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.ServiceBus;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class LoginPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<LoginPushNotification> _logger;

        public LoginPushNotification(INotificationService notificationService, ILogger<LoginPushNotification> logger,
            ISubscriber<SessionAuditEvent> subscriber)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var executor = new ExecutorWithRetry<SessionAuditEvent>(
                HandleEvent, 
                _logger, 
                e => $"Cannot send login push for trader: {e.Session.TraderId}.", 
                e => e.Session.RootSessionId.ToString(),
                10,
                5000);
            subscriber.Subscribe(executor.Execute);
        }

        private async ValueTask HandleEvent(SessionAuditEvent auditEvent)
        {
            if (auditEvent.Action == SessionAuditEvent.SessionAction.Login)
            {
                await _notificationService.SendPushLogin(new LoginPushRequest()
                {
                    ClientId = auditEvent.Session.TraderId,
                    Date = auditEvent.Session.CreateTime,
                    Ip = auditEvent.Session.IP
                });
            }
        }
    }
}