using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.ServiceBus;
using Service.ClientProfile.Domain.Models;
using Service.KYC.Domain.Models.Enum;
using Service.KYC.Domain.Models.Messages;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class TwoFaPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<TwoFaPushNotification> _logger;

        public TwoFaPushNotification(
            INotificationService notificationService, 
            ILogger<TwoFaPushNotification> logger,
            ISubscriber<ClientProfileUpdateMessage> subscriber)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var executor = new ExecutorWithRetry<ClientProfileUpdateMessage>(
                HandleEvent, 
                _logger, 
                e => $"Cannot handle transfer for client {e.NewProfile.ClientId}.", 
                e => e.Timestamp.ToString(),
                10,
                500);
            
            subscriber.Subscribe(executor.Execute);
            
        }

        private async ValueTask HandleEvent(ClientProfileUpdateMessage profileUpdate)
        {
            var request = new TwoFaRequest() { ClientId = profileUpdate.NewProfile.ClientId };
            
            if (profileUpdate.OldProfile.Status2FA != Status2FA.Enabled && profileUpdate.NewProfile.Status2FA == Status2FA.Enabled)
                await _notificationService.SendTwoFaEnabled(request);

            if (profileUpdate.OldProfile.Status2FA != Status2FA.Disabled && profileUpdate.NewProfile.Status2FA == Status2FA.Disabled)
                await _notificationService.SendTwoFaDisabled(request);
        }
    }
}