using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.ServiceBus;
using Service.KYC.Domain.Models.Enum;
using Service.KYC.Domain.Models.Messages;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class KycPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<KycPushNotification> _logger;

        public KycPushNotification(
            INotificationService notificationService, 
            ILogger<KycPushNotification> logger,
            ISubscriber<KycProfileUpdatedMessage> subscriber)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            // var executor = new ExecutorWithRetry<KycProfileUpdatedMessage>(
            //     HandleEvent, 
            //     _logger, 
            //     e => $"Cannot send kyc push for trader: {e.ClientId}.", 
            //     e => e.ClientId,
            //     10,
            //     5000);
            subscriber.Subscribe(HandleEvent);
        }

        private async ValueTask HandleEvent(KycProfileUpdatedMessage profileUpdate)
        {
            if (string.IsNullOrWhiteSpace(profileUpdate.OldProfile.BlockingReason) &&
                !string.IsNullOrWhiteSpace(profileUpdate.NewProfile.BlockingReason))
            {
                await _notificationService.SendPushKycUserBanned(new ()
                {
                    ClientId = profileUpdate.ClientId
                });
            }
           
            if (profileUpdate.OldProfile.DepositStatus == KycOperationStatus.KycInProgress &&
                profileUpdate.NewProfile.DepositStatus == KycOperationStatus.KycRequired)
            {
                await _notificationService.SendPushCryptoWithdrawalDecline(new ()
                {
                    ClientId = profileUpdate.ClientId
                });
            }
            
            if (profileUpdate.OldProfile.DepositStatus == KycOperationStatus.KycInProgress &&
                profileUpdate.NewProfile.DepositStatus == KycOperationStatus.Allowed)
            {
                await _notificationService.SendPushKycDocumentsApproved(new ()
                {
                    ClientId = profileUpdate.ClientId
                });
            }
        }
    }
}