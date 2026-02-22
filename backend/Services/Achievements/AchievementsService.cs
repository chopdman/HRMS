using backend.DTO.Achievements;
using backend.DTO.Common;
using backend.Entities.Achievements;
using backend.Repositories.Achievements;
using backend.Repositories.Common;
using backend.Services.Common;

namespace backend.Services.Achievements;

public class AchievementsService
{
    private readonly IAchievementsRepository _repo;
    private readonly IUserRepository _userRepository;
    private readonly EmailService _emailService;

    public AchievementsService(IAchievementsRepository repo, IUserRepository userRepository, EmailService emailService)
    {
        _repo = repo;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<IReadOnlyCollection<AchievementPostDto>> GetFeedAsync(long currentUserId, AchievementFeedFilter filter)
    {
        await EnsureDailyCelebrationsAsync(DateTime.UtcNow);
        var posts = await _repo.GetFeedAsync(filter);
        return posts.Select(post => MapPost(post, currentUserId)).ToList();
    }

    public async Task<AchievementPostDto> CreatePostAsync(long authorId, AchievementPostCreateDto dto, CloudinaryUploadResult? uploadResult = null)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        var post = new AchievementPost
        {
            AuthorId = authorId,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Tags = NormalizeTags(dto.Tags),
            Visibility = dto.Visibility ?? PostVisibility.AllEmployees,
            PostType = PostType.Achievement,
            IsSystemGenerated = false,
            AttachmentUrl = uploadResult?.Url,
            AttachmentFileName = uploadResult?.FileName,
            AttachmentPublicId = uploadResult?.PublicId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddPostAsync(post);
        var created = await _repo.GetPostWithDetailsAsync(post.PostId);
        if (created is null)
        {
            throw new ArgumentException("Post could not be created.");
        }

        return MapPost(created, authorId);
    }

    public async Task<AchievementPostDto> UpdatePostAsync(long postId, long userId, bool isHr, AchievementPostUpdateDto dto)
    {
        var post = await _repo.GetPostByIdAsync(postId);
        if (post is null)
        {
            throw new ArgumentException("Post not found.");
        }

        if (!isHr && post.AuthorId != userId)
        {
            throw new ArgumentException("You do not have permission to edit this post.");
        }

        if (post.IsSystemGenerated)
        {
            throw new ArgumentException("System-generated posts cannot be edited.");
        }

        post.Title = dto.Title.Trim();
        post.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        post.Tags = NormalizeTags(dto.Tags);
        post.Visibility = dto.Visibility ?? PostVisibility.AllEmployees;
        post.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveAsync();
        var updated = await _repo.GetPostWithDetailsAsync(postId);
        if (updated is null)
        {
            throw new ArgumentException("Post not found.");
        }

        return MapPost(updated, userId);
    }

    public async Task DeletePostAsync(long postId, long userId, bool isHr, string? reason)
    {
        var post = await _repo.GetPostByIdAsync(postId);
        if (post is null)
        {
            throw new ArgumentException("Post not found.");
        }

        if (post.IsSystemGenerated && !isHr)
        {
            throw new ArgumentException("You do not have permission to delete this post.");
        }

        if (!isHr && post.AuthorId != userId)
        {
            throw new ArgumentException("You do not have permission to delete this post.");
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveAsync();

        if (isHr)
        {
            await LogRemovalAndWarnAsync(ContentType.Post, post.PostId, userId, post.AuthorId, reason);
        }
    }

    public async Task<AchievementCommentDto> AddCommentAsync(long postId, long authorId, AchievementCommentCreateDto dto)
    {
        var post = await _repo.GetPostByIdAsync(postId);
        if (post is null)
        {
            throw new ArgumentException("Post not found.");
        }

        // If replying to a comment, validate the parent comment exists
        if (dto.ParentCommentId.HasValue)
        {
            var parentComment = await _repo.GetCommentByIdAsync(dto.ParentCommentId.Value);
            if (parentComment is null || parentComment.IsDeleted)
            {
                throw new ArgumentException("Parent comment not found.");
            }
        }

        var comment = new PostComment
        {
            PostId = postId,
            AuthorId = authorId,
            CommentText = dto.CommentText.Trim(),
            ParentCommentId = dto.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddCommentAsync(comment);
        var loaded = await _repo.GetCommentByIdAsync(comment.CommentId);
        if (loaded is null || loaded.Author is null)
        {
            throw new ArgumentException("Comment could not be created.");
        }

        return MapComment(loaded);
    }

    public async Task<AchievementCommentDto> UpdateCommentAsync(long commentId, long userId, bool isHr, AchievementCommentUpdateDto dto)
    {
        var comment = await _repo.GetCommentByIdAsync(commentId);
        if (comment is null)
        {
            throw new ArgumentException("Comment not found.");
        }

        if (!isHr && comment.AuthorId != userId)
        {
            throw new ArgumentException("You do not have permission to edit this comment.");
        }

        comment.CommentText = dto.CommentText.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveAsync();

        var updated = await _repo.GetCommentByIdAsync(commentId);
        if (updated is null || updated.Author is null)
        {
            throw new ArgumentException("Comment not found.");
        }

        return MapComment(updated);
    }

    public async Task DeleteCommentAsync(long commentId, long userId, bool isHr, string? reason)
    {
        var comment = await _repo.GetCommentByIdAsync(commentId);
        if (comment is null)
        {
            throw new ArgumentException("Comment not found.");
        }

        if (!isHr && comment.AuthorId != userId)
        {
            throw new ArgumentException("You do not have permission to delete this comment.");
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveAsync();

        if (isHr)
        {
            await LogRemovalAndWarnAsync(ContentType.Comment, comment.CommentId, userId, comment.AuthorId, reason);
        }
    }

    public async Task LikePostAsync(long postId, long userId)
    {
        var post = await _repo.GetPostByIdAsync(postId);
        if (post is null)
        {
            throw new ArgumentException("Post not found.");
        }

        var existing = await _repo.GetPostLikeAsync(postId, userId);
        if (existing is not null)
        {
            return;
        }

        var like = new PostLike
        {
            PostId = postId,
            UserId = userId,
            LikedAt = DateTime.UtcNow
        };

        await _repo.AddPostLikeAsync(like);
    }

    public async Task UnlikePostAsync(long postId, long userId)
    {
        var existing = await _repo.GetPostLikeAsync(postId, userId);
        if (existing is null)
        {
            return;
        }

        await _repo.RemovePostLikeAsync(existing);
    }

    public async Task EnsureDailyCelebrationsAsync(DateTime dateUtc)
    {
        var today = dateUtc.Date;
        var existingKeys = (await _repo.GetSystemKeysForDateAsync(today)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var birthdays = await _repo.GetUsersWithBirthdaysAsync(today);
        foreach (var user in birthdays)
        {
            var key = $"birthday:{user.UserId}:{today:yyyyMMdd}";
            if (existingKeys.Contains(key))
            {
                continue;
            }

            var post = new AchievementPost
            {
                AuthorId = user.UserId,
                Title = $"Today is {user.FullName}'s birthday",
                Description = "Send your wishes and celebrate together.",
                Tags = "birthday,celebration",
                PostType = PostType.Birthday,
                Visibility = PostVisibility.AllEmployees,
                IsSystemGenerated = true,
                SystemKey = key,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddPostAsync(post);
            existingKeys.Add(key);
        }

        var anniversaries = await _repo.GetUsersWithAnniversariesAsync(today);
        foreach (var user in anniversaries)
        {
            var years = today.Year - user.DateOfJoining.Year;
            if (years <= 0)
            {
                continue;
            }

            var key = $"anniversary:{user.UserId}:{today:yyyyMMdd}";
            if (existingKeys.Contains(key))
            {
                continue;
            }

            var post = new AchievementPost
            {
                AuthorId = user.UserId,
                Title = $"{user.FullName} completes {years} years at the organization",
                Description = "Celebrate this milestone with a note of appreciation.",
                Tags = "anniversary,celebration",
                PostType = PostType.WorkAnniversary,
                Visibility = PostVisibility.AllEmployees,
                IsSystemGenerated = true,
                SystemKey = key,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddPostAsync(post);
            existingKeys.Add(key);
        }
    }

    private async Task LogRemovalAndWarnAsync(ContentType contentType, long contentId, long deletedBy, long authorId, string? reason)
    {
        var removed = new RemovedContent
        {
            ContentType = contentType,
            ContentId = contentId,
            DeletedBy = deletedBy,
            AuthorId = authorId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            DeletedAt = DateTime.UtcNow
        };

        await _repo.AddRemovedContentAsync(removed);

        var author = await _userRepository.GetByIdAsync(authorId);
        if (author is null || string.IsNullOrWhiteSpace(author.Email))
        {
            return;
        }

        var contentLabel = contentType == ContentType.Post ? "achievement post" : "comment";
        var subject = "Content removed from achievements feed";
        var body = $"Your {contentLabel} was removed by HR for violating content guidelines.";

        if (!string.IsNullOrWhiteSpace(removed.Reason))
        {
            body += $" Reason: {removed.Reason}.";
        }

        await _emailService.SendAsync(author.Email, subject, body);
    }

    private static AchievementPostDto MapPost(AchievementPost post, long currentUserId)
    {
        var tags = SplitTags(post.Tags);
        var likes = post.Likes ?? new List<PostLike>();
        var comments = (post.Comments ?? new List<PostComment>())
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .Select(MapComment)
            .ToList();

        var recentLikers = likes
            .OrderByDescending(l => l.LikedAt)
            .Take(5)
            .Select(l => l.User)
            .Where(u => u is not null)
            .Select(u => new AchievementUserDto(u!.UserId, u.FullName, u.ProfilePhotoUrl))
            .ToList();

        return new AchievementPostDto(
            post.PostId,
            new AchievementUserDto(post.Author?.UserId ?? 0, post.Author?.FullName ?? "System", post.Author?.ProfilePhotoUrl),
            post.Title ?? string.Empty,
            post.Description,
            tags,
            post.PostType,
            post.Visibility,
            post.IsSystemGenerated,
            post.CreatedAt,
            post.UpdatedAt,
            likes.Count,
            likes.Any(l => l.UserId == currentUserId),
            recentLikers,
            comments.Count,
            comments,
            post.AttachmentUrl,
            post.AttachmentFileName
        );
    }

    private static AchievementCommentDto MapComment(PostComment comment)
    {
        var author = comment.Author is null
            ? new AchievementUserDto(0, "Unknown", null)
            : new AchievementUserDto(comment.Author.UserId, comment.Author.FullName, comment.Author.ProfilePhotoUrl);

        var replies = (comment.Replies ?? new List<PostComment>())
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .Select(MapComment)
            .ToList();

        return new AchievementCommentDto(
            comment.CommentId,
            comment.PostId,
            author,
            comment.CommentText ?? string.Empty,
            comment.CreatedAt,
            comment.UpdatedAt,
            comment.ParentCommentId,
            replies
        );
    }

    private static string? NormalizeTags(IReadOnlyCollection<string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return null;
        }

        var normalized = tags
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            return null;
        }

        return string.Join(',', normalized);
    }

    private static IReadOnlyCollection<string> SplitTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return Array.Empty<string>();
        }

        return tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}