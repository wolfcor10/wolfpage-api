using Microsoft.AspNetCore.Identity;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Domain.Entities;
using AspNetPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<WolfPage.Api.Domain.Entities.User>;

namespace WolfPage.Api.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    private readonly AspNetPasswordHasher _passwordHasher = new();

    public string HashPassword(User user, string password) =>
        _passwordHasher.HashPassword(user, password);

    public bool VerifyHashedPassword(User user, string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
