using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;

namespace MyApp.DataLayer.Models
{
    [Table(Name = "Assemblies")]
    public class Assembly : INotifyPropertyChanged
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int AssemblyID { get; set; }

        [Column]
        public int NameId { get; set; }

        [Column]
        public string Description { get; set; }

        private decimal _rating;
        [Column]
        public decimal Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged(nameof(Rating));
            }
        }

        [Association(Storage = "_User", ThisKey = "NameId", OtherKey = "NameId")]
        public User User { get; set; }

        [Association(Storage = "_AssemblyComponents", OtherKey = "AssemblyID")]
        public EntitySet<AssemblyComponent> AssemblyComponents { get; set; }

        [Association(Storage = "_Reviews", OtherKey = "AssemblyID")]
        public EntitySet<Review> Reviews { get; set; }

        [Association(Storage = "_UserVotes", OtherKey = "AssemblyID")]
        public EntitySet<UserVote> UserVotes { get; set; }

        public decimal TotalPrice
        {
            get
            {
                if (AssemblyComponents == null) return 0;
                return AssemblyComponents.Sum(ac => ac.Component?.Price ?? 0);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Table(Name = "AssemblyComponents")]
    public class AssemblyComponent
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int AssemblyComponentID { get; set; }

        [Column]
        public int AssemblyID { get; set; }

        [Column]
        public int ComponentID { get; set; }

        [Association(Storage = "_Assembly", ThisKey = "AssemblyID", OtherKey = "AssemblyID")]
        public Assembly Assembly { get; set; }

        [Association(Storage = "_Component", ThisKey = "ComponentID", OtherKey = "ComponentID")]
        public Component Component { get; set; }
    }
}