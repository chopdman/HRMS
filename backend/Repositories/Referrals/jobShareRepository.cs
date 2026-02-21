using backend.Data;
using backend.Entities.Referrals;

namespace backend.Repositories.Referrals
{
    public class JobShareRepository : IJobShareRepository
    {
        private readonly AppDbContext _db;

        public JobShareRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(IEnumerable<JobShare> shares)
        {
            _db.Set<JobShare>().AddRange(shares);
            await _db.SaveChangesAsync();
        }
    }
}