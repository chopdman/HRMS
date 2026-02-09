using backend.DTO;

namespace backend.Repositories;


public interface IRoleRepository
{
    Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto);
    Task<IReadOnlyCollection<RoleResponseDto>> GetRolesAsync();
}
