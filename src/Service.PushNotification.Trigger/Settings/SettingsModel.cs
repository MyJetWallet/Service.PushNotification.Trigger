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
    }
}
