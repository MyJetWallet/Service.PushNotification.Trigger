using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.FutureSimpleToken.ServiceBus;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class FutureSimpleTokenInitPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<CryptoDepositPushNotification> _logger;

        public FutureSimpleTokenInitPushNotification(
            ISubscriber<InitialFuturePayment> subscriber, 
            INotificationService notificationService, 
            ILogger<CryptoDepositPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var executor = new ExecutorWithRetry<InitialFuturePayment>(
                HandleDeposit, 
                _logger, 
                e => $"Cannot send deposit push to client {e.WalletId.Replace("SP-", "")}.", 
                e => $"{e.WalletId}",
                10,
                5000);
            
            subscriber.Subscribe(executor.Execute);
        }

        private async ValueTask HandleDeposit(InitialFuturePayment deposit)
        {
            await _notificationService.SendPushCryptoDeposit(new DepositRequest()
                {
                    ClientId = deposit.WalletId.Replace("SP-", ""),
                    Amount = (decimal) deposit.Balance,
                    Symbol = deposit.AssetId
                });
            
                _logger.LogInformation("Client {clientId} [{walletId}] receive {amount} {assetSymbol}",
                    deposit.WalletId.Replace("SP-", ""), deposit.WalletId, deposit.Balance, deposit.AssetId);
        }
    }
}
