using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using RequestReplyPattern.Lib.Model.Options;
using System.Collections.Concurrent;

namespace RequestReplyPattern.Lib.Factory
{
    public interface IServiceBusFactory
    {
        ServiceBusSender CreateSender(string queueName);
        ServiceBusReceiver CreateReceiver(string queueName);
        ServiceBusSessionProcessor CreateSessionProcessor(string queueName);
        ServiceBusProcessor CreateProcessor(string queueName);
        Task<ServiceBusSessionReceiver> CreateSessionReceiver(string queueName, string sessionId);
        Task<ServiceBusSessionReceiver> CreateAnySessionReceiver(string queueName);
    }
    public class ServiceBusFactory : IServiceBusFactory
    {
        private readonly ServiceBusClient _client;

        private readonly ConcurrentDictionary<string, ServiceBusSender> _sendersCollection;
        private readonly ConcurrentDictionary<string, ServiceBusReceiver> _receiversCollection;

        public ServiceBusFactory(IOptions<ServiceBusOptions> options)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);

            _sendersCollection = new ConcurrentDictionary<string, ServiceBusSender>();
            _receiversCollection = new ConcurrentDictionary<string, ServiceBusReceiver>();
        }

        public ServiceBusSender CreateSender(string queueName)
        {
            if(string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(queueName);
            return _sendersCollection.GetOrAdd(queueName, name => _client.CreateSender(name));
        }

        public ServiceBusReceiver CreateReceiver(string queueName)
        {
            if(string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(queueName);
            return _receiversCollection.GetOrAdd(queueName, name => _client.CreateReceiver(name));
        }
        public async Task<ServiceBusSessionReceiver> CreateAnySessionReceiver(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(queueName);

            var receiverOptions = new ServiceBusSessionReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            };
            return await _client.AcceptNextSessionAsync(queueName, receiverOptions);
        }
        public async Task<ServiceBusSessionReceiver> CreateSessionReceiver(string queueName, string sessionId)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(queueName);
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException(sessionId);

            var receiverOptions = new ServiceBusSessionReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            };
            return await _client.AcceptSessionAsync(queueName, sessionId, receiverOptions);
        }
        public ServiceBusProcessor CreateProcessor(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(queueName);
            var options = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1
            };
            return _client.CreateProcessor(queueName, options);
        }
        public ServiceBusSessionProcessor CreateSessionProcessor(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException(queueName);
            var options = new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = 2
            };
            return _client.CreateSessionProcessor(queueName, options);
        }
    }
}
