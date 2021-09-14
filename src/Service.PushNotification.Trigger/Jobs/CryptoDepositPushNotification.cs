using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class CryptoDepositPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<CryptoDepositPushNotification> _logger;

        public CryptoDepositPushNotification(ISubscriber<IReadOnlyList<Deposit>> subscriber, INotificationService notificationService, ILogger<CryptoDepositPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            subscriber.Subscribe(HandleDeposit);
        }

        private async ValueTask HandleDeposit(IReadOnlyList<Deposit> depositEventList)
        {
            foreach (var deposit in depositEventList.Where(e => e.Status == DepositStatus.Processed))
            {
                await _notificationService.SendPushCryptoDeposit(new DepositRequest()
                {
                    ClientId = deposit.ClientId,
                    Amount = (decimal)deposit.Amount,
                    Symbol = deposit.AssetSymbol
                });

                _logger.LogInformation("Client {clientId} [{walletId}] receive {amount} {assetSymbol}", deposit.ClientId, deposit.WalletId, deposit.Amount, deposit.AssetSymbol);
            }
        }
    }
}
