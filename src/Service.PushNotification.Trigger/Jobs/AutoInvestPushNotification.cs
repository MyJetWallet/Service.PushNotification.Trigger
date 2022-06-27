using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Authorization.ServiceBus;
using MyJetWallet.Sdk.ServiceBus;
using Service.AutoInvestManager.Domain.Models;
using Service.ClientProfile.Domain.Models;
using Service.KYC.Domain.Models.Enum;
using Service.KYC.Domain.Models.Messages;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class AutoInvestPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AutoInvestPushNotification> _logger;

        public AutoInvestPushNotification(
            INotificationService notificationService, 
            ILogger<AutoInvestPushNotification> logger,
            ISubscriber<InvestInstruction> instructionSubscriber,
            ISubscriber<InvestOrder> orderSubscriber)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var instructionExecutor = new ExecutorWithRetry<InvestInstruction>(
                HandleEvent, 
                _logger, 
                e => $"Cannot handle invest instructions for client {e.ClientId}.", 
                e => e.Id,
                10,
                500);
            
            instructionSubscriber.Subscribe(instructionExecutor.Execute);
            
            var orderExecutor = new ExecutorWithRetry<InvestOrder>(
                HandleEvent, 
                _logger, 
                e => $"Cannot handle invest order for client {e.ClientId}.", 
                e => e.Id,
                10,
                500);
            
            orderSubscriber.Subscribe(orderExecutor.Execute);
            
        }

        private async ValueTask HandleEvent(InvestOrder item)
        {
            if (item.Status == OrderStatus.Executed)
                await _notificationService.SendAutoInvestExecute(new AutoInvestExecuteRequest
                {
                    ClientId = item.ClientId,
                    ToAsset = item.ToAsset,
                    ToAmount = item.ToAmount,
                    FromAmount = item.FromAmount,
                    FromAsset = item.FromAsset,
                    ExecutionTime = item.ExecutionTime
                });
            if (item.Status == OrderStatus.Failed)
            {
                switch (item.ErrorCode)
                {
                    case ErrorCode.NoError:
                        return;
                    case ErrorCode.LowBalance:
                        await _notificationService.SendAutoInvestFail_LowBalance(new AutoInvestFailRequest
                        {
                            ClientId = item.ClientId,
                            ToAsset = item.ToAsset,
                            FromAmount = item.FromAmount,
                            FromAsset = item.FromAsset,
                            FailTime = item.ExecutionTime == DateTime.MinValue ? DateTime.UtcNow : item.ExecutionTime
                        });
                        return;
                    case ErrorCode.PairNotSupported:
                        await _notificationService.SendAutoInvestFail_InvalidPair(new AutoInvestFailRequest
                        {
                            ClientId = item.ClientId,
                            ToAsset = item.ToAsset,
                            FromAmount = item.FromAmount,
                            FromAsset = item.FromAsset,
                            FailTime = item.ExecutionTime == DateTime.MinValue ? DateTime.UtcNow : item.ExecutionTime
                        });
                        return;
                    case ErrorCode.InternalServerError:
                        await _notificationService.SendAutoInvestFail_InternalError(new AutoInvestFailRequest
                        {
                            ClientId = item.ClientId,
                            ToAsset = item.ToAsset,
                            FromAmount = item.FromAmount,
                            FromAsset = item.FromAsset,
                            FailTime = item.ExecutionTime == DateTime.MinValue ? DateTime.UtcNow : item.ExecutionTime
                        });
                        return;
                }
            }
                
        }

        private async ValueTask HandleEvent(InvestInstruction item)
        {
            switch (item.ScheduleType)
            {
                case ScheduleType.Daily:
                    await _notificationService.SendAutoInvestCreate_Daily(new AutoInvestCreateRequest
                    {
                        ClientId = item.ClientId,
                        ToAsset = item.ToAsset,
                        FromAmount = item.FromAmount,
                        FromAsset = item.FromAsset
                    });
                    return;
                case ScheduleType.Weekly:
                    await _notificationService.SendAutoInvestCreate_Weekly(new AutoInvestCreateRequest
                    {
                        ClientId = item.ClientId,
                        ToAsset = item.ToAsset,
                        FromAmount = item.FromAmount,
                        FromAsset = item.FromAsset
                    });
                    return;;
                case ScheduleType.Biweekly:
                    await _notificationService.SendAutoInvestCreate_BiWeekly(new AutoInvestCreateRequest
                    {
                        ClientId = item.ClientId,
                        ToAsset = item.ToAsset,
                        FromAmount = item.FromAmount,
                        FromAsset = item.FromAsset
                    });
                    return;;
                case ScheduleType.Monthly:
                    await _notificationService.SendAutoInvestCreate_Monthly(new AutoInvestCreateRequest
                    {
                        ClientId = item.ClientId,
                        ToAsset = item.ToAsset,
                        FromAmount = item.FromAmount,
                        FromAsset = item.FromAsset
                    });
                    return;;
            }
            
        }
    }
}