﻿using System.Collections.Generic;
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

        [NotMapped] // Это поле не будет сохраняться в БД
        public decimal TotalPrice
        {
            get
            {
                if (AssemblyComponents == null) return 0;
                return AssemblyComponents.Sum(ac => ac.Component?.Price ?? 0);
            }
        }

        public ICollection<AssemblyComponent> AssemblyComponents { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public virtual ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();



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