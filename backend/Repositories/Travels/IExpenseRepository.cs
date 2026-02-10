using backend.Entities.Travels;

namespace backend.Repositories.Travels;

public interface IExpenseRepository
{
      Task<Expense> AddAsync(Expense expense);
    Task<Expense?> GetByIdAsync(int expenseId);
    Task<IReadOnlyCollection<Expense>> GetByAssigneeAsync(int employeeId);
    Task<IReadOnlyCollection<Expense>> GetFilteredAsync(int? employeeId, int? travelId, DateTime? from, DateTime? to, string? status);
    Task SaveAsync();
}