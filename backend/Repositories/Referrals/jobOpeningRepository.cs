using backend.Data;
using backend.Entities.Referrals;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Referrals
{
    public class JobOpeningRepository : IJobOpeningRepository
    {
        private readonly AppDbContext _db;

        public JobOpeningRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<JobOpening?> GetByIdAsync(long jobId)
        {
            return await _db.Set<JobOpening>().FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<IReadOnlyCollection<JobOpening>> GetActiveAsync(bool activeOnly)
        {
            var query = _db.Set<JobOpening>().AsQueryable();
            if (activeOnly)
            {
                query = query.Where(j => j.IsActive);
            }

            return await query
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
        }

        public async Task<JobOpening> AddAsync(JobOpening opening)
        {
            _db.Set<JobOpening>().Add(opening);
            await _db.SaveChangesAsync();
            return opening;
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}