using backend.Data;
using backend.Entities.Referrals;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Referrals
{
    public class ReferralStatusLogRepository : IReferralStatusLogRepository
    {
        private readonly AppDbContext _db;

        public ReferralStatusLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ReferralStatusLog log)
        {
            _db.Set<ReferralStatusLog>().Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<ReferralStatusLog>> GetByReferralAsync(long referralId)
        {
            return await _db.Set<ReferralStatusLog>()
                .Where(l => l.ReferralId == referralId)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();
        }
    }
}