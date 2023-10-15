using QueueConnect.Client;

namespace QueueConnect.Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IQueueClient _serviceBroker;

        public Worker(ILogger<Worker> logger, IQueueClient serviceBroker)
        {
            _logger = logger;
            _serviceBroker = serviceBroker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var mockMessage = new
                {
                    Id = Guid.NewGuid(),
                    Name = Path.GetRandomFileName(),
                };
                await _serviceBroker.PublishAsync(mockMessage, stoppingToken);
            }
        }
    }
}