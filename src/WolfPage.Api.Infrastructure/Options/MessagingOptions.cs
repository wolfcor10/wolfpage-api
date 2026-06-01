namespace WolfPage.Api.Infrastructure.Options;

public class MessagingOptions
{
    public const string SectionName = "Messaging";

    public string Provider { get; set; } = "RabbitMq";
}
