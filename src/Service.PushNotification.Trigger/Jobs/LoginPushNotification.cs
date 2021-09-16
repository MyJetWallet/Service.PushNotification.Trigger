using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class LoginPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<LoginPushNotification> _logger;
        private readonly ISubscriber<IReadOnlyList<SessionAuditEvent>> _sessionAudit;

        public LoginPushNotification(INotificationService notificationService, ILogger<LoginPushNotification> logger,
            ISubscriber<IReadOnlyList<SessionAuditEvent>> sessionAudit)
        {
            _notificationService = notificationService;
            _logger = logger;
            _sessionAudit = sessionAudit;
            
            _sessionAudit.Subscribe(HandleEvent);
        }

        private async ValueTask HandleEvent(IReadOnlyList<SessionAuditEvent> events)
        {
            var taskList = new List<Task>();

            try
            {
                foreach (var auditEvent in events.Where(e => e.Action == SessionAuditEvent.SessionAction.Login))
                {
                    var task = _notificationService.SendPushLogin(new LoginPushRequest()
                    {
                        ClientId = auditEvent.Session.TraderId,
                        Date = auditEvent.Session.CreateTime,
                        Ip = auditEvent.Session.IP
                    });
                
                    taskList.Add(task);
                }
            
                await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot send login push");
            }
        }
    }
}