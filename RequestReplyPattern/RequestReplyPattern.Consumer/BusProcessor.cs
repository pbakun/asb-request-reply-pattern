using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RequestReplyPattern.Lib;
using RequestReplyPattern.Lib.Factory;
using RequestReplyPattern.Lib.Model;
using RequestReplyPattern.Lib.Model.Options;
using System.Text.Json;

namespace RequestReplyPattern.Consumer
{
    public class BusProcessor : BackgroundService
    {
        private readonly QueueClientOptions _queueOptions;
        private readonly IServiceBusFactory _serviceBusFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BusProcessor> _logger;
        public BusProcessor(IOptions<QueueClientOptions> options, 
                            IServiceBusFactory serviceBusFactory,
                            IServiceScopeFactory scopeFactory,
                            ILogger<BusProcessor> logger)
        {
            _queueOptions = options.Value;
            _serviceBusFactory = serviceBusFactory;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ServiceBusProcessor processor = _serviceBusFactory.CreateProcessor(_queueOptions.RequestQueueName);

            processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

            await processor.StartProcessingAsync(stoppingToken);
        }

        private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError("Error in processed message");
            return Task.CompletedTask;
        }

        private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            Message message = arg.Message.Body.ToObjectFromJson<Message>(new JsonSerializerOptions());
            using (var scope = _scopeFactory.CreateScope())
            {
                IQueueConsumer consumer = scope.ServiceProvider.GetRequiredService<IQueueConsumer>();
                await consumer.Consume(message, _queueOptions.ReplyQueueName, arg.Message.MessageId);
                await arg.CompleteMessageAsync(arg.Message);
            }
        }
    }
}
