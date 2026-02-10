using backend.Entities.Travels;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Travels
{

public record ExpenseCreateDto(
    [Required] int AssignId,
    [Required] int CategoryId,
    [Required] decimal Amount,
    [Required, MaxLength(10)] string Currency,
    [Required] DateTime ExpenseDate
);

public record ExpenseResponseDto(
    int ExpenseId,
    // int AssignId,
    int CategoryId,
    decimal Amount,
    string Currency,
    DateTime ExpenseDate,
    ExpenseStatus Status,
    DateTime? SubmittedAt,
    int? ReviewedById,
    DateTime? ReviewedAt,
    string? Remarks
);

public record ExpenseReviewDto(
    [Required] ExpenseStatus Status,
    [MaxLength(2000)] string? Remarks
);

public class ExpenseProofUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
}