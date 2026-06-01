using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WolfPage.Api.Application.Messaging;
using WolfPage.Api.Infrastructure.Options;

namespace WolfPage.Api.Infrastructure.Messaging;

public sealed class AzureServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly AzureServiceBusOptions _options;
    private readonly ILogger<AzureServiceBusPublisher> _logger;
    private readonly ServiceBusClient _client;

    public AzureServiceBusPublisher(
        IOptions<AzureServiceBusOptions> options,
        ILogger<AzureServiceBusPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = CreateClient(_options);
    }

    public async Task PublishAsync<T>(
        T message,
        string queueName,
        CancellationToken cancellationToken = default) where T : class
    {
        var targetQueue = string.IsNullOrWhiteSpace(queueName)
            ? _options.QueueName
            : queueName.Trim();

        await using var sender = _client.CreateSender(targetQueue);

        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromString(json))
        {
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString("N"),
            Subject = typeof(T).Name
        };

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

        _logger.LogInformation("Mensaje publicado en Azure Service Bus. Queue={Queue}, MessageId={MessageId}",
            targetQueue, serviceBusMessage.MessageId);
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();

    private static ServiceBusClient CreateClient(AzureServiceBusOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return new ServiceBusClient(options.ConnectionString);

        if (string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace))
        {
            throw new InvalidOperationException(
                "AzureServiceBus requiere ConnectionString o FullyQualifiedNamespace.");
        }

        return new ServiceBusClient(options.FullyQualifiedNamespace, new DefaultAzureCredential());
    }
}
