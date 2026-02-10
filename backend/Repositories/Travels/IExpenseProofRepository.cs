using backend.Entities.Travels;
namespace backend.Repositories.Travels;
public interface IExpenseProofRepository
{
      Task<ExpenseProof> AddAsync(ExpenseProof document);
    Task<IReadOnlyCollection<ExpenseProof>> GetByExpenseIdAsync(int expenseId);
}