using System.ServiceModel;
using System.Threading.Tasks;
using Service.PushNotification.Trigger.Grpc.Models;

namespace Service.PushNotification.Trigger.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}