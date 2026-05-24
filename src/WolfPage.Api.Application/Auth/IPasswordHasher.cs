using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Auth;

public interface IPasswordHasher
{
    string HashPassword(User user, string password);
    bool VerifyHashedPassword(User user, string passwordHash, string password);
}
