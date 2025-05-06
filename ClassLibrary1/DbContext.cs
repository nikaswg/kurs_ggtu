using Microsoft.EntityFrameworkCore;
using MyApp.DataLayer.Models; // Убедитесь, что пространство имен Models правильное
using System.Collections.Generic;
using System.Linq;


namespace MyApp.DataLayer
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Component> Components { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; } // Добавил DbSet<User>

        public AppDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Component>().HasKey(c => c.ComponentID);
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryID);
            modelBuilder.Entity<User>().HasKey(u => u.NameId); // Добавил конфигурацию для User

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Укажите строку подключения к вашему SQL Server
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=bdkurs;Trusted_Connection=True;");
        }

        public List<Component> GetAllComponents()
        {
            // Используем LINQ для получения всех комплектующих
            return Components.AsNoTracking().ToList(); // Добавил Include для Category
        }

        public void AddComponent(Component component)
        {
            // Добавляем комплектующее и сохраняем изменения
            Components.Add(component);
            SaveChanges();
        }

        public List<User> GetUsers()
        {
            // Используем LINQ для получения всех пользователей
            return Users.AsNoTracking().ToList();
        }

        public void AddUser(User user)
        {
            // Добавляем пользователя и сохраняем изменения
            Users.Add(user);
            SaveChanges();
        }
    }
}