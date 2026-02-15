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

        if (role.Equals("HR", StringComparison.OrdinalIgnoreCase) && !employeeId.HasValue)
        {
            var travelExists = await _db.Travels.AnyAsync(t => t.TravelId == dto.TravelId);
            if (!travelExists)
            {
                throw new ArgumentException("Travel not found.");
            }
        }
        else if (!employeeId.HasValue)
        {
            throw new ArgumentException("EmployeeId is required for this upload.");
        }

        if (employeeId.HasValue)
        {
            var assignmentExists = await _db.TravelAssignments
                .AnyAsync(a => a.TravelId == dto.TravelId && a.EmployeeId == employeeId.Value);

            if (!assignmentExists)
            {
                throw new ArgumentException("Employee is not assigned to this travel.");
            }
        }

        var upload = await _cloudinary.UploadAsync(dto.File, "travel-documents");
        var document = new TravelDocument
        {
            TravelId = dto.TravelId,
            EmployeeId = employeeId,
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

        var uploaderName = await _db.Users
            .Where(u => u.UserId == currentUserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync();

        return new TravelDocumentDto(
            saved.DocumentId,
            saved.TravelId,
            saved.EmployeeId,
            saved.UploadedBy,
            uploaderName,
            saved.OwnerType.ToString(),
            saved.DocumentType,
            saved.FileName,
            saved.FilePath,
            saved.UploadedAt);
    }

    public async Task<IReadOnlyCollection<TravelDocumentDto>> ListAsync(long currentUserId, string role, long? travelId, long? employeeId)
    {
        IQueryable<TravelDocument> query = _db.TravelDocuments.AsNoTracking();

        if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            var assignedTravelIds = await _db.TravelAssignments
                .Where(a => a.EmployeeId == currentUserId)
                .Select(a => a.TravelId)
                .ToListAsync();

            query = query.Where(d =>
                (d.EmployeeId == currentUserId) ||
                (d.OwnerType == DocumentOwnerType.HR && d.EmployeeId == null && assignedTravelIds.Contains(d.TravelId))
            );
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

            var assignedTravelIds = await _db.TravelAssignments
                .Where(a => a.EmployeeId == employeeId.Value)
                .Select(a => a.TravelId)
                .ToListAsync();

            query = query.Where(d =>
                (d.EmployeeId == employeeId.Value) ||
                (d.OwnerType == DocumentOwnerType.HR && d.EmployeeId == null && assignedTravelIds.Contains(d.TravelId))
            );
        }
        else
        {
            if (employeeId.HasValue)
            {
                query = query.Where(d => d.EmployeeId == employeeId.Value);
            }
        }

        if (travelId.HasValue)
        {
            query = query.Where(d => d.TravelId == travelId.Value);
        }

        var documents = await query
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        var uploaderIds = documents
            .Select(d => d.UploadedBy)
            .Distinct()
            .ToList();

        var uploaderNames = await _db.Users
            .Where(u => uploaderIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.FullName })
            .ToDictionaryAsync(u => u.UserId, u => u.FullName);

        return documents.Select(d => new TravelDocumentDto(
            d.DocumentId,
            d.TravelId,
            d.EmployeeId,
            d.UploadedBy,
            uploaderNames.GetValueOrDefault(d.UploadedBy),
            d.OwnerType.ToString(),
            d.DocumentType,
            d.FileName,
            d.FilePath,
            d.UploadedAt)).ToList();
    }

    public async Task<TravelDocumentDto> UpdateAsync(long documentId, TravelDocumentUpdateDto dto, long currentUserId)
    {
        var document = await _repository.GetByIdAsync(documentId);
        if (document is null)
        {
            throw new ArgumentException("Document not found.");
        }

        if (document.UploadedBy != currentUserId)
        {
            throw new ArgumentException("You can only update documents you uploaded.");
        }

        if (!string.IsNullOrWhiteSpace(dto.DocumentType))
        {
            document.DocumentType = dto.DocumentType;
        }

        if (dto.File is not null)
        {
            var upload = await _cloudinary.UploadAsync(dto.File, "travel-documents");
            document.FileName = upload.FileName;
            document.FilePath = upload.Url;
            document.UploadedAt = DateTime.UtcNow;
        }

        await _repository.SaveAsync();

        var uploaderName = await _db.Users
            .Where(u => u.UserId == document.UploadedBy)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync();

        return new TravelDocumentDto(
            document.DocumentId,
            document.TravelId,
            document.EmployeeId,
            document.UploadedBy,
            uploaderName,
            document.OwnerType.ToString(),
            document.DocumentType,
            document.FileName,
            document.FilePath,
            document.UploadedAt);
    }

    public async Task DeleteAsync(long documentId, long currentUserId)
    {
        var document = await _repository.GetByIdAsync(documentId);
        if (document is null)
        {
            throw new ArgumentException("Document not found.");
        }

        if (document.UploadedBy != currentUserId)
        {
            throw new ArgumentException("You can only delete documents you uploaded.");
        }

        await _repository.DeleteAsync(document);
    }
}