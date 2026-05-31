using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Email;
using WolfPage.Api.Application.Features.Auth.Dtos;
using WolfPage.Api.Application.Features.Auth.Options;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Auth.Services;

public class AuthService : IAuthService
{
    private const string EmailConfirmationPurpose = "email_confirmation";
    private const string GoogleProvider = "google";

    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly AuthOptions _authOptions;
    private readonly PortalOptions _portalOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUser currentUser,
        IEmailSender emailSender,
        IGoogleTokenValidator googleTokenValidator,
        IOptions<AuthOptions> authOptions,
        IOptions<PortalOptions> portalOptions,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUser = currentUser;
        _emailSender = emailSender;
        _googleTokenValidator = googleTokenValidator;
        _authOptions = authOptions.Value;
        _portalOptions = portalOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Workspace)
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash)
            || !_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        if (_authOptions.RequireConfirmedEmail && !user.EmailConfirmed)
            throw new UnauthorizedAccessException("Debes confirmar tu correo antes de iniciar sesion.");

        return await CreateLoginResponseAsync(user.Id, request.WorkspaceId, cancellationToken);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var email = NormalizeEmail(request.Email);
        var existingUser = await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);

        if (existingUser)
            throw new InvalidOperationException("Ya existe una cuenta con ese correo.");

        var workspaceType = ParseEnum<WorkspaceType>(request.WorkspaceType, nameof(request.WorkspaceType));
        var profileType = ParseEnum<ProfileType>(request.ProfileType, nameof(request.ProfileType));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = request.FullName.Trim(),
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = now,
            UpdatedAt = now
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var workspace = await CreateWorkspaceForUserAsync(
            user,
            request.WorkspaceName.Trim(),
            NormalizeEmail(request.WorkspaceEmail ?? email),
            workspaceType,
            profileType,
            now,
            cancellationToken);

        _dbContext.Users.Add(user);
        _dbContext.Workspaces.Add(workspace);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var emailSent = await TrySendEmailConfirmationAsync(user, cancellationToken);
        var session = _authOptions.RequireConfirmedEmail
            ? null
            : await CreateLoginResponseAsync(user.Id, workspace.Id, cancellationToken);

        return new RegisterResponse
        {
            RequiresEmailConfirmation = _authOptions.RequireConfirmedEmail,
            EmailConfirmationSent = emailSent,
            Session = session,
            Message = emailSent
                ? "Cuenta creada. Revisa tu correo para confirmar la cuenta."
                : "Cuenta creada. No se pudo enviar el correo de confirmacion en este momento."
        };
    }

    public async Task<EmailConfirmationResponse> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tokenHash = HashToken(request.Token.Trim());

        var token = await _dbContext.UserTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Purpose == EmailConfirmationPurpose && x.TokenHash == tokenHash,
                cancellationToken);

        if (token is null || token.UsedAt.HasValue || token.ExpiresAt < now)
        {
            return new EmailConfirmationResponse
            {
                Succeeded = false,
                Message = "El enlace de confirmacion no es valido o ya expiro."
            };
        }

        token.UsedAt = now;
        token.User.EmailConfirmed = true;
        token.User.EmailConfirmedAt = now;
        token.User.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EmailConfirmationResponse
        {
            Succeeded = true,
            Message = "Correo confirmado correctamente."
        };
    }

    public async Task<EmailConfirmationResponse> ResendEmailConfirmationAsync(
        ResendEmailConfirmationRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken);

        if (user is null)
        {
            return new EmailConfirmationResponse
            {
                Succeeded = true,
                Message = "Si el correo existe, enviaremos un nuevo enlace de confirmacion."
            };
        }

        if (user.EmailConfirmed)
        {
            return new EmailConfirmationResponse
            {
                Succeeded = true,
                Message = "El correo ya esta confirmado."
            };
        }

        var emailSent = await TrySendEmailConfirmationAsync(user, cancellationToken);
        return new EmailConfirmationResponse
        {
            Succeeded = emailSent,
            Message = emailSent
                ? "Se envio un nuevo enlace de confirmacion."
                : "No se pudo enviar el correo de confirmacion en este momento."
        };
    }

    public async Task<LoginResponse> LoginWithGoogleAsync(
        GoogleAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);
        if (!googleUser.EmailVerified)
            throw new UnauthorizedAccessException("Google no confirmo el correo de la cuenta.");

        var now = DateTime.UtcNow;
        var email = NormalizeEmail(googleUser.Email);

        var externalLogin = await _dbContext.UserExternalLogins
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Provider == GoogleProvider && x.ProviderUserId == googleUser.ProviderUserId,
                cancellationToken);

        var user = externalLogin is not null
            ? await _dbContext.Users
                .Include(x => x.ExternalLogins)
                .Include(x => x.WorkspaceMemberships)
                .ThenInclude(x => x.Workspace)
                .FirstOrDefaultAsync(x => x.Id == externalLogin.UserId, cancellationToken)
            : await _dbContext.Users
                .Include(x => x.ExternalLogins)
                .Include(x => x.WorkspaceMemberships)
                .ThenInclude(x => x.Workspace)
                .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = googleUser.FullName,
                IsActive = true,
                EmailConfirmed = true,
                EmailConfirmedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.Users.Add(user);
        }
        else if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("La cuenta esta inactiva.");
        }
        else
        {
            user.EmailConfirmed = true;
            user.EmailConfirmedAt ??= now;
            user.UpdatedAt = now;
        }

        if (!user.ExternalLogins.Any(x => x.Provider == GoogleProvider && x.ProviderUserId == googleUser.ProviderUserId))
        {
            user.ExternalLogins.Add(new UserExternalLogin
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = GoogleProvider,
                ProviderUserId = googleUser.ProviderUserId,
                Email = email,
                CreatedAt = now
            });
        }

        if (!user.WorkspaceMemberships.Any(x => x.Status == WorkspaceMemberStatus.Active && x.Workspace.IsActive))
        {
            var workspaceType = ParseEnum<WorkspaceType>(request.WorkspaceType, nameof(request.WorkspaceType));
            var profileType = ParseEnum<ProfileType>(request.ProfileType, nameof(request.ProfileType));
            var workspaceName = string.IsNullOrWhiteSpace(request.WorkspaceName)
                ? BuildDefaultWorkspaceName(googleUser.FullName)
                : request.WorkspaceName.Trim();

            var workspace = await CreateWorkspaceForUserAsync(
                user,
                workspaceName,
                NormalizeEmail(request.WorkspaceEmail ?? email),
                workspaceType,
                profileType,
                now,
                cancellationToken);

            _dbContext.Workspaces.Add(workspace);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await CreateLoginResponseAsync(user.Id, null, cancellationToken);
    }

    private async Task<LoginResponse> CreateLoginResponseAsync(
        Guid userId,
        Guid? requestedWorkspaceId,
        CancellationToken cancellationToken)
    {
        var user = await LoadUserForAuthAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciales invalidas.");

        var activeMemberships = GetActiveMemberships(user);
        if (!activeMemberships.Any())
            throw new UnauthorizedAccessException("El usuario no tiene workspaces activos.");

        var activeWorkspaceId = ResolveActiveWorkspaceId(activeMemberships, requestedWorkspaceId);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = user.LastLoginAt.Value;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAt = token.ExpiresAt,
            User = MapCurrentUser(user, activeWorkspaceId)
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return null;

        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Workspace)
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == _currentUser.UserId.Value && x.IsActive, cancellationToken);

        if (user is null)
            return null;

        var activeMemberships = GetActiveMemberships(user);
        var activeWorkspaceId = ResolveActiveWorkspaceId(activeMemberships, _currentUser.WorkspaceId);

        return MapCurrentUser(user, activeWorkspaceId);
    }

    private async Task<User?> LoadUserForAuthAsync(Guid userId, CancellationToken cancellationToken) =>
        await _dbContext.Users
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Workspace)
            .Include(x => x.WorkspaceMemberships)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

    private async Task<Workspace> CreateWorkspaceForUserAsync(
        User user,
        string workspaceName,
        string workspaceEmail,
        WorkspaceType workspaceType,
        ProfileType profileType,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var adminRole = await EnsureAdminRoleAsync(now, cancellationToken);
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = workspaceName,
            Email = workspaceEmail,
            WorkspaceType = workspaceType,
            ProfileType = profileType,
            IsActive = true,
            CreatedAt = now
        };

        workspace.Members.Add(new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = adminRole.Id,
            WorkspaceId = workspace.Id,
            Status = WorkspaceMemberStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            JoinedAt = now
        });

        return workspace;
    }

    private async Task<Role> EnsureAdminRoleAsync(DateTime now, CancellationToken cancellationToken)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Code == "admin", cancellationToken);
        if (role is not null)
            return role;

        role = new Role
        {
            Id = Guid.NewGuid(),
            Code = "admin",
            Name = "Administrador",
            Description = "Acceso administrativo completo.",
            CreatedAt = now
        };

        _dbContext.Roles.Add(role);
        return role;
    }

    private async Task<bool> TrySendEmailConfirmationAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            await SendEmailConfirmationAsync(user, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el correo de confirmacion para {Email}.", user.Email);
            return false;
        }
    }

    private async Task SendEmailConfirmationAsync(User user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var rawToken = GenerateSecureToken();
        var token = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Purpose = EmailConfirmationPurpose,
            TokenHash = HashToken(rawToken),
            ExpiresAt = now.AddHours(Math.Max(1, _authOptions.EmailConfirmationTokenHours)),
            CreatedAt = now
        };

        _dbContext.UserTokens.Add(token);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var confirmationUrl = $"{_portalOptions.BaseUrl.TrimEnd('/')}/confirm-email?token={Uri.EscapeDataString(rawToken)}";
        var safeName = WebUtility.HtmlEncode(user.FullName);
        var safeUrl = WebUtility.HtmlEncode(confirmationUrl);

        var html = $"""
            <p>Hola {safeName},</p>
            <p>Confirma tu cuenta de WolfPage usando este enlace:</p>
            <p><a href="{safeUrl}">Confirmar correo</a></p>
            <p>Si no creaste esta cuenta, puedes ignorar este mensaje.</p>
            """;

        await _emailSender.SendAsync(
            new EmailRequest(
                user.Email,
                "Confirma tu cuenta de WolfPage",
                html,
                $"Confirma tu cuenta de WolfPage: {confirmationUrl}"),
            cancellationToken);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string BuildDefaultWorkspaceName(string fullName)
    {
        var name = fullName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return "Personal Workspace";

        var firstName = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(firstName)
            ? "Personal Workspace"
            : $"{firstName} Workspace";
    }

    private static TEnum ParseEnum<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new InvalidOperationException($"{fieldName} '{value}' no es valido.");
    }

    private static Guid? ResolveActiveWorkspaceId(
        IReadOnlyCollection<WorkspaceMember> activeMemberships,
        Guid? requestedWorkspaceId)
    {
        if (!activeMemberships.Any())
            return null;

        if (requestedWorkspaceId.HasValue
            && activeMemberships.Any(x => x.WorkspaceId == requestedWorkspaceId.Value))
        {
            return requestedWorkspaceId.Value;
        }

        return activeMemberships
            .OrderBy(x => x.Workspace.Name)
            .Select(x => x.WorkspaceId)
            .First();
    }

    private static WorkspaceMember[] GetActiveMemberships(User user) =>
        user.WorkspaceMemberships
            .Where(x => x.Status == WorkspaceMemberStatus.Active && x.Workspace.IsActive)
            .ToArray();

    private static CurrentUserDto MapCurrentUser(User user, Guid? activeWorkspaceId)
    {
        var memberships = GetActiveMemberships(user);
        var roles = activeWorkspaceId.HasValue
            ? memberships
                .Where(x => x.WorkspaceId == activeWorkspaceId.Value)
                .Select(x => x.Role.Code)
                .Distinct()
                .OrderBy(x => x)
                .ToArray()
            : [];

        return new CurrentUserDto
        {
            Id = user.Id,
            ActiveWorkspaceId = activeWorkspaceId,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            Workspaces = memberships
                .GroupBy(x => x.WorkspaceId)
                .Select(group =>
                {
                    var workspace = group.First().Workspace;
                    return new CurrentUserWorkspaceDto
                    {
                        Id = workspace.Id,
                        Name = workspace.Name,
                        Email = workspace.Email,
                        WorkspaceType = workspace.WorkspaceType.ToString(),
                        ProfileType = workspace.ProfileType.ToString(),
                        Roles = group
                            .Select(x => x.Role.Code)
                            .Distinct()
                            .OrderBy(x => x)
                            .ToArray()
                    };
                })
                .OrderBy(x => x.Name)
                .ToArray()
        };
    }
}
