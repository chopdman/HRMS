using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public record RoleCreateDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);

public record RoleResponseDto(
    int RoleId,
    string Name,
    string? Description
);