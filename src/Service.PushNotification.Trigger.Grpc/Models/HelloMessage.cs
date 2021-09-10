using System.Runtime.Serialization;
using Service.PushNotification.Trigger.Domain.Models;

namespace Service.PushNotification.Trigger.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}