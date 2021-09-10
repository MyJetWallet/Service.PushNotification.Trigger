using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.PushNotification.Trigger.Grpc;
using Service.PushNotification.Trigger.Grpc.Models;
using Service.PushNotification.Trigger.Settings;

namespace Service.PushNotification.Trigger.Services
{
    public class HelloService: IHelloService
    {
        private readonly ILogger<HelloService> _logger;

        public HelloService(ILogger<HelloService> logger)
        {
            _logger = logger;
        }

        public Task<HelloMessage> SayHelloAsync(HelloRequest request)
        {
            _logger.LogInformation("Hello from {name}", request.Name);

            return Task.FromResult(new HelloMessage
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
