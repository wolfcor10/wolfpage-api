using WolfPage.Api.Application.Features.Auth.Dtos;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Auth;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(User user);
}
