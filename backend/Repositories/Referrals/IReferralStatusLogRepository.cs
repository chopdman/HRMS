using backend.Entities.Referrals;

namespace backend.Repositories.Referrals
{
    public interface IReferralStatusLogRepository
    {
        Task AddAsync(ReferralStatusLog log);
        Task<IReadOnlyCollection<ReferralStatusLog>> GetByReferralAsync(long referralId);
    }
}