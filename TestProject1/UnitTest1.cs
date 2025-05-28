using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using MyApp.BusinessLogic;
using MyApp.BusinessLogicLayer.Services;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using System.ComponentModel;
using System.Reflection;
using System.Data.Linq;
using DataComponent = MyApp.DataLayer.Models.Component;

namespace MyApp.Tests
{
    public class LinqToSqlTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AssemblyQueryService _assemblyQueryService;
        private readonly AssemblyService _assemblyService;
        private readonly UserService _userService;
        private readonly ComponentService _componentService;

        public LinqToSqlTests()
        {
            _context = new AppDbContext();
            _assemblyQueryService = new AssemblyQueryService(_context);
            _assemblyService = new AssemblyService();
            _userService = new UserService(_context);
            _componentService = new ComponentService();

            // Очистка базы данных перед каждым тестом
            ClearDatabase();
        }

        private void ClearDatabase()
        {
            // Удаляем в правильном порядке, учитывая зависимости
            _context.UserVotes.DeleteAllOnSubmit(_context.UserVotes);
            _context.SubmitChanges();

            _context.Reviews.DeleteAllOnSubmit(_context.Reviews);
            _context.SubmitChanges();

            _context.AssemblyComponents.DeleteAllOnSubmit(_context.AssemblyComponents);
            _context.SubmitChanges();

            _context.Assemblies.DeleteAllOnSubmit(_context.Assemblies);
            _context.SubmitChanges();

            _context.Components.DeleteAllOnSubmit(_context.Components);
            _context.SubmitChanges();

            _context.Users.DeleteAllOnSubmit(_context.Users);
            _context.SubmitChanges();

            _context.Categories.DeleteAllOnSubmit(_context.Categories);
            _context.SubmitChanges();
        }

        public void Dispose()
        {
            ClearDatabase();
            _context.Dispose();
            _componentService.Dispose();
        }

        [Fact]
        public async Task GetAllAssembliesWithDetailsAsync_ShouldReturnAssemblies_WhenTheyExist()
        {
            // Arrange
            var user = new User { Name = "Test User", Email = "test@example.com", Password = "pass" };
            var category = new Category { Name = "Процессор" };
            var component = new DataComponent { Name = "CPU i7", Category = category, Price = 300 };

            _context.Users.InsertOnSubmit(user);
            _context.Categories.InsertOnSubmit(category);
            _context.Components.InsertOnSubmit(component);
            _context.SubmitChanges();

            // Сначала создаем сборку без компонентов
            var assembly = new MyApp.DataLayer.Models.Assembly
            {
                Description = "Test Assembly",
                NameId = user.NameId,
                Rating = 5
            };

            _context.Assemblies.InsertOnSubmit(assembly);
            _context.SubmitChanges(); // Сохраняем, чтобы получить AssemblyID

            // Затем добавляем компонент в сборку
            var assemblyComponent = new AssemblyComponent
            {
                AssemblyID = assembly.AssemblyID,
                ComponentID = component.ComponentID
            };

            _context.AssemblyComponents.InsertOnSubmit(assemblyComponent);
            _context.SubmitChanges();

            // Act
            var result = await _assemblyQueryService.GetAllAssembliesWithDetailsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Assembly", result[0].Description);
            Assert.Single(result[0].AssemblyComponents);
            Assert.Equal("CPU i7", result[0].AssemblyComponents.First().Component.Name);
        }

        [Fact]
        public async Task GetAllAssembliesWithDetailsAsync_ShouldReturnEmptyList_WhenNoAssemblies()
        {
            // Act
            var result = await _assemblyQueryService.GetAllAssembliesWithDetailsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CreateAssembly_ShouldReturnTrue_WhenValidData()
        {
            // Arrange
            var user = new User { Name = "Test User", Email = "test@example.com", Password = "pass" };
            _context.Users.InsertOnSubmit(user);
            _context.SubmitChanges();

            var categories = new List<Category>
            {
                new Category { Name = "Процессор" },
                new Category { Name = "Материнская плата" },
                new Category { Name = "Блок питания" },
                new Category { Name = "Корпус" }
            };
            _context.Categories.InsertAllOnSubmit(categories);
            _context.SubmitChanges();

            var components = new List<DataComponent>
            {
                new DataComponent { Name = "CPU", Category = categories[0], CategoryID = categories[0].CategoryID },
                new DataComponent { Name = "Motherboard", Category = categories[1], CategoryID = categories[1].CategoryID },
                new DataComponent { Name = "PSU", Category = categories[2], CategoryID = categories[2].CategoryID },
                new DataComponent { Name = "Case", Category = categories[3], CategoryID = categories[3].CategoryID }
            };
            _context.Components.InsertAllOnSubmit(components);
            _context.SubmitChanges();

            var componentIds = components.Select(c => c.ComponentID).ToList();

            // Act
            var result = _assemblyService.CreateAssembly("Test Assembly", componentIds, user.NameId, out string errorMessage);

            // Assert
            Assert.True(result);
            Assert.True(string.IsNullOrEmpty(errorMessage));
            Assert.Single(_context.Assemblies);
        }

        [Fact]
        public void CreateAssembly_ShouldReturnFalse_WhenDescriptionIsEmpty()
        {
            // Arrange
            var user = new User { Name = "Test User", Email = "test@example.com", Password = "pass" };
            _context.Users.InsertOnSubmit(user);
            _context.SubmitChanges();

            var componentIds = new List<int> { 1, 2 };

            // Act
            var result = _assemblyService.CreateAssembly("", componentIds, user.NameId, out string errorMessage);

            // Assert
            Assert.False(result);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void Register_ValidUser_AddsUserSuccessfully()
        {
            // Act
            _userService.Register("Test User", "test@example.com", "password123");

            // Assert
            var user = _context.Users.FirstOrDefault(u => u.Email == "test@example.com");
            Assert.NotNull(user);
            Assert.Equal("Test User", user.Name);
            Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.Password));
        }

        [Fact]
        public void Authenticate_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
            var user = new User { Name = "Test User", Email = "test@example.com", Password = passwordHash };
            _context.Users.InsertOnSubmit(user);
            _context.SubmitChanges();

            // Act
            var result = _userService.Authenticate("test@example.com", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public void GetAllComponents_ShouldReturnComponents_WhenTheyExist()
        {
            // Arrange
            var category = new Category { Name = "Процессор" };
            _context.Categories.InsertOnSubmit(category);

            var component = new DataComponent { Name = "CPU i7", Category = category, Price = 300 };
            _context.Components.InsertOnSubmit(component);
            _context.SubmitChanges();

            // Act
            var result = _componentService.GetAllComponents();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("CPU i7", result[0].Name);
            Assert.Equal("Процессор", result[0].Category.Name);
        }

        [Fact]
        public void GetComponentsPaged_ShouldReturnCorrectPage()
        {
            // Arrange
            var category = new Category { Name = "Процессор" };
            _context.Categories.InsertOnSubmit(category);

            for (int i = 1; i <= 15; i++)
            {
                _context.Components.InsertOnSubmit(new DataComponent
                {
                    Name = $"Component {i}",
                    Category = category,
                    Price = i * 100
                });
            }
            _context.SubmitChanges();

            // Act
            var result = _componentService.GetComponentsPaged(2, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Results.Count());
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal("Component 6", result.Results.First().Name);
        }
    }
}