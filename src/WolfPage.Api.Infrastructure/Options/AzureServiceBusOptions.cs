namespace WolfPage.Api.Infrastructure.Options;

public class AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public string? ConnectionString { get; set; }
    public string? FullyQualifiedNamespace { get; set; }
    public string QueueName { get; set; } = "site.generate";
}
