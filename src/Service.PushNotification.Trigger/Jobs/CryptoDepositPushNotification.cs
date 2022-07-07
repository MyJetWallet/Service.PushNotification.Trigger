using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class CryptoDepositPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<CryptoDepositPushNotification> _logger;

        public CryptoDepositPushNotification(
            ISubscriber<Deposit> subscriber, 
            INotificationService notificationService, 
            ILogger<CryptoDepositPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var executor = new ExecutorWithRetry<Deposit>(
                HandleDeposit, 
                _logger, 
                e => $"Cannot send deposit push to client {e.ClientId}.", 
                e => $"{e.Id}::{e.Status}",
                10,
                5000);
            subscriber.Subscribe(executor.Execute);
        }

        private async ValueTask HandleDeposit(Deposit deposit)
        {
            if (deposit.Status == DepositStatus.Processed)
            {
                await _notificationService.SendPushCryptoDeposit(new DepositRequest()
                {
                    ClientId = deposit.BeneficiaryClientId,
                    Amount = (decimal) deposit.Amount,
                    Symbol = deposit.AssetSymbol
                });
                _logger.LogInformation("Client {clientId} [{walletId}] receive {amount} {assetSymbol}",
                    deposit.BeneficiaryClientId, deposit.WalletId, deposit.Amount, deposit.AssetSymbol);
            }
        }
    }
}
