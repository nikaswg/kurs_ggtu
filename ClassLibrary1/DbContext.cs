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
        public DbSet<User> Users { get; set; }
        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<AssemblyComponent> AssemblyComponents { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Component>().HasKey(c => c.ComponentID);
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryID);
            modelBuilder.Entity<User>().HasKey(u => u.NameId);
            modelBuilder.Entity<Assembly>().HasKey(a => a.AssemblyID);
            modelBuilder.Entity<AssemblyComponent>().HasKey(ac => ac.AssemblyComponentID);
            

            modelBuilder.Entity<Assembly>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.NameId);

            modelBuilder.Entity<AssemblyComponent>()
                .HasOne(ac => ac.Assembly)
                .WithMany(a => a.AssemblyComponents)
                .HasForeignKey(ac => ac.AssemblyID);

            modelBuilder.Entity<AssemblyComponent>()
                .HasOne(ac => ac.Component)
                .WithMany()
                .HasForeignKey(ac => ac.ComponentID);
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Укажите строку подключения к вашему SQL Server
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=bdkurs2;Trusted_Connection=True;");
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