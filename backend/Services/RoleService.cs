using backend.Repositories;
using backend.DTO;



namespace backend.Services{
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Role name cannot be empty.");
        }

        return await _roleRepository.CreateRoleAsync(dto);
    }

    public async Task<IReadOnlyCollection<RoleResponseDto>> GetAllRolesAsync()
    {
        return await _roleRepository.GetRolesAsync();
    }
}
}