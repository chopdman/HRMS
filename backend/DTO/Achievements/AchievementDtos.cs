using System.ComponentModel.DataAnnotations;
using backend.Entities.Achievements;

namespace backend.DTO.Achievements;

public record AchievementUserDto(
    long Id,
    string FullName,
    string? ProfilePhotoUrl
);

public record AchievementCommentDto(
    long CommentId,
    long PostId,
    AchievementUserDto Author,
    string CommentText,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AchievementPostDto(
    long PostId,
    AchievementUserDto Author,
    string Title,
    string? Description,
    IReadOnlyCollection<string> Tags,
    PostType PostType,
    PostVisibility Visibility,
    bool IsSystemGenerated,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int LikeCount,
    bool HasLiked,
    IReadOnlyCollection<AchievementUserDto> RecentLikers,
    int CommentCount,
    IReadOnlyCollection<AchievementCommentDto> Comments
);

public record AchievementPostCreateDto(
    [Required, MaxLength(255)] string Title,
    string? Description,
    IReadOnlyCollection<string>? Tags,
    PostVisibility? Visibility
);

public record AchievementPostUpdateDto(
    [Required, MaxLength(255)] string Title,
    string? Description,
    IReadOnlyCollection<string>? Tags,
    PostVisibility? Visibility
);

public record AchievementCommentCreateDto(
    [Required, MaxLength(2000)] string CommentText
);

public record AchievementCommentUpdateDto(
    [Required, MaxLength(2000)] string CommentText
);

public record AchievementFeedFilterDto(
    long? AuthorId,
    string? Author,
    string? Tag,
    DateTime? FromDate,
    DateTime? ToDate
);