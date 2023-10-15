using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using QueueConnect.Client.Helpers;
using QueueConnect.Client.Mappers;
using QueueConnect.Client.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueConnect.Client
{
    public class QueueClient : IQueueClient
    {
        private readonly QueueClientOptions _serviceBrokerSettings;
        private readonly IDistributedCache _distributedCache;

        public QueueClient(IOptions<QueueClientOptions> options, IDistributedCache distributedCache)
        {
            _serviceBrokerSettings = options.Value;
            _distributedCache = distributedCache;
        }

        public async Task ConsumeAsync(Func<Message, Task> messageCallback, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_serviceBrokerSettings.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                var sqlTransaction = transaction as SqlTransaction;
                using var command = connection.CreateCommand();
                command.Transaction = sqlTransaction;
                command.CommandText = $@"WAITFOR (RECEIVE TOP(1) * FROM [{_serviceBrokerSettings.SubscriberQueueName}]), TIMEOUT 25000;";
                Message message = null;
                using (var sqlDataReader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await sqlDataReader.ReadAsync(cancellationToken))
                    {
                        message = MessageMapper.Map(sqlDataReader);
                    }
                }
                if (message != null)
                {
                    await HandleMessageAsync(messageCallback, message, connection, sqlTransaction, cancellationToken);
                }
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }

        public async Task ConsumeAsync<T>(Func<Message<T>, Task> messageCallback, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_serviceBrokerSettings.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                var sqlTransaction = transaction as SqlTransaction;
                using var command = connection.CreateCommand();
                command.Transaction = sqlTransaction;
                command.CommandText = $@"WAITFOR (RECEIVE TOP(1) * FROM [{_serviceBrokerSettings.SubscriberQueueName}]), TIMEOUT 25000;";
                using var sqlDataReader = await command.ExecuteReaderAsync(cancellationToken);
                if (await sqlDataReader.ReadAsync(cancellationToken))
                {
                    var message = await MessageMapper.MapAsync<T>(MessageMapper.Map(sqlDataReader), cancellationToken);
                    await HandleMessageAsync(messageCallback, message, connection, sqlTransaction, cancellationToken);
                }
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }

        public async Task PublishAsync(string message, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_serviceBrokerSettings.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction as SqlTransaction;
                    command.CommandText = $@"
                        DECLARE @ConversationHandle UNIQUEIDENTIFIER;
                        BEGIN DIALOG @ConversationHandle
                        FROM SERVICE [{_serviceBrokerSettings.SubscriberServiceName}]
                        TO SERVICE @toService
                        ON CONTRACT [{_serviceBrokerSettings.ContractName}]
                        WITH ENCRYPTION = OFF;
                        SEND ON CONVERSATION @ConversationHandle
                        MESSAGE TYPE [{_serviceBrokerSettings.MessageTypeName}] (@message)";
                    command.Parameters.AddWithValue("@toService", _serviceBrokerSettings.PublisherServiceName);
                    command.Parameters.AddWithValue("@message", message);
                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken)
        {
            var bytes = await SerializationHelper.SerializeAsync(message, cancellationToken);
            await PublishAsync(Encoding.UTF8.GetString(bytes), cancellationToken);
        }

        private async Task HandleMessageAsync(Func<Message, Task> messageCallback, Message message, SqlConnection sqlConnection, SqlTransaction sqlTransaction, CancellationToken cancellationToken = default)
        {
            try
            {
                await messageCallback(message);
                using (var command = new SqlCommand("END CONVERSATION @conversationHandle", sqlConnection, sqlTransaction))
                {
                    command.Parameters.AddWithValue("@conversationHandle", message.ConversationHandle);
                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                }
                await sqlTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var messageMetaData = new MessageMetaData();
                var cacheKey = message.ConversationHandle.ToString();
                var bytes = await _distributedCache.GetAsync(cacheKey, cancellationToken);
                if (bytes != null)
                {
                    messageMetaData = await SerializationHelper.DeserializeAsync<MessageMetaData>(bytes, cancellationToken);
                }

                messageMetaData.Errors.Add(DateTimeOffset.UtcNow, ex.Message);
                bytes = await SerializationHelper.SerializeAsync(messageMetaData, cancellationToken);
                await _distributedCache.SetAsync(cacheKey, bytes, cancellationToken);

                if (messageMetaData.Errors.Count >= 3)
                {
                    using (var command = new SqlCommand("END CONVERSATION @conversationHandle WITH ERROR = @errorCode DESCRIPTION = @description;", sqlConnection, sqlTransaction))
                    {
                        command.Parameters.AddWithValue("@conversationHandle", message.ConversationHandle);
                        command.Parameters.AddWithValue("@errorCode", 1);
                        command.Parameters.AddWithValue("@description", "Poison");
                        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    await sqlTransaction.CommitAsync(cancellationToken);
                }
                else
                {
                    await sqlTransaction.RollbackAsync(cancellationToken);
                }
            }
        }

        private async Task HandleMessageAsync<T>(Func<Message<T>, Task> messageCallback, Message<T> message, SqlConnection sqlConnection, SqlTransaction sqlTransaction, CancellationToken cancellationToken = default)
        {
            try
            {
                await messageCallback(message);
                using (var command = new SqlCommand("END CONVERSATION @conversationHandle", sqlConnection, sqlTransaction))
                {
                    command.Parameters.AddWithValue("@conversationHandle", message.ConversationHandle);
                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                }
                await sqlTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var messageMetaData = new MessageMetaData();
                var cacheKey = message.ConversationHandle.ToString();
                var bytes = await _distributedCache.GetAsync(cacheKey, cancellationToken);
                if (bytes != null)
                {
                    messageMetaData = await SerializationHelper.DeserializeAsync<MessageMetaData>(bytes, cancellationToken);
                }

                messageMetaData.Errors.Add(DateTimeOffset.UtcNow, ex.Message);
                bytes = await SerializationHelper.SerializeAsync(messageMetaData, cancellationToken);
                await _distributedCache.SetAsync(cacheKey, bytes, cancellationToken);

                if (messageMetaData.Errors.Count >= 3)
                {
                    using (var command = new SqlCommand("END CONVERSATION @conversationHandle WITH ERROR = @errorCode DESCRIPTION = @description;", sqlConnection, sqlTransaction))
                    {
                        command.Parameters.AddWithValue("@conversationHandle", message.ConversationHandle);
                        command.Parameters.AddWithValue("@errorCode", 1);
                        command.Parameters.AddWithValue("@description", "Poison");
                        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    await sqlTransaction.CommitAsync(cancellationToken);
                }
                else
                {
                    await sqlTransaction.RollbackAsync(cancellationToken);
                }
            }
        }
    }
}
