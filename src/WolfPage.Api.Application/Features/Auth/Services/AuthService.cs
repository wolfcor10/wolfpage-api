using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Auth.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUser _currentUser;

    public AuthService(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUser = currentUser;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var usersQuery = _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.Email == email && x.IsActive && x.Tenant.IsActive);

        if (request.TenantId.HasValue)
            usersQuery = usersQuery.Where(x => x.TenantId == request.TenantId.Value);

        var users = await usersQuery.Take(2).ToListAsync(cancellationToken);

        if (users.Count != 1)
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        var user = users[0];
        if (!_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = user.LastLoginAt.Value;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var currentUser = MapCurrentUser(user);
        var token = _jwtTokenGenerator.GenerateToken(user, currentUser.Roles);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = currentUser
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return null;

        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId.Value && x.IsActive, cancellationToken);

        return user is null ? null : MapCurrentUser(user);
    }

    private static CurrentUserDto MapCurrentUser(User user) => new()
    {
        Id = user.Id,
        TenantId = user.TenantId,
        Email = user.Email,
        FullName = user.FullName,
        Roles = user.UserRoles
            .Select(x => x.Role.Code)
            .OrderBy(x => x)
            .ToArray()
    };
}
