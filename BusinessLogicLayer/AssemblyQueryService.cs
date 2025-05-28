using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;

namespace MyApp.BusinessLogic
{
    public class AssemblyQueryService
    {
        private readonly string _connectionString;

        public AssemblyQueryService(AppDbContext dbContext)
        {
            _connectionString = dbContext.Connection.ConnectionString;
        }

        /// <summary>
        /// Получить все сборки с деталями (LINQ to SQL версия)
        /// </summary>
        public List<Assembly> GetAllAssembliesWithDetails()
        {
            using (var context = new AppDbContext())
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Assembly>(a => a.AssemblyComponents);
                loadOptions.LoadWith<AssemblyComponent>(ac => ac.Component);
                loadOptions.LoadWith<Assembly>(a => a.User);

                context.LoadOptions = loadOptions;

                return context.Assemblies.ToList();
            }
        }

        /// <summary>
        /// Асинхронная версия (использует Task.Run)
        /// </summary>
        public Task<List<Assembly>> GetAllAssembliesWithDetailsAsync()
        {
            return Task.Run(() => GetAllAssembliesWithDetails());
        }
    }
}