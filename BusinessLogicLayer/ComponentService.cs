using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using MyApp;

namespace MyApp.BusinessLogic
{
    public class ComponentService
    {
        private readonly AppDbContext _dbContext;

        public ComponentService()
        {
            _dbContext = new AppDbContext();
        }

        public List<Component> GetAllComponents()
        {
            return _dbContext.Components
                .Include(c => c.Category)
                .AsNoTracking()
                .ToList();
        }

        public PagedResult<Component> GetComponentsPaged(int pageNumber, int pageSize)
        {
            var components = _dbContext.Components
                .Include(c => c.Category)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalCount = _dbContext.Components.Count();

            return new PagedResult<Component>
            {
                Results = components,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public List<Component> SearchComponents(string searchText, decimal? minPrice = null, decimal? maxPrice = null, bool exactMatch = false)
        {
            var query = _dbContext.Components.Include(c => c.Category).AsNoTracking();



            // Фильтрация по цене
            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            return query.ToList();
        }

        public List<Component> FilterComponents(decimal? minPrice, decimal? maxPrice)
        {
            return SearchComponents(null, minPrice, maxPrice);
        }

        public List<Component> SortComponents(List<Component> components, string sortField, ref int sortDirection)
        {
            switch (sortField)
            {
                case "Name":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Name.Length).ToList()
                        : components.OrderByDescending(c => c.Name.Length).ToList();
                    break;
                case "Description":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Description.Length).ToList()
                        : components.OrderByDescending(c => c.Description.Length).ToList();
                    break;
                case "Price":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Price).ToList()
                        : components.OrderByDescending(c => c.Price).ToList();
                    break;
                case "Category.Name":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Category?.Name?.Length ?? 0).ToList()
                        : components.OrderByDescending(c => c.Category?.Name?.Length ?? 0).ToList();
                    break;
            }

            sortDirection = sortDirection == 0 ? 1 : (sortDirection == 1 ? -1 : 0);

            return sortDirection == 0 ? components.OrderBy(c => c.ComponentID).ToList() : components;
        }

        public void AddComponent(Component component, string currentUserRole)
        {
            if (currentUserRole != "Admin")
                throw new UnauthorizedAccessException("Only admins can add components");

            _dbContext.Components.Add(component);
            _dbContext.SaveChanges();
        }

        public void AddSimpleComponent(string name, string description, decimal price, string currentUserRole)
        {
            if (currentUserRole != "Admin")
                throw new UnauthorizedAccessException("Only admins can add components");

            var component = new Component
            {
                Name = name,
                Description = description,
                Price = price,
                CategoryID = 0
            };

            _dbContext.Components.Add(component);
            _dbContext.SaveChanges();
        }

        public List<Component> SearchComponentsByName(string name, bool exactMatch = false)
        {
            var query = _dbContext.Components.Include(c => c.Category).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (exactMatch)
                {
                    query = query.Where(c => c.Name == name);
                }
                else
                {
                    query = query.Where(c => c.Name.Contains(name));
                }
            }

            return query.ToList();
        }

        public void UpdateComponent(Component component)
        {
            var existingComponent = _dbContext.Components.Find(component.ComponentID);
            if (existingComponent != null)
            {
                _dbContext.Entry(existingComponent).CurrentValues.SetValues(component);
                _dbContext.SaveChanges();
            }
        }

        public class PagedResult<T>
        {
            public IEnumerable<T> Results { get; set; }
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
    }
}