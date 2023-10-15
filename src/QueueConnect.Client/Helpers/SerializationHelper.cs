using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace QueueConnect.Client.Helpers
{
    public static class SerializationHelper
    {
        public static async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream(data);
            return await JsonSerializer.DeserializeAsync<T>(memoryStream, cancellationToken: cancellationToken);
        }

        public static async Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, data, cancellationToken: cancellationToken);
            return memoryStream.ToArray();
        }
    }
}
