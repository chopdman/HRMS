using backend.Data;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications)
    {
        _db.Set<Notification>().AddRange(notifications);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Notification>> GetByUserAsync(int userId)
    {
        return await _db.Set<Notification>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int notificationId, int userId)
    {
        return await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}