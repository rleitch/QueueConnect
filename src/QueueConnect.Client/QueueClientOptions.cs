namespace QueueConnect.Client
{
    public class QueueClientOptions
    {
        public string ConnectionString { get; set; }

        public string MessageTypeName { get; set; }

        public string ContractName { get; set; }

        public string SubscriberQueueName { get; set; }

        public string SubscriberServiceName { get; set; }

        public string PublisherServiceName { get; set; }
    }
}