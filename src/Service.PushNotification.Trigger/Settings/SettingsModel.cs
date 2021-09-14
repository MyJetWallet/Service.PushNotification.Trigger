using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.PushNotification.Trigger.Settings
{
    public class SettingsModel
    {
        [YamlProperty("PushNotificationTrigger.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("PushNotificationTrigger.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("PushNotificationTrigger.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("PushNotificationTrigger.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }
        
        [YamlProperty("PushNotificationTrigger.AuthServiceBusHostPort")]
        public string AuthServiceBusHostPort { get; set; }

        [YamlProperty("PushNotificationTrigger.PushNotificationGrpcServiceUrl")]
        public string PushNotificationGrpcServiceUrl { get; set; }

    }
}
