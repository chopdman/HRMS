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

    public async Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, IReadOnlyCollection<long> employeeIds, long currentUserId)
    {
        var travel = new Travel
        {
            TravelName = dto.TravelName,
            Destination = dto.Destination,
            Purpose = dto.Purpose,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedBy = currentUserId
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

    // public async Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(long employeeId)
    // {
    //     return await _db.TravelAssignments
    //         .Where(a => a.EmployeeId == employeeId)
    //         .Include(a => a.Travel)
    //         .Select(a => new TravelAssignedDto(
    //             a.Travel!.TravelId,
    //             a.Travel.TravelName!,
    //             a.Travel.Destination!,
    //             a.Travel.StartDate,
    //             a.Travel.EndDate
    //         ))
    //         .ToListAsync();
    // }

    public async Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(long employeeId, long? createdById)
    {
        var query = _db.TravelAssignments
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.Travel)
            .ThenInclude(t => t.Assignments)
            .AsQueryable();

        if (createdById.HasValue)
        {
            query = query.Where(a => a.Travel != null && a.Travel.CreatedBy == createdById.Value);
        }

        return await query
            .Select(a => new TravelAssignmentDto(
                a.AssignmentId,
                a.Travel!.TravelId,
                a.Travel.TravelName!,
                a.Travel.Destination!,
                a.Travel.Purpose,
                a.Travel.StartDate,
                a.Travel.EndDate,
                a.Travel.Assignments.Select(assignment => assignment.EmployeeId).ToList()
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<TravelAssignedDto>> GetCreatedTravelsAsync(long createdById)
    {
        return await _db.Travels
            .Where(t => t.CreatedBy == createdById)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TravelAssignedDto(
                t.TravelId,
                t.TravelName ?? string.Empty,
                t.Destination ?? string.Empty,
                t.StartDate,
                t.EndDate
            ))
            .ToListAsync();
    }

    public async Task<Travel?> GetByIdAsync(long travelId)
    {
        return await _db.Travels.FirstOrDefaultAsync(t => t.TravelId == travelId);
    }

    public async Task<Travel?> GetByIdWithAssignmentsAsync(long travelId)
    {
        return await _db.Travels
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.TravelId == travelId);
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Travel travel)
    {
        _db.Travels.Remove(travel);
        await _db.SaveChangesAsync();
    }
}