using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.InternalTransfer.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class TransferReceivePushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<TransferReceivePushNotification> _logger;

        public TransferReceivePushNotification(ISubscriber<IReadOnlyList<Transfer>> subscriber,
            INotificationService notificationService, 
            ILogger<TransferReceivePushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            subscriber.Subscribe(HandleDeposit);
        }

        private async ValueTask HandleDeposit(IReadOnlyList<Transfer> transferEventList)
        {
            foreach (var transfer in transferEventList.Where(e => e.Status == TransferStatus.Completed))
            {
                try
                {
                    await _notificationService.SendPushTransferReceive(new SendPushTransferRequest()
                    {
                        SenderClientId = transfer.ClientId,
                        DestinationClientId = transfer.DestinationClientId,
                        Amount = transfer.Amount,
                        AssetSymbol = transfer.AssetSymbol,
                        DestinationPhoneNumber = transfer.DestinationPhoneNumber,
                        SenderPhoneNumber = transfer.SenderPhoneNumber
                    });

                    _logger.LogInformation("Client {clientId} [{walletId}] receive {amount} {assetSymbol} from {fromClientId} [{fromWalletId}]",
                        transfer.DestinationClientId, transfer.DestinationWalletId,
                        transfer.Amount, transfer.AssetSymbol, 
                        transfer.ClientId, transfer.WalletId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot push receive transfer to client {ClientId}", transfer.DestinationClientId);
                }
            }
        }
    }
}
