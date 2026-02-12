using backend.Data;
using backend.Repositories.Travels;
using backend.Services.Common;
using backend.Entities.Travels;
using backend.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using backend.DTO.Travels;

namespace backend.Services.Travels;

public class ExpenseService
{
    private readonly AppDbContext _db;
    private readonly IExpenseRepository _expenses;
    private readonly IExpenseProofRepository _documents;
    private readonly CloudinaryService _cloudinary;
    private readonly NotificationService _notifications;
    // private readonly EmailService _email;
    // private readonly IOptions<EmailSettings> _emailSettings;

    public ExpenseService(AppDbContext db, IExpenseRepository expenses, IExpenseProofRepository documents, CloudinaryService cloudinary, NotificationService notifications
    // , EmailService email, IOptions<EmailSettings> emailSettings
    )
    {
        _db = db;
        _expenses = expenses;
        _documents = documents;
        _cloudinary = cloudinary;
        _notifications = notifications;
        // _email = email;
        // _emailSettings = emailSettings;
    }

    public async Task<ExpenseResponseDto> CreateDraftAsync(ExpenseCreateDto dto, long currentUserId)
    {
        var assignment = await _db.TravelAssignments
            .Include(a => a.Travel)
            .FirstOrDefaultAsync(a => a.AssignmentId == dto.AssignId);

        if (assignment is null)
        {
            throw new ArgumentException("Assignment not found.");
        }

        if (assignment.EmployeeId != currentUserId)
        {
            throw new ArgumentException("You can only create expenses for your own travel.");
        }

        ValidateExpenseWindow(dto.ExpenseDate, assignment.Travel!);
        await ValidateAmountAsync(dto.CategoryId, dto.Amount);

        var expense = new Expense
        {
            TravelId = assignment.TravelId,
            EmployeeId = assignment.EmployeeId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExpenseDate = dto.ExpenseDate,
            Status = ExpenseStatus.Draft
        };

        var saved = await _expenses.AddAsync(expense);
        return Map(saved);
    }

    public async Task UploadProofAsync(long expenseId, ExpenseProofUploadDto dto, long currentUserId)
    {
        var expense = await _expenses.GetByIdAsync(expenseId);
        if (expense is null)
        {
            throw new ArgumentException("Expense not found.");
        }

        if (expense.EmployeeId != currentUserId)
        {
            throw new ArgumentException("You can only upload proofs for your own expense.");
        }

        var upload = await _cloudinary.UploadAsync(dto.File, "expense-proofs");

        var document = new ExpenseProof
        {
            ExpenseId = expenseId,
            FileName = upload.FileName,
            FilePath = upload.Url,
            FileType = dto.File.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
                ? ExpenseFileType.Pdf
                : ExpenseFileType.Image,
            UploadedAt = DateTime.UtcNow
        };

        await _documents.AddAsync(document);
    }

    public async Task<ExpenseResponseDto> SubmitAsync(long expenseId, long currentUserId)
    {
        var expense = await _expenses.GetByIdAsync(expenseId);
        if (expense is null)
        {
            throw new ArgumentException("Expense not found.");
        }

        if (expense.EmployeeId != currentUserId)
        {
            throw new ArgumentException("You can only submit your own expense.");
        }

        if (expense.Status != ExpenseStatus.Draft)
        {
            throw new ArgumentException("Only draft expenses can be submitted.");
        }

        var proofs = await _documents.GetByExpenseIdAsync(expenseId);
        if (proofs.Count == 0)
        {
            throw new ArgumentException("At least one proof document is required.");
        }


        await ValidateAmountAsync(expense.CategoryId, expense.Amount);

        expense.Status = ExpenseStatus.Submitted;
        expense.SubmittedAt = DateTime.UtcNow;

        await _expenses.SaveAsync();

        var hrRoleId = await _db.Roles
            .Where(r => r.Name == "HR")
            .Select(r => r.RoleId)
            .FirstOrDefaultAsync();

        if (hrRoleId > 0)
        {
            var hrUsers = await _db.Users
                .Where(u => u.RoleId == hrRoleId)
                .Select(u => new { u.UserId, u.Email })
                .ToListAsync();

            var title = "Expense submitted";
            var message = $"Expense #{expense.ExpenseId} has been submitted for review.";

            await _notifications.CreateForUsersAsync(hrUsers.Select(u => u.UserId), title, message);

            // var hrMailbox = _emailSettings.Value.HrMailbox;
            // if (!string.IsNullOrWhiteSpace(hrMailbox))
            // {
            //     await _email.SendAsync(hrMailbox, title, message);
            // }
        }

        return Map(expense);
    }

