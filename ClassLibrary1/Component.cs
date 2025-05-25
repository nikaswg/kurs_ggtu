using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.DataLayer.Models
{
    [System.Data.Linq.Mapping.Table(Name = "Components")]
    public class Component : INotifyPropertyChanged
    {
        // Объявляем поле для хранения связи с категорией
        private EntityRef<Category> _categoryRef;

        // Конструктор должен инициализировать EntityRef
        public Component()
        {
            _categoryRef = new EntityRef<Category>();
        }

        [System.Data.Linq.Mapping.Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int ComponentID { get; set; }

        [System.Data.Linq.Mapping.Column]
        public string Name { get; set; }

        [System.Data.Linq.Mapping.Column]
        public string Description { get; set; }

        [System.Data.Linq.Mapping.Column]
        public decimal Price { get; set; }

        [System.Data.Linq.Mapping.Column]
        public int CategoryID { get; set; }

        [Association(Name = "Component_Category", Storage = "_categoryRef",
                    ThisKey = "CategoryID", OtherKey = "CategoryID", IsForeignKey = true)]
        public Category Category
        {
            get { return _categoryRef.Entity; }
            set
            {
                var previousValue = _categoryRef.Entity;
                if (previousValue != value || !_categoryRef.HasLoadedOrAssignedValue)
                {
                    _categoryRef.Entity = value;
                    if (value != null)
                    {
                        CategoryID = value.CategoryID;
                    }
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}