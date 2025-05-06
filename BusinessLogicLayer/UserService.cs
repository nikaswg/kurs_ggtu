using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MyApp.DataLayer.Models;
using MyApp.DataLayer;

namespace MyApp.BusinessLogicLayer.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;

        public UserService()
        {
            _dbContext = new AppDbContext();
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
            string[] support_mass =
            {
               name,
               email,
               password,
               "имя",
               "почта",
               "пароль",
            };

            string support_message = "";
            int emptyCount = support_mass.Count(s => string.IsNullOrEmpty(s));

            if (emptyCount > 0)
            {
                support_message = emptyCount == 1 ? "Поле " : "Поля: ";

                for (int i = 0; i < 3; i++)
                {
                    if (string.IsNullOrEmpty(support_mass[i]))
                    {
                        support_message += support_mass[i + 3] + ", ";
                    }
                }

                // Убираем последнюю запятую и пробел
                if (support_message.EndsWith(", "))
                {
                    support_message = support_message.Remove(support_message.Length - 2);
                }

                support_message += emptyCount == 1 ? " пустое. Заполните его." : " пустые. Заполните их.";
                throw new Exception(support_message);
            }

            if (!email.Contains('@'))
                throw new Exception("У электронной почты должен присутствовать символ '@'.");

            if (_dbContext.GetUsers().Any(u => u.Email == email))
                throw new Exception("Пользователь с таким email уже существует.");

            if (password.Length < 8)
                throw new Exception("Пароль должен содержать минимум 8 символов.");

            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User",
            };

            _dbContext.AddUser(newUser);
        }
    }
}