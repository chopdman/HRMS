using backend.Data;
using backend.Entities.Referrals;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Referrals
{
    public class ReferralRepository : IReferralRepository
    {
        private readonly AppDbContext _db;

        public ReferralRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Referral?> GetByIdAsync(long referralId)
        {
            return await _db.Set<Referral>().FirstOrDefaultAsync(r => r.ReferralId == referralId);
        }

        public async Task<IReadOnlyCollection<Referral>> GetByJobAsync(long jobId)
        {
            return await _db.Set<Referral>()
                .Where(r => r.JobId == jobId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Referral> AddAsync(Referral referral)
        {
            _db.Set<Referral>().Add(referral);
            await _db.SaveChangesAsync();
            return referral;
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}