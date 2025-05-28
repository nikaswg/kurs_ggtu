using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp.DataLayer.Models
{
    [Table(Name = "UserVotes")]
    public class UserVote : INotifyPropertyChanged
    {
        private int _voteID;
        private int _assemblyID;
        private string _userEmail;
        private int _voteType;
        private Assembly _assembly;

        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int VoteID
        {
            get => _voteID;
            set
            {
                _voteID = value;
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
        public string UserEmail
        {
            get => _userEmail;
            set
            {
                _userEmail = value;
                OnPropertyChanged();
            }
        }

        [Column]
        public int VoteType
        {
            get => _voteType;
            set
            {
                _voteType = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}