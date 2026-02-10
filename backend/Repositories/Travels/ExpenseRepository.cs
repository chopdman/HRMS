using backend.Data;
using backend.Entities.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Travels;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _db;

    public ExpenseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Expense> AddAsync(Expense expense)
    {
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return expense;
    }

    public async Task<Expense?> GetByIdAsync(long expenseId)
    {
        return await _db.Expenses
            // .Include(e => e.Assignment)
            .Include(e => e.ProofDocuments)
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
    }

    public async Task<IReadOnlyCollection<Expense>> GetByAssigneeAsync(long employeeId)
    {
        return await _db.Expenses
            // .Include(e => e.Assignment)
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Expense>> GetFilteredAsync(long? employeeId, long? travelId, DateTime? from, DateTime? to, string? status)
    {
        var query = _db.Expenses
            // .Include(e => e.Assignment)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(e => e.EmployeeId == employeeId.Value);
        }

        if (travelId.HasValue)
        {
            query = query.Where(e => e.TravelId == travelId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ExpenseStatus>(status, true, out var parsed))
        {
            query = query.Where(e => e.Status == parsed);
        }

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}