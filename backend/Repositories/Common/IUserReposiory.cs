using backend.DTO.Common;
using backend.Entities.Common;

namespace backend.Repositories.Common;


public interface IUserRepository
{
Task<int> AddUserAsync(User user);

Task<User?> GetUserByEmailAsync(string email);

Task<List<UserResponseDto>> GetListOfEmployeeAsync();

 Task<object> SearchEmployeeAsync(string trimmed);
}

