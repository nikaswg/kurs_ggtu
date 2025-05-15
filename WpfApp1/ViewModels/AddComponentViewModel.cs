using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MyApp.BusinessLogic;
using MyApp.DataLayer;
using MyApp.DataLayer.Models;
using static MyApp.WPF.ComponentListViewModel;
using Component = MyApp.DataLayer.Models.Component;

namespace MyApp.WPF
{
    public class AddComponentViewModel : INotifyPropertyChanged
    {
        private readonly ComponentService _componentService;
        private readonly AppDbContext _dbContext;
        private Component _newComponent = new Component();

        public AddComponentViewModel()
        {
            _componentService = new ComponentService();
            _dbContext = new AppDbContext();
            AddCommand = new RelayCommand(AddComponent);
            LoadCategories();
        }

        public Component NewComponent
        {
            get => _newComponent;
            set
            {
                _newComponent = value;
                OnPropertyChanged(nameof(NewComponent));
            }
        }

        public List<Category> Categories { get; private set; }

        public ICommand AddCommand { get; }

        private void LoadCategories()
        {
            Categories = _dbContext.Categories.AsNoTracking().ToList();
            OnPropertyChanged(nameof(Categories));
        }

        private void AddComponent()
        {
            _componentService.AddComponent(NewComponent, App.Role);
            NewComponent = new Component(); // Сброс формы после добавления
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}