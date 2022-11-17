using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RequestReplyPattern.Lib.Factory;
using RequestReplyPattern.Lib.Model;
using RequestReplyPattern.Lib.Model.Options;

namespace RequestReplyPattern.Lib
{
    public interface IQueueProducer
    {
        Task<string> Produce(string jsonMessage);

        Task<string> GetMessageResponse(string messageId);

        Task<string> GetMessageState(string messageId);
    }
    public class QueueProducer : IQueueProducer
    {
        private readonly IServiceBusFactory _serviceBusFactory;
        private readonly ILogger<QueueProducer> _logger;
        private readonly QueueClientOptions _queueOptions;

        public QueueProducer(IServiceBusFactory serviceBusFactory, IOptions<QueueClientOptions> options, ILogger<QueueProducer> logger)
        {
            _serviceBusFactory = serviceBusFactory;
            _logger = logger;
            _queueOptions = options.Value;
        }

        public async Task<string> GetMessageResponse(string messageId)
        {
            await using ServiceBusSessionReceiver receiver = await _serviceBusFactory.CreateSessionReceiver(_queueOptions.ReplyQueueName, messageId);
            _logger.LogInformation($"Attempt to receive message with id {messageId}");
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
            if (receivedMessage == null)
                return string.Empty;

            _logger.LogInformation("Received response from queue");
            await receiver.SetSessionStateAsync(null);
            Message responseMessage = receivedMessage.Body.ToObjectFromJson<Message>(new System.Text.Json.JsonSerializerOptions());
            return responseMessage.Content;
        }

        public async Task<string> GetMessageState(string messageId)
        {
            await using ServiceBusSessionReceiver receiver = await _serviceBusFactory.CreateSessionReceiver(_queueOptions.ReplyQueueName, messageId);
            _logger.LogInformation($"Attempt to get state of message with id {messageId}");
            BinaryData data = await receiver.GetSessionStateAsync();
            if (data == null)
                return "No message";
            string response = data.ToString();
            _logger.LogInformation("Received state: {0}", response);
            return response;
        }

        public async Task<string> Produce(string jsonMessage)
        {
            ServiceBusSender sender = _serviceBusFactory.CreateSender(_queueOptions.RequestQueueName);

            var busMessage = new ServiceBusMessage(jsonMessage)
            {
                MessageId = Guid.NewGuid().ToString(),
            };

            _logger.LogInformation($"Send message with id {busMessage.MessageId}");
            await sender.SendMessageAsync(busMessage);
            return busMessage.MessageId;
        }
    }
}
