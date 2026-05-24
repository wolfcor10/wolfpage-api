namespace WolfPage.Api.Domain.Entities;

public class DomainBinding
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string? DomainName { get; set; }
    public string? Subdomain { get; set; }
    public bool IsPrimary { get; set; }
    public string? SslStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public Page Page { get; set; } = default!;
}
