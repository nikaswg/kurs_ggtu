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

        public bool CreateAssembly(string description, List<int> componentIds, int userId, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Проверяем ограничения на компоненты
                var componentGroups = _dbContext.Components
                    .Where(c => componentIds.Contains(c.ComponentID))
                    .GroupBy(c => c.Category.Name)
                    .ToList();

                foreach (var group in componentGroups)
                {
                    var categoryName = group.Key;
                    var count = group.Count();

                    bool isValid = categoryName switch
                    {
                        "Процессор" => count <= 1,
                        "Материнская плата" => count <= 1,
                        "Блок питания" => count <= 1,
                        "Корпус" => count <= 1,
                        "Видеокарта" => count <= 2,
                        "Оперативная память" => count <= 2,
                        "SSD" or "HDD" => count <= 3,
                        "Охлаждение" => count <= 1,
                        _ => true
                    };

                    if (!isValid)
                    {
                        errorMessage = $"Превышено максимальное количество компонентов типа {categoryName}";
                        return false;
                    }
                }

                // Проверяем обязательные компоненты
                var requiredCategories = new[] { "Процессор", "Материнская плата", "Блок питания", "Корпус" };
                var missingCategories = requiredCategories
                    .Except(componentGroups.Select(g => g.Key))
                    .ToList();

                if (missingCategories.Any())
                {
                    errorMessage = $"Отсутствуют обязательные компоненты: {string.Join(", ", missingCategories)}";
                    return false;
                }

                // Создаем сборку
                var assembly = new Assembly
                {
                    Description = description,
                    NameId = userId,
                    Rating = 0
                };

                // Добавляем сборку в контекст
                _dbContext.Assemblies.InsertOnSubmit(assembly);
                _dbContext.SubmitChanges(); // Сохраняем, чтобы получить AssemblyID

                // Создаем связи с компонентами
                var assemblyComponents = componentIds.Select(compId => new AssemblyComponent
                {
                    AssemblyID = assembly.AssemblyID,
                    ComponentID = compId
                }).ToList();

                // Добавляем связи компонентов
                _dbContext.AssemblyComponents.InsertAllOnSubmit(assemblyComponents);
                _dbContext.SubmitChanges();

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Ошибка: {ex.Message}";
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