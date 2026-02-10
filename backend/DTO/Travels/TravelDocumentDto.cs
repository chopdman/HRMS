using backend.Entities.Travels;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Travels;

public class TravelDocumentUploadDto
{

    [Required]
    public int TravelId { get; set; }

    [Required]
    public int? EmployeeId { get; set; }

    [Required, MaxLength(200)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;
}

public record TravelDocumentDto(
    int DocumentId,
    int TravelId,
    int EmployeeId,
    int UploadedById,
    DocumentOwnerType OwnerType,
    string? DocumentType,
    string? FileName,
    string? FilePath,
    DateTime UploadedAt
);