using backend.Entities.Referrals;

namespace backend.Repositories.Referrals
{
    public interface IJobOpeningRepository
    {
        Task<JobOpening?> GetByIdAsync(long jobId);
        Task<IReadOnlyCollection<JobOpening>> GetActiveAsync(bool activeOnly);
        Task<JobOpening> AddAsync(JobOpening opening);
        Task SaveAsync();
    }
}