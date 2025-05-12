using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.DataLayer.Models
{
    [Table("Reviews")] // Укажите имя таблицы, если оно отличается от имени класса
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewID { get; set; }

        [Required]
        public int AssemblyID { get; set; }

        [ForeignKey("AssemblyID")]
        public virtual Assembly Assembly { get; set; }
        [Required]
        public int NameID { get; set; } // Связь с Users (не nullable)

        [ForeignKey("NameID")]
        public virtual User User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Rating { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }

        [NotMapped]
        public string UserEmail => User?.Email ?? "Guest";
    }
}
