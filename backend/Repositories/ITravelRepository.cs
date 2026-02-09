using backend.DTO.Travel;

namespace backend.Repositories;

public interface ITravelRepository
{
    Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, IReadOnlyCollection<int> employeeIds);
    Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(int employeeId);
    Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(int employeeId);
}