namespace WolfPage.Api.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class;
}
