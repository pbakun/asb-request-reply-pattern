using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using RequestReplyPattern.Lib.Factory;
using RequestReplyPattern.Lib.Model;
using System.Text.Json;

namespace RequestReplyPattern.Lib
{
    public interface IQueueConsumer
    {
        public Task Consume<T>(T jsonMessage, string queueName, string sessionId);
    }
    public class QueueConsumer : IQueueConsumer
    {
        private readonly IServiceBusFactory _serviceBusFactory;
        private readonly ILogger<QueueConsumer> _logger;

        public QueueConsumer(IServiceBusFactory serviceBusFactory, ILogger<QueueConsumer> logger)
        {
            _serviceBusFactory = serviceBusFactory;
            _logger = logger;
        }

        public async Task Consume<T>(T jsonMessage, string queueName, string sessionId)
        {
            _logger.LogInformation("Received message: {0}", jsonMessage?.ToString());
            await SetSessionState(queueName, sessionId, MessageState.Processing);
            string message = JsonSerializer.Serialize(new Message
            {
                Content = $"Thanks for message with session id {sessionId}! This is my response"
            });

            ServiceBusSender sender = _serviceBusFactory.CreateSender(queueName);

            var busMessage = new ServiceBusMessage(message)
            {
                SessionId = sessionId
            };

            //delay to simulate longer running process
            await Task.Delay(TimeSpan.FromSeconds(15));

            _logger.LogInformation("Sending response to messageId {0}", sessionId);
            await sender.SendMessageAsync(busMessage);
            await SetSessionState(queueName, sessionId, MessageState.Ready);
        }

        private async Task SetSessionState(string queueName, string sessionId, MessageState messageState)
        {
            await using ServiceBusSessionReceiver receiver = await _serviceBusFactory.CreateSessionReceiver(queueName, sessionId);
            _logger.LogInformation("Setting state session id {0} to {1}", sessionId, messageState.ToString());
            var state = new BinaryData(messageState.ToString());
            await receiver.SetSessionStateAsync(state);
        }
    }
}
