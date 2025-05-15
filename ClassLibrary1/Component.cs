using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.DataLayer.Models
{
    [Table("Components")] // Укажите имя таблицы, если оно отличается от имени класса
    public class Component
    {
        [Key] // Указывает, что это первичный ключ
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Указывает, что ключ генерируется базой данных
        public int ComponentID { get; set; }

        [Required] // Указывает, что поле обязательно для заполнения
        [StringLength(100)] // Ограничение на длину строки
        public string Name { get; set; }

        [StringLength(150)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Добавьте эту аннотацию
        public decimal Price { get; set; }

        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")] // Указывает, что это внешний ключ
        public Category Category { get; set; } // Навигационное свойство

        [NotMapped]
        public string NameWithCategory => $"{Name} ({Category?.Name})";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}