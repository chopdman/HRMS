using backend.DTO.Common;

namespace backend.Services.Common
{
    public interface IRoleService
    {
        Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto);
        Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync();
    }
}