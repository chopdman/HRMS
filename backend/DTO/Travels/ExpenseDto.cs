using backend.Entities.Travels;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Travels
{

public record ExpenseCreateDto(
    [Required] long AssignId,
    [Required] long CategoryId,
    [Required] decimal Amount,
    [Required, MaxLength(10)] string Currency,
    [Required] DateTime ExpenseDate
);

public record ExpenseResponseDto(
    long ExpenseId,
    // long AssignId,
    long CategoryId,
    decimal Amount,
    string Currency,
    DateTime ExpenseDate,
    ExpenseStatus Status,
    DateTime? SubmittedAt,
    long? ReviewedById,
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