using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;

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
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Component>(c => c.Category);
            _dbContext.LoadOptions = loadOptions;

            return _dbContext.Components.ToList();
        }

        public PagedResult<Component> GetComponentsPaged(int pageNumber, int pageSize)
        {
            using (var dbContext = new AppDbContext())
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Component>(c => c.Category);
                dbContext.LoadOptions = loadOptions;

                var query = dbContext.Components;
                var totalCount = query.Count();

                var components = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResult<Component>
                {
                    Results = components,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public List<Component> SearchComponents(string searchText = null, decimal? minPrice = null, decimal? maxPrice = null, bool exactMatch = false)
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Component>(c => c.Category);
            _dbContext.LoadOptions = loadOptions;

            var query = _dbContext.Components.AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = exactMatch
                    ? query.Where(c => c.Name == searchText || c.Description == searchText)
                    : query.Where(c => c.Name.Contains(searchText) || c.Description.Contains(searchText));
            }

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            return query.ToList();
        }

        public List<Component> SearchByName(string name, bool exactMatch = false)
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Component>(c => c.Category);
            _dbContext.LoadOptions = loadOptions;

            var query = _dbContext.Components.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = exactMatch
                    ? query.Where(c => c.Name == name)
                    : query.Where(c => c.Name.Contains(name));
            }

            return query.ToList();
        }

        public List<Component> FilterByPrice(decimal? minPrice, decimal? maxPrice)
        {
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<Component>(c => c.Category);
            _dbContext.LoadOptions = loadOptions;

            var query = _dbContext.Components.AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            return query.ToList();
        }

        public List<Component> SortComponents(List<Component> components, string sortField, ref int sortDirection)
        {
            switch (sortField)
            {
                case "Name":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Name).ToList()
                        : components.OrderByDescending(c => c.Name).ToList();
                    break;
                case "Description":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Description).ToList()
                        : components.OrderByDescending(c => c.Description).ToList();
                    break;
                case "Price":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Price).ToList()
                        : components.OrderByDescending(c => c.Price).ToList();
                    break;
                case "Category.Name":
                    components = sortDirection == 1
                        ? components.OrderBy(c => c.Category?.Name ?? "").ToList()
                        : components.OrderByDescending(c => c.Category?.Name ?? "").ToList();
                    break;
            }

            sortDirection = sortDirection == 0 ? 1 : (sortDirection == 1 ? -1 : 0);
            return sortDirection == 0 ? components.OrderBy(c => c.ComponentID).ToList() : components;
        }

        public void AddComponent(Component component, string currentUserRole)
        {
            if (currentUserRole != "Admin")
                throw new UnauthorizedAccessException("Only admins can add components");

            _dbContext.Components.InsertOnSubmit(component);
            _dbContext.SubmitChanges();
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

            _dbContext.Components.InsertOnSubmit(component);
            _dbContext.SubmitChanges();
        }

        public void UpdateComponent(Component component)
        {
            var existingComponent = _dbContext.Components
                .FirstOrDefault(c => c.ComponentID == component.ComponentID);

            if (existingComponent != null)
            {
                existingComponent.Name = component.Name;
                existingComponent.Description = component.Description;
                existingComponent.Price = component.Price;
                existingComponent.CategoryID = component.CategoryID;
                _dbContext.SubmitChanges();
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