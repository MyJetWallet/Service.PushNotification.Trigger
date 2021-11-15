using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.Converter.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
    public class ConvertPushNotification
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ConvertPushNotification> _logger;

        public ConvertPushNotification(
            ISubscriber<SwapMessage> subscriber, 
            INotificationService notificationService, 
            ILogger<ConvertPushNotification> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
            
            var executor = new ExecutorWithRetry<SwapMessage>(
                HandleConvert, 
                _logger, 
                e => $"Cannot send ConvertPush to {e.AccountId1}; {e.AccountId2}.", 
                e => e.Id,
                10,
                5000);
            subscriber.Subscribe(executor.Execute);
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

            _logger.LogInformation(
                "Convert success. Swap from {fromAmount} {fromAssetSymbol} [{fromWalletId}] to {toAmount} {toAssetSymbol} [{toWalletId}]",
                convert.Volume1, convert.AssetId1, convert.WalletId1, convert.Volume2, convert.AssetId2,
                convert.WalletId2);
        }
    }
}