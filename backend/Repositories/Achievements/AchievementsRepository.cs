using backend.Data;
using backend.Entities.Achievements;
using backend.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Achievements;

public class AchievementsRepository : IAchievementsRepository
{
    private readonly AppDbContext _db;

    public AchievementsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<AchievementPost>> GetFeedAsync(AchievementFeedFilter filter)
    {
        var query = _db.AchievementPosts
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
                .ThenInclude(l => l.User)
            .Where(p => !p.IsDeleted);

        if (filter.AuthorId.HasValue)
        {
            query = query.Where(p => p.AuthorId == filter.AuthorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Author))
        {
            var trimmed = filter.Author.Trim().ToLower();
            query = query.Where(p => p.Author != null && p.Author.FullName.ToLower().Contains(trimmed));
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var tag = filter.Tag.Trim();
            query = query.Where(p => p.Tags != null && p.Tags.Contains(tag));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<AchievementPost?> GetPostByIdAsync(long postId)
    {
        return await _db.AchievementPosts
            .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);
    }

    public async Task<AchievementPost?> GetPostWithDetailsAsync(long postId)
    {
        return await _db.AchievementPosts
            .Include(p => p.Author)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
                .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);
    }

    public async Task AddPostAsync(AchievementPost post)
    {
        _db.AchievementPosts.Add(post);
        await _db.SaveChangesAsync();
    }

    public async Task<PostComment?> GetCommentByIdAsync(long commentId)
    {
        return await _db.PostComments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);
    }

    public async Task AddCommentAsync(PostComment comment)
    {
        _db.PostComments.Add(comment);
        await _db.SaveChangesAsync();
    }

    public async Task<PostLike?> GetPostLikeAsync(long postId, long userId)
    {
        return await _db.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task AddPostLikeAsync(PostLike like)
    {
        _db.PostLikes.Add(like);
        await _db.SaveChangesAsync();
    }

    public async Task RemovePostLikeAsync(PostLike like)
    {
        _db.PostLikes.Remove(like);
        await _db.SaveChangesAsync();
    }

    public async Task AddRemovedContentAsync(RemovedContent removedContent)
    {
        _db.RemovedContents.Add(removedContent);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<User>> GetUsersWithBirthdaysAsync(DateTime date)
    {
        var month = date.Month;
        var day = date.Day;

        return await _db.Users
            .Where(u => u.IsActive && u.DateOfBirth.HasValue && u.DateOfBirth.Value.Month == month && u.DateOfBirth.Value.Day == day)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<User>> GetUsersWithAnniversariesAsync(DateTime date)
    {
        var month = date.Month;
        var day = date.Day;

        return await _db.Users
            .Where(u => u.IsActive && u.DateOfJoining.Month == month && u.DateOfJoining.Day == day)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<string>> GetSystemKeysForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        return await _db.AchievementPosts
            .Where(p => p.IsSystemGenerated && p.SystemKey != null && p.CreatedAt >= start && p.CreatedAt < end)
            .Select(p => p.SystemKey!)
            .ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}