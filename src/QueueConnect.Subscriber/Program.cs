using QueueConnect.Client.Extensions;

namespace QueueConnect.Subscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                    services.AddServiceBroker();
                })
                .Build();

            host.Run();
        }
    }
}