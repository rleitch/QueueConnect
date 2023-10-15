using System;

namespace QueueConnect.Client.Models
{
    public abstract class BaseMessage
    {
        public Guid ConversationGroupId { get; set; }

        public Guid ConversationHandle { get; set; }

        public DateTime MessageEnqueueTime { get; set; }

        public long MessageSequenceNumber { get; set; }

        public int MessageTypeId { get; set; }

        public string MessageTypeName { get; set; }

        public byte Priority { get; set; }

        public long QueuingOrder { get; set; }

        public int ServiceContractId { get; set; }

        public string ServiceContractName { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public byte Status { get; set; }

        public string Validation { get; set; }
    }
}