using Microsoft.Data.SqlClient;
using QueueConnect.Client.Constants;
using QueueConnect.Client.Helpers;
using System.Threading.Tasks;
using System.Threading;
using QueueConnect.Client.Models;

namespace QueueConnect.Client.Mappers
{
    public static class MessageMapper
    {
        public static async Task<Message<T>> MapAsync<T>(Message message, CancellationToken cancellationToken)
        {
            var messageBody = await SerializationHelper.DeserializeAsync<T>(message.MessageBody, cancellationToken);
            return new Message<T>
            {
                ConversationGroupId = message.ConversationGroupId,
                ConversationHandle = message.ConversationHandle,
                MessageBody = messageBody,
                MessageEnqueueTime = message.MessageEnqueueTime,
                MessageSequenceNumber = message.MessageSequenceNumber,
                MessageTypeId = message.MessageTypeId,
                MessageTypeName = message.MessageTypeName,
                Priority = message.Priority,
                QueuingOrder = message.QueuingOrder,
                ServiceContractId = message.ServiceContractId,
                ServiceContractName = message.ServiceContractName,
                ServiceId = message.ServiceId,
                ServiceName = message.ServiceName,
                Status = message.Status,
                Validation = message.Validation
            };
        }

        public static Message Map(SqlDataReader sqlDataReader)
        {
            return new Message
            {
                ConversationGroupId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal(ColumnNames.ConversationGroupId)),
                ConversationHandle = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal(ColumnNames.ConversationHandle)),
                MessageBody = sqlDataReader.GetSqlBytes(sqlDataReader.GetOrdinal(ColumnNames.MessageBody)).Value,
                MessageEnqueueTime = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal(ColumnNames.MessageEnqueueTime)),
                MessageSequenceNumber = sqlDataReader.GetInt64(sqlDataReader.GetOrdinal(ColumnNames.MessageSequenceNumber)),
                MessageTypeId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal(ColumnNames.MessageTypeId)),
                MessageTypeName = sqlDataReader.GetString(sqlDataReader.GetOrdinal(ColumnNames.MessageTypeName)),
                Priority = sqlDataReader.GetByte(sqlDataReader.GetOrdinal(ColumnNames.Priority)),
                QueuingOrder = sqlDataReader.GetInt64(sqlDataReader.GetOrdinal(ColumnNames.QueuingOrder)),
                ServiceContractId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal(ColumnNames.ServiceContractId)),
                ServiceContractName = sqlDataReader.GetString(sqlDataReader.GetOrdinal(ColumnNames.ServiceContractName)),
                ServiceId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal(ColumnNames.ServiceId)),
                ServiceName = sqlDataReader.GetString(sqlDataReader.GetOrdinal(ColumnNames.ServiceName)),
                Status = sqlDataReader.GetByte(sqlDataReader.GetOrdinal(ColumnNames.Status)),
                Validation = sqlDataReader.GetString(sqlDataReader.GetOrdinal(ColumnNames.Validation)),
            };
        }

        public static async Task<Message<T>> MapAsync<T>(SqlDataReader sqlDataReader, CancellationToken cancellationToken) =>
            await MapAsync<T>(Map(sqlDataReader), cancellationToken);
    }
}
