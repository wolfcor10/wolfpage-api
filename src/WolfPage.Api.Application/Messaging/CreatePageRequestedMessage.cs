namespace WolfPage.Api.Application.Messaging;

public class CreatePageRequestedMessage
{
    public Guid RequestId { get; set; }
    public string CorrelationId { get; set; } = default!;
}
