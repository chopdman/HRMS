using backend.Data;
using backend.Entities.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Travels
{
    
    public class TravelDocumentRepository :ITravelDocumentRepository
    {
        private readonly AppDbContext _db;

        public TravelDocumentRepository(AppDbContext db)
        {
            _db = db;
        }

         public async Task<TravelDocument> AddAsync(TravelDocument document)
    {
        _db.TravelDocuments.Add(document);
        await _db.SaveChangesAsync();
        return document;
    }

    public async Task<IReadOnlyCollection<TravelDocument>> GetAsync(long? travelId, long? employeeId)
    {
        var query = _db.TravelDocuments.AsNoTracking().AsQueryable();

        if (travelId.HasValue)
        {
            query = query.Where(d => d.TravelId == travelId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(d => d.EmployeeId == employeeId.Value);
        }

        return await query
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }
        
    }
}