using backend.DTO.Travels;
using backend.Data;
using backend.Services.Common;
using backend.Repositories.Travels;
using Microsoft.EntityFrameworkCore;
 
namespace backend.Services.Travels;
 
public class TravelService
{
    private readonly AppDbContext _db;
    private readonly ITravelRepository _repository;
 
    private readonly NotificationService _notifications;
    private readonly EmailService _email;
 
    public TravelService(AppDbContext db, ITravelRepository repository, NotificationService notifications, EmailService email)
    {
        _db = db;
        _repository = repository;
        _notifications = notifications;
        _email = email;
    }
 
    public async Task<TravelResponseDto> CreateTravelAsync(TravelCreateDto dto, long currentUserId)
    {
        if (dto.StartDate > dto.EndDate)
        {
            throw new ArgumentException("Start date must be on or before end date.");
        }
 
        if (dto.Assignments is null || dto.Assignments.Count == 0)
        {
            throw new ArgumentException("At least one employee must be assigned.");
        }
 
 
        var employeeIds = dto.Assignments.Select(a => a.EmployeeId).Distinct().ToList();
        var employeesFound = await _db.Users.Where(u => employeeIds.Contains(u.UserId)).Select(u => u.UserId).ToListAsync();
        if (employeesFound.Count != employeeIds.Count)
        {
            throw new ArgumentException("One or more assigned employees not found.");
        }
 
        var created = await _repository.CreateTravelAsync(dto, employeeIds, currentUserId);
 
        var employees = await _db.Users
            .Where(u => employeeIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.Email, u.FullName })
            .ToListAsync();
 
        var title = "New travel assigned";
        var message = $"You have been assigned to travel '{created.TravelName}' ({created.Destination}) from {created.StartDate} to {created.EndDate}.";
 
        await _notifications.CreateForUsersAsync(employees.Select(e => e.UserId), title, message);
 
        foreach (var employee in employees)
        {
            await _email.SendAsync(employee.Email, title, message);
        }
 
        return created;
    }
 
    // public async Task<IReadOnlyCollection<TravelAssignedDto>> GetAssignedTravelsAsync(long employeeId)
    // {
    //     return await _repository.GetAssignedTravelsAsync(employeeId);
    // }
 
    public async Task<IReadOnlyCollection<TravelAssignmentDto>> GetAssignmentsForEmployeeAsync(long employeeId)
    {
        return await _repository.GetAssignmentsForEmployeeAsync(employeeId);
    }
 
}
 