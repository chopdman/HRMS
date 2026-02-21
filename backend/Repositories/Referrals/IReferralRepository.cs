using backend.Entities.Referrals;

namespace backend.Repositories.Referrals
{
    public interface IReferralRepository
    {
        Task<Referral?> GetByIdAsync(long referralId);
        Task<IReadOnlyCollection<Referral>> GetByJobAsync(long jobId);
        Task<Referral> AddAsync(Referral referral);
        Task SaveAsync();
    }
}