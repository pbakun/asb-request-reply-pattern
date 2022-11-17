using RequestReplyPattern.Lib;
using RequestReplyPattern.Lib.Factory;
using RequestReplyPattern.Lib.Model;
using RequestReplyPattern.Lib.Model.Options;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceBusOptions>(options => 
    options.ConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnString") ?? throw new ArgumentNullException(options.ConnectionString));
builder.Services.AddSingleton<IServiceBusFactory, ServiceBusFactory>();

builder.Services.Configure<QueueClientOptions>(options => 
    options.RequestQueueName = Environment.GetEnvironmentVariable("RequestQueueName") ?? throw new ArgumentNullException(options.RequestQueueName));
builder.Services.Configure<QueueClientOptions>(options =>
    options.ReplyQueueName = Environment.GetEnvironmentVariable("ReplyQueueName") ?? throw new ArgumentNullException(options.ReplyQueueName));
builder.Services.AddTransient<IQueueProducer, QueueProducer>();


var app = builder.Build();

app.MapPost("/send", async (string message) => {

    var payload = new Message
    {
        Content = message
    };
    string jsonMessage = JsonSerializer.Serialize<Message>(payload);

    using (var scope = app.Services.CreateScope())
    {
        IQueueProducer producer = scope.ServiceProvider.GetRequiredService<IQueueProducer>();
        var id = await producer.Produce(jsonMessage);
        if (!string.IsNullOrEmpty(id))
            return id;
    }

    return string.Empty;
});

app.MapGet("/getresponse", async (string messageId) =>
{
    using (var scope = app.Services.CreateScope())
    {
        IQueueProducer producer = scope.ServiceProvider.GetRequiredService<IQueueProducer>();
        return await producer.GetMessageResponse(messageId);
    }
});

app.MapGet("/getstate", async (string messageId) =>
{
    using (var scope = app.Services.CreateScope())
    {
        IQueueProducer producer = scope.ServiceProvider.GetRequiredService<IQueueProducer>();
        var res = await producer.GetMessageState(messageId);

        return res;
    }
});
app.Run();
