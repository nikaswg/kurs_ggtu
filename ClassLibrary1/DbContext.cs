using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using MyApp.DataLayer.Models;

namespace MyApp.DataLayer
{
    public class DbContext
    {
        private readonly string _connectionString;

        public DbContext()
        {
            // Укажите строку подключения к вашему SQL Server
            _connectionString = "Server=(localdb)\\mssqllocaldb;Database=bdkurs;Trusted_Connection=True;";
        }

        public List<User> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Users", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Name = reader["Name"].ToString(),
                            Email = reader["email"].ToString(),
                            Password = reader["password"].ToString(),
                            Role = reader["role"].ToString(),
                            NameId = Convert.ToInt32(reader["NameId"])
                        });
                    }
                }
            }
            return users;
        }

        public void AddUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Указываем только те столбцы, для которых передаются значения
                var command = new SqlCommand(
                    "INSERT INTO Users (Name, email, password, role) VALUES (@Name, @Email, @Password, @Role)",
                    connection);

                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@Role", user.Role);

                command.ExecuteNonQuery(); // Выполняем запрос
            }
        }
    }
}