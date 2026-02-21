using backend.Data;
using backend.Entities.Common;

namespace backend.Repositories.Common
{
    public class EmailLogRepository : IEmailLogRepository
    {
        private readonly AppDbContext _db;

        public EmailLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(IEnumerable<EmailLog> logs)
        {
            _db.Set<EmailLog>().AddRange(logs);
            await _db.SaveChangesAsync();
        }
    }
}