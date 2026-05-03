using WolfPage.Api.Application.Features.Users.Dtos;

namespace WolfPage.Api.Application.Features.Users.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
}
