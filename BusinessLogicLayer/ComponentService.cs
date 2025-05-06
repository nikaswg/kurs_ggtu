using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            return _dbContext.Components
                .AsNoTracking()
                .ToList();
        }

        public PagedResult<Component> GetComponentsPaged(int pageNumber, int pageSize)
        {
            var components = _dbContext.Components
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

        public List<Component> SearchComponents(string searchText)
        {
            return _dbContext.Components
                .AsNoTracking()
                .Where(c => c.Name.Contains(searchText) || c.Description.Contains(searchText))
                .ToList();
        }

        public List<Component> FilterComponents(decimal? minPrice, decimal? maxPrice)
        {
            IQueryable<Component> query = _dbContext.Components
                .AsNoTracking();

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

        public void AddComponent(Component component)
        {
            _dbContext.Components.Add(component);
            _dbContext.SaveChanges();
        }

        // Альтернативный метод добавления без Category
        public void AddSimpleComponent(string name, string description, decimal price)
        {
            var component = new Component
            {
                Name = name,
                Description = description,
                Price = price,
                CategoryID = 0 // или другое значение по умолчанию
            };

            _dbContext.Components.Add(component);
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