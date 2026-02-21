using backend.Entities.Referrals;

namespace backend.Repositories.Referrals
{
    public interface IJobShareRepository
    {
        Task AddRangeAsync(IEnumerable<JobShare> shares);
    }
}