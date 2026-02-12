using backend.Data;
using backend.Entities.Travels;
using backend.DTO.Travels;
using backend.Services.Common;
using backend.Repositories.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Travels;

public class TravelDocumentService
{
    private readonly AppDbContext _db;
    private readonly ITravelDocumentRepository _repository;
    private readonly CloudinaryService _cloudinary;

    public TravelDocumentService(AppDbContext db, ITravelDocumentRepository repository, CloudinaryService cloudinary)
    {
        _db = db;
        _repository = repository;
        _cloudinary = cloudinary;
    }

    public async Task<TravelDocumentDto> UploadAsync(TravelDocumentUploadDto dto, long currentUserId, string role)
    {
        if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Managers cannot upload travel documents.");
        }

        var employeeId = dto.EmployeeId;
        if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            employeeId = currentUserId;
        }

        if (!employeeId.HasValue)
        {
            throw new ArgumentException("EmployeeId is required for this upload.");
        }

        var upload = await _cloudinary.UploadAsync(dto.File, "travel-documents");
// Console.WriteLine(role);
        var document = new TravelDocument
        {
            TravelId = dto.TravelId,
            EmployeeId = employeeId.Value,
            UploadedBy = currentUserId,
            OwnerType = role.Equals("HR")
                ? DocumentOwnerType.HR
                : DocumentOwnerType.Employee,
            DocumentType = dto.DocumentType,
            FileName = upload.FileName,
            FilePath = upload.Url,
            UploadedAt = DateTime.UtcNow
        };

        var saved = await _repository.AddAsync(document);

        return new TravelDocumentDto(
            saved.DocumentId,
            saved.TravelId,
            saved.EmployeeId,
            saved.UploadedBy,
            saved.OwnerType.ToString(),
            saved.DocumentType,
            saved.FileName,
            saved.FilePath,
            saved.UploadedAt);
    }

    public async Task<IReadOnlyCollection<TravelDocumentDto>> ListAsync(long currentUserId, string role, long? travelId, long? employeeId)
    {
        if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            employeeId = currentUserId;
        }
        else if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (!employeeId.HasValue)
            {
                throw new ArgumentException("employeeId is required for manager view.");
            }

            var isTeamMember = await _db.Users
                .AnyAsync(u => u.UserId == employeeId.Value && u.ManagerId == currentUserId);

            if (!isTeamMember)
            {
                throw new ArgumentException("Manager can only view team member documents.");
            }
        }

        var documents = await _repository.GetAsync(travelId, employeeId);
        return documents.Select(d => new TravelDocumentDto(
            d.DocumentId,
            d.TravelId,
            d.EmployeeId,
            d.UploadedBy,
            d.OwnerType.ToString(),
            d.DocumentType,
            d.FileName,
            d.FilePath,
            d.UploadedAt)).ToList();
    }
}