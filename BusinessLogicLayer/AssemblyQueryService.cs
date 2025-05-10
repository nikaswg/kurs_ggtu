using Microsoft.EntityFrameworkCore;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.BusinessLogic
{
    public class AssemblyQueryService
    {
        private readonly AppDbContext _dbContext;

        public AssemblyQueryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Получить все сборки.
        /// </summary>
        public async Task<List<Assembly>> GetAllAssembliesWithDetailsAsync()
        {
            return await _dbContext.Assemblies
                .AsNoTracking()
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.User) // Если нужно отображать пользователя
                .ToListAsync();
        }
    }
}