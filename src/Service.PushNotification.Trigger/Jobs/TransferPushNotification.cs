using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.InternalTransfer.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class TransferPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<TransferPushNotification> _logger;

        public TransferPushNotification(ISubscriber<Transfer> subscriber,
            INotificationService notificationService, 
            ILogger<TransferPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;

            var executor = new ExecutorWithRetry<Transfer>(
                HandleTransfer, 
                _logger, 
                e => $"Cannot handle transfer for client {e.ClientId}.", 
                e => e.Id.ToString(),
                10,
                5000);
            subscriber.Subscribe(executor.Execute);
        }

        private async ValueTask HandleTransfer(Transfer transfer)
        {
            await _notificationService.SendPushTransferSend(new SendPushTransferRequest()
            {
                SenderClientId = transfer.ClientId,
                DestinationClientId = transfer.DestinationClientId,
                Amount = transfer.Amount,
                AssetSymbol = transfer.AssetSymbol,
                DestinationPhoneNumber = transfer.DestinationPhoneNumber,
                SenderPhoneNumber = transfer.SenderPhoneNumber
            });
            _logger.LogInformation("Client {clientId} [{walletId}] send {amount} {assetSymbol} to {toClientId} [{toWalletId}]",
                transfer.ClientId, transfer.WalletId,
                transfer.Amount, transfer.AssetSymbol, 
                transfer.DestinationClientId, transfer.DestinationWalletId);
                
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
    }
}
