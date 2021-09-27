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
    public class TransferSendPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<TransferSendPushNotification> _logger;

        public TransferSendPushNotification(ISubscriber<IReadOnlyList<Transfer>> subscriber,
            INotificationService notificationService, 
            ILogger<TransferSendPushNotification> logger)
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
                    await _notificationService.SendPushTransferSend(new SendPushTransferSendRequest()
                    {
                        ClientId = transfer.ClientId,
                        Amount = transfer.Amount,
                        AssetSymbol = transfer.AssetSymbol,
                        DestinationPhoneNumber = transfer.DestinationPhoneNumber,
                        SenderPhoneNumber = transfer.SenderPhoneNumber
                    });

                    _logger.LogInformation("Client {clientId} [{walletId}] send {amount} {assetSymbol} to {toClientId} [{toWalletId}]",
                        transfer.ClientId, transfer.WalletId,
                        transfer.Amount, transfer.AssetSymbol, 
                        transfer.DestinationClientId, transfer.DestinationWalletId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot send transfer push to client {ClientId}", transfer.ClientId);
                }
            }
        }
    }
}
