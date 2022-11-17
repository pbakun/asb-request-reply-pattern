using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RequestReplyPattern.Consumer;
using RequestReplyPattern.Lib;
using RequestReplyPattern.Lib.Factory;
using RequestReplyPattern.Lib.Model.Options;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.Configure<ServiceBusOptions>(options =>
        options.ConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnString") ?? throw new ArgumentNullException(options.ConnectionString));
    services.AddSingleton<IServiceBusFactory, ServiceBusFactory>();

    services.Configure<QueueClientOptions>(options =>
        options.RequestQueueName = Environment.GetEnvironmentVariable("RequestQueueName") ?? throw new ArgumentNullException(options.RequestQueueName));
    services.Configure<QueueClientOptions>(options =>
        options.ReplyQueueName = Environment.GetEnvironmentVariable("ReplyQueueName") ?? throw new ArgumentNullException(options.ReplyQueueName));

    services.AddTransient<IQueueConsumer, QueueConsumer>();
    services.AddHostedService<BusProcessor>();
});

var app = builder.Build();
app.Run();
