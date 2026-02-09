using backend.DTO;

namespace backend.Services{
public interface IRoleService
{
    Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto);
    Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync();
}
}