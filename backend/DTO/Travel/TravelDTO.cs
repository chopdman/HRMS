using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Travel;

public record TravelAssignmentCreateDto(
    [Required] int EmployeeId
);

public record TravelCreateDto(
    [Required, MaxLength(200)] string TravelName,
    [Required, MaxLength(200)] string Destination,
    [MaxLength(2000)] string? Purpose,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] int CreatedById,
    [MinLength(1)] List<TravelAssignmentCreateDto> Assignments
);

public record TravelResponseDto(
    int TravelId,
    string TravelName,
    string Destination,
    string? Purpose,
    DateTime StartDate,
    DateTime EndDate,
    int CreatedById,
    IReadOnlyCollection<int> AssignedEmployeeIds
);

public record TravelAssignedDto(
    int TravelId,
    string TravelName,
    string Destination,
    DateTime StartDate,
    DateTime EndDate
);

public record TravelAssignmentDto(
    int AssignId,
    int TravelId,
    string TravelName,
    string Destination,
    DateTime StartDate,
    DateTime EndDate
);