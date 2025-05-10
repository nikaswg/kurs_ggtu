    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

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

            private decimal rating;
            public decimal Rating
            {
                get => rating;
                set
                {
                    rating = value;
                    OnPropertyChanged(nameof(Rating));
                }
            }

            // Связь с оценками
            public virtual ICollection<AssemblyRating> Ratings { get; set; } = new List<AssemblyRating>();

            public ICollection<AssemblyComponent> AssemblyComponents { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void Like(string userEmail)
            {
                if (!Ratings.Any(r => r.UserEmail == userEmail))
                {
                    Ratings.Add(new AssemblyRating { UserEmail = userEmail, AssemblyId = this.AssemblyID, IsLiked = true });
                    Rating++;
                }
            }

            public void Dislike(string userEmail)
            {
                if (!Ratings.Any(r => r.UserEmail == userEmail))
                {
                    Ratings.Add(new AssemblyRating { UserEmail = userEmail, AssemblyId = this.AssemblyID, IsLiked = false });
                    Rating--;
                }
            }
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

        public class AssemblyRating
        {
            public int Id { get; set; }
            public string UserEmail { get; set; }
            public int AssemblyId { get; set; }
            public bool IsLiked { get; set; }
        }
    }