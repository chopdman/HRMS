using backend.DTO.Travels;

namespace backend.Repositories.Travels;

public interface ITravelRepository
{
    Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, IReadOnlyCollection<int> employeeIds);
    Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(int employeeId);
    Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(int employeeId);
}