namespace MyApp.DataLayer.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int NameId { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}