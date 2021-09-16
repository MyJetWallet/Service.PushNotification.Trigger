using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Bitgo.WithdrawalProcessor.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
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
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot send start withdrawal push to client {clientId}", withdrawal.ClientId);
                }
            }

            foreach (var withdrawal in events.Where(e => e.Status == WithdrawalStatus.Cancelled && e.WorkflowState == WithdrawalWorkflowState.OK))
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot send decline withdrawal to client {clientId}", withdrawal.ClientId);
                }
            }

            foreach (var withdrawal in events.Where(e => e.Status == WithdrawalStatus.Success && e.WorkflowState == WithdrawalWorkflowState.OK))
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot send complite withdrawal to client {clientId}", withdrawal.ClientId);
                }
                
            }
        }
    }
}