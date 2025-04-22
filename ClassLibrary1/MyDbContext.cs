using System.Data.Linq;

public class MyDbContext : DataContext
{
    public MyDbContext(string connectionString) : base(connectionString) { }
    public Table<User> Users => GetTable<User>(); // Связь с таблицей Users
}