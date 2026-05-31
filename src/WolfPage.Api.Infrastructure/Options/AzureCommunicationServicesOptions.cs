namespace WolfPage.Api.Infrastructure.Options;

public class AzureCommunicationServicesOptions
{
    public const string SectionName = "AzureCommunicationServices";

    public string ConnectionString { get; set; } = string.Empty;
}
