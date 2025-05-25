using MyApp.DataLayer;
using System.Security.Authentication;


namespace MyApp.BusinessLogicLayer.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }



        public User Authenticate(string email, string password)
        {
            // Проверка ввода
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Введите email");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Введите пароль");

            // Используем String.Compare без StringComparison
            var user = _dbContext.Users
                       .AsEnumerable() // Переключаемся на LINQ to Objects
                       .FirstOrDefault(u =>
                           string.Compare(u.Email, email.Trim(), ignoreCase: true) == 0);

            if (user == null)
                throw new InvalidOperationException("Пользователь не найден");

            // Проверка пароля
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new InvalidOperationException("Неверный пароль");
            }

            return user;
        }

        public void Register(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя не может быть пустым");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new ArgumentException("Неверный формат email");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Пароль должен содержать минимум 8 символов");

            if (_dbContext.Users.Any(u => u.Email == email))
                throw new InvalidOperationException("Пользователь с таким email уже существует");

            var newUser = new User
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };

            _dbContext.AddUser(newUser);
        }
    }
}