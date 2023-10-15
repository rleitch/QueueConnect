using QueueConnect.Client;
using System.Text;

namespace QueueConnect.Subscriber
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IQueueClient _serviceBroker;
        private readonly Random random = new Random();

        public Worker(ILogger<Worker> logger, IQueueClient serviceBroker)
        {
            _logger = logger;
            _serviceBroker = serviceBroker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _serviceBroker.ConsumeAsync(async (message) =>
                {
                    _logger.LogInformation("{time}: {conversationId} - {body}", DateTimeOffset.Now, message.ConversationHandle, Encoding.UTF8.GetString(message.MessageBody));
                    var randomNumber = random.Next(1, 6);
                    await Task.Delay(randomNumber * 1000);
                }, stoppingToken);
            }
        }
    }
}