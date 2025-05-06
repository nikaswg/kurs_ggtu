using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using MyApp.DataLayer.Models;

namespace MyApp.DataLayer
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.NameId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Укажите строку подключения к вашему SQL Server
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=bdkurs;Trusted_Connection=True;");
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