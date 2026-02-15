using backend.DTO.Travels;
using backend.Entities.Travels;

namespace backend.Repositories.Travels;

public interface ITravelRepository
{
    Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, IReadOnlyCollection<long> employeeIds, long currentUserId);
    // Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(long employeeId);
    Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(long employeeId, long? createdById);
    Task<IReadOnlyCollection<TravelAssignedDto>> GetCreatedTravelsAsync(long createdById);
    Task<Travel?> GetByIdAsync(long travelId);
    Task<Travel?> GetByIdWithAssignmentsAsync(long travelId);
    Task SaveAsync();
    Task DeleteAsync(Travel travel);
}