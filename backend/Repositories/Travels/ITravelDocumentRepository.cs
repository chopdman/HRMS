using backend.Entities.Travels;

namespace backend.Repositories.Travels;

public interface ITravelDocumentRepository
{
    Task<TravelDocument> AddAsync(TravelDocument document);
    Task<IReadOnlyCollection<TravelDocument>> GetAsync(long? travelId, long? employeeId);
}