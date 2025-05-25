using System.Data.Linq.Mapping;

namespace MyApp.DataLayer.Models
{
    [Table(Name = "UserVotes")]
    public class UserVote
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int VoteID { get; set; }

        [Column]
        public int AssemblyID { get; set; }

        [Column]
        public string UserEmail { get; set; }

        [Column]
        public int VoteType { get; set; }

        [Association(Storage = "_Assembly", ThisKey = "AssemblyID", OtherKey = "AssemblyID")]
        public Assembly Assembly { get; set; }
    }
}