    public async Task<ExpenseResponseDto> ReviewAsync(long expenseId, ExpenseReviewDto dto, long reviewerId)
    {
        if (dto.Status is not (ExpenseStatus.Approved or ExpenseStatus.Rejected))
        {
            throw new ArgumentException("Status must be Approved or Rejected.");
        }

        if (dto.Status == ExpenseStatus.Rejected && string.IsNullOrWhiteSpace(dto.Remarks))
        {
            throw new ArgumentException("Remarks are required when rejecting.");
        }
        Console.WriteLine("hiiiiiiiiiiiiiiiiiiiiiiiiiiiii");

        var expense = await _expenses.GetByIdAsync(expenseId);
        if (expense is null)
        {
            throw new ArgumentException("Expense not found.");
        }
        if(dto.Status == ExpenseStatus.Approved)
        {
            expense.Status = ExpenseStatus.Approved;
        }
        if (dto.Status == ExpenseStatus.Rejected)
        {
            expense.Status = ExpenseStatus.Rejected;
        }

    
        expense.HrRemarks = dto.Remarks;
        expense.ReviewedBy = reviewerId;
        expense.ReviewedAt = DateTime.UtcNow;

        await _expenses.SaveAsync();
        return Map(expense);
    }

    public async Task<IReadOnlyCollection<ExpenseResponseDto>> ListForEmployeeAsync(long employeeId)
    {
        var expenses = await _expenses.GetByAssigneeAsync(employeeId);
        return expenses.Select(Map).ToList();
    }

    public async Task<IReadOnlyCollection<ExpenseResponseDto>> ListForHrAsync(long? employeeId, long? travelId, DateTime? from, DateTime? to, string? status)
    {
        var expenses = await _expenses.GetFilteredAsync(employeeId, travelId, from, to, status);
        return expenses.Select(Map).ToList();
    }

    private void ValidateExpenseWindow(DateTime expenseDate, Travel travel)
    {
        var lastAllowed = travel.EndDate.AddDays(10);
        if (expenseDate < travel.StartDate || expenseDate > lastAllowed)
        {
            throw new ArgumentException("Expense date is outside the submission window.");
        }
    }

    private async Task ValidateAmountAsync(long categoryId, decimal amount)
    {
        var maxPerDay = await _db.ExpenseCategories
            .Where(c => c.CategoryId == categoryId)
            .Select(c => c.MaxAmountPerDay)
            .FirstOrDefaultAsync();

        if (maxPerDay <= 0)
        {
            return;
        }

        // var overrideRule = await _db.ExpenseValidationRules
        //     .Where(r => r.IsActive && r.ScopeType == ValidationRuleScope.Category && r.CategoryId == categoryId && r.RuleKey == "MAX_PER_DAY")
        //     .Select(r => r.RuleValue)
        //     .FirstOrDefaultAsync();

        // if (decimal.TryParse(overrideRule, out var overrideValue) && overrideValue > 0)
        // {
        //     maxPerDay = overrideValue;
        // }

        if (amount > maxPerDay)
        {
            throw new ArgumentException("Amount exceeds the allowed maximum per day.");
        }
    }

    private static ExpenseResponseDto Map(Expense expense)
    {
        return new ExpenseResponseDto(
            expense.ExpenseId,
            // expense.AssignId,
            expense.CategoryId,
            expense.Amount,
            expense.Currency,
            expense.ExpenseDate,
            expense.Status,
            expense.SubmittedAt,
            expense.ReviewedBy,
            expense.ReviewedAt,
            expense.HrRemarks);
    }
}