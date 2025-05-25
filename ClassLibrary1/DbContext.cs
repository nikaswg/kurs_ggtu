using MyApp.DataLayer.Models;
using System.Data.Linq;

namespace MyApp.DataLayer
{
    public class AppDbContext : DataContext
    {
        public AppDbContext() : base("Server=(localdb)\\mssqllocaldb;Database=bdkurs21;Trusted_Connection=True;") { }

        public Table<User> Users => GetTable<User>();
        public Table<Review> Reviews => GetTable<Review>();
        public Table<Assembly> Assemblies => GetTable<Assembly>();
        public Table<AssemblyComponent> AssemblyComponents => GetTable<AssemblyComponent>();
        public Table<Category> Categories => GetTable<Category>();
        public Table<Component> Components => GetTable<Component>();
        public Table<UserVote> UserVotes => GetTable<UserVote>();

        public User GetUserByEmail(string email)
        {
            var options = new DataLoadOptions();
            options.LoadWith<User>(u => u.Reviews);
            LoadOptions = options;

            return Users.FirstOrDefault(u => u.Email == email);
        }

        public void AddUser(User user)
        {
            Users.InsertOnSubmit(user);
            SubmitChanges();
        }

        public List<User> GetAllUsers()
        {
            return Users.ToList();
        }
    }
}