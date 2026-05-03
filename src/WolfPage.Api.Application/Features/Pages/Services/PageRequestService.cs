using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Messaging;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Pages.Services;

public class PageRequestService : IPageRequestService
{
    private const string PageGenerationQueue = "site.generate";

    private readonly IAppDbContext _dbContext;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<PageRequestService> _logger;
    private readonly ICurrentUser _currentUser;

    public PageRequestService(
        IAppDbContext dbContext,
        IMessagePublisher publisher,
        ILogger<PageRequestService> logger,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<PageRequestResponseDto> CreateAsync(CreatePageRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (_currentUser.TenantId is not Guid tenantId || dto.TenantId != tenantId)
            throw new UnauthorizedAccessException("Tenant no autorizado.");

        var tenantExists = await _dbContext.Tenants
            .AnyAsync(x => x.Id == dto.TenantId && x.IsActive, cancellationToken);

        if (!tenantExists)
            throw new InvalidOperationException($"Tenant {dto.TenantId} no existe o está inactivo.");

        var templateVersion = await _dbContext.TemplateVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.TemplateVersionId, cancellationToken);

        if (templateVersion is null)
            throw new InvalidOperationException($"TemplateVersion {dto.TemplateVersionId} no existe.");

        if (!templateVersion.IsPublished)
            throw new InvalidOperationException($"TemplateVersion {dto.TemplateVersionId} no está publicada.");

        var slugTaken = await _dbContext.Pages.AnyAsync(x => x.Slug == dto.Slug, cancellationToken);
        if (slugTaken)
            throw new InvalidOperationException($"El slug '{dto.Slug}' ya está en uso.");

        var correlationId = Guid.NewGuid().ToString("N");

        var request = new PageGenerationRequest
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            TemplateVersionId = dto.TemplateVersionId,
            CorrelationId = correlationId,
            PageName = dto.PageName,
            Slug = dto.Slug,
            ContentJson = JsonSerializer.Serialize(dto.Content),
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PageGenerationRequests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var message = new CreatePageRequestedMessage
        {
            RequestId = request.Id,
            CorrelationId = request.CorrelationId
        };

        await _publisher.PublishAsync(message, PageGenerationQueue, cancellationToken);

        _logger.LogInformation(
            "PageGenerationRequest creado y publicado. RequestId={RequestId}, CorrelationId={CorrelationId}",
            request.Id, request.CorrelationId);

        return new PageRequestResponseDto
        {
            RequestId = request.Id,
            CorrelationId = request.CorrelationId,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt
        };
    }

    public async Task<PageRequestResponseDto?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUser.TenantId ?? throw new UnauthorizedAccessException("Usuario sin tenant.");

        var request = await _dbContext.PageGenerationRequests
            .AsNoTracking()
            .Include(x => x.Page)
            .FirstOrDefaultAsync(x => x.Id == requestId && x.TenantId == tenantId, cancellationToken);

        if (request is null)
            return null;

        return new PageRequestResponseDto
        {
            RequestId = request.Id,
            CorrelationId = request.CorrelationId,
            Status = request.Status.ToString(),
            PageId = request.Page?.Id,
            ErrorMessage = request.ErrorMessage,
            CreatedAt = request.CreatedAt,
            ProcessedAt = request.ProcessedAt
        };
    }
}
