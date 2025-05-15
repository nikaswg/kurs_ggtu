using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyApp.DataLayer.Models
{
    [Table("UserVotes")]
    public class UserVote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoteID { get; set; }

        public int AssemblyID { get; set; }
        public string UserEmail { get; set; }
        public int VoteType { get; set; }

        [ForeignKey("AssemblyID")]
        public Assembly Assembly { get; set; }
    }
}