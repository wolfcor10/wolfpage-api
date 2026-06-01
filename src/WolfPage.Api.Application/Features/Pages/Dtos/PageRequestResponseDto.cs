namespace WolfPage.Api.Application.Features.Pages.Dtos;

public class PageRequestResponseDto
{
    public Guid RequestId { get; set; }
    public Guid PageId { get; set; }
    public string CorrelationId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
