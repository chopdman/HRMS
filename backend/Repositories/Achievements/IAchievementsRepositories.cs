using backend.Entities.Achievements;
using backend.Entities.Common;

namespace backend.Repositories.Achievements;

public interface IAchievementsRepository
{
    Task<IReadOnlyCollection<AchievementPost>> GetFeedAsync(AchievementFeedFilter filter);
    Task<AchievementPost?> GetPostByIdAsync(long postId);
    Task<AchievementPost?> GetPostWithDetailsAsync(long postId);
    Task AddPostAsync(AchievementPost post);
    Task<PostComment?> GetCommentByIdAsync(long commentId);
    Task AddCommentAsync(PostComment comment);
    Task<PostLike?> GetPostLikeAsync(long postId, long userId);
    Task AddPostLikeAsync(PostLike like);
    Task RemovePostLikeAsync(PostLike like);
    Task AddRemovedContentAsync(RemovedContent removedContent);
    Task<IReadOnlyCollection<User>> GetUsersWithBirthdaysAsync(DateTime date);
    Task<IReadOnlyCollection<User>> GetUsersWithAnniversariesAsync(DateTime date);
    Task<IReadOnlyCollection<string>> GetSystemKeysForDateAsync(DateTime date);
    Task SaveAsync();
}

public record AchievementFeedFilter(
    long? AuthorId,
    string? Author,
    string? Tag,
    DateTime? FromDate,
    DateTime? ToDate
);