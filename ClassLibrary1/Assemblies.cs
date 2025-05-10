using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyApp.DataLayer.Models
{
    public class Assembly : INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssemblyID { get; set; }

        [Required]
        public int NameId { get; set; }

        [ForeignKey("NameId")]
        public User User { get; set; }

        [Required]
        [StringLength(300)]
        public string Description { get; set; }

        private decimal _rating;
        public decimal Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged(nameof(Rating));
            }
        }

        public ICollection<AssemblyComponent> AssemblyComponents { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class AssemblyComponent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssemblyComponentID { get; set; }

        public int AssemblyID { get; set; }
        public int ComponentID { get; set; }

        [ForeignKey("AssemblyID")]
        public Assembly Assembly { get; set; }

        [ForeignKey("ComponentID")]
        public Component Component { get; set; }
    }


}