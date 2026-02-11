using backend.Data;
using backend.Entities.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Travels;

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly AppDbContext _db;

    public ExpenseCategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ExpenseCategory> AddCategoryAsync(ExpenseCategory category)
    {
        _db.ExpenseCategories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<IReadOnlyCollection<ExpenseCategory>> GetCategoriesAsync()
    {
        return await _db.ExpenseCategories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }
}