using Microsoft.EntityFrameworkCore;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.BusinessLogic
{
    public class AssemblyService
    {
        private readonly AppDbContext _dbContext;

        public AssemblyService()
        {
            _dbContext = new AppDbContext();
        }

        public bool CreateAssembly(string name, string description, List<int> componentIds, int userId, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Проверяем, существует ли пользователь
                var userExists = _dbContext.Users.Any(u => u.NameId == userId);
                if (!userExists)
                {
                    errorMessage = "Пользователь не найден.";
                    return false;
                }

                var assembly = new Assembly
                {
                    Description = description,
                    NameId = userId,
                    Rating = 0,
                    AssemblyComponents = componentIds.Select(compId => new AssemblyComponent { ComponentID = compId }).ToList()
                };

                _dbContext.Assemblies.Add(assembly);
                _dbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                errorMessage = $"Ошибка при сохранении сборки: {ex.InnerException?.Message ?? ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Непредвиденная ошибка: {ex.Message}";
                return false;
            }
        }

        public List<Assembly> GetUserAssemblies(int userId)
        {
            return _dbContext.Assemblies
                .Include(a => a.AssemblyComponents)
                .ThenInclude(ac => ac.Component)
                .Where(a => a.NameId == userId)
                .ToList();
        }
    }
}