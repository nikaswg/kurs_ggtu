using System;
using System.Linq;
using MyApp.DataLayer.Models;

namespace MyApp.DataLayer.Services
{
    public class UserService
    {
        private readonly DbContext _dbContext;

        public UserService()
        {
            _dbContext = new DbContext();
        }

        public User Authenticate(string email, string password)
        {
            var user = _dbContext.GetUsers().FirstOrDefault(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public void Register(string name, string email, string password)
        {
            if (_dbContext.GetUsers().Any(u => u.Email == email))
                throw new Exception("Пользователь с таким email уже существует.");

            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User",
                NameId = _dbContext.GetUsers().Count + 1
            };

            _dbContext.AddUser(newUser);
        }
    }
}