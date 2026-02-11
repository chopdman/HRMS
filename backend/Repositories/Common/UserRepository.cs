using backend.Data;
using backend.DTO.Common;
using backend.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Common
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> AddUserAsync(User user)
        {
            _db.Users.Add(user);
            return await _db.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        }

        public async Task<List<UserResponseDto>> GetListOfEmployeeAsync()
        {
            return _db.Users.Join(_db.Roles,
         u => EF.Property<long?>(u, "RoleId"),
         r => EF.Property<long?>(r, "RoleId"),
         (o, i) => new { User = o, Role = i }).Join(
        _db.Users,
        ur => EF.Property<long?>(ur.User, "ManagerId"),
        m => EF.Property<long?>(m, "UserId"),
        (ur, manager) => new
        {
            ur.User,
            ur.Role,
            ManagerName = manager.FullName
        }
    )
     .Where(u => u.Role != null)
     .AsEnumerable()
     .OrderBy(u => u.User.FullName).Select(u => new UserResponseDto(
         u.User.UserId,
         u.User.FullName,
         u.User.Email,
         u.User.Phone,
         u.User.DateOfBirth,
         u.User.DateOfJoining,
         u.User.ProfilePhotoUrl,
         u.User.Department,
         u.User.Designation,
         u.ManagerName,
         u.Role.Name
     ))
     .ToList();
        }

        public async Task<object> SearchEmployeeAsync(string trimmed)
        {
            return  _db.Users
     .Join(
         _db.Roles,
         u => EF.Property<long?>(u, "RoleId")!,
         r => EF.Property<long?>(r, "RoleId")!,
         (u, r) => new { User = u, Role = r }
     )
     .Where(ti => ti.Role != null && ti.Role.Name == "Employee")
  .Where(ti => ti.User.FullName.ToLower().Contains(trimmed.ToLower()) || 
             ti.User.Email.ToLower().Contains(trimmed.ToLower()))
     .OrderBy(ti => ti.User.FullName)
     .Select(ti => new EmployeeLookupDto(ti.User.UserId, ti.User.FullName, ti.User.Email))
     .ToList();
        }
    }
}