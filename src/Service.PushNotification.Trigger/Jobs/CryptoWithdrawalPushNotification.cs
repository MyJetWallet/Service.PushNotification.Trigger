using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Bitgo.WithdrawalProcessor.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class CryptoWithdrawalPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<CryptoWithdrawalPushNotification> _logger;

        public CryptoWithdrawalPushNotification(ISubscriber<Withdrawal> subscriber, 
            INotificationService notificationService,
            ILogger<CryptoWithdrawalPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var executor = new ExecutorWithRetry<Withdrawal>(
                HandleWithdrawal, 
                _logger, 
                e => $"Cannot send withdrawal push to client {e.ClientId}.", 
                e => e.Id.ToString(),
                10,
                5000);
            subscriber.Subscribe(executor.Execute);
        }

        private async ValueTask HandleWithdrawal(Withdrawal withdrawal)
        {
            if (withdrawal.Status == WithdrawalStatus.BlockchainPending &&
                withdrawal.WorkflowState == WithdrawalWorkflowState.OK)
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

            if (withdrawal.Status == WithdrawalStatus.Cancelled &&
                withdrawal.WorkflowState == WithdrawalWorkflowState.OK)
            {
                await _notificationService.SendPushCryptoWithdrawalDecline(new CryptoWithdrawalRequest()
                {
                    ClientId = withdrawal.ClientId,
                    Symbol = withdrawal.AssetSymbol,
                    Amount = (decimal) withdrawal.Amount,
                    Destination = withdrawal.ToAddress
                });

                _logger.LogInformation(
                    $"Client {withdrawal.ClientId} [{withdrawal.WalletId}] decline withdrawal {withdrawal.Amount} {withdrawal.AssetSymbol}",
                    withdrawal.ClientId, withdrawal.WalletId, withdrawal.Amount, withdrawal.AssetSymbol);
            }

            if (withdrawal.Status == WithdrawalStatus.Success && withdrawal.WorkflowState == WithdrawalWorkflowState.OK)
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
}