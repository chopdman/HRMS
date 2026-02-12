using backend.DTO.Travels;

namespace backend.Repositories.Travels;

public interface ITravelRepository
{
    Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, IReadOnlyCollection<long> employeeIds,long currentUserId);
    // Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(long employeeId);
    Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(long employeeId);
}