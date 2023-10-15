namespace QueueConnect.Client.Models
{
    public class Message : BaseMessage
    {
        public byte[] MessageBody { get; set; }
    }

    public class Message<T> : BaseMessage
    {
        public T MessageBody { get; set; }
    }
}