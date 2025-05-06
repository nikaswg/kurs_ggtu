using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.DataLayer.Models
{
    [Table("Category")] // Укажите имя таблицы, если оно отличается от имени класса
    public class Category
    {
        [Key] // Указывает, что это первичный ключ
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Указывает, что ключ генерируется базой данных
        public int CategoryID { get; set; }

        [Required] // Указывает, что поле обязательно для заполнения
        [StringLength(50)] // Ограничение на длину строки
        public string Name { get; set; }
    }
}