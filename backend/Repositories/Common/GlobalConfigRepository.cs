using backend.Data;
using backend.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Common
{
    public class GlobalConfigRepository : IGlobalConfigRepository
    {
        private readonly AppDbContext _db;

        public GlobalConfigRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<GlobalConfig?> GetByFieldAsync(string field)
        {
            return await _db.Set<GlobalConfig>().FirstOrDefaultAsync(c => c.ConfigField == field);
        }

        public async Task<GlobalConfig> UpsertAsync(GlobalConfig config)
        {
            var existing = await _db.Set<GlobalConfig>().FirstOrDefaultAsync(c => c.ConfigField == config.ConfigField);
            if (existing is null)
            {
                _db.Set<GlobalConfig>().Add(config);
            }
            else
            {
                existing.ConfigValue = config.ConfigValue;
                existing.RelatedTable = config.RelatedTable;
                existing.UpdatedBy = config.UpdatedBy;
                existing.UpdatedAt = config.UpdatedAt;
            }

            await _db.SaveChangesAsync();
            return existing ?? config;
        }
    }
}