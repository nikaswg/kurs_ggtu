using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MyApp.DataLayer.Models
{
    [Table(Name = "Assemblies")]
    public class Assembly : INotifyPropertyChanged
    {
        private int _assemblyID;
        private int _nameId;
        private string _description;
        private decimal _rating;
        private User _user;
        private EntitySet<AssemblyComponent> _assemblyComponents;
        private EntitySet<Review> _reviews;
        private EntitySet<UserVote> _userVotes;

        public Assembly()
        {
            _assemblyComponents = new EntitySet<AssemblyComponent>();
            _reviews = new EntitySet<Review>();
            _userVotes = new EntitySet<UserVote>();
        }

        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int AssemblyID
        {
            get => _assemblyID;
            set
            {
                _assemblyID = value;
                OnPropertyChanged();
            }
        }

        [Column]
        public int NameId
        {
            get => _nameId;
            set
            {
                _nameId = value;
                OnPropertyChanged();
            }
        }

        [Column]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        [Column]
        public decimal Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged();
            }
        }

        [Association(Storage = "_user", ThisKey = "NameId", OtherKey = "NameId")]
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }

        [Association(Storage = "_assemblyComponents", OtherKey = "AssemblyID", DeleteRule = "CASCADE")]
        public EntitySet<AssemblyComponent> AssemblyComponents
        {
            get => _assemblyComponents;
            set => _assemblyComponents.Assign(value);
        }

        [Association(Storage = "_reviews", OtherKey = "AssemblyID", DeleteRule = "CASCADE")]
        public EntitySet<Review> Reviews
        {
            get => _reviews;
            set => _reviews.Assign(value);
        }

        [Association(Storage = "_userVotes", OtherKey = "AssemblyID", DeleteRule = "CASCADE")]
        public EntitySet<UserVote> UserVotes
        {
            get => _userVotes;
            set => _userVotes.Assign(value);
        }

        public decimal TotalPrice => AssemblyComponents?.Sum(ac => ac.Component?.Price ?? 0) ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Table(Name = "AssemblyComponents")]
    public class AssemblyComponent : INotifyPropertyChanged
    {
        private int _assemblyComponentID;
        private int _assemblyID;
        private int _componentID;
        private Assembly _assembly;
        private Component _component;

        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int AssemblyComponentID
        {
            get => _assemblyComponentID;
            set
            {
                _assemblyComponentID = value;
                OnPropertyChanged();
            }
        }

        [Column]
        public int AssemblyID
        {
            get => _assemblyID;
            set
            {
                _assemblyID = value;
                OnPropertyChanged();
            }
        }

        [Column]
        public int ComponentID
        {
            get => _componentID;
            set
            {
                _componentID = value;
                OnPropertyChanged();
            }
        }

        [Association(Storage = "_assembly", ThisKey = "AssemblyID", OtherKey = "AssemblyID")]
        public Assembly Assembly
        {
            get => _assembly;
            set
            {
                _assembly = value;
                OnPropertyChanged();
            }
        }

        [Association(Storage = "_component", ThisKey = "ComponentID", OtherKey = "ComponentID")]
        public Component Component
        {
            get => _component;
            set
            {
                _component = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}