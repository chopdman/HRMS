using backend.Entities;

namespace backend.Repositories;

public interface INotificationRepository
{
    Task AddRangeAsync(IEnumerable<Notification> notifications);
    Task<IReadOnlyCollection<Notification>> GetByUserAsync(int userId);
    Task<Notification?> GetByIdAsync(int notificationId, int userId);
    Task SaveAsync();
}