namespace backend.DTO.Common;

public record EmployeeLookupDto(
    long Id,
    string FullName,
    string Email
);
public record UserResponseDto(
    long Id,
    string FullName,
    string Email,
    string Phone,
    DateTime? DateOfBirth,
    DateTime DateOfJoining,
    string? ProfilePhotoUrl,
    string? Department,
    string? Designation,
    string? Manager,
    string? Role
);