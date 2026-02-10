using backend.Entities.Travels;

namespace backend.Repositories.Travels;

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory> AddCategoryAsync(ExpenseCategory category);
    Task<IReadOnlyCollection<ExpenseCategory>> GetCategoriesAsync();

}