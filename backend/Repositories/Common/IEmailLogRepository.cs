using backend.Entities.Common;

namespace backend.Repositories.Common
{
    public interface IEmailLogRepository
    {
        Task AddRangeAsync(IEnumerable<EmailLog> logs);
    }
}