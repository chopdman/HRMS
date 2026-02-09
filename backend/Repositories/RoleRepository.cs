using backend.Data;
using backend.Entities;
using backend.DTO;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto)
    {
        var exists = await _db.Roles.AnyAsync(r => r.Name == dto.Name);
        if (exists)
        {
            throw new ArgumentException("Role name already exists.");
        }

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return new RoleResponseDto(role.RoleId, role.Name, role.Description);
    }

    public async Task<IReadOnlyCollection<RoleResponseDto>> GetRolesAsync()
    {
        return await _db.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponseDto(r.RoleId, r.Name, r.Description))
            .ToListAsync();
    }
}
