using backend.DTO.Travels;

namespace backend.Repositories.Travels;

public interface IManagerRepository
{
    Task<IReadOnlyCollection<TeamMemberDto>> GetTeamMembersAsync(int managerId);
    Task<IReadOnlyCollection<TeamExpenseDto>> GetTeamExpensesAsync(int managerId, DateTime? from, DateTime? to);
}