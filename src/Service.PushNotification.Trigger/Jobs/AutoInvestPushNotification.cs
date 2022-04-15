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
                await _notificationService.SendAutoInvestFail(new AutoInvestFailRequest
                {
                    ClientId = item.ClientId,
                    ToAsset = item.ToAsset,
                    FailureReason = item.ErrorText,
                    FromAmount = item.FromAmount,
                    FromAsset = item.FromAsset,
                    FailTime = item.ExecutionTime
                });
        }

        private async ValueTask HandleEvent(InvestInstruction item)
        {
            await _notificationService.SendAutoInvestCreate(new AutoInvestCreateRequest
            {
                ClientId = item.ClientId,
                ToAsset = item.ToAsset,
                ScheduleType = item.ScheduleType.ToString().ToLower(),
                FromAmount = item.FromAmount,
                FromAsset = item.FromAsset
            });
        }
    }
}