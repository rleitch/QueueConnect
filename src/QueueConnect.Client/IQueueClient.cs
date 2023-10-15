using QueueConnect.Client.Models;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace QueueConnect.Client
{
    public interface IQueueClient
    {
        Task PublishAsync(string message, CancellationToken cancellationToken);

        Task PublishAsync<T>(T message, CancellationToken cancellationToken);

        Task ConsumeAsync(Func<Message, Task> messageCallback, CancellationToken cancellationToken);

        Task ConsumeAsync<T>(Func<Message<T>, Task> messageCallback, CancellationToken cancellationToken);
    }
}
