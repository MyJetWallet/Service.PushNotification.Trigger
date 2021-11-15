using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.PushNotification.Trigger.Client;
using Service.PushNotification.Trigger.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(10);
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();



            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
