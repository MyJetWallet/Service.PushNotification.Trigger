using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.WithdrawalProcessor.Domain.Models;
using Service.Liquidity.Converter.Domain.Models;
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

    public class CryptoWithdrawalPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<CryptoWithdrawalPushNotification> _logger;

        public CryptoWithdrawalPushNotification(ISubscriber<IReadOnlyList<Withdrawal>> subscriber, INotificationService notificationService, ILogger<CryptoWithdrawalPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            subscriber.Subscribe(HandleWithdrawal);
        }

        private async ValueTask HandleWithdrawal(IReadOnlyList<Withdrawal> events)
        {
            foreach (var withdrawal in events.Where(e => e.Status == WithdrawalStatus.BlockchainPending && e.WorkflowState == WithdrawalWorkflowState.OK))
            {
                await _notificationService.SendPushCryptoWithdrawalStarted(new CryptoWithdrawalRequest()
                {
                    ClientId = withdrawal.ClientId,
                    Symbol = withdrawal.AssetSymbol,
                    Amount = (decimal)withdrawal.Amount,
                    Destination = withdrawal.ToAddress
                });

                _logger.LogInformation($"Client {withdrawal.ClientId} [{withdrawal.WalletId}] start withdrawal {withdrawal.Amount} {withdrawal.AssetSymbol}", 
                    withdrawal.ClientId, withdrawal.WalletId, withdrawal.Amount, withdrawal.AssetSymbol);
            }

            foreach (var withdrawal in events.Where(e => e.Status == WithdrawalStatus.Cancelled && e.WorkflowState == WithdrawalWorkflowState.OK))
            {
                await _notificationService.SendPushCryptoWithdrawalDecline(new CryptoWithdrawalRequest()
                {
                    ClientId = withdrawal.ClientId,
                    Symbol = withdrawal.AssetSymbol,
                    Amount = (decimal)withdrawal.Amount,
                    Destination = withdrawal.ToAddress
                });

                _logger.LogInformation($"Client {withdrawal.ClientId} [{withdrawal.WalletId}] decline withdrawal {withdrawal.Amount} {withdrawal.AssetSymbol}",
                    withdrawal.ClientId, withdrawal.WalletId, withdrawal.Amount, withdrawal.AssetSymbol);
            }

            foreach (var withdrawal in events.Where(e => e.Status == WithdrawalStatus.Success && e.WorkflowState == WithdrawalWorkflowState.OK))
            {
                await _notificationService.SendPushCryptoWithdrawalComplete(new CryptoWithdrawalRequest()
                {
                    ClientId = withdrawal.ClientId,
                    Symbol = withdrawal.AssetSymbol,
                    Amount = (decimal)withdrawal.Amount,
                    Destination = withdrawal.ToAddress
                });

                _logger.LogInformation($"Client {withdrawal.ClientId} [{withdrawal.WalletId}] complete withdrawal {withdrawal.Amount} {withdrawal.AssetSymbol}",
                    withdrawal.ClientId, withdrawal.WalletId, withdrawal.Amount, withdrawal.AssetSymbol);
            }
        }
    }

    public class ConvertPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ConvertPushNotification> _logger;

        public ConvertPushNotification(ISubscriber<SwapMessage> subscriber, INotificationService notificationService, ILogger<ConvertPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            subscriber.Subscribe(HandleConvert);
        }

        private async ValueTask HandleConvert(SwapMessage convert)
        {
            await _notificationService.SendPushCryptoConvert(new ConvertRequest()
            {
                ClientId = convert.AccountId1,
                FromAmount = decimal.Parse(convert.Volume1),
                FromSymbol = convert.AssetId1,
                ToAmount = decimal.Parse(convert.Volume2),
                ToSymbol = convert.AssetId2
            });

            await _notificationService.SendPushCryptoConvert(new ConvertRequest()
            {
                ClientId = convert.AccountId2,
                FromAmount = decimal.Parse(convert.Volume2),
                FromSymbol = convert.AssetId2,
                ToAmount = decimal.Parse(convert.Volume1),
                ToSymbol = convert.AssetId1
            });

            _logger.LogInformation("Convert success. Swap from {fromAmount} {fromAssetSymbol} [{fromWalletId}] to {toAmount} {toAssetSymbol} [{toWalletId}]",
                convert.Volume1, convert.AssetId1, convert.WalletId1, convert.Volume2, convert.AssetId2, convert.WalletId2);
        }
    }
}
