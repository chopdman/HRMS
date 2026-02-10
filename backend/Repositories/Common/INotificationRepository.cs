using backend.Entities.Common;

namespace backend.Repositories.Common;

public interface INotificationRepository
{
    Task AddRangeAsync(IEnumerable<Notification> notifications);
    Task<IReadOnlyCollection<Notification>> GetByUserAsync(int userId);
    Task<Notification?> GetByIdAsync(int notificationId, int userId);
    Task SaveAsync();
}