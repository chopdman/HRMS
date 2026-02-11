using backend.Data;
using backend.Entities.Travels;
using backend.DTO.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Travels;

public class TravelRepository : ITravelRepository
{
    private readonly AppDbContext _db;

    public TravelRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, IReadOnlyCollection<long> employeeIds)
    {
        var travel = new Travel
        {
            TravelName = dto.TravelName,
            Destination = dto.Destination,
            Purpose = dto.Purpose,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedBy = dto.CreatedById
        };

        foreach (var employeeId in employeeIds)
        {
            travel.Assignments.Add(new TravelAssignment
            {
                EmployeeId = employeeId
            });
        }

        _db.Travels.Add(travel);
        await _db.SaveChangesAsync();

        return new TravelResponseDto(
            travel.TravelId,
            travel.TravelName,
            travel.Destination,
            travel.Purpose,
            travel.StartDate,
            travel.EndDate,
            travel.CreatedBy,
            travel.Assignments.Select(a => a.EmployeeId).ToList());
    }

    public async Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(long employeeId)
    {
        return await _db.TravelAssignments
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.Travel)
            .Select(a => new TravelAssignedDto(
                a.Travel!.TravelId,
                a.Travel.TravelName!,
                a.Travel.Destination!,
                a.Travel.StartDate,
                a.Travel.EndDate
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(long employeeId)
    {
        return await _db.TravelAssignments
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.Travel)
            .Select(a => new TravelAssignmentDto(
                a.AssignmentId,
                a.Travel!.TravelId,
                a.Travel.TravelName!,
                a.Travel.Destination!,
                a.Travel.StartDate,
                a.Travel.EndDate
            ))
            .ToListAsync();
    }
}