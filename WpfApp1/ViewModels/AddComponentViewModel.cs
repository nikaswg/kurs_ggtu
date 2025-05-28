using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
            try
            {
                Categories = _dbContext.Categories.ToList();
                OnPropertyChanged(nameof(Categories));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddComponent()
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(NewComponent.Name))
                    throw new ArgumentException("Название компонента не может быть пустым");

                if (string.IsNullOrWhiteSpace(NewComponent.Description))
                    throw new ArgumentException("Описание компонента не может быть пустым");

                if (NewComponent.Price <= 0)
                    throw new ArgumentException("Цена должна быть больше 0");

                if (NewComponent.CategoryID <= 0)
                    throw new ArgumentException("Необходимо выбрать категорию");

                // Добавление компонента
                _dbContext.Components.InsertOnSubmit(NewComponent);
                _dbContext.SubmitChanges();

                // Сброс формы
                NewComponent = new Component();

                MessageBox.Show("Компонент успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении компонента: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}