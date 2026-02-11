using backend.DTO.Common;
using backend.Entities.Common;
using backend.Repositories.Common;

namespace backend.Services.Common
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository userRepository)
        {
            _repo = userRepository;
        }

        public async Task<int> AddUser(User user)
        {
            return await _repo.AddUserAsync(user);

        }
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _repo.GetUserByEmailAsync(email);

        }

        public async Task<List<UserResponseDto>> GetListOfEmployee()
        {
            return await _repo.GetListOfEmployeeAsync();
        }

        public async Task<object> SearchEmployee(string trimmed)
        {
            return await _repo.SearchEmployeeAsync(trimmed);
        }

    }
}