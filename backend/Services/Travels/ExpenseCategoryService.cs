using backend.Data;
using backend.Entities.Travels;
using backend.DTO.Travels;
using backend.Repositories.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Travels;

public class ExpenseCategoryService
{
    private readonly AppDbContext _db;
    private readonly IExpenseCategoryRepository _repo;

    public ExpenseCategoryService(AppDbContext db, IExpenseCategoryRepository repo)
    {
        _db = db;
        _repo = repo;
    }

    public async Task<ExpenseCategoryResponseDto> CreateCategoryAsync(ExpenseCategoryCreateDto dto)
    {
        var exists = await _db.ExpenseCategories.AnyAsync(c => c.CategoryName == dto.CategoryName);
        if (exists)
        {
            throw new ArgumentException("Category already exists.");
        }

        var category = new ExpenseCategory
        {
            CategoryName = dto.CategoryName,
            MaxAmountPerDay = dto.MaxAmountPerDay
        };

        var saved = await _repo.AddCategoryAsync(category);
        return new ExpenseCategoryResponseDto(saved.CategoryId, saved.CategoryName, saved.MaxAmountPerDay);
    }

    public async Task<IReadOnlyCollection<ExpenseCategoryResponseDto>> GetCategoriesAsync()
    {
        var categories = await _repo.GetCategoriesAsync();
        return categories.Select(c => new ExpenseCategoryResponseDto(c.CategoryId, c.CategoryName, c.MaxAmountPerDay)).ToList();
    }
 
}