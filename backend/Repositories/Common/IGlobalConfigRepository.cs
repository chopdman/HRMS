using backend.Entities.Common;

namespace backend.Repositories.Common
{
    public interface IGlobalConfigRepository
    {
        Task<GlobalConfig?> GetByFieldAsync(string field);
        Task<GlobalConfig> UpsertAsync(GlobalConfig config);
    }
}