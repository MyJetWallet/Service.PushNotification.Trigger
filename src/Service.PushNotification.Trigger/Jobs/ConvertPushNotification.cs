using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Liquidity.Converter.Domain.Models;
using Service.PushNotification.Grpc;
using Service.PushNotification.Grpc.Models.Requests;

namespace Service.PushNotification.Trigger.Jobs
{
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot send ConvertPush to {clientId1}; {clientId2}", convert.AccountId1, convert.AccountId2);
            }
        }
    }
